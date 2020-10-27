using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FNDTAPI.DataModels.Calendar;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace FNDTAPI.Controllers {

	/// <summary>
	/// Controller, which contains actions for Calendar Event and Participation Registration.
	/// </summary>
	[Route ("api/v1.0/[controller]")]
	[ApiController]
	public class CalendarController : ControllerBase {

		/// <summary>
		/// [HTTP POST] Adds <see cref="CalendarEvent"/> to database.
		/// </summary>
		/// <param name="calendarEvent">Calendar Event to add.</param>
		/// <param name="mongoCollection">Mongo collection, where value will be added. Dependency Injection handles that value.</param>
		/// <returns>As a result returns Json with Guid of added calendar event.</returns>
		[HttpPost]
		[Route ("events")]
		public async Task<IActionResult> AddCalendarEventAsync (CalendarEvent calendarEvent, [FromServices] IMongoCollection<CalendarEvent> mongoCollection) {
			calendarEvent.ID = Guid.NewGuid ();
			await mongoCollection.InsertOneAsync (calendarEvent);
			return new JsonResult (new { CalendarEventID = calendarEvent.ID });
		}

		/// <summary>
		/// [HTTP DELETE] Deletes <see cref="CalendarEvent"/> from database.
		/// </summary>
		/// <param name="data">Generated from body of a HTTP Request.</param>
		/// <param name="mongoCollection">Provided by Dependency Injection.</param>
		/// <param name="particiationCollection">Provided by Dependency Injection.</param>
		/// <returns>Returns 200OK, when succeeded, 403Forbidden, if person who didn't created object tries to delete it, 404NotFound if there's no event with given ID.</returns>
		[HttpDelete]
		[Route ("events")]
		public async Task<IActionResult> DeleteCalendarEventAsync (Dictionary<string, string> data, [FromServices] IMongoCollection<CalendarEvent> mongoCollection,
																[FromServices] IMongoCollection<ParticipationRegistration> particiationCollection) {
			Guid calendarEventID = Guid.Parse (data["calendarEventID"]);
			string email = data["email"];
			CalendarEvent temp = await (await mongoCollection.FindAsync (x => x.ID == calendarEventID)).FirstOrDefaultAsync ();
			if (temp == null) return NotFound ();
			if (temp.CreatorEmail != email) return Forbid ();
			DeleteResult result = await mongoCollection.DeleteOneAsync (x => x.ID == calendarEventID);
			DeleteResult result2 = await particiationCollection.DeleteManyAsync (x => x.CalendarEventID == calendarEventID);
			if (result.IsAcknowledged && result2.IsAcknowledged)
				return Ok ();
			else return NotFound ();
		}

		/// <summary>
		/// [HTTP GET] Returns <see cref="JsonResult"/> with List of <see cref="CalendarEvent"/>, where each element meets given conditions.
		/// </summary>
		/// <param name="month">Number of the month, from which you want to get Events.</param>
		/// <param name="year">Year from which you want to get Events.</param>
		/// <param name="groups">Group to which user belongs. If there's more, separate them with symbol of new line.</param>
		/// <param name="email">Email of a user.</param>
		/// <param name="mongoCollection">Provided by Dependency Injection.</param>
		/// <returns><see cref="JsonResult"/> with List of <see cref="CalendarEvent"/></returns>
		[HttpGet]
		[Route ("events")]
		public async Task<IActionResult> GetCalendarEventsAsync (int month, int year, string groups, string email, [FromServices] IMongoCollection<CalendarEvent> mongoCollection) {
			DateTime begining = DateTime.Parse ($"{year}-{Extensions.GenerateTwoDigitMonth (month)}-01T00:00:00 Z");
			DateTime ending = DateTime.Parse ($"{year}-{Extensions.GenerateTwoDigitMonth (month)}-31T23:59:59.99 Z");
			IAsyncCursor<CalendarEvent> cursor = await mongoCollection.FindAsync (x => x.WhenBegins >= begining && x.WhenEnds <= ending);
			List<CalendarEvent> temp = await cursor.ToListAsync ();
			return new JsonResult (temp.Where (x => groups.Contains (x.ForWho) || x.ForWho.Contains (email)));
		}

		/// <summary>
		/// [HTTP PATCH] Updates given <see cref="CalendarEvent"/>.
		/// </summary>
		/// <param name="calendarEvent">New value, which should be stored in database. DO NOT change value of a ID!</param>
		/// <param name="mongoCollection">Provided by Dependency Injection.</param>
		/// <returns>Returns 200OK, when succeeded, 404NotFound, if there's no event with given ID, Json with error value if code somehow fails.</returns>
		[HttpPatch]
		[Route ("events")]
		public async Task<IActionResult> UpdateCalendarEventAsync (CalendarEvent calendarEvent, [FromServices] IMongoCollection<CalendarEvent> mongoCollection) {
			CalendarEvent currentValue = await mongoCollection.FirstOrDefaultAsync (x => x.ID == calendarEvent.ID);
			if (currentValue == null) return NotFound ();
			UpdateResult result = await mongoCollection.UpdateOneAsync (x => x.ID == calendarEvent.ID, Extensions.GenerateUpdateDefinition<CalendarEvent> (currentValue, calendarEvent));
			if (result.IsAcknowledged) return Ok ();
			else return new JsonResult (new { Type = "Error", Details = "Value wasn't updated!" });
		}

		/// <summary>
		/// [HTTP GET] Returns List of <see cref="CalendarEventCategory"/>, which belongs to given group or person.
		/// </summary>
		/// <param name="group">Group, whose Categories you want to get.</param>
		/// <param name="email">Person's email, whose Categories you want to get.</param>
		/// <param name="mongoCollection">Provided by Dependency Injection.</param>
		/// <returns>Json with List of <see cref="CalendarEventCategory"/>, which met conditions.</returns>
		[HttpGet]
		[Route ("categories")]
		public async Task<IActionResult> GetCategoriesAsync (string group, string email, [FromServices] IMongoCollection<CalendarEventCategory> mongoCollection) {
			IAsyncCursor<CalendarEventCategory> cursor = await mongoCollection.FindAsync (x => x.Owner.Contains(group) || x.Owner == email);
			return new JsonResult (await cursor.ToListAsync ());
		}

		/// <summary>
		/// [HTTP POST] Adds <see cref="CalendarEventCategory"/> to database.
		/// </summary>
		/// <param name="category">Value to be added.</param>
		/// <param name="mongoCollection">Provided by Dependency Injection.</param>
		/// <returns>Json with ID of the added value.</returns>
		[HttpPost]
		[Route ("categories")]
		public async Task<IActionResult> AddCategoryAsync (CalendarEventCategory category, [FromServices] IMongoCollection<CalendarEventCategory> mongoCollection) {
			category.ID = Guid.NewGuid ();
			await mongoCollection.InsertOneAsync (category);
			return new JsonResult (new { CalendarEventCategoryID = category.ID });
		}

		/// <summary>
		/// [HTTP PATCH] Updates <see cref="CalendarEventCategory"/> in database.
		/// </summary>
		/// <param name="category">New value, which should be stored in database. DO NOT change value of a ID!</param>
		/// <param name="mongoCollection">Provided by Dependency Injection.</param>
		/// <returns>Returns 200OK, when succeeded, 404NotFound, if there's no event with given ID, Json with error value if code somehow fails.</returns>
		[HttpPatch]
		[Route ("categories")]
		public async Task<IActionResult> UpdateCategoryAsync (CalendarEventCategory category, [FromServices] IMongoCollection<CalendarEventCategory> mongoCollection) {
			CalendarEventCategory currentValue = await mongoCollection.FirstOrDefaultAsync (x => x.ID == category.ID);
			if (currentValue == null) return NotFound ();
			UpdateResult result = await mongoCollection.UpdateOneAsync (x => x.ID == category.ID, Extensions.GenerateUpdateDefinition<CalendarEventCategory> (currentValue, category));
			if (result.IsAcknowledged) return Ok ();
			else return new JsonResult (new { Type = "Error", Details = "Value wasn't updated!" });
		}

	}
}