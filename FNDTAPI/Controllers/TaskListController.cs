using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FNDTAPI.DataModels.Calendar;
using FNDTAPI.DataModels.TaskLists;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace FNDTAPI.Controllers {

	[Route ("api/v1.0/[controller]")]
	[ApiController]
	public class TaskListController : ControllerBase {

		[HttpPost]
		[Route ("tasklists")]
		public async Task<IActionResult> AddTaskListAsync (TaskList taskList, [FromServices] IMongoCollection<TaskList> mongoCollection) {
			if (taskList == null || !taskList.AreValuesCorrect ())
				return new JsonResult (new { Type = "Error", Details = "TaskList is null or it's properties are empty!" });
			taskList.ID = Guid.NewGuid ();
			await mongoCollection.InsertOneAsync (taskList);
			return new JsonResult (new { Type = "Success", Details = taskList.ID });
		}

		[HttpDelete]
		[Route ("tasklists")]
		public async Task<IActionResult> DeleteTaskListAsync (Dictionary<string, string> data, [FromServices] IMongoCollection<TaskList> mongoCollection,
																[FromServices] IMongoCollection<DataModels.TaskLists.Task> taskCollection, [FromServices] IMongoCollection<PersonTaskCompletionDeclaration> declarationCollection, [FromServices] IMongoCollection<CalendarEvent> eventsMongoCollection) {
			Guid taskListID = Guid.Parse (data["taskListID"]);
			string owner = data["owner"];
			TaskList temp = await (await mongoCollection.FindAsync (x => x.ID == taskListID)).FirstOrDefaultAsync ();
			if (temp == null)
				return new JsonResult (new { Type = "Error", Details = "There's no TaskList with given ID!" });
			if (temp.Owner != owner)
				return new JsonResult (new { Type = "Error", Details = "You have not permission." });
			DeleteResult result = await mongoCollection.DeleteOneAsync (x => x.ID == taskListID);
			DeleteResult result2 = await taskCollection.DeleteManyAsync (x => x.OwnerID == taskListID);
			List<DataModels.TaskLists.Task> list = await (await taskCollection.FindAsync (x => x.OwnerID == taskListID)).ToListAsync ();
			bool removalOfDeclarations = true;
			foreach (Guid item in list.Select (x => x.ID)) {
				DeleteResult result3 = await declarationCollection.DeleteManyAsync (x => x.Task == item);
				removalOfDeclarations = removalOfDeclarations && result3.IsAcknowledged;
				if (!removalOfDeclarations) break;
			}
			CalendarEvent calendarEvent = await eventsMongoCollection.FirstOrDefaultAsync (x => x.TaskListID == taskListID);
			if (calendarEvent != null) {
				CalendarEvent newValue = calendarEvent;
				newValue.TaskListID = Guid.Empty;
				await eventsMongoCollection.UpdateOneAsync (x => x.ID == calendarEvent.ID, Extensions.GenerateUpdateDefinition (calendarEvent, newValue));
			}
			if (result.IsAcknowledged && result2.IsAcknowledged && removalOfDeclarations)
				return new JsonResult (new { Type = "Success", Details = "" });
			else return new JsonResult (new { Type = "Error", Details = "Something failed. Most likely some mentions of given TaskList were not removed." }); ;
		}


		[HttpGet]
		[Route ("tasklists")]
		public async Task<IActionResult> GetTaskListsAsync (string owner, [FromServices] IMongoCollection<TaskList> mongoCollection, [FromServices] IMongoCollection<ParticipationRegistration> registrationMongoCollection, [FromServices] IMongoCollection<CalendarEvent> eventsMongoCollection) {
			IAsyncCursor<TaskList> cursor = await mongoCollection.FindAsync (x => x.Owner == owner);
			List<TaskList> output = await cursor.ToListAsync ();
			List<ParticipationRegistration> registrations = await (await registrationMongoCollection.FindAsync (x => x.User == owner)).ToListAsync ();
			foreach (ParticipationRegistration item in registrations) {
				CalendarEvent calendarEvent = await eventsMongoCollection.FirstOrDefaultAsync (x => x.ID == item.CalendarEventID);
				if (calendarEvent == null) {
					await registrationMongoCollection.DeleteManyAsync (x => x.CalendarEventID == item.CalendarEventID);
					continue;
				}
				if (calendarEvent.TaskListID == Guid.Empty) continue;
				output.Add (await mongoCollection.FirstOrDefaultAsync (x => x.ID == calendarEvent.TaskListID));
			}
			output.RemoveAll (x => x == null);
			return new JsonResult (new { Type = "Success", Details = new HashSet<TaskList> (output) });
		}

		[HttpGet]
		[Route ("tasklists")]
		public async Task<IActionResult> GetEventsTaskListAsync (Guid eventID, [FromServices] IMongoCollection<TaskList> mongoCollection, [FromServices] IMongoCollection<CalendarEvent> eventsMongoCollection) {
			if (eventID == Guid.Empty)
				return new JsonResult (new { Type = "Error", Details = "Guid is empty!" });
			CalendarEvent calendarEvent = await eventsMongoCollection.FirstOrDefaultAsync (x => x.ID == eventID);
			return new JsonResult (new {
				Type = "Success",
				Details = await mongoCollection.FirstOrDefaultAsync (x => x.ID == calendarEvent.TaskListID)
			});
		}

		[HttpPost]
		[Route ("tasklists/event")]
		public async Task<IActionResult> AddTaskListToEventAsync (Dictionary<string, string> data, [FromServices] IMongoCollection<TaskList> mongoCollection, [FromServices] IMongoCollection<CalendarEvent> eventsMongoCollection) {
			Guid eventID = Guid.Parse (data["eventID"]);
			TaskList value = JsonConvert.DeserializeObject<TaskList> (data["taskList"]);
			if (eventID == Guid.Empty)
				return new JsonResult (new { Type = "Error", Details = "EventID is empty!" });
			if (value == null || !value.AreValuesCorrect ())
				return new JsonResult (new { Type = "Error", Details = "Sended taskList is null or has empty properties!" });
			CalendarEvent calendarEvent = await eventsMongoCollection.FirstOrDefaultAsync (x => x.ID == eventID);
			if (calendarEvent == null)
				return new JsonResult (new { Type = "Error", Details = "There's no Calendar Event with given eventID!" });
			if (calendarEvent.TaskListID != Guid.Empty)
				return new JsonResult (new { Type = "Error", Details = "Given Event already has the TaskList!" });
			value.ID = Guid.NewGuid ();
			await mongoCollection.InsertOneAsync (value);
			CalendarEvent newValue = calendarEvent;
			newValue.TaskListID = value.ID;
			await eventsMongoCollection.UpdateOneAsync (x => x.ID == eventID, Extensions.GenerateUpdateDefinition (calendarEvent, newValue));
			return new JsonResult (new { Type = "Success", Details = value.ID });
		}

		[HttpPatch]
		[Route ("tasklists")]
		public async Task<IActionResult> UpdateTaskListAsync (TaskList taskList, [FromServices] IMongoCollection<TaskList> mongoCollection) {
			if (taskList == null || !taskList.AreValuesCorrect ())
				return new JsonResult (new { Type = "Error", Details = "TaskList is null or it's properties are empty!" });

			TaskList currentValue = await mongoCollection.FirstOrDefaultAsync (x => x.ID == taskList.ID);
			if (currentValue == null)
				return new JsonResult (new { Type = "Error", Details = "Sended TaskList to update has altered ID! Unable to update value!" });

			UpdateResult result = await mongoCollection.UpdateOneAsync (x => x.ID == taskList.ID, Extensions.GenerateUpdateDefinition (currentValue, taskList));

			if (result.IsAcknowledged) return new JsonResult (new { Type = "Success", Details = "" });
			else return new JsonResult (new { Type = "Error", Details = "Update somehow failed!" });
		}

		[HttpPost]
		[Route ("tasks")]
		public async Task<IActionResult> AddTaskAsync (DataModels.TaskLists.Task task, [FromServices] IMongoCollection<DataModels.TaskLists.Task> mongoCollection) {
			if (task == null || !task.AreValuesCorrect ())
				return new JsonResult (new { Type = "Error", Details = "Task is null or it's properties are empty!" });
			task.ID = Guid.NewGuid ();
			await mongoCollection.InsertOneAsync (task);
			return new JsonResult (new { Type = "Success", Details = task.ID });
		}

		[HttpDelete]
		[Route ("tasks")]
		public async Task<IActionResult> DeleteTaskAsync (Dictionary<string, string> data, [FromServices] IMongoCollection<DataModels.TaskLists.Task> mongoCollection, [FromServices] IMongoCollection<TaskList> taskListMongoCollection, [FromServices] IMongoCollection<PersonTaskCompletionDeclaration> declarationsMongoCollection) {
			Guid taskID = Guid.Parse (data["taskID"]);
			string owner = data["owner"];
			DataModels.TaskLists.Task task = await mongoCollection.FirstOrDefaultAsync (x => x.ID == taskID);
			if (task == null) return new JsonResult (new { Type = "Error", Details = "There's no such Task!" });
			TaskList taskList = await taskListMongoCollection.FirstOrDefaultAsync (x => x.ID == task.ID);
			if (taskList == null || (!taskList.Owner.Contains (owner) && !owner.Contains (taskList.Owner)))
				return new JsonResult (new { Type = "Error", Details = "You don't have access to that Task!" });

			DeleteResult result = await mongoCollection.DeleteOneAsync (x => x.ID == taskID);
			DeleteResult result2 = await declarationsMongoCollection.DeleteManyAsync (x => x.Task == taskID);
			if (result.IsAcknowledged && result2.IsAcknowledged) return new JsonResult (new { Type = "Success", Details = "" });
			else return new JsonResult (new { Type = "Error", Details = "Deletion failed for some reason!" });
		}

		[HttpPatch]
		[Route ("tasks")]
		public async Task<IActionResult> UpdateTaskAsync (DataModels.TaskLists.Task task, [FromServices] IMongoCollection<DataModels.TaskLists.Task> mongoCollection) {
			if (task == null || !task.AreValuesCorrect ())
				return new JsonResult (new { Type = "Error", Details = "Task is null or it's properties are empty!" });

			DataModels.TaskLists.Task currentValue = await mongoCollection.FirstOrDefaultAsync (x => x.ID == task.ID);

			if (currentValue == null) return new JsonResult (new { Type = "Error", Details = "Task's ID has been altered or such Task never existed! Update failed!" });

			UpdateResult result = await mongoCollection.UpdateOneAsync (x => x.ID == task.ID, Extensions.GenerateUpdateDefinition (currentValue, task));

			if (result.IsAcknowledged) return new JsonResult (new { Type = "Success", Details = "" });
			else return new JsonResult (new { Type = "Error", Details = "Update somehow failed!" });
		}

		[HttpGet]
		[Route ("tasks")]
		public async Task<IActionResult> GetTasksAsync (Guid taskListID, [FromServices] IMongoCollection<DataModels.TaskLists.Task> mongoCollection) {
			IAsyncCursor<DataModels.TaskLists.Task> cursor = await mongoCollection.FindAsync (x => x.OwnerID == taskListID);
			return new JsonResult (new { Type = "Success", Details = await cursor.ToListAsync () });
		}

		[HttpPost]
		[Route ("declarations")]
		public async Task<IActionResult> AddTaskCompletionDeclarationAsync (PersonTaskCompletionDeclaration declaration, [FromServices] IMongoCollection<PersonTaskCompletionDeclaration> mongoCollection, [FromServices] IMongoCollection<DataModels.TaskLists.Task> tasksMongoCollection) {
			if (declaration == null || !declaration.AreValuesCorrect ())
				return new JsonResult (new { Type = "Error", Details = "PersonTaskCompletionDeclaration is null or it's properties are empty!" });
			DataModels.TaskLists.Task task = await tasksMongoCollection.FirstOrDefaultAsync (x => x.ID == declaration.Task);
			if (task == null) return new JsonResult (new { Type = "Error", Details = "PersonTaskCompletionDeclaration has been created for wrong Task!" });
			long howManyDeclarationForGiveTask = await mongoCollection.CountDocumentsAsync (x => x.Task == task.ID);
			if(howManyDeclarationForGiveTask >= task.MaximumCountOfPeopleWhoCanDoIt)
				return new JsonResult (new { Type = "Warming", Details = "You cannot complete that task, because there's maximum amount of people declared." });
			declaration.ID = Guid.NewGuid ();
			await mongoCollection.InsertOneAsync (declaration);
			return new JsonResult (new { Type = "Success", Details = declaration.ID });
		}

		[HttpDelete]
		[Route ("declarations")]
		public async Task<IActionResult> DeleteDeclarationAsync (Dictionary<string, string> data, [FromServices] IMongoCollection<PersonTaskCompletionDeclaration> mongoCollection) {
			Guid taskID = Guid.Parse (data["taskID"]);
			string person = data["owner"];
			DeleteResult result = await mongoCollection.DeleteOneAsync (x => x.Person == person && x.Task == taskID);
			if (result.IsAcknowledged) return Ok ();
			else return NotFound ();
		}

		[HttpGet]
		[Route ("declarations")]
		public async Task<IActionResult> GetDeclarations (Guid taskID, [FromServices] IMongoCollection<PersonTaskCompletionDeclaration> mongoCollection) {
			IAsyncCursor<PersonTaskCompletionDeclaration> cursor = await mongoCollection.FindAsync (x => x.Task == taskID);
			return new JsonResult (await cursor.ToListAsync ());
		}

	}
}