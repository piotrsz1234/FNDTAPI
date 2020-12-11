using System;
using FNDTAPI.DataModels.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FNDTAPI.DataModels.Notifications {
	public abstract class Notification : IDataModel {

		[BsonId]
		[BsonRepresentation (BsonType.String)]
		public Guid ID { get; set; }
		public string ForWho { get; set; }
		public string Creator { get; set; }

		public Notification () { }

		public Notification (Guid id, string forWho, string creator) {
			ID = id;
			ForWho = forWho;
			Creator = creator;
		}

		public bool AreValuesCorrect () {
			return !(string.IsNullOrWhiteSpace (ForWho) || string.IsNullOrWhiteSpace (Creator));
		}
	}
}
