using System;
using FNDTAPI.DataModels.Shared;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FNDTAPI.DataModels.Calendar {
	public class CalendarEventCategory {

		/// <summary>
		/// Id of a Category.
		/// </summary>
		[BsonId]
		[BsonRepresentation (BsonType.String)]
		public Guid ID { get; set; }
		/// <summary>
		/// Name of a Category.
		/// </summary>
		[BsonRequired]
		public string Name { get; set; }
		/// <summary>
		/// Color of the Category.
		/// </summary>
		[BsonRequired]
		public SerializableColor Color { get; set; }
		/// <summary>
		/// Is category own by single person or native group.
		/// </summary>
		[BsonRepresentation (BsonType.Boolean)]
		public bool IsPersonal { get; set; }
		/// <summary>
		/// Identifier of a owner of category.
		/// </summary>
		[BsonRequired]
		public string Owner { get; set; }
	}
}
