using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MandelCalculation {

	// An object that reads packed ints from an array
	public class ResultReader {

		private byte[] _data;
		private int _pos;

		public ResultReader(byte[] data) {
			_data = data;
			_pos = 0;
		}

		public int ReadInt() {
			int i = 0, b, shift = 0;
			do {
				b = _data[_pos++];
				i |= (b & 127) << shift;
				shift += 7;
			} while (b >= 128);
			return i;
		}

	}

	// An object that writes packed ints to a list
	public class ResultWriter {

		private List<byte> _result;

		public ResultWriter() {
			_result = new List<byte>();
		}

		public void WriteInt(int i) {
			do {
				int b = i & 127;
				i >>= 7;
				_result.Add((byte)(b | (i > 0 ? 128 : 0)));
			} while (i > 0);
		}

		public byte[] ToArray() => _result.ToArray();

	}

	public static class CalcUtil {

		// Packs a result array into a byte stream
		public static byte[] PackPixels(int[,] data) {
			// Get array size
			int w = data.GetLength(0);
			int h = data.GetLength(1);
			// Get lowest value used
			int offset = Int32.MaxValue;
			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					int d = data[x, y];
					if (d < offset) {
						offset = d;
					}
				}
			}
			// Write values to a list
			ResultWriter writer = new ResultWriter();
			writer.WriteInt(offset);
			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					writer.WriteInt(data[x, y] - offset);
				}
			}
			return writer.ToArray();
		}

		// Unpacks a byte stream to a result array
		public static int[,] UnpackPixels(byte[] data, int w, int h) {
			int[,] result = new int[w, h];
			ResultReader reader = new ResultReader(data);
			int offset = reader.ReadInt();
			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					result[x, y] = reader.ReadInt() + offset;
				}
			}
			return result;
		}

		// Calculate microseconds from a stopwatch
		public static long MicroSeconds(Stopwatch sw) => 1000000 * sw.ElapsedTicks / Stopwatch.Frequency;

	}

}
