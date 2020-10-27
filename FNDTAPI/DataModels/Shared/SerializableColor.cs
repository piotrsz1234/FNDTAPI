using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace FNDTAPI.DataModels.Shared {
	
	/// <summary>
	/// Class required to serialize color.
	/// </summary>
	[Serializable]
	public class SerializableColor {

		/// <summary>
		/// Red
		/// </summary>
		public int R { get; set; }
		/// <summary>
		/// Green
		/// </summary>
		public int G { get; set; }
		/// <summary>
		/// Blue
		/// </summary>
		public int B { get; set; }
		/// <summary>
		/// Alpha
		/// </summary>
		public int A { get; set; }

		/// <summary>
		/// Empty constructor.
		/// </summary>
		public SerializableColor () { }

		/// <summary>
		/// Creates object based on <see cref="Color"./>
		/// </summary>
		/// <param name="color">Value to copy.</param>
		public SerializableColor(Color color) {
			R = color.R;
			G = color.G;
			B = color.B;
			A = color.A;
		}

		/// <summary>
		/// Converts to <see cref="Color"./>
		/// </summary>
		/// <returns>Converted value.</returns>
		public Color Get() {
			return Color.FromArgb (R, G, B, A);
		}


	}
}
