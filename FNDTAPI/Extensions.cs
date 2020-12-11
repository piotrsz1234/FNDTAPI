using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using FNDTAPI.DataModels.Notifications;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace FNDTAPI {
	
	/// <summary>
	/// Static class with Extension Methods used in project and static methods, which helps a lot.
	/// </summary>
	public static class Extensions {

		/// <summary>
		/// Generic method, which generates <see cref="UpdateDefinition{T}"/>.
		/// </summary>
		/// <typeparam name="T">Type, for which will be generated UpdateDefinition.</typeparam>
		/// <param name="oldValue">Current value, which will be updated.</param>
		/// <param name="newValue">New value to which will be updated.</param>
		/// <returns>Generated <see cref="UpdateDefinition{TDocument}"/> for given parameters, which updates <paramref name="oldValue"/> to <paramref name="newValue"/></returns>
		public static UpdateDefinition<T> GenerateUpdateDefinition<T> (T oldValue, T newValue) {
			FieldInfo[] fields = typeof (T).GetFields ();
			PropertyInfo[] properties = typeof (T).GetProperties ();
			UpdateDefinition<T> output = null;
			foreach (var item in fields) {
				if (item.GetValue (oldValue) != item.GetValue (newValue))
					if (output == null) output = Builders<T>.Update.Set (item.Name, item.GetValue (newValue));
					else output = output.Set (item.Name, item.GetValue (newValue));
			}
			foreach (var item in properties) {
				if (item.GetValue (oldValue) != item.GetValue (newValue))
					if (output == null) output = Builders<T>.Update.Set (item.Name, item.GetValue (newValue));
					else output = output.Set (item.Name, item.GetValue (newValue));
			}
			return output;
		}

		/// <summary>
		/// Returns first element of <see cref="IMongoCollection{TDocument}"/>, for which <paramref name="predicate"/> returns true,
		/// or returns default value, if there's no such element.
		/// </summary>
		/// <typeparam name="T">Type of Collection's elements</typeparam>
		/// <param name="collection"><see cref="IMongoCollection{TDocument}"/> on which operations will be made.</param>
		/// <param name="predicate">Function, which determines, which element should be returned.</param>
		/// <returns>First occurrence of elements for which <paramref name="predicate"/> returns true and default value if there's no such element.</returns>
		public static async Task<T> FirstOrDefaultAsync<T> (this IMongoCollection<T> collection, Expression<Func<T, bool>> predicate) {
			var temp = await collection.FindAsync (predicate);
			return await temp.FirstOrDefaultAsync ();
		}

		/// <summary>
		/// Converts number of a month to a string.
		/// </summary>
		/// <param name="t">Month's number</param>
		/// <returns>String in which month is written with two digits.</returns>
		public static string GenerateTwoDigitMonth(int t) {
			if (t < 10) return $"0{t}";
			else return t.ToString ();
		}

		/// <summary>
		/// Generates copy of an instance.
		/// </summary>
		/// <typeparam name="T">Object's Type</typeparam>
		/// <param name="val">Instance to copy.</param>
		/// <returns>Copy of an instance.</returns>
		public static T Copy<T> (this T val) {
			return JsonConvert.DeserializeObject<T> (JsonConvert.SerializeObject (val));
		}

		[NonAction]
		public static JsonResult Success(this ControllerBase controller, object details) {
			return new JsonResult (new { Type = "Success", Details = details });
		}

		[NonAction]
		public static JsonResult Error (this ControllerBase controller, string details) {
			return new JsonResult (new { Type = "Error", Details = details });
		}

		[NonAction]
		public static JsonResult Warming (this ControllerBase controller, string details) {
			return new JsonResult (new { Type = "Warming", Details = details });
		}

		public static void AddRange<T> (this HashSet<T> hashSet, IEnumerable<T> collection) {
			foreach (var item in collection) {
				hashSet.Add (item);
			}
		}

		public static async Task AddNotificationAsync (this ControllerBase controller, Notification notification, [FromServices] IMongoCollection<Notification> mongoCollection) {
			if (!notification.AreValuesCorrect ()) return;
			await mongoCollection.InsertOneAsync (notification);
		}

	}
}
