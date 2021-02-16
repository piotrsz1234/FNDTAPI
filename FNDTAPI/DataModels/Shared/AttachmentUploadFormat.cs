using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace FDNTAPI.DataModels.Shared {
	
	/// <summary>
	/// Represents data received, while uploading attachments to post.
	/// </summary>
	[Serializable]
	public class AttachmentUploadFormat {

		/// <summary>
		/// Id of a Post.
		/// </summary>
		public Guid ID { get; set; }
		/// <summary>
		/// List of <see cref="IFormFile"/>.
		/// </summary>
		public List<IFormFile> Files { get; set; }

	}
}
