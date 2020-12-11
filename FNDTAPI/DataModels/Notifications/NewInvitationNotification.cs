using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FNDTAPI.DataModels.Notifications {
	public class NewInvitationNotification : Notification {

		[BsonRepresentation (BsonType.String)]
		public Guid RegistrationID { get; set; }

		public NewInvitationNotification () { }

		public NewInvitationNotification (Guid id, string forWho, string creator, Guid regID) : base (id, forWho, creator) {
			RegistrationID = regID;
		}

	}
}
