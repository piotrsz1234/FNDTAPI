using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FDNTAPI.DataModels.Notifications {
	public class NewPostNotification : Notification {

		[BsonRepresentation (BsonType.String)]
		public Guid PostID { get; set; }

		public NewPostNotification () { }

		public NewPostNotification (Guid id, string forWho, string creator, Guid postID) : base(id, forWho, creator) {
			PostID = postID;
		}

	}
}
