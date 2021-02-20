using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FDNTAPI.DataModels.Calendar;
using FDNTAPI.DataModels.TaskLists;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace FDNTAPI.Controllers {

    [Route("api/v1.0/[controller]")]
    [ApiController]
    public class TaskListController : ControllerBase {

        [HttpPost]
        [Route("tasklists")]
        public async Task<IActionResult> AddTaskListAsync(TaskList taskList,
            [FromServices] IMongoCollection<TaskList> mongoCollection,
            [FromServices] IMongoCollection<CalendarEvent> eventCollection) {
            if (taskList == null || !taskList.AreValuesCorrect())
                return this.Error(HttpStatusCode.UnprocessableEntity, "TaskList is null or it's properties are empty!");
            taskList.ID = Guid.NewGuid();
            await mongoCollection.InsertOneAsync(taskList);
            return this.Success(taskList.ID);
        }

        [HttpDelete]
        [Route("tasklists")]
        public async Task<IActionResult> DeleteTaskListAsync(Dictionary<string, string> data,
            [FromServices] IMongoCollection<TaskList> mongoCollection,
            [FromServices] IMongoCollection<DataModels.TaskLists.Task> taskCollection,
            [FromServices] IMongoCollection<PersonTaskCompletionDeclaration> declarationCollection,
            [FromServices] IMongoCollection<CalendarEvent> eventsMongoCollection) {
            Guid taskListID = Guid.Parse(data["taskListID"]);
            string owner = data["owner"];
            TaskList temp = await (await mongoCollection.FindAsync(x => x.ID == taskListID)).FirstOrDefaultAsync();
            if (temp == null)
                return this.Error(HttpStatusCode.NotFound, "There's no TaskList with given Id!");
            if (temp.Owner != owner)
                return this.Error(HttpStatusCode.Forbidden, "You have not permission.");
            DeleteResult result = await mongoCollection.DeleteOneAsync(x => x.ID == taskListID);
            DeleteResult result2 = await taskCollection.DeleteManyAsync(x => x.OwnerId == taskListID);
            List<DataModels.TaskLists.Task> list =
                await (await taskCollection.FindAsync(x => x.OwnerId == taskListID)).ToListAsync();
            bool removalOfDeclarations = true;
            foreach (Guid item in list.Select(x => x.ID)) {
                DeleteResult result3 = await declarationCollection.DeleteManyAsync(x => x.Task == item);
                removalOfDeclarations = removalOfDeclarations && result3.IsAcknowledged;
                if (!removalOfDeclarations) break;
            }

            CalendarEvent calendarEvent =
                await eventsMongoCollection.FirstOrDefaultAsync(x => x.TaskListID == taskListID);
            if (calendarEvent != null) {
                CalendarEvent newValue = calendarEvent;
                newValue.TaskListID = Guid.Empty;
                await eventsMongoCollection.UpdateOneAsync(x => x.Id == calendarEvent.Id,
                    Extensions.GenerateUpdateDefinition(calendarEvent, newValue));
            }

            if (result.IsAcknowledged && result2.IsAcknowledged && removalOfDeclarations)
                return this.Success("");
            else
                return this.Error(HttpStatusCode.InternalServerError,
                    "Something failed. Most likely some mentions of given TaskList were not removed.");
            ;
        }


        [HttpGet]
        [Route("tasklists")]
        public async Task<IActionResult> GetTaskListsAsync(string owner,
            [FromServices] IMongoCollection<TaskList> mongoCollection,
            [FromServices] IMongoCollection<ParticipationRegistration> registrationMongoCollection,
            [FromServices] IMongoCollection<CalendarEvent> eventsMongoCollection) {
            IAsyncCursor<TaskList> cursor = await mongoCollection.FindAsync(x => x.Owner == owner);
            List<TaskList> output = await cursor.ToListAsync();
            List<ParticipationRegistration> registrations =
                await (await registrationMongoCollection.FindAsync(x => x.User == owner)).ToListAsync();
            foreach (ParticipationRegistration item in registrations) {
                CalendarEvent calendarEvent =
                    await eventsMongoCollection.FirstOrDefaultAsync(x => x.Id == item.CalendarEventId);
                if (calendarEvent == null) {
                    await registrationMongoCollection.DeleteManyAsync(x => x.CalendarEventId == item.CalendarEventId);
                    continue;
                }

                if (calendarEvent.TaskListID == Guid.Empty) continue;
                output.Add(await mongoCollection.FirstOrDefaultAsync(x => x.ID == calendarEvent.TaskListID));
            }

            output.RemoveAll(x => x == null);
            return this.Success(new HashSet<TaskList>(output));
        }

        [HttpGet]
        [Route("tasklist")]
        public async Task<IActionResult> GetEventsTaskListAsync(Guid eventID,
            [FromServices] IMongoCollection<TaskList> mongoCollection,
            [FromServices] IMongoCollection<CalendarEvent> eventsMongoCollection) {
            if (eventID == Guid.Empty)
                return this.Error(HttpStatusCode.UnprocessableEntity, "Guid is empty!");
            CalendarEvent calendarEvent = await eventsMongoCollection.FirstOrDefaultAsync(x => x.Id == eventID);
            return this.Success(await mongoCollection.FirstOrDefaultAsync(x => x.ID == calendarEvent.TaskListID));
        }

        [HttpPost]
        [Route("tasklists/event")]
        public async Task<IActionResult> AddTaskListToEventAsync(Dictionary<string, string> data,
            [FromServices] IMongoCollection<TaskList> mongoCollection,
            [FromServices] IMongoCollection<CalendarEvent> eventsMongoCollection) {
            Guid eventId = Guid.Parse(data["eventID"]);
            Guid value = JsonConvert.DeserializeObject<Guid>(data["taskListId"]);
            if (eventId == Guid.Empty)
                return this.Error(HttpStatusCode.UnprocessableEntity, "EventID is empty!");
            if (value == Guid.Empty)
                return this.Error(HttpStatusCode.UnprocessableEntity,
                    "Sent taskList id is empty!");
            CalendarEvent calendarEvent = await eventsMongoCollection.FirstOrDefaultAsync(x => x.Id == eventId);
            if (calendarEvent == null)
                return this.Error(HttpStatusCode.NotFound, "There's no Calendar Event with given eventID!");
            if (calendarEvent.TaskListID != Guid.Empty)
                return this.Error(HttpStatusCode.BadRequest, "Given Event already has the TaskList!");
            CalendarEvent newValue = calendarEvent;
            newValue.TaskListID = value;
            await eventsMongoCollection.UpdateOneAsync(x => x.Id == eventId,
                Extensions.GenerateUpdateDefinition(calendarEvent, newValue));
            return Ok();
        }

        [HttpPatch]
        [Route("tasklists")]
        public async Task<IActionResult> UpdateTaskListAsync(TaskList taskList,
            [FromServices] IMongoCollection<TaskList> mongoCollection) {
            if (taskList == null || !taskList.AreValuesCorrect())
                return this.Error(HttpStatusCode.UnprocessableEntity, "TaskList is null or it's properties are empty!");

            TaskList currentValue = await mongoCollection.FirstOrDefaultAsync(x => x.ID == taskList.ID);
            if (currentValue == null)
                return this.Error(HttpStatusCode.BadRequest,
                    "Sended TaskList to update has altered Id! Unable to update value!");

            UpdateResult result = await mongoCollection.UpdateOneAsync(x => x.ID == taskList.ID,
                Extensions.GenerateUpdateDefinition(currentValue, taskList));

            if (result.IsAcknowledged) return Ok();
            return this.Error(HttpStatusCode.InternalServerError, "Update somehow failed!");
        }

        [HttpPost]
        [Route("tasks")]
        public async Task<IActionResult> AddTaskAsync(DataModels.TaskLists.Task task,
            [FromServices] IMongoCollection<DataModels.TaskLists.Task> mongoCollection) {
            if (task == null || !task.AreValuesCorrect())
                return this.Error(HttpStatusCode.UnprocessableEntity, "Task is null or it's properties are empty!");
            task.ID = Guid.NewGuid();
            await mongoCollection.InsertOneAsync(task);
            return this.Success(task.ID);
        }

        [HttpDelete]
        [Route("tasks")]
        public async Task<IActionResult> DeleteTaskAsync(Dictionary<string, string> data,
            [FromServices] IMongoCollection<DataModels.TaskLists.Task> mongoCollection,
            [FromServices] IMongoCollection<TaskList> taskListMongoCollection,
            [FromServices] IMongoCollection<PersonTaskCompletionDeclaration> declarationsMongoCollection) {
            Guid taskID = Guid.Parse(data["taskId"]);
            string owner = data["owner"];
            DataModels.TaskLists.Task task = await mongoCollection.FirstOrDefaultAsync(x => x.ID == taskID);
            if (task == null) return this.Error(HttpStatusCode.NotFound, "There's no such Task!");
            TaskList taskList = await taskListMongoCollection.FirstOrDefaultAsync(x => x.ID == task.ID);
            if (taskList == null || (!taskList.Owner.Contains(owner) && !owner.Contains(taskList.Owner)))
                return this.Error(HttpStatusCode.Forbidden, "You don't have access to that Task!");

            DeleteResult result = await mongoCollection.DeleteOneAsync(x => x.ID == taskID);
            DeleteResult result2 = await declarationsMongoCollection.DeleteManyAsync(x => x.Task == taskID);
            if (result.IsAcknowledged && result2.IsAcknowledged) return this.Success("");
            else return this.Error(HttpStatusCode.InternalServerError, "Deletion failed for some reason!");
        }

        [HttpPatch]
        [Route("tasks")]
        public async Task<IActionResult> UpdateTaskAsync(DataModels.TaskLists.Task task,
            [FromServices] IMongoCollection<DataModels.TaskLists.Task> mongoCollection) {
            if (task == null || !task.AreValuesCorrect())
                return this.Error(HttpStatusCode.UnprocessableEntity, "Task is null or it's properties are empty!");

            DataModels.TaskLists.Task currentValue = await mongoCollection.FirstOrDefaultAsync(x => x.ID == task.ID);

            if (currentValue == null)
                return this.Error(HttpStatusCode.UnprocessableEntity,
                    "Task's Id has been altered or such Task never existed! Update failed!");

            UpdateResult result = await mongoCollection.UpdateOneAsync(x => x.ID == task.ID,
                Extensions.GenerateUpdateDefinition(currentValue, task));

            if (result.IsAcknowledged) return this.Success("");
            else return this.Error(HttpStatusCode.InternalServerError, "Update somehow failed!");
        }

        [HttpGet]
        [Route("tasks")]
        public async Task<IActionResult> GetTasksAsync(Guid taskListID,
            [FromServices] IMongoCollection<DataModels.TaskLists.Task> mongoCollection) {
            IAsyncCursor<DataModels.TaskLists.Task> cursor =
                await mongoCollection.FindAsync(x => x.OwnerId == taskListID);
            return this.Success(await cursor.ToListAsync());
        }

        [HttpPost]
        [Route("declarations")]
        public async Task<IActionResult> AddTaskCompletionDeclarationAsync(PersonTaskCompletionDeclaration declaration,
            [FromServices] IMongoCollection<PersonTaskCompletionDeclaration> mongoCollection,
            [FromServices] IMongoCollection<DataModels.TaskLists.Task> tasksMongoCollection) {
            if (declaration == null || !declaration.AreValuesCorrect())
                return this.Error(HttpStatusCode.UnprocessableEntity,
                    "PersonTaskCompletionDeclaration is null or it's properties are empty!");
            DataModels.TaskLists.Task task =
                await tasksMongoCollection.FirstOrDefaultAsync(x => x.ID == declaration.Task);
            if (task == null)
                return this.Error(HttpStatusCode.BadRequest,
                    "PersonTaskCompletionDeclaration has been created for wrong Task!");
            long howManyDeclarationForGiveTask = await mongoCollection.CountDocumentsAsync(x => x.Task == task.ID);
            if (howManyDeclarationForGiveTask >= task.MaximumCountOfPeopleWhoCanDoIt)
                return this.Error(HttpStatusCode.BadRequest,
                    "You cannot complete that task, because there's maximum amount of people declared.");
            declaration.ID = Guid.NewGuid();
            await mongoCollection.InsertOneAsync(declaration);
            return this.Success(declaration.ID);
        }

        [HttpDelete]
        [Route("declarations")]
        public async Task<IActionResult> DeleteDeclarationAsync(Dictionary<string, string> data,
            [FromServices] IMongoCollection<PersonTaskCompletionDeclaration> mongoCollection) {
            Guid taskID = Guid.Parse(data["taskId"]);
            string person = data["owner"];
            DeleteResult result = await mongoCollection.DeleteOneAsync(x => x.Person == person && x.Task == taskID);
            if (result.IsAcknowledged) return Ok();
            else
                return this.Error(HttpStatusCode.NotFound,
                    "There's no such declaration or for some unknown reason it's impossible to delete.");
        }

        [HttpGet]
        [Route("declarations")]
        public async Task<IActionResult> GetDeclarations(Guid taskId,
            [FromServices] IMongoCollection<PersonTaskCompletionDeclaration> mongoCollection) {
            IAsyncCursor<PersonTaskCompletionDeclaration> cursor =
                await mongoCollection.FindAsync(x => x.Task == taskId);
            return this.Success(await cursor.ToListAsync());
        }

        [HttpPatch]
        [Route("declarations")]
        public async Task<IActionResult> UpdateDeclarationAsync(PersonTaskCompletionDeclaration declaration,
            [FromServices] IMongoCollection<PersonTaskCompletionDeclaration> mongoCollection) {
            Guid declarationId = declaration.ID;
            if (declarationId == Guid.Empty)
                return this.Error(HttpStatusCode.UnprocessableEntity, "Sent Guid is empty!");
            var result = await mongoCollection.FirstOrDefaultAsync(x => x.ID == declarationId);
            if (result == null)
                return this.Error(HttpStatusCode.NotFound, "There's no such Declaration!");
            var newValue = result;
            newValue.IsCompleted = true;
            var output = await mongoCollection.UpdateOneAsync(x => x.ID == declarationId,
                Extensions.GenerateUpdateDefinition(result, newValue));
            return output.IsAcknowledged
                ? Ok()
                : this.Error(HttpStatusCode.InternalServerError, "Update process somehow failed!");
        }

    }

}