﻿using System;
using System.Collections.Generic;
using FDNTAPI.DataModels.Interfaces;
using FDNTAPI.DataModels.Shared;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FDNTAPI.DataModels.Calendar {
	public class CalendarEventCategory : IDataModel {

		/// <summary>
		/// Id of a Category.
		/// </summary>
		[BsonId]
		[BsonRepresentation (BsonType.String)]
		public Guid Id { get; set; }
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

		public bool AreValuesCorrect() {
			if (string.IsNullOrWhiteSpace (Owner) || string.IsNullOrWhiteSpace (Name))
				return false;
			return true;
		}

	}
}