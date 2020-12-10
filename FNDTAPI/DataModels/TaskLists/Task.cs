using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FNDTAPI.DataModels.TaskLists {

	/// <summary>
	/// Class, which represents single Task in TaskList.
	/// </summary>
	public class Task {

		/// <summary>
		/// ID of a Task.
		/// </summary>
		[BsonId]
		[BsonRepresentation (BsonType.String)]
		public Guid ID { get; set; }
		/// <summary>
		/// Description of the Task.
		/// </summary>
		[BsonRequired]
		public string Text { get; set; }
		/// <summary>
		/// Has the Task been completed.
		/// </summary>
		[BsonRequired]
		[BsonRepresentation (BsonType.Boolean)]
		public bool IsCompleted { get; set; }
		/// <summary>
		/// Guid of a TaskList to which Task has been added.
		/// </summary>
		[BsonRequired]
		[BsonRepresentation (BsonType.String)]
		public Guid OwnerID { get; set; }
		/// <summary>
		/// Defines how many people can declare completion of the task.
		/// </summary>
		[BsonRequired]
		[BsonDefaultValue (1)]
		public int MaximumCountOfPeopleWhoCanDoIt { get; set; }

		public bool AreValuesCorrect() {
			if (string.IsNullOrWhiteSpace (Text) || OwnerID == Guid.Empty) return false;
			return true;
		}

	}
}
