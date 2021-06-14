using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FDNTAPI.DataModels.Interfaces {
	public interface IDataModel {
		
		/// <summary>
		/// Checks does Data Model's properties have correct values.
		/// </summary>
		/// <returns>Returns does values are correct or not.</returns>
		bool AreValuesCorrect ();

		/// <summary>
		/// Gets dictionary with changes and creates new object, as a copy with applied changes.
		/// </summary>
		/// <param name="changes">Changes</param>
		/// <returns>Copy of object with created changes.</returns>
		public IDataModel ApplyChanges(Dictionary<string, object> changes) {
			var changed = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(this));
			foreach (var pair in changes) {
				if(pair.Key == "Id") continue;
				var temp = this.GetType().GetProperty(pair.Key);
				if (temp == null) return null;
				temp.SetValue(changed, pair.Value);
			}
			return changed as IDataModel;
		}

	}
}
