using System;
using FDNTAPI.DataModels.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FDNTAPI.DataModels.TaskLists {

	/// <summary>
	/// Class, which represents single Task List.
	/// </summary>
	public class TaskList : IDataModel {
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
		/// <summary>
		/// Contains information about owner/creator.
		/// </summary>
		public string Owner { get; set; }

		public bool AreValuesCorrect() {
			if (string.IsNullOrWhiteSpace (Name)) return false;
			return true;
		}

	}
}
