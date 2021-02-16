using System;
using FDNTAPI.DataModels.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FDNTAPI.DataModels.TaskLists {
	/// <summary>
	/// Class, which represents person's declaration of task completion
	/// </summary>
	public class PersonTaskCompletionDeclaration : IDataModel {

		/// <summary>
		/// Id of a declaration.
		/// </summary>
		[BsonId]
		[BsonRepresentation (BsonType.String)]
		public Guid ID { get; set; }
		/// <summary>
		/// Unique string, which represents given person, who made declaration.
		/// </summary>
		[BsonRequired]
		public string Person { get; set; }
		/// <summary>
		/// Guid of a Task, which completion has been declared.
		/// </summary>
		[BsonRequired]
		[BsonRepresentation (BsonType.String)]
		public Guid Task { get; set; }

		public bool AreValuesCorrect() {
			if (string.IsNullOrWhiteSpace (Person) || Task == Guid.Empty)
				return false;
			return true;
		}

	}
}
