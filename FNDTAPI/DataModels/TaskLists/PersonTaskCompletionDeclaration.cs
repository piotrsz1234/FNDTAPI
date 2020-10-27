using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FNDTAPI.DataModels.TaskLists {
	/// <summary>
	/// Class, which represents person's declaration of task completion
	/// </summary>
	public class PersonTaskCompletionDeclaration {

		/// <summary>
		/// ID of a declaration.
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

	}
}
