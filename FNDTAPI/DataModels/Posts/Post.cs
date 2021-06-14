using System;
using System.Collections.Generic;
using FDNTAPI.DataModels.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace FDNTAPI.DataModels.Posts {

	/// <summary>
	/// Data Model for Post in Database.
	/// </summary>
	[Serializable]
	public class Post : IDataModel {
		
		/// <summary>
		/// Id of a <see cref="Post"/> in Database.
		/// </summary>
		[BsonRepresentation(BsonType.String)]
		[BsonId]
		public Guid Id { get; set; }
		/// <summary>
		/// Title of a <see cref="Post"/>
		/// </summary>
		public string Title { get; set; }
		/// <summary>
		/// HTML of a <see cref="Post"/>, which also contains images.
		/// </summary>
		public string Html { get; set; }
		/// <summary>
		/// <see cref="DateTime"/> of a Publication.
		/// </summary>
		[BsonRepresentation(BsonType.DateTime)]
		public DateTime PublishTime { get; set; }
		/// <summary>
		/// Declares, who can see that <see cref="Post"/>.
		/// </summary>
		[BsonRequired]
		public string ForWho { get; set; }
		/// <summary>
		/// Who can edit/remove <see cref="Post"/>.
		/// </summary>
		[BsonRequired]
		public string Owner { get; set; }
		/// <summary>
		/// Has <see cref="Post"/> been published?
		/// </summary>
		public bool IsPublished { get; set; }
		/// <summary>
		/// Contains information, when last change happened.
		/// </summary>
		[BsonRepresentation(BsonType.DateTime)]
		public DateTime UpdateTime { get; set; }

		public bool AreValuesCorrect() {
			return !(string.IsNullOrWhiteSpace (ForWho) || string.IsNullOrWhiteSpace (Owner));
		}

		
		
	}
}
