﻿using System;
using System.Collections.Generic;
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
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        /// <summary>
        /// Unique string, which represents given person, who made declaration.
        /// </summary>
        [BsonRequired]
        public string Person { get; set; }

        /// <summary>
        /// Guid of a Task, which completion has been declared.
        /// </summary>
        [BsonRequired]
        [BsonRepresentation(BsonType.String)]
        public Guid Task { get; set; }

        /// <summary>
        /// Does a person declares, that the completed their part of a <see cref="Task"/>.
        /// </summary>
        public bool IsCompleted { get; set; }

        public bool AreValuesCorrect() {
            if (string.IsNullOrWhiteSpace(Person) || Task == Guid.Empty)
                return false;
            return true;
        }
        
    }

}