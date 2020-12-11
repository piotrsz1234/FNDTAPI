using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FNDTAPI.DataModels.Notifications;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

namespace FNDTAPI.Hubs {
	public class NotificationsHub : Hub {

		private readonly IMongoCollection<Notification> notificationCollection;

		public NotificationsHub (IMongoCollection<Notification> mongoCollection) {
			notificationCollection = mongoCollection;
		}

		public async Task Notification (Guid id) {
			var result = await (await notificationCollection.FindAsync (x => x.ID == id)).FirstOrDefaultAsync ();
			if (result != null)
				await Clients.All.SendAsync ("showNotification", result);
		}

	}
}
