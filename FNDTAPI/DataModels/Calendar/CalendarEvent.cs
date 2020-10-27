using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FNDTAPI.DataModels.Calendar {

	/// <summary>
	/// Class, which represents Event in Calendar
	/// </summary>
	public class CalendarEvent {

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

	}
}
