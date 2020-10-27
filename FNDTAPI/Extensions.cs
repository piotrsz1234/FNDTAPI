using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using MongoDB.Driver;

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

		public static string GenerateTwoDigitMonth(int t) {
			if (t < 10) return $"0{t}";
			else return t.ToString ();
		}

	}
}
