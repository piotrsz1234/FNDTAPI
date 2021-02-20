using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FDNTAPI.DataModels.Notifications;
using FDNTAPI.DataModels.Posts;
using FDNTAPI.DataModels.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace FDNTAPI.Controllers {

	/// <summary>
	///	Controller contains actions for <see cref="Post"/> and <see cref="OldVersionOfPost"/> and <see cref="Attachment"/>. 
	/// </summary>
	[Route ("api/v1.0/[controller]")]
	[ApiController]
	public class PostController : ControllerBase {

		private readonly IConfiguration _configuration;

		public PostController (IConfiguration configuration) {
			_configuration = configuration;
		}

		[HttpPost]
		[Route ("posts")]
		public async Task<IActionResult> AddPostAsync (Post post, [FromServices] IMongoCollection<Post> mongoCollection) {
			if (post == null || !post.AreValuesCorrect ())
				return this.Error (HttpStatusCode.UnprocessableEntity, "Post is null or it's properties are empty!");
			post.ID = Guid.NewGuid ();
			post.PublishTime = DateTime.Now;
			await mongoCollection.InsertOneAsync (post);
			return this.Success (post.ID);
		}

		[HttpPatch]
		[Route ("posts/publish")]
		public async Task<IActionResult> PublishChangesAsync (Post newPost, [FromServices] IMongoCollection<Post> mongoCollection, [FromServices] IMongoCollection<OldVersionOfPost> oldCollection, [FromServices] IMongoCollection<Notification> notificationsCollection) {
			Post currentValue = await (await mongoCollection.FindAsync (x => x.ID == newPost.ID)).FirstOrDefaultAsync ();
			if (currentValue == null)
				return this.Error (HttpStatusCode.NotFound, "There's no such Post!");
			OldVersionOfPost oldVersion = new OldVersionOfPost (currentValue) {
				ID = Guid.NewGuid ()
			};
			if (!currentValue.IsPublished)
				await oldCollection.InsertOneAsync (oldVersion);
			if(newPost.ID == Guid.Empty) newPost.ID = Guid.NewGuid();
			newPost.PublishTime = DateTime.Now;
			newPost.IsPublished = true;
			UpdateResult result = await mongoCollection.UpdateOneAsync (x => x.ID == newPost.ID, Extensions.GenerateUpdateDefinition (currentValue, newPost));
			if (result.IsAcknowledged)
				return this.Success (newPost.ID);
			else return this.Error (HttpStatusCode.InternalServerError, "Publishing changes failed!");
		}

		[HttpDelete]
		[Route ("posts")]
		public async Task<IActionResult> DeletePostAsync (Post post, [FromServices] IMongoCollection<Post> mongoCollection, [FromServices] IMongoCollection<OldVersionOfPost> oldCollection) {
			Post currentValue = await (await mongoCollection.FindAsync (x => x.ID == post.ID)).FirstOrDefaultAsync ();
			if (currentValue == null)
				return this.Error (HttpStatusCode.NotFound, $"There's no post with Id={post.ID}");
			if (currentValue.IsPublished) {
				OldVersionOfPost temp = new OldVersionOfPost (currentValue) {
					ID = Guid.NewGuid ()
				};
				await oldCollection.InsertOneAsync (temp);
			}
			DeleteResult result = await mongoCollection.DeleteOneAsync (x => x.ID == post.ID);
			if (result.IsAcknowledged)
				return this.Ok ();
			return this.Error (HttpStatusCode.InternalServerError, "Deleting of a post failed!");
		}

		[HttpPatch]
		[Route ("posts")]
		public async Task<IActionResult> UpdatePostAsync (Post newPost, [FromServices] IMongoCollection<Post> mongoCollection) {
			if (newPost == null || !newPost.AreValuesCorrect () || newPost.IsPublished)
				return this.Error (HttpStatusCode.UnprocessableEntity, "Sent post is null or has incorrect values or is already published. This action is for unpublished posts");
			Post currentValue = await (await mongoCollection.FindAsync (x => x.ID == newPost.ID)).FirstOrDefaultAsync ();
			if (currentValue == null || currentValue.IsPublished)
				return this.Error (HttpStatusCode.NotFound, $"There's no such Post with Id: {newPost.ID} or is published. If it's published, use 'post/publish'.");
			UpdateResult result = await mongoCollection.UpdateOneAsync (x => x.ID == newPost.ID, Extensions.GenerateUpdateDefinition (currentValue, newPost));
			if (result.IsAcknowledged)
				return this.Success (newPost.ID);
			else return this.Error (HttpStatusCode.InternalServerError, "Updating of a post failed!");
		}

		[HttpGet]
		[Route ("posts")]
		public async Task<IActionResult> GetAvailablePosts (string email, string groups, int howMany, int fromWhere, [FromServices] IMongoCollection<Post> mongoCollection) {
			List<Post> result = new List<Post> ();
			using (var cursor = await mongoCollection.FindAsync (x => x.IsPublished)) {
				do {
					if (cursor.Current == null) continue;
					var temp = cursor.Current?.Where(x =>
						x.ForWho.Contains(email) || groups.Split('\n').Any(y => x.ForWho.Contains(y)));
					result.AddRange (temp);
				} while (await cursor.MoveNextAsync ());
			}
			result.Sort((x, y) => DateTime.Compare(x.PublishTime, y.PublishTime));
			return this.Success (result.Subset(fromWhere, howMany));
		}

		[HttpGet]
		[Route ("posts/mine")]
		public async Task<IActionResult> GetMinePost (string user, [FromServices] IMongoCollection<Post> mongoCollection) {
			var result = await (await mongoCollection.FindAsync (x => x.Owner == user)).ToListAsync ();
			return this.Success (result);
		}

		[HttpPost]
		[Route ("attachments")]
		public async Task<IActionResult> AddAttachmentsAsync (AttachmentUploadFormat attachmentUpload, [FromServices] IMongoCollection<Attachment> mongoCollection) {
			int output = 0;
			if (attachmentUpload == null) return this.Error (HttpStatusCode.UnprocessableEntity, "Data is null!");
			foreach (IFormFile item in attachmentUpload.Files) {
				if (item.Length > 10240 * 1024 || item.Length == 0) continue;
				Attachment value = new Attachment {
					ID = Guid.NewGuid (),
					OriginalFileName = item.FileName,
					CurrentFileName = Path.GetRandomFileName ()
				};
				string filePath = Path.Combine (_configuration["FilesStoragePath"], attachmentUpload.ID.ToString (), value.CurrentFileName);
				await using (FileStream stream = System.IO.File.Create (filePath)) {
					await item.CopyToAsync (stream);
				}
				await mongoCollection.InsertOneAsync (value);
				output++;
			}
			return this.Success (output);
		}

		[HttpGet]
		[Route ("attachments")]
		public async Task<IActionResult> GetAttachmentsAsync (Guid postId, [FromServices] IMongoCollection<Attachment> mongoCollection) {
			List<Attachment> result = await (await mongoCollection.FindAsync (x => x.PostID == postId)).ToListAsync ();
			return this.Success (result);
		}

		[HttpGet]
		[Route ("attachments")]
		public async Task<IActionResult> GetAttachmentAsync (Guid id, string name, [FromServices] IMongoCollection<Attachment> mongoCollection) {
			Attachment attachment = await (await mongoCollection.FindAsync (x => x.PostID == id && x.OriginalFileName == name)).FirstOrDefaultAsync ();
			if (attachment == null)
				return NotFound ();
			string filePath = Path.Combine (_configuration["FilesStoragePath"], ((attachment.PostID == Guid.Empty) ? attachment.OldVersionID : attachment.PostID).ToString (), name);
			using (Stream stream = System.IO.File.OpenRead (filePath)) {
				return File (stream, "application/octet-stream", attachment.OriginalFileName);
			}
		}

		[HttpDelete]
		[Route ("attachments")]
		public async Task<IActionResult> RemoveAttachment (Guid attachmentId, [FromServices] IMongoCollection<Attachment> mongoCollection) {
			Attachment result = await (await mongoCollection.FindAsync (x => x.ID == attachmentId)).FirstOrDefaultAsync ();
			if (result == null)
				return this.Error (HttpStatusCode.NotFound,$"There's no such attachment with Id={attachmentId}");
			Attachment newValue = result.Copy ();
			newValue.OldVersionID = result.PostID;
			newValue.PostID = Guid.Empty;
			UpdateResult temp = await mongoCollection.UpdateOneAsync (x => x.ID == attachmentId, Extensions.GenerateUpdateDefinition (result, newValue));
			if (temp.IsAcknowledged)
				return this.Success (null);
			else return this.Error (HttpStatusCode.InternalServerError,"Removing attachment failed!");
		}

		[HttpGet]
		[Route ("oldposts")]
		public async Task<IActionResult> GetOldVersionsOfPost(Guid postId, [FromServices] IMongoCollection<OldVersionOfPost> mongoCollection) {
			var result = await (await mongoCollection.FindAsync (x => x.PostID == postId)).ToListAsync ();
			return this.Success (result);
		}

	}
}
