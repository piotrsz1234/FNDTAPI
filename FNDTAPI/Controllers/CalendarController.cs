﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FDNTAPI.DataModels.Calendar;
using FDNTAPI.DataModels.Notifications;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FDNTAPI.Controllers {

    /// <summary>
    /// Controller, which contains actions for Calendar Event and Participation Registration.
    /// </summary>
    [Route("api/v1.0/[controller]")]
    [ApiController]
    public class CalendarController : ControllerBase {

        /// <summary>
        /// [HTTP POST] Adds <see cref="CalendarEvent"/> to database.
        /// </summary>
        /// <param name="calendarEvent">Calendar Event to add.</param>
        /// <param name="mongoCollection">Mongo collection, where value will be added. Dependency Injection handles that value.</param>
        /// <returns>As a result returns Json with Guid of added calendar event.</returns>
        [HttpPost]
        [Route("events")]
        public async Task<IActionResult> AddCalendarEventAsync(CalendarEvent calendarEvent,
            [FromServices] IMongoCollection<CalendarEvent> mongoCollection,
            [FromServices] IMongoCollection<Notification> notificationsCollection) {
            if (calendarEvent == null || !calendarEvent.AreValuesCorrect())
                return this.Error(HttpStatusCode.UnprocessableEntity,
                    "CalendarEvent is null or it's properties are empty!");
            calendarEvent.Id = Guid.NewGuid();
            await mongoCollection.InsertOneAsync(calendarEvent);
            await this.AddNotificationAsync(
                new NewEventNotification(Guid.NewGuid(), calendarEvent.ForWho, calendarEvent.CreatorEmail,
                    calendarEvent.Id), notificationsCollection);
            return this.Success(calendarEvent.Id);
        }

        /// <summary>
        /// [HTTP DELETE] Deletes <see cref="CalendarEvent"/> from database.
        /// </summary>
        /// <param name="data">Generated from body of a HTTP Request.</param>
        /// <param name="mongoCollection">Provided by Dependency Injection.</param>
        /// <param name="particiationCollection">Provided by Dependency Injection.</param>
        /// <returns>Returns 200OK, when succeeded, 403Forbidden, if person who didn't created object tries to delete it, 404NotFound if there's no event with given Id.</returns>
        [HttpDelete]
        [Route("events")]
        public async Task<IActionResult> DeleteCalendarEventAsync(Dictionary<string, string> data,
            [FromServices] IMongoCollection<CalendarEvent> mongoCollection,
            [FromServices] IMongoCollection<ParticipationRegistration> particiationCollection) {
            Guid calendarEventId = Guid.Parse(data["calendarEventID"]);
            string email = data["owner"];
            CalendarEvent temp =
                await (await mongoCollection.FindAsync(x => x.Id == calendarEventId)).FirstOrDefaultAsync();
            if (temp == null)
                return this.Error(HttpStatusCode.NotFound, "There's no such CalendarEvent!");
            if (temp.CreatorEmail != email)
                return this.Error(HttpStatusCode.Forbidden, "You don't have access to that CalendarEvent!");
            DeleteResult result = await mongoCollection.DeleteOneAsync(x => x.Id == calendarEventId);
            DeleteResult result2 =
                await particiationCollection.DeleteManyAsync(x => x.CalendarEventId == calendarEventId);
            if (result.IsAcknowledged && result2.IsAcknowledged)
                return Ok();
            else return this.Error(HttpStatusCode.InternalServerError, "Failed to remove object or it's mentions!");
        }

        /// <summary>
        /// [HTTP GET] Returns <see cref="JsonResult"/> with List of <see cref="CalendarEvent"/>, where each element meets given conditions.
        /// </summary>
        /// <param name="groups">Group to which user belongs. If there's more, separate them with symbol of new line.</param>
        /// <param name="email">Email of a user.</param>
        /// <param name="mongoCollection">Provided by Dependency Injection.</param>
        /// <returns><see cref="JsonResult"/> with List of <see cref="CalendarEvent"/></returns>
        [HttpGet]
        [Route("events")]
        public async Task<IActionResult> GetCalendarEventsAsync(string groups, string email,
            [FromServices] IMongoCollection<CalendarEvent> mongoCollection) {
            List<CalendarEvent> result = new List<CalendarEvent>();
            using (var cursor = await mongoCollection.FindAsync(x => true)) {
                do {
                    if (cursor.Current == null) continue;
                    result.AddRange(cursor.Current.Where(x =>
                        x.CreatorEmail == email || x.ForWho.Contains(email) ||
                        x.ForWho.Split('\n').Any(y => groups.Contains(y))));
                } while (await cursor.MoveNextAsync());
            }

            return this.Success(result);
        }

        /// <summary>
        /// [HTTP PATCH] Updates given <see cref="CalendarEvent"/>.
        /// </summary>
        /// <param name="calendarEvent">New value, which should be stored in database. DO NOT change value of a Id!</param>
        /// <param name="mongoCollection">Provided by Dependency Injection.</param>
        /// <returns>Returns 200OK, when succeeded, 404NotFound, if there's no event with given Id, Json with error value if code somehow fails.</returns>
        [HttpPatch]
        [Route("events")]
        public async Task<IActionResult> UpdateCalendarEventAsync(CalendarEvent calendarEvent,
            [FromServices] IMongoCollection<CalendarEvent> mongoCollection) {
            if (calendarEvent == null || !calendarEvent.AreValuesCorrect())
                return this.Error(HttpStatusCode.UnprocessableEntity,
                    "CalendarEvent is null or it's properties are empty!");
            CalendarEvent currentValue = await mongoCollection.FirstOrDefaultAsync(x => x.Id == calendarEvent.Id);
            if (currentValue == null)
                return this.Error(HttpStatusCode.UnprocessableEntity,
                    "Sended CalendarEvent to update has altered Id! Unable to update value!");
            UpdateResult result = await mongoCollection.UpdateOneAsync(x => x.Id == calendarEvent.Id,
                Extensions.GenerateUpdateDefinition(currentValue, calendarEvent));
            if (result.IsAcknowledged) return this.Success("");
            else return this.Error(HttpStatusCode.InternalServerError, "Value wasn't updated!");
        }

        /// <summary>
        /// [HTTP GET] Returns List of <see cref="CalendarEventCategory"/>, which belongs to given groups or person.
        /// </summary>
        /// <param name="groups">Group, whose Categories you want to get.</param>
        /// <param name="email">Person's email, whose Categories you want to get.</param>
        /// <param name="mongoCollection">Provided by Dependency Injection.</param>
        /// <returns>Json with List of <see cref="CalendarEventCategory"/>, which met conditions.</returns>
        [HttpGet]
        [Route("categories")]
        public async Task<IActionResult> GetCategoriesAsync(string groups, string email,
            [FromServices] IMongoCollection<CalendarEventCategory> mongoCollection) {
            List<CalendarEventCategory> output = new List<CalendarEventCategory>();
            if (!groups.Contains('\n')) groups += '\n';
            using (var cursor = await mongoCollection.FindAsync(x => true)) {
                do {
                    if (cursor.Current == null) continue;
                    output.AddRange(cursor.Current.Where(x =>
                        (x.IsPersonal && x.Owner == (email)) || x.Owner.Split('\n').Any(y => groups.Contains(y))));
                } while (await cursor.MoveNextAsync());
            }

            return this.Success(output);
        }

        [HttpGet]
        [Route("category")]
        public async Task<IActionResult> GetCategoryAsync(string categoryId,
            [FromServices] IMongoCollection<CalendarEventCategory> mongoCollection) {
            Guid id = Guid.Parse(categoryId);
            if (id == Guid.Empty)
                return this.Error(HttpStatusCode.UnprocessableEntity, "Category Id cannot be empty!");
            var result = await (await mongoCollection.FindAsync(x => x.ID == id)).FirstOrDefaultAsync();
            if (result == null)
                return this.Error(HttpStatusCode.NotFound, "There's no such category!");
            return this.Success(result);
        }

        /// <summary>
        /// [HTTP POST] Adds <see cref="CalendarEventCategory"/> to database.
        /// </summary>
        /// <param name="category">Value to be added.</param>
        /// <param name="mongoCollection">Provided by Dependency Injection.</param>
        /// <returns>Json with Id of the added value or information about error.</returns>
        [HttpPost]
        [Route("categories")]
        public async Task<IActionResult> AddCategoryAsync(CalendarEventCategory category,
            [FromServices] IMongoCollection<CalendarEventCategory> mongoCollection) {
            if (category == null || !category.AreValuesCorrect())
                return this.Error(HttpStatusCode.UnprocessableEntity,
                    "CalendarEventCategory is null or it's properties are empty!");
            category.ID = Guid.NewGuid();
            await mongoCollection.InsertOneAsync(category);
            return this.Success(category.ID);
        }

        /// <summary>
        /// [HTTP PATCH] Updates <see cref="CalendarEventCategory"/> in database.
        /// </summary>
        /// <param name="category">New value, which should be stored in database. DO NOT change value of a Id!</param>
        /// <param name="mongoCollection">Provided by Dependency Injection.</param>
        /// <returns>Returns 200OK, when succeeded, 404NotFound, if there's no event with given Id, Json with error value if code somehow fails.</returns>
        [HttpPatch]
        [Route("categories")]
        public async Task<IActionResult> UpdateCategoryAsync(CalendarEventCategory category,
            [FromServices] IMongoCollection<CalendarEventCategory> mongoCollection) {
            if (category == null || !category.AreValuesCorrect())
                return this.Error(HttpStatusCode.UnprocessableEntity,
                    "CalendarEventCategory is null or it's properties are empty!");
            CalendarEventCategory currentValue = await mongoCollection.FirstOrDefaultAsync(x => x.ID == category.ID);
            if (currentValue == null)
                return this.Error(HttpStatusCode.UnprocessableEntity,
                    "Sended CalendarEventCategory to update has altered Id! Unable to update value!");
            UpdateResult result = await mongoCollection.UpdateOneAsync(x => x.ID == category.ID,
                Extensions.GenerateUpdateDefinition<CalendarEventCategory>(currentValue, category));
            if (result.IsAcknowledged) return this.Success("");
            else return this.Error(HttpStatusCode.InternalServerError, "Value wasn't updated!");
        }

        /// <summary>
        /// [HTTP POST] Adds <see cref="ParticipationRegistration"/> to database.
        /// </summary>
        /// <param name="registration">User's registration</param>
        /// <param name="mongoCollection">Provided by Dependency Injection</param>
        /// <returns>Returns Guid of added value.</returns>
        [HttpPost]
        [Route("participations")]
        public async Task<IActionResult> RegisterParticiationDeclarationAsync(ParticipationRegistration registration,
            [FromServices] IMongoCollection<ParticipationRegistration> mongoCollection) {
            if (registration == null || !registration.AreValuesCorrect())
                return this.Error(HttpStatusCode.UnprocessableEntity, "Registration data was not valid!");
            registration.ID = Guid.NewGuid();
            registration.HasParticipantConfirmed = true;
            registration.HasOwnerConfirmed = false; 
            long result = await mongoCollection.CountDocumentsAsync(x =>
                x.CalendarEventId == registration.CalendarEventId && x.User == registration.User &&
                x.HasParticipantConfirmed);
            if (result > 0) return this.Error(HttpStatusCode.BadRequest, "Such declaration already exists!");
            await mongoCollection.InsertOneAsync(registration);
            return this.Success(registration.ID);
        }

        /// <summary>
        /// [HTTP DELETE] Deletes from database given <see cref="ParticipationRegistration"/>.
        /// </summary>
        /// <param name="registration">Registration to deletion.</param>
        /// <param name="mongoCollection">Provided by Dependency Injection.</param>
        /// <returns>Returns 200OK if finds given registration, otherwise 404NotFound.</returns>
        [HttpDelete]
        [Route("participations")]
        public async Task<IActionResult> RemoveParticiationDeclarationAsync(Dictionary<string, string> data,
            [FromServices] IMongoCollection<ParticipationRegistration> mongoCollection,
            [FromServices] IMongoCollection<CalendarEvent> eventsCollection) {
            Guid calendarEventId = Guid.Parse(data["calendarEventID"]);
            string owner = data["owner"];
            var isRemovingTheEventOwner =
                (await eventsCollection.FindAsync(x => x.Id == calendarEventId && x.CreatorEmail == owner)) != null;
            DeleteResult result =
                await mongoCollection.DeleteOneAsync(x => x.CalendarEventId == calendarEventId && (x.User == owner || isRemovingTheEventOwner));
            if (result.IsAcknowledged)
                return this.Ok();
            else return this.Error(HttpStatusCode.NotFound, "There's no such ParticipationRegistration!");
        }

        /// <summary>
        /// [HTTP GET] Gets from database list of <see cref="ParticipationRegistration"/>s.
        /// </summary>
        /// <param name="eventID"><see cref="Guid"/> of the event</param>
        /// <param name="mongoCollection">Provided by Dependency Injection</param>
        /// <returns>JsonResult with list of registrations.</returns>
        [HttpGet]
        [Route("participations")]
        public async Task<IActionResult> GetParticiationDeclarationsAsync(Guid eventID,
            [FromServices] IMongoCollection<ParticipationRegistration> mongoCollection) {
            IAsyncCursor<ParticipationRegistration> cursor =
                await mongoCollection.FindAsync(x => x.CalendarEventId == eventID);
            return this.Success(await cursor.ToListAsync());
        }

        [HttpGet]
        [Route("participation")]
        public async Task<IActionResult> GetUsersParticipationDeclarationAsync(Guid eventId, string email,
            [FromServices] IMongoCollection<ParticipationRegistration> mongoCollection) {
            var result = await mongoCollection.FindAsync(x =>
                x.User == email && x.CalendarEventId == eventId && x.HasParticipantConfirmed);
            return this.Success(await result.FirstOrDefaultAsync());
        }

        [HttpPost]
        [Route("participation")]
        public async Task<IActionResult> ConfirmParticipationAsync(ParticipationRegistration participation,
            [FromServices] IMongoCollection<ParticipationRegistration> mongoCollection) {
            Guid participationId = participation.ID;
            if (participationId == Guid.Empty)
                return this.Error(HttpStatusCode.UnprocessableEntity, "Sent Guid is empty!");
            var currentValue = await mongoCollection.FirstOrDefaultAsync(x => x.ID == participationId);
            if (currentValue == null)
                return this.Error(HttpStatusCode.NotFound, "There's no such ParticipationRegistration");
            var newValue = currentValue;
            newValue.HasOwnerConfirmed = newValue.HasParticipantConfirmed = true;
            var result = await mongoCollection.UpdateOneAsync(x => x.ID == participationId,
                Extensions.GenerateUpdateDefinition(currentValue, newValue));
            if (result.IsAcknowledged)
                return this.Ok();
            else return this.Error(HttpStatusCode.InternalServerError, "Process somehow failed!");
        }
        
        [HttpGet]
        [Route("invitations")]
        public async Task<IActionResult> GetInvitationsAsync(string user,
            [FromServices] IMongoCollection<ParticipationRegistration> mongoCollection) {
            IAsyncCursor<ParticipationRegistration> result =
                await mongoCollection.FindAsync(
                    x => x.User == user && x.HasOwnerConfirmed && !x.HasParticipantConfirmed);
            if (!await result.AnyAsync()) return this.Success(new List<object>());
            return this.Success(await result.ToListAsync());
        }

        [HttpPatch]
        [Route("invitations")]
        public async Task<IActionResult> ConfirmInvitationAsync(Guid registrationID,
            [FromServices] IMongoCollection<ParticipationRegistration> mongoCollection) {
            ParticipationRegistration currentValue =
                await (await mongoCollection.FindAsync(x => x.ID == registrationID)).FirstOrDefaultAsync();
            if (currentValue == null)
                return this.Error(HttpStatusCode.NotFound, "There's no such invitation!");
            ParticipationRegistration newValue = Extensions.Copy(currentValue);
            newValue.HasParticipantConfirmed = true;
            UpdateResult result = await mongoCollection.UpdateOneAsync(x => x.ID == registrationID,
                Extensions.GenerateUpdateDefinition(currentValue, newValue));
            if (result.IsAcknowledged)
                return this.Success("");
            else return this.Error(HttpStatusCode.InternalServerError, "Failed to accept invitation!");
        }

        public async Task<IActionResult> InviteAsync(Dictionary<string, string> data,
            [FromServices] IMongoCollection<ParticipationRegistration> mongoCollection,
            [FromServices] IMongoCollection<Notification> notificationCollection) {
            if (data == null || !data.ContainsKey("Who") || !data.ContainsKey("EventID") || !data.ContainsKey("Sender"))
                return this.Error(HttpStatusCode.UnprocessableEntity,
                    "Json sent is null or doesn't contains: Who : string and EventID : Guid and Sender : string!");
            string who = data["Who"];
            Guid eventID;
            if (!Guid.TryParse(data["EventID"], out eventID))
                return this.Error(HttpStatusCode.UnprocessableEntity, "Couldn't parse EventID to Guid!");
            ParticipationRegistration temp = new ParticipationRegistration() {
                ID = Guid.NewGuid(),
                CalendarEventId = eventID,
                HasOwnerConfirmed = true,
                HasParticipantConfirmed = false,
                User = who
            };
            await mongoCollection.InsertOneAsync(temp);
            await this.AddNotificationAsync(new NewInvitationNotification(Guid.NewGuid(), who, data["sender"], temp.ID),
                notificationCollection);
            return this.Success(temp.ID);
        }

        [HttpPatch]
        [Route("participation")]
        public async Task<IActionResult> AcceptRegistration(Guid registrationID,
            [FromServices] IMongoCollection<ParticipationRegistration> mongoCollection,
            [FromServices] IMongoCollection<Notification> notificationCollection) {
            ParticipationRegistration currentValue =
                await (await mongoCollection.FindAsync(x => x.ID == registrationID)).FirstOrDefaultAsync();
            if (currentValue == null)
                return this.Error(HttpStatusCode.NotFound, "There's no such registration!");
            ParticipationRegistration newValue = Extensions.Copy(currentValue);
            newValue.HasOwnerConfirmed = true;
            UpdateResult result = await mongoCollection.UpdateOneAsync(x => x.ID == registrationID,
                Extensions.GenerateUpdateDefinition(currentValue, newValue));
            if (result.IsAcknowledged) {
                await this.AddNotificationAsync(
                    new ConfirmationNotification(Guid.NewGuid(), currentValue.User, "", currentValue.CalendarEventId,
                        currentValue.ID), notificationCollection);
                return Ok();
            }
            else return this.Error(HttpStatusCode.InternalServerError, "Failed to accept registration!");
        }

    }

}