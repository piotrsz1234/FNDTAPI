using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FNDTAPI.DataModels.TaskLists {

	/// <summary>
	/// Class, which represents single Task List.
	/// </summary>
	public class TaskList {
		/// <summary>
		/// Guid of Task List.
		/// </summary>
		[BsonId]
		[BsonRepresentation (BsonType.String)]
		public Guid ID { get; set; }
		/// <summary>
		/// Name of the Task List.
		/// </summary>
		[BsonRequired]
		public string Name { get; set; }
		/// <summary>
		/// Is Task List belongs to person or Calendar Event.
		/// </summary>
		public bool IsPersonal { get; set; }
	}
}
