using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FDNTAPI.DataModels.Interfaces;
using FDNTAPI.DataModels.Posts;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace FDNTAPI.Controllers {

    /// <summary>
    ///	Controller contains actions for <see cref="Post"/> and <see cref="OldVersionOfPost"/>. 
    /// </summary>
    [Route("api/v1.0/[controller]")]
    [ApiController]
    public class PostController : ControllerBase {

        private readonly IMongoCollection<Post> _postsMongoCollection;
        private readonly IMongoCollection<OldVersionOfPost> _oldPostsMongoCollection;

        public PostController([FromServices] IMongoCollection<Post> postsMongoCollection,
            [FromServices] IMongoCollection<OldVersionOfPost> oldPostsMongoCollection) {
            _postsMongoCollection = postsMongoCollection;
            _oldPostsMongoCollection = oldPostsMongoCollection;
        }

        [HttpPost]
        [Route("posts")]
        public async Task<IActionResult> AddPostAsync(Post post) {
            if (post == null || !post.AreValuesCorrect())
                return this.Error(HttpStatusCode.UnprocessableEntity, "Post is null or it's properties are empty!");
            post.Id = Guid.NewGuid();
            post.PublishTime = DateTime.Now;
            await _postsMongoCollection.InsertOneAsync(post);
            return this.Success(post.Id);
        }

        [HttpPatch]
        [Route("posts/publish")]
        public async Task<IActionResult> PublishChangesAsync(Dictionary<string, object> changes) {
            if (changes == null || changes.Count == 0 || !changes.ContainsKey("Id"))
                return this.Error(HttpStatusCode.UnprocessableEntity,
                    "Send changes data is empty or doesn't contain Id");
            Post currentValue =
                await (await _postsMongoCollection.FindAsync(x => x.Id == Guid.Parse(changes["Id"] as string))).FirstOrDefaultAsync();
            if (currentValue == null)
                return this.Error(HttpStatusCode.NotFound, "There's no such Post!");
            OldVersionOfPost oldVersion = new OldVersionOfPost(currentValue) {
                Id = Guid.NewGuid()
            };
            if (!currentValue.IsPublished)
                await _oldPostsMongoCollection.InsertOneAsync(oldVersion);
            var newPost = (Post) (currentValue as IDataModel).ApplyChanges(changes);
            
            if (newPost == null)
                return this.Error(HttpStatusCode.BadRequest,
                    "Changes contains properties, that Post does not contain!");
            
            if(!currentValue.IsPublished)
                newPost.PublishTime = DateTime.Now;
            else newPost.UpdateTime = DateTime.Now;
            newPost.IsPublished = true;
            UpdateResult result = await _postsMongoCollection.UpdateOneAsync(x => x.Id == newPost.Id,
                Extensions.GenerateUpdateDefinition(currentValue, newPost));
            if (result.IsAcknowledged)
                return this.Success(newPost.Id);
            else return this.Error(HttpStatusCode.InternalServerError, "Publishing changes failed!");
        }

        [HttpDelete]
        [Route("posts")]
        public async Task<IActionResult> DeletePostAsync(Post post) {
            Post currentValue =
                await (await _postsMongoCollection.FindAsync(x => x.Id == post.Id)).FirstOrDefaultAsync();
            if (currentValue == null)
                return this.Error(HttpStatusCode.NotFound, $"There's no post with Id={post.Id}");
            if (currentValue.IsPublished) {
                OldVersionOfPost temp = new OldVersionOfPost(currentValue) {
                    Id = Guid.NewGuid()
                };
                await _oldPostsMongoCollection.InsertOneAsync(temp);
            }

            DeleteResult result = await _postsMongoCollection.DeleteOneAsync(x => x.Id == post.Id);
            if (result.IsAcknowledged)
                return this.Ok();
            return this.Error(HttpStatusCode.InternalServerError, "Deleting of a post failed!");
        }

        [HttpPatch]
        [Route("posts")]
        public async Task<IActionResult> UpdatePostAsync(Dictionary<string, object> changes) {
            if (changes == null || changes.Count == 0 || !changes.ContainsKey("Id"))
                return this.Error(HttpStatusCode.UnprocessableEntity,
                    "Send changes data is empty or doesn't contain Id");
            Post currentValue =
                await _postsMongoCollection.FirstOrDefaultAsync(x => x.Id == Guid.Parse(changes["Id"] as string));
            if (currentValue == null || currentValue.IsPublished)
                return this.Error(HttpStatusCode.NotFound,
                    $"There's no such Post with Id: {changes["Id"]} or it's is published. If it's published, use 'post/publish'.");
            
            var newPost = (Post) ((IDataModel) currentValue).ApplyChanges(changes);
            if (newPost == null)
                return this.Error(HttpStatusCode.BadRequest,
                    "Changes contains properties, that Post does not contain!");
            newPost.UpdateTime = DateTime.Now;
            UpdateResult result = await _postsMongoCollection.UpdateOneAsync(x => x.Id == newPost.Id,
                Extensions.GenerateUpdateDefinition(currentValue, newPost));
            if (result.IsAcknowledged)
                return this.Success(newPost.Id);
            else return this.Error(HttpStatusCode.InternalServerError, "Updating of a post failed!");
        }

        [HttpGet]
        [Route("posts")]
        public async Task<IActionResult> GetAvailablePosts(string email, string groups, int howMany, int fromWhere,
            DateTime lastUpdated) {
            List<Post> result = new List<Post>();
            var groupsArray = groups.Split('\n');
            using (var cursor = await _postsMongoCollection.FindAsync(x => x.IsPublished)) {
                do {
                    if (cursor.Current == null) continue;
                    var temp = cursor.Current?.Where(x =>
                        x.ForWho.Contains(email) || groupsArray.Any(y => x.ForWho.Contains(y)));
                    result.AddRange(temp);
                } while (await cursor.MoveNextAsync());
            }

            result.Sort((x, y) => DateTime.Compare(x.PublishTime, y.PublishTime));
            var response = result.Subset(fromWhere, howMany);
            if (response.All(x => x.PublishTime < lastUpdated && x.UpdateTime < lastUpdated))
                return this.Error(HttpStatusCode.NotModified, "Collection was not changed");
            return this.Success(response);
        }

        [HttpGet]
        [Route("posts/mine")]
        public async Task<IActionResult> GetMinePostsAsync(string user) {
            var result = await (await _postsMongoCollection.FindAsync(x => x.Owner == user)).ToListAsync();
            return this.Success(result);
        }

        [HttpGet]
        [Route("oldposts")]
        public async Task<IActionResult> GetOldVersionsOfPost(Guid postId) {
            var result = await (await _oldPostsMongoCollection.FindAsync(x => x.PostId == postId)).ToListAsync();
            return this.Success(result);
        }

    }

}