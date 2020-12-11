using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FNDTAPI.DataModels.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace FNDTAPI.Controllers {

	[Route ("api/v1.0/[controller]")]
	[ApiController]
	public class NotificationsController : ControllerBase {

		[HttpGet]
		[Route ("")]
		public async Task<IActionResult> GetUnreadNotificationsAsync (string user, string group, [FromServices] IMongoCollection<Notification> mongoCollection, [FromServices] IMongoCollection<NotificationRead> readCollection) {
			var readNotifications = await (await readCollection.FindAsync (x => x.User == user)).ToListAsync ();
			var filter1 = Builders<Notification>.Filter.Regex (x => x.ForWho, group);
			var filter2 = Builders<Notification>.Filter.Regex (x => x.ForWho, user);
			var notifications = new HashSet<Notification> (await (await mongoCollection.FindAsync (filter1)).ToListAsync ());
			notifications.AddRange (await (await mongoCollection.FindAsync (filter2)).ToListAsync ());
			notifications.RemoveWhere (x => readNotifications.Any (y => y.ID == x.ID));
			return this.Success (notifications);
		}

		[HttpPost]
		[Route("")]
		public async Task<IActionResult> AddReadNotificationAsync (Dictionary<string, string> data, [FromServices] IMongoCollection<NotificationRead> mongoCollection) {
			if (data == null || !data.ContainsKey ("NotificationID") || !data.ContainsKey ("User"))
				return this.Error ("Json should contain Dictionary with <string, string> with Keys: NotificationID : Guid and User : string!");
			Guid notificationID;
			if (!Guid.TryParse (data["NotificationID"], out notificationID))
				return this.Error ("NotificationID wrong format. It should be Guid!");
			string user = data["User"];
			NotificationRead temp = new NotificationRead {
				ID = Guid.NewGuid (),
				NotificationID = notificationID,
				User = user
			};
			await mongoCollection.InsertOneAsync (temp);
			return this.Success (temp.ID);
		}

	}
}
