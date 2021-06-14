using System;
using System.Collections.Generic;
using FDNTAPI.DataModels.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FDNTAPI.DataModels.Calendar {

	/// <summary>
	/// Class, which represents registration of user in CalendarEvent.
	/// </summary>
	public class ParticipationRegistration : IDataModel {

		/// <summary>
		/// Guid, which is key in database's collection.
		/// </summary>
		[BsonRepresentation (BsonType.String)]
		[BsonId]
		public Guid Id { get; set; }
		/// <summary>
		/// Unique string, which represents user. In this case: Email.
		/// </summary>
		[BsonRequired]
		public string User { get; set; }
		/// <summary>
		/// Guid of Calendar Event, to which user has registered participation.
		/// </summary>
		[BsonRequired]
		[BsonRepresentation (BsonType.String)]
		public Guid CalendarEventId { get; set; }
		/// <summary>
		/// Tells, if creator of event confirmed registration.
		/// </summary>
		[BsonRepresentation (BsonType.Boolean)]
		public bool HasOwnerConfirmed { get; set; }
		/// <summary>
		/// Tells, if user accepted invitation to participation in the event.
		/// </summary>
		[BsonRepresentation (BsonType.Boolean)]
		public bool HasParticipantConfirmed { get; set; }

		public bool AreValuesCorrect() {
			if (string.IsNullOrWhiteSpace (User) || CalendarEventId == Guid.Empty)
				return false;
			return true;
		}
		
	}
}
