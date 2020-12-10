using System;
using FNDTAPI.DataModels.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FNDTAPI.DataModels.Posts {

	/// <summary>
	/// Represents previous, published versions of a <see cref="Post"/>.
	/// </summary>
	[Serializable]
	public class OldVersionOfPost : IDataModel {

		/// <summary>
		/// ID of a version in Database.
		/// </summary>
		[BsonId]
		[BsonRepresentation(BsonType.String)]
		public Guid ID { get; set; }
		/// <summary>
		/// <see cref="Guid"/> of a current version of a <see cref="Post"/>.
		/// </summary>
		[BsonRepresentation(BsonType.String)]
		public Guid PostID { get; set; }
		/// <summary>
		/// <see cref="DateTime"/> of a publication of this version.
		/// </summary>
		[BsonRepresentation(BsonType.DateTime)]
		public DateTime PublishTime { get; set; }
		/// <summary>
		/// HTML of this version post.
		/// </summary>
		public string Html { get; set; }
		/// <summary>
		/// For who this version of a <see cref="Post"/> was dedicated.
		/// </summary>
		public string ForWho { get; set; }

		/// <summary>
		/// Creates empty instance of an <see cref="OldVersionOfPost"/>.
		/// </summary>
		public OldVersionOfPost () { }

		/// <summary>
		/// Creates instance of a <see cref="OldVersionOfPost"/> based on given <see cref="Post"/>.
		/// </summary>
		/// <param name="post">Which post is a base for creating instance.</param>
		public OldVersionOfPost(Post post) {
			PostID = post.ID;
			PublishTime = post.PublishTime;
			Html = post.Html;
			ForWho = post.ForWho;
		}

		public bool AreValuesCorrect () {
			return !(PostID == Guid.Empty || PublishTime == null || string.IsNullOrEmpty (ForWho));
		}
	}
}
