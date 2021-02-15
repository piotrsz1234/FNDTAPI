using System;
using FDNTAPI.DataModels.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FDNTAPI.DataModels.Calendar {

	/// <summary>
	/// Class, which represents Event in Calendar
	/// </summary>
	public class CalendarEvent : IDataModel {

		/// <summary>
		/// Unique ID of Event.
		/// </summary>
		[BsonId]
		[BsonRepresentation (BsonType.String)]
		public Guid ID { get; set; }
		/// <summary>
		/// Name of Event.
		/// </summary>
		[BsonRequired]
		public string Name { get; set; }
		/// <summary>
		/// For who is Event organized.
		/// If Event is for dedicated group, then it contains emails of possible participants. Otherwise it is name of one of natives groups.
		/// </summary>
		[BsonRequired]
		public string ForWho { get; set; }
		/// <summary>
		/// Date and Time, when Event begins.
		/// </summary>
		[BsonRequired]
		[BsonRepresentation (BsonType.DateTime)]
		public DateTime WhenBegins { get; set; }
		/// <summary>
		/// Date and Time, when Event ends.
		/// </summary>
		[BsonRequired]
		[BsonRepresentation (BsonType.DateTime)]
		public DateTime WhenEnds { get; set; }
		/// <summary>
		/// Location, where Event is happening.
		/// </summary>
		[BsonRequired]
		public string Location { get; set; }
		/// <summary>
		/// Guid of Task List, which has been added to Event.
		/// </summary>
		[BsonRepresentation (BsonType.String)]
		public Guid TaskListID { get; set; }
		/// <summary>
		/// Is Event only for native group or only for selected people
		/// </summary>
		[BsonRepresentation(BsonType.Boolean)]
		public bool IsForDedicatedGroup { get; set; }
		/// <summary>
		/// Guid of the category of the CalendarEvent
		/// </summary>
		[BsonRequired]
		[BsonRepresentation (BsonType.String)]
		public Guid Category { get; set; }

		/// <summary>
		/// Email of a person, who created Calendar Event
		/// </summary>
		[BsonRequired]
		public string CreatorEmail { get; set; }

		public bool AreValuesCorrect() {
			if (string.IsNullOrWhiteSpace (Name) || string.IsNullOrWhiteSpace (ForWho) || string.IsNullOrWhiteSpace (Location) || Category == Guid.Empty || WhenEnds < WhenBegins)
				return false;
			return true;
		}

	}
}
