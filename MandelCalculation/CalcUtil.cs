using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MandelCalculation {

	public static class CalcUtil {

		private static void WriteInt(List<byte> result, int i) {
			do {
				int b = i & 127;
				i >>= 7;
				result.Add((byte)(b | (i > 0 ? 128 : 0)));
			} while (i > 0);
		}

		private static int ReadInt(byte[] data, ref int pos) {
			int i = 0, b, shift = 0;
			do {
				b = data[pos++];
				i |= (b & 127) << shift;
				shift += 7;
			} while (b >= 128);
			return i;
		}

		public static byte[] PackPixels(int[,] data) {
			int w = data.GetLength(0);
			int h = data.GetLength(1);
			int offset = Int32.MaxValue;
			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					int d = data[x, y];
					if (d < offset) {
						offset = d;
					}
				}
			}
			List<byte> result = new List<byte>();
			WriteInt(result, offset);
			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					WriteInt(result, data[x, y] - offset);
				}
			}
			byte[] res = result.ToArray();
			return res;
		}

		public static int[,] UnpackPixels(byte[] data, int w, int h) {
			int[,] result = new int[w, h];
			int pos = 0;
			int offset = ReadInt(data, ref pos);
			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					result[x, y] = ReadInt(data, ref pos) + offset;
				}
			}
			return result;
		}

		public static long MicroSeconds(Stopwatch sw) => 1000000 * sw.ElapsedTicks / Stopwatch.Frequency;

	}

}
