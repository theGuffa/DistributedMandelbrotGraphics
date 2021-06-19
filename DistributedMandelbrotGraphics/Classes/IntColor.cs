using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DistributedMandelbrotGraphics.Classes {

	// A struct used for the color palette, wich quick access to color components
	[StructLayout(LayoutKind.Explicit)]
	public struct IntColor {

		[FieldOffset(0)] public int Color;
		[FieldOffset(0)] public byte B;
		[FieldOffset(1)] public byte G;
		[FieldOffset(2)] public byte R;
		[FieldOffset(3)] public byte A;

		public IntColor(int color) {
			A = 0;
			R = 0;
			G = 0;
			B = 0;
			Color = color;
		}

		public IntColor(byte r, byte g, byte b) {
			Color = 0;
			A = 255;
			R = r;
			G = g;
			B = b;
		}

		// Calculate average between two colors
		public static IntColor Avg(IntColor c1, IntColor c2) {
			return new IntColor(
				(byte)((c1.R + c2.R) >> 1),
				(byte)((c1.G + c2.G) >> 1),
				(byte)((c1.B + c2.B) >> 1)
			);
		}

		// Calculate average between four colors
		public static IntColor Avg(IntColor c1, IntColor c2, IntColor c3, IntColor c4) {
			return new IntColor(
				(byte)((c1.R + c2.R + c3.R + c4.R) >> 2),
				(byte)((c1.G + c2.G + c3.G + c4.G) >> 2),
				(byte)((c1.B + c2.B + c3.B + c4.B) >> 2)
			);
		}

	}

}
