using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FNDTAPI.DataModels.Posts;
using FNDTAPI.DataModels.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace FNDTAPI.Controllers {

	/// <summary>
	///	Controller contains actions for <see cref="Post"/> and <see cref="OldVersionOfPost"/> and <see cref="Attachment"/>. 
	/// </summary>
	[Route ("api/v1.0/[controller]")]
	[ApiController]
	public class PostController : ControllerBase {

		private readonly IConfiguration config;

		public PostController (IConfiguration configuration) {
			config = configuration;
		}

		[HttpPost]
		[Route ("post")]
		public async Task<IActionResult> AddPostAsync (Post post, [FromServices] IMongoCollection<Post> mongoCollection) {
			if (post == null || !post.AreValuesCorrect ())
				return this.Error ("Post is null or it's properties are empty!");
			post.ID = Guid.NewGuid ();
			post.PublishTime = DateTime.Now;
			await mongoCollection.InsertOneAsync (post);
			return this.Success (post.ID);
		}

		[HttpPatch]
		[Route ("post/publish")]
		public async Task<IActionResult> PublishChangesAsync (Post newPost, [FromServices] IMongoCollection<Post> mongoCollection, [FromServices] IMongoCollection<OldVersionOfPost> oldCollection) {
			Post currentValue = await (await mongoCollection.FindAsync (x => x.ID == newPost.ID)).FirstOrDefaultAsync ();
			if (currentValue == null)
				return this.Error ("There's no such Post!");
			OldVersionOfPost oldVersion = new OldVersionOfPost (currentValue) {
				ID = Guid.NewGuid ()
			};
			if (currentValue != null || !currentValue.IsPublished)
				await oldCollection.InsertOneAsync (oldVersion);
			newPost.PublishTime = DateTime.Now;
			newPost.IsPublished = true;
			UpdateResult result = await mongoCollection.UpdateOneAsync (x => x.ID == newPost.ID, Extensions.GenerateUpdateDefinition (currentValue, newPost));
			if (result.IsAcknowledged)
				return this.Success (newPost.ID);
			else return this.Error ("Publishing changes failed!");
		}

		[HttpDelete]
		[Route ("post")]
		public async Task<IActionResult> DeletePostAsync (Guid id, [FromServices] IMongoCollection<Post> mongoCollection, [FromServices] IMongoCollection<OldVersionOfPost> oldCollection) {
			Post currentValue = await (await mongoCollection.FindAsync (x => x.ID == id)).FirstOrDefaultAsync ();
			if (currentValue == null)
				return this.Error ($"There's no post with ID={id}");
			if (currentValue.IsPublished) {
				OldVersionOfPost temp = new OldVersionOfPost (currentValue) {
					ID = Guid.NewGuid ()
				};
				await oldCollection.InsertOneAsync (temp);
			}
			DeleteResult result = await mongoCollection.DeleteOneAsync (x => x.ID == id);
			if (result.IsAcknowledged)
				return this.Success ("Post has been removed successfully");
			else return this.Error ("Deleting of a post failed!");
		}

		[HttpPatch]
		[Route ("post")]
		public async Task<IActionResult> UpdatePostAsync (Post newPost, [FromServices] IMongoCollection<Post> mongoCollection) {
			if (newPost == null || !newPost.AreValuesCorrect () || newPost.IsPublished)
				return this.Error ("Sent post is null or has incorrect values or is already published. This action is for unpublished posts");
			Post currentValue = await (await mongoCollection.FindAsync (x => x.ID == newPost.ID)).FirstOrDefaultAsync ();
			if (currentValue == null || currentValue.IsPublished)
				return this.Error ($"There's no such Post with ID: {newPost.ID} or is published. If it's published, use 'post/publish'.");
			UpdateResult result = await mongoCollection.UpdateOneAsync (x => x.ID == newPost.ID, Extensions.GenerateUpdateDefinition (currentValue, newPost));
			if (result.IsAcknowledged)
				return this.Success (newPost.ID);
			else return this.Error ("Updating of a post failed!");
		}

		[HttpGet]
		[Route ("post")]
		public async Task<IActionResult> GetAvailablePosts (string email, [FromServices] IMongoCollection<Post> mongoCollection) {
			List<Post> result = new List<Post> ();
			using (var cursor = await mongoCollection.FindAsync (x => x != null)) {
				do {
					result.AddRange (cursor.Current.Where (x => x.ForWho.Contains (email)));
				} while (await cursor.MoveNextAsync ());
			}
			return this.Success (result);
		}

		[HttpGet]
		[Route ("post")]
		public async Task<IActionResult> GetMinePost (string user, [FromServices] IMongoCollection<Post> mongoCollection) {
			var result = await (await mongoCollection.FindAsync (x => x.Owner == user)).ToListAsync ();
			return this.Success (result);
		}

		[HttpPost]
		[Route ("attachments")]
		public async Task<IActionResult> AddAttachmentsAsync (AttachmentUploadFormat attachmentUpload, [FromServices] IMongoCollection<Attachment> mongoCollection) {
			int output = 0;
			foreach (IFormFile item in attachmentUpload.Files) {
				if (item.Length > 10240 * 1024 || item.Length == 0) continue;
				Attachment value = new Attachment {
					ID = Guid.NewGuid (),
					OriginalFileName = item.FileName,
					CurrentFileName = Path.GetRandomFileName ()
				};
				string filePath = Path.Combine (config["FilesStoragePath"], attachmentUpload.ID.ToString (), value.CurrentFileName);
				using (FileStream stream = System.IO.File.Create (filePath)) {
					await item.CopyToAsync (stream);
				}
				await mongoCollection.InsertOneAsync (value);
				output++;
			}
			return this.Success (output);
		}

		[HttpGet]
		[Route ("attachments")]
		public async Task<IActionResult> GetAttachmentsAsync (Guid postID, [FromServices] IMongoCollection<Attachment> mongoCollection) {
			List<Attachment> result = await (await mongoCollection.FindAsync (x => x.PostID == postID)).ToListAsync ();
			IEnumerable<string> output = result.Select (x => x.OriginalFileName);
			return this.Success (result);
		}

		[HttpGet]
		[Route ("attachments")]
		public async Task<IActionResult> GetAttachmentAsync (Guid id, string name, [FromServices] IMongoCollection<Attachment> mongoCollection) {
			Attachment attachment = await (await mongoCollection.FindAsync (x => x.PostID == id && x.OriginalFileName == name)).FirstOrDefaultAsync ();
			if (attachment == null)
				return NotFound ();
			string filePath = Path.Combine (config["FilesStoragePath"], ((attachment.PostID == Guid.Empty) ? attachment.OldVersionID : attachment.PostID).ToString (), name);
			using (Stream stream = System.IO.File.OpenRead (filePath)) {
				return File (stream, "application/octet-stream", attachment.OriginalFileName);
			}
		}

		[HttpDelete]
		[Route ("attachments")]
		public async Task<IActionResult> RemoveAttachment (Guid attachmentID, [FromServices] IMongoCollection<Attachment> mongoCollection) {
			Attachment result = await (await mongoCollection.FindAsync (x => x.ID == attachmentID)).FirstOrDefaultAsync ();
			if (result == null)
				return this.Error ($"There's no such attachment with ID={attachmentID}");
			Attachment newValue = result.Copy ();
			newValue.OldVersionID = result.PostID;
			newValue.PostID = Guid.Empty;
			UpdateResult temp = await mongoCollection.UpdateOneAsync (x => x.ID == attachmentID, Extensions.GenerateUpdateDefinition (result, newValue));
			if (temp.IsAcknowledged)
				return this.Success (null);
			else return this.Error ("Removing attachment failed!");
		}

		[HttpGet]
		[Route ("oldposts")]
		public async Task<IActionResult> GetOldVersionsOfPost(Guid postID, [FromServices] IMongoCollection<OldVersionOfPost> mongoCollection) {
			var result = await (await mongoCollection.FindAsync (x => x.PostID == postID)).ToListAsync ();
			return this.Success (result);
		}

	}
}
