using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace MandelCalculation {

	public enum CalcPrecision {
		Single = 1,
		Double = 2,
		FixedPoint = 3
	}

	public enum SmoothingMode {
		None = 0,
		Double = 1,
		Quadruple = 2
	}

	// An object that contains the result and statistics for a performed calculation
	public class CalcResult {

		public int[,] Pixels { get; private set; }
		public long Micro { get; private set; }

		public CalcResult(int[,] pixels, long micro) {
			Pixels = pixels;
			Micro = micro;
		}

	}

	// An object that contains the information for calculating an image section and methods to do the calculation
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

		// Creates the calculation from its string representation
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

		private static string FormatFloat(decimal f) => f.ToString(CultureInfo.InvariantCulture);

		// The string representation of the calculation to send to external workers
		public override string ToString() {
			return $"{W};{H};{FormatFloat(Left)};{FormatFloat(Top)};{FormatFloat(Scale)};{(int)Precision};{Depth};{(int)Smoothing}";
		}

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

		private int[,] CalcFixedPoint(int cores, decimal xScale, decimal yScale, int w, int h) {
			int[,] result = new int[w, h];
			ParallelOptions opt = new ParallelOptions { MaxDegreeOfParallelism = cores };
			FixedPoint two = new FixedPoint(2.0m);
			Parallel.For(0, w * h, opt, i => {
				int x = i % w;
				int y = i / w;
				int cnt = 0;
				decimal r = Left + x * xScale;
				decimal im = Top - y * yScale;
				if (r > -4 && r < 4 && im > -4 && im < 4) {
					FixedPoint cr = new FixedPoint(r);
					FixedPoint ci = new FixedPoint(im);
					FixedPoint zr = cr;
					FixedPoint zi = ci;
					while (cnt < Depth) {
						FixedPoint zr2 = zr.Sqr();
						FixedPoint zi2 = zi.Sqr();
						if ((zr2 + zi2).ToDouble >= 4.0) break;
						zi = two * zr * zi + ci;
						zr = zr2 - zi2 + cr;
						cnt++;
					}
				}
				result[x, y] = cnt;
			});
			return result;
		}

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
				case CalcPrecision.FixedPoint:
					result = CalcFixedPoint(cores, xScale, yScale, w, h);
					break;
			}
			sw.Stop();
			return new CalcResult(result, CalcUtil.MicroSeconds(sw));
		}

	}

}
