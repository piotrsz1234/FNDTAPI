using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FNDTAPI.DataModels.Notifications {
	public class ConfirmationNotification : Notification {

		[BsonRepresentation (BsonType.String)]
		public Guid EventID { get; set; }
		[BsonRepresentation (BsonType.String)]
		public Guid RegistrationID { get; set; }

		public ConfirmationNotification () { }

		public ConfirmationNotification (Guid id, string forWho, string creator, Guid eventID, Guid regID) : base (id, forWho, creator) {
			EventID = eventID;
			RegistrationID = regID;
		}

	}
}
