using System;
using FDNTAPI.DataModels.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FDNTAPI.DataModels.TaskLists {

    /// <summary>
    /// Class, which represents single Task in TaskList.
    /// </summary>
    public class Task : IDataModel {

        /// <summary>
        /// Id of a Task.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid ID { get; set; }

        /// <summary>
        /// Title of the Task
        /// </summary>
        [BsonRequired]
        public string Title { get; set; }

        /// <summary>
        /// Description of the Task.
        /// </summary>
        [BsonRequired]
        public string Text { get; set; }

        /// <summary>
        /// Guid of a TaskList to which Task has been added.
        /// </summary>
        [BsonRequired]
        [BsonRepresentation(BsonType.String)]
        public Guid OwnerId { get; set; }

        /// <summary>
        /// Defines how many people can declare completion of the task.
        /// </summary>
        [BsonRequired]
        [BsonDefaultValue(1)]
        public int MaximumCountOfPeopleWhoCanDoIt { get; set; }

        public bool AreValuesCorrect() {
            return !(string.IsNullOrWhiteSpace(Text) || OwnerId == Guid.Empty || string.IsNullOrWhiteSpace(Title));
        }

    }

}