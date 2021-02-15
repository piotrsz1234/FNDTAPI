using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FDNTAPI.DataModels.Interfaces {
	public interface IDataModel {
		/// <summary>
		/// Checks does Data Model's properties have correct values.
		/// </summary>
		/// <returns>Returns does values are correct or not.</returns>
		bool AreValuesCorrect ();
	}
}
