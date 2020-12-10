using System;
using FNDTAPI.DataModels.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FNDTAPI.DataModels.Posts {

	/// <summary>
	/// Represents attachment added to <see cref="Post"/>
	/// </summary>
	[Serializable]
	public class Attachment : IDataModel {

		/// <summary>
		/// ID of a <see cref="Attachment"/> in Database.
		/// </summary>
		[BsonId]
		[BsonRepresentation(BsonType.String)]
		public Guid ID { get; set; }
		/// <summary>
		/// ID of a <see cref="Post"/>.
		/// </summary>
		[BsonRepresentation(BsonType.String)]
		public Guid PostID { get; set; }
		/// <summary>
		/// Original - user's name of a file.
		/// </summary>
		[BsonRequired]
		public string OriginalFileName { get; set; }
		/// <summary>
		/// Name of a file created by server.
		/// </summary>
		[BsonRequired]
		public string CurrentFileName { get; set; }
		/// <summary>
		/// <see cref="Guid"/> of a <see cref="OldVersionOfPost"/>, if this <see cref="Attachment"/> has been removed from newer versions.
		/// </summary>
		[BsonRepresentation(BsonType.String)]
		public Guid OldVersionID { get; set; }

		public bool AreValuesCorrect () {
			return !(string.IsNullOrWhiteSpace(OriginalFileName) || string.IsNullOrWhiteSpace(CurrentFileName));
		}
	
	}

}
