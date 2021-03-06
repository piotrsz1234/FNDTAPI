﻿using System;
using System.Collections.Generic;
using FDNTAPI.DataModels.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FDNTAPI.DataModels.Posts {

	/// <summary>
	/// Represents previous, published versions of a <see cref="Post"/>.
	/// </summary>
	[Serializable]
	public class OldVersionOfPost : IDataModel {

		/// <summary>
		/// Id of a version in Database.
		/// </summary>
		[BsonId]
		[BsonRepresentation(BsonType.String)]
		public Guid Id { get; set; }
		/// <summary>
		/// <see cref="Guid"/> of a current version of a <see cref="Post"/>.
		/// </summary>
		[BsonRepresentation(BsonType.String)]
		public Guid PostId { get; set; }
		/// <summary>
		/// <see cref="DateTime"/> of a publication of this version.
		/// </summary>
		[BsonRepresentation(BsonType.DateTime)]
		public DateTime PublishTime { get; set; }
		/// <summary>
		/// HTML of this version post.
		/// </summary>
		public string Html { get; set; }
		/// <summary>
		/// For who this version of a <see cref="Post"/> was dedicated.
		/// </summary>
		public string ForWho { get; set; }
		/// <summary>
		/// Title of this version of a post.
		/// </summary>
		public string Title { get; set; }
		
		/// <summary>
		/// Creates empty instance of an <see cref="OldVersionOfPost"/>.
		/// </summary>
		public OldVersionOfPost () { }

		/// <summary>
		/// Creates instance of a <see cref="OldVersionOfPost"/> based on given <see cref="Post"/>.
		/// </summary>
		/// <param name="post">Which post is a base for creating instance.</param>
		public OldVersionOfPost(Post post) {
			PostId = post.Id;
			PublishTime = post.PublishTime;
			Html = post.Html;
			ForWho = post.ForWho;
			Title = post.Title;
		}

		public bool AreValuesCorrect () {
			return !(PostId == Guid.Empty || string.IsNullOrEmpty (ForWho));
		}

	}
}
