using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FDNTAPI.DataModels.Notifications {
	public class NewEventNotification : Notification {

		[BsonRepresentation (BsonType.String)]
		public Guid EventID { get; set; }

		public NewEventNotification () { }

		public NewEventNotification (Guid id, string forWho, string creator, Guid eventID) : base (id, forWho, creator) {
			EventID = eventID;
		}

	}
}
