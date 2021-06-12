using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace MandelCalculation {

	public enum CalcPrecision {
		Single = 1,
		Double = 2,
		Decimal = 3
	}

	public enum SmoothingMode {
		None = 0,
		Double = 1,
		Quadruple = 2
	}

	public class CalcResult {

		public int[,] Pixels { get; private set; }
		public long Micro { get; private set; }

		public CalcResult(int[,] pixels, long micro) {
			Pixels = pixels;
			Micro = micro;
		}

	}

	public class Calculation {

		public int W { get; protected set; }
		public int H { get; protected set; }
		public decimal Left { get; protected set; }
		public decimal Top { get; protected set; }
		public decimal Scale { get; protected set; }
		public CalcPrecision Precision { get; protected set; }
		public int Depth { get; protected set; }
		public SmoothingMode Smoothing { get; protected set; }

		public Calculation(int w, int h, decimal left, decimal top, decimal scale, CalcPrecision precision, int depth, SmoothingMode smoothing) {
			W = w;
			H = h;
			Left = left;
			Top = top;
			Scale = scale;
			Precision = precision;
			Depth = depth;
			Smoothing = smoothing;
		}

		public Calculation(string settings) {
			string[] parts = settings.Split(';');
			W = Int32.Parse(parts[0]);
			H = Int32.Parse(parts[1]);
			Left = Decimal.Parse(parts[2], CultureInfo.InvariantCulture);
			Top = Decimal.Parse(parts[3], CultureInfo.InvariantCulture);
			Scale = Decimal.Parse(parts[4], CultureInfo.InvariantCulture);
			Precision = (CalcPrecision)Int32.Parse(parts[5]);
			Depth = Int32.Parse(parts[6]);
			Smoothing = (SmoothingMode)Int32.Parse(parts[7]);
		}

		private string FormatFloat(decimal f) => f.ToString(CultureInfo.InvariantCulture);

		public override string ToString() {
			return $"{W};{H};{FormatFloat(Left)};{FormatFloat(Top)};{FormatFloat(Scale)};{(int)Precision};{Depth};{(int)Smoothing}";
		}

		//private int[,] CalcSingle(decimal xScale, decimal yScale, int w, int h) {
		//	int[,] result = new int[w, h];
		//	float xs = (float)xScale;
		//	float ys = (float)yScale;
		//	float ci = (float)Top;
		//	for (int y = 0; y < h; y++) {
		//		float cr = (float)Left;
		//		for (int x = 0; x < w; x++) {
		//			float zr = cr;
		//			float zi = ci;
		//			int cnt = 0;
		//			while (cnt < Depth) {
		//				float zr2 = zr * zr;
		//				float zi2 = zi * zi;
		//				if (zr2 + zi2 >= 4) break;
		//				zi = 2 * zr * zi + ci;
		//				zr = zr2 - zi2 + cr;
		//				cnt++;
		//			}
		//			result[x, y] = cnt;
		//			cr += xs;
		//		}
		//		ci -= ys;
		//	}
		//	return result;
		//}

		private int[,] CalcSingle(int cores, decimal xScale, decimal yScale, int w, int h) {
			int[,] result = new int[w, h];
			ParallelOptions opt = new ParallelOptions { MaxDegreeOfParallelism = cores };
			Parallel.For(0, w * h, opt, i => {
				int x = i % w;
				int y = i / w;
				float cr = (float)(Left + x * xScale);
				float ci = (float)(Top - y * yScale);
				float zr = cr;
				float zi = ci;
				int cnt = 0;
				while (cnt < Depth) {
					float zr2 = zr * zr;
					float zi2 = zi * zi;
					if (zr2 + zi2 >= 4) break;
					zi = 2 * zr * zi + ci;
					zr = zr2 - zi2 + cr;
					cnt++;
				}
				result[x, y] = cnt;
			});
			return result;
		}

		//private int[,] CalcDouble(decimal xScale, decimal yScale, int w, int h) {
		//	int[,] result = new int[w, h];
		//	double xs = (double)xScale;
		//	double ys = (double)yScale;
		//	double ci = (double)Top;
		//	for (int y = 0; y < h; y++) {
		//		double cr = (double)Left;
		//		for (int x = 0; x < w; x++) {
		//			double zr = cr;
		//			double zi = ci;
		//			int cnt = 0;
		//			while (cnt < Depth) {
		//				double zr2 = zr * zr;
		//				double zi2 = zi * zi;
		//				if (zr2 + zi2 >= 4) break;
		//				zi = 2 * zr * zi + ci;
		//				zr = zr2 - zi2 + cr;
		//				cnt++;
		//			}
		//			result[x, y] = cnt;
		//			cr += xs;
		//		}
		//		ci -= ys;
		//	}
		//	return result;
		//}

		private int[,] CalcDouble(int cores, decimal xScale, decimal yScale, int w, int h) {
			int[,] result = new int[w, h];
			ParallelOptions opt = new ParallelOptions { MaxDegreeOfParallelism = cores };
			Parallel.For(0, w * h, opt, i => {
				int x = i % w;
				int y = i / w;
				double cr = (double)(Left + x * xScale);
				double ci = (double)(Top - y * yScale);
				double zr = cr;
				double zi = ci;
				int cnt = 0;
				while (cnt < Depth) {
					double zr2 = zr * zr;
					double zi2 = zi * zi;
					if (zr2 + zi2 >= 4) break;
					zi = 2 * zr * zi + ci;
					zr = zr2 - zi2 + cr;
					cnt++;
				}
				result[x, y] = cnt;
			});
			return result;
		}

		//private int[,] CalcDecimal(decimal xScale, decimal yScale, int w, int h) {
		//	int[,] result = new int[w, h];
		//	decimal ci = Top;
		//	for (int y = 0; y < h; y++) {
		//		decimal cr = Left;
		//		for (int x = 0; x < w; x++) {
		//			decimal zr = cr;
		//			decimal zi = ci;
		//			int cnt = 0;
		//			while (cnt < Depth && zr * zr + zi * zi < 4) {
		//				decimal zr2 = zr * zr;
		//				decimal zi2 = zi * zi;
		//				if (zr2 + zi2 >= 4) break;
		//				zi = 2 * zr * zi + ci;
		//				zr = zr2 - zi2 + cr;
		//				cnt++;
		//			}
		//			result[x, y] = cnt;
		//			cr += xScale;
		//		}
		//		ci -= yScale;
		//	}
		//	return result;
		//}

		private int[,] CalcDecimal(int cores, decimal xScale, decimal yScale, int w, int h) {
			int[,] result = new int[w, h];
			ParallelOptions opt = new ParallelOptions { MaxDegreeOfParallelism = cores };
			Parallel.For(0, w * h, opt, i => {
				int x = i % w;
				int y = i / w;
				decimal cr = Left + x * xScale;
				decimal ci = Top - y * yScale;
				decimal zr = cr;
				decimal zi = ci;
				int cnt = 0;
				while (cnt < Depth && zr * zr + zi * zi < 4) {
					decimal zr2 = zr * zr;
					decimal zi2 = zi * zi;
					if (zr2 + zi2 >= 4) break;
					zi = 2 * zr * zi + ci;
					zr = zr2 - zi2 + cr;
					cnt++;
				}
				result[x, y] = cnt;
			});
			return result;
		}

		//public CalcResult Calculate() {
		//	int w = W, h = H;
		//	decimal xScale = Scale, yScale = Scale;
		//	if (Smoothing != SmoothingMode.None) {
		//		w *= 2;
		//		xScale *= 0.5m;
		//	}
		//	if (Smoothing == SmoothingMode.Quadruple) {
		//		h *= 2;
		//		yScale *= 0.5m;
		//	}
		//	int[,] result = null;
		//	Stopwatch sw = Stopwatch.StartNew();
		//	switch (Precision) {
		//		case CalcPrecision.Single:
		//			result = CalcSingle(xScale, yScale, w, h);
		//			break;
		//		case CalcPrecision.Double:
		//			result = CalcDouble(xScale, yScale, w, h);
		//			break;
		//		case CalcPrecision.Decimal:
		//			result = CalcDecimal(xScale, yScale, w, h);
		//			break;
		//	}
		//	sw.Stop();
		//	return new CalcResult(result, CalcUtil.MicroSeconds(sw));
		//}

		public CalcResult Calculate(int cores) {
			int w = W, h = H;
			decimal xScale = Scale, yScale = Scale;
			if (Smoothing != SmoothingMode.None) {
				w *= 2;
				xScale *= 0.5m;
			}
			if (Smoothing == SmoothingMode.Quadruple) {
				h *= 2;
				yScale *= 0.5m;
			}
			int[,] result = null;
			Stopwatch sw = Stopwatch.StartNew();
			switch (Precision) {
				case CalcPrecision.Single:
					result = CalcSingle(cores, xScale, yScale, w, h);
					break;
				case CalcPrecision.Double:
					result = CalcDouble(cores, xScale, yScale, w, h);
					break;
				case CalcPrecision.Decimal:
					result = CalcDecimal(cores, xScale, yScale, w, h);
					break;
			}
			sw.Stop();
			return new CalcResult(result, CalcUtil.MicroSeconds(sw));
		}

	}

}
