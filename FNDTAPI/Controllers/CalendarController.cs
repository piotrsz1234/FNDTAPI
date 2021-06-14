using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FDNTAPI.DataModels.Calendar;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FDNTAPI.Controllers {

    [Route("api/v1.0/[controller]")]
    [ApiController]
    public class CalendarController : ControllerBase {

        [HttpPost]
        [Route("events")]
        public async Task<IActionResult> AddCalendarEventAsync(CalendarEvent calendarEvent,
            [FromServices] IMongoCollection<CalendarEvent> mongoCollection) {
            if (calendarEvent == null || !calendarEvent.AreValuesCorrect())
                return this.Error(HttpStatusCode.UnprocessableEntity,
                    "CalendarEvent is null or it's properties are empty!");
            calendarEvent.Id = Guid.NewGuid();
            await mongoCollection.InsertOneAsync(calendarEvent);
            return this.Success(calendarEvent.Id);
        }

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
            var result = await (await mongoCollection.FindAsync(x => x.Id == id)).FirstOrDefaultAsync();
            if (result == null)
                return this.Error(HttpStatusCode.NotFound, "There's no such category!");
            return this.Success(result);
        }

        [HttpPost]
        [Route("categories")]
        public async Task<IActionResult> AddCategoryAsync(CalendarEventCategory category,
            [FromServices] IMongoCollection<CalendarEventCategory> mongoCollection) {
            if (category == null || !category.AreValuesCorrect())
                return this.Error(HttpStatusCode.UnprocessableEntity,
                    "CalendarEventCategory is null or it's properties are empty!");
            category.Id = Guid.NewGuid();
            await mongoCollection.InsertOneAsync(category);
            return this.Success(category.Id);
        }

        [HttpPatch]
        [Route("categories")]
        public async Task<IActionResult> UpdateCategoryAsync(CalendarEventCategory category,
            [FromServices] IMongoCollection<CalendarEventCategory> mongoCollection) {
            if (category == null || !category.AreValuesCorrect())
                return this.Error(HttpStatusCode.UnprocessableEntity,
                    "CalendarEventCategory is null or it's properties are empty!");
            CalendarEventCategory currentValue = await mongoCollection.FirstOrDefaultAsync(x => x.Id == category.Id);
            if (currentValue == null)
                return this.Error(HttpStatusCode.UnprocessableEntity,
                    "Sended CalendarEventCategory to update has altered Id! Unable to update value!");
            UpdateResult result = await mongoCollection.UpdateOneAsync(x => x.Id == category.Id,
                Extensions.GenerateUpdateDefinition<CalendarEventCategory>(currentValue, category));
            if (result.IsAcknowledged) return this.Success("");
            else return this.Error(HttpStatusCode.InternalServerError, "Value wasn't updated!");
        }

        [HttpPost]
        [Route("participations")]
        public async Task<IActionResult> RegisterParticiationDeclarationAsync(ParticipationRegistration registration,
            [FromServices] IMongoCollection<ParticipationRegistration> mongoCollection) {
            if (registration == null || !registration.AreValuesCorrect())
                return this.Error(HttpStatusCode.UnprocessableEntity, "Registration data was not valid!");
            registration.Id = Guid.NewGuid();
            registration.HasParticipantConfirmed = true;
            registration.HasOwnerConfirmed = false;
            long result = await mongoCollection.CountDocumentsAsync(x =>
                x.CalendarEventId == registration.CalendarEventId && x.User == registration.User &&
                x.HasParticipantConfirmed);
            if (result > 0) return this.Error(HttpStatusCode.BadRequest, "Such declaration already exists!");
            await mongoCollection.InsertOneAsync(registration);
            return this.Success(registration.Id);
        }

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
                await mongoCollection.DeleteOneAsync(x =>
                    x.CalendarEventId == calendarEventId && (x.User == owner || isRemovingTheEventOwner));
            if (result.IsAcknowledged)
                return this.Ok();
            else return this.Error(HttpStatusCode.NotFound, "There's no such ParticipationRegistration!");
        }

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
            Guid participationId = participation.Id;
            if (participationId == Guid.Empty)
                return this.Error(HttpStatusCode.UnprocessableEntity, "Sent Guid is empty!");
            var currentValue = await mongoCollection.FirstOrDefaultAsync(x => x.Id == participationId);
            if (currentValue == null)
                return this.Error(HttpStatusCode.NotFound, "There's no such ParticipationRegistration");
            var newValue = currentValue;
            newValue.HasOwnerConfirmed = newValue.HasParticipantConfirmed = true;
            var result = await mongoCollection.UpdateOneAsync(x => x.Id == participationId,
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
                await (await mongoCollection.FindAsync(x => x.Id == registrationID)).FirstOrDefaultAsync();
            if (currentValue == null)
                return this.Error(HttpStatusCode.NotFound, "There's no such invitation!");
            ParticipationRegistration newValue = Extensions.Copy(currentValue);
            newValue.HasParticipantConfirmed = true;
            UpdateResult result = await mongoCollection.UpdateOneAsync(x => x.Id == registrationID,
                Extensions.GenerateUpdateDefinition(currentValue, newValue));
            if (result.IsAcknowledged)
                return this.Success("");
            else return this.Error(HttpStatusCode.InternalServerError, "Failed to accept invitation!");
        }

        public async Task<IActionResult> InviteAsync(Dictionary<string, string> data,
            [FromServices] IMongoCollection<ParticipationRegistration> mongoCollection) {
            if (data == null || !data.ContainsKey("Who") || !data.ContainsKey("EventID") || !data.ContainsKey("Sender"))
                return this.Error(HttpStatusCode.UnprocessableEntity,
                    "Json sent is null or doesn't contains: Who : string and EventID : Guid and Sender : string!");
            string who = data["Who"];
            Guid eventID;
            if (!Guid.TryParse(data["EventID"], out eventID))
                return this.Error(HttpStatusCode.UnprocessableEntity, "Couldn't parse EventID to Guid!");
            ParticipationRegistration temp = new ParticipationRegistration() {
                Id = Guid.NewGuid(),
                CalendarEventId = eventID,
                HasOwnerConfirmed = true,
                HasParticipantConfirmed = false,
                User = who
            };
            await mongoCollection.InsertOneAsync(temp);
            return this.Success(temp.Id);
        }

        [HttpPatch]
        [Route("participation")]
        public async Task<IActionResult> AcceptRegistration(Guid registrationID,
            [FromServices] IMongoCollection<ParticipationRegistration> mongoCollection) {
            ParticipationRegistration currentValue =
                await (await mongoCollection.FindAsync(x => x.Id == registrationID)).FirstOrDefaultAsync();
            if (currentValue == null)
                return this.Error(HttpStatusCode.NotFound, "There's no such registration!");
            ParticipationRegistration newValue = Extensions.Copy(currentValue);
            newValue.HasOwnerConfirmed = true;
            UpdateResult result = await mongoCollection.UpdateOneAsync(x => x.Id == registrationID,
                Extensions.GenerateUpdateDefinition(currentValue, newValue));
            if (result.IsAcknowledged)
                return Ok();
            return this.Error(HttpStatusCode.InternalServerError, "Failed to accept registration!");
        }

    }

}