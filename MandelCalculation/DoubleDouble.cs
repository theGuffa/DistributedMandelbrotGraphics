using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelCalculation {

	public struct FloatFloat {

		private readonly float hi, lo;

		public FloatFloat(float high, float low) {
			hi = high;
			lo = low;
		}

		public FloatFloat(double n) {
			hi = (float)n;
			lo = (float)(n - (double)hi);
		}

		public float ToFloat => hi + lo;

		public double ToDouble => (double)hi + (double)lo;

		public static FloatFloat operator -(FloatFloat n) => new FloatFloat(-n.hi, -n.lo);

		public static FloatFloat operator -(FloatFloat a, FloatFloat b) => a + -b;

		private static FloatFloat quickTwoSum(float a, float b) {
			float s = a + b;
			float e = b - (s - a);
			return new FloatFloat(s, e);
		}

		private static FloatFloat twoSum(float a, float b) {
			float s = a + b;
			float v = s - a;
			float e = (a - (s - v)) + (b - v);
			return new FloatFloat(s, e);
		}

		public static FloatFloat operator +(FloatFloat a, FloatFloat b) {
			FloatFloat s = twoSum(a.hi, b.hi);
			FloatFloat t = twoSum(a.lo, b.lo);
			s = quickTwoSum(s.hi, s.lo + t.hi);
			return quickTwoSum(s.hi, s.lo + t.lo);
		}

	}

	public struct DoubleDouble {

		private readonly double hi, lo;

		public DoubleDouble(double high,  double low) {
			hi = high;
			lo = low;
		}

		public DoubleDouble(double n) {
			hi = n;
			lo = 0.0;
		}

		public DoubleDouble(decimal n) {
			hi = (double)n;
			lo = (double)(n - (decimal)hi);
		}

		public double ToDouble => hi + lo;

		public decimal ToDecimal => (decimal)hi + (decimal)lo;

		#region Operators

		public static DoubleDouble operator -(DoubleDouble n) => new DoubleDouble(-n.hi, - n.lo);

		public static DoubleDouble operator -(DoubleDouble a, DoubleDouble b) => a + -b;
		//public static DoubleDouble operator -(DoubleDouble a, double b) => a + -b;
		//public static DoubleDouble operator +(double a, DoubleDouble b) => b + a;
		//public static DoubleDouble operator -(double a, DoubleDouble b) => -b + a;
		//public static DoubleDouble operator *(double a, DoubleDouble b) => b * a;

		private static DoubleDouble quickTwoSum(double a, double b) {
			double s = a + b;
			double e = b - (s - a);
			return new DoubleDouble(s, e);
		}

		private static DoubleDouble twoSum(double a, double b) {
			double s = a + b;
			double v = s - a;
			double e = (a - (s - v)) + (b - v);
			return new DoubleDouble(s, e);
		}

		public static DoubleDouble operator +(DoubleDouble x, DoubleDouble y) {
			double H, h, T, t, S, s, e, f;
			double t1;

			S = x.hi + y.hi;
			T = x.lo + y.lo;
			e = S - x.hi;
			f = T - x.lo;

			t1 = S - e;
			t1 = x.hi - t1;
			s = y.hi - e;
			s = s + t1;

			t1 = T - f;
			t1 = x.lo - t1;
			t = y.lo - f;
			t = t + t1;

			s = s + T;
			H = S + s;
			h = S - H;
			h = h + s;

			h = h + t;
			e = H + h;
			f = H - e;
			f = f + h;

			return new DoubleDouble(e, f);
		}

		//private static DoubleDouble Renormalize(DoubleDouble s, DoubleDouble t) {
		//	s = QuickTwoSum(s.hi, s.lo + t.hi);
		//	return QuickTwoSumD(s.hi, s.lo + t.lo);
		//}

		//// ok
		//public static DoubleDouble operator +(DoubleDouble a, DoubleDouble b) {
		//	DoubleDouble x = TwoSum(a.hi, b.hi);
		//	DoubleDouble y = TwoSum(a.lo, b.lo);
		//	x = QuickTwoSum(x.hi, x.lo + y.hi);
		//	DoubleDouble z = QuickTwoSum(x.hi, x.lo + y.lo);
		//	return z;
		//}

		// ok 2
		// off
		//public static DoubleDouble operator +(DoubleDouble a, double b) {
		//	DoubleDouble aa = TwoSum(a.hi, b);
		//	//if (Double.IsFinite(aa.hi)) {
		//	//DoubleDouble t = TwoSum(a.lo, aa.lo);
		//	//aa = ThreeSum(aa.hi, t.hi, t.lo);
		//	//}
		//	//return aa;
		//	return QuickTwoSumD(aa.hi, aa.lo + a.lo);
		//}

		//public static DoubleDouble QuickTwoSumD(double a, double b) {
		//	double sum = a + b;
		//	return new DoubleDouble(sum, b - (sum - a));
		//}

		//public static DoubleDouble operator -(DoubleDouble x, DoubleDouble y) {
		//	//	DoubleDouble s = TwoDiff(left.hi, right.hi);
		//	//	DoubleDouble t = TwoDiff(left.lo, right.lo);
		//	//	return Renormalize(s, t);
		//	double H, h, T, t, S, s, e, f;
		//	double t1, yhi, ylo;

		//	yhi = -y.hi;
		//	ylo = -y.lo;

		//	S = x.hi + yhi;
		//	T = x.lo + ylo;
		//	e = S - x.hi;
		//	f = T - x.lo;

		//	t1 = S - e;
		//	t1 = x.hi - t1;
		//	s = yhi - e;
		//	s = s + t1;

		//	t1 = T - f;
		//	t1 = x.lo - t1;
		//	t = ylo - f;
		//	t = t + t1;


		//	s = s + T;
		//	H = S + s;
		//	h = S - H;
		//	h = h + s;

		//	h = h + t;
		//	e = H + h;
		//	f = H - e;
		//	f = f + h;

		//	return new DoubleDouble(e, f);
		//}

		//private static DoubleDouble TwoDiff(double a, double b) {
		//}

		//public static DoubleDouble operator -(DoubleDouble left, double right) {
		//	DoubleDouble s = TwoDiff(left.hi, right);
		//	return QuickTwoSumD(s.hi, s.lo + left.lo);
		//}

		private static DoubleDouble split(double a) {
			const double split = (1 << 28) + 1;
			double t = a * split;
			double ahi = t - (t - a);
			double alo = a - ahi;
			return new DoubleDouble(ahi, alo);
		}

		private static DoubleDouble twoProd(double a, double b) {
			double p = a * b;
			DoubleDouble aS = split(a);
			DoubleDouble bS = split(b);
			double err = ((aS.hi * bS.hi - p) + aS.hi * bS.lo + aS.lo * bS.hi) + aS.lo * bS.lo;
			return new DoubleDouble(p, err);
		}

		// ok 2
		// off
		public static DoubleDouble operator *(DoubleDouble a, DoubleDouble b) {
			DoubleDouble p = twoProd(a.hi, b.hi);
			double plo = p.lo + a.hi * b.lo;
			plo += a.lo * b.hi;
			p = quickTwoSum(p.hi, plo);
			return p;
			//	DoubleDouble p1 = TwoProd(a.hi, b.hi);
			//	//if (Double.IsFinite(p0)) {
			//	//DoubleDouble p2 = TwoProd(a.hi, b.lo);
			//	//DoubleDouble p3 = TwoProd(a.lo, b.hi);
			//	//DoubleDouble p4 = ThreeSum(p1.lo, p2.hi, p3.hi);
			//	//DoubleDouble z = ThreeSum(p1.hi, p4.hi, p4.lo + p2.lo + p3.lo + a.lo * b.lo);
			//	//return z;
			//	//} else {
			//	//	return new DoubleDouble(p0);
			//	//}
			//	return QuickTwoSumD(p1.hi, p1.lo + a.lo * b.hi + a.lo * b.hi);
		}

		// ok 2
		// off
		//public static DoubleDouble operator *(DoubleDouble a, double b) {
		//	DoubleDouble x = TwoProd(a.hi, b);
		//	//if (Double.IsFinite(hi)) {
		//	//DoubleDouble y = ThreeSum(x.hi, a.lo * b, x.lo);
		//	//return y;
		//	//} else {
		//	//	return new DoubleDouble(hi);
		//	//}
		//	return QuickTwoSumD(x.hi, x.lo + a.lo * b);
		//}

		#endregion

		// ok 2
		// off, but identical to a*a
		public DoubleDouble Sqr() {
		//	//	//if (std::isnan(a)) return a;
		//	//	DoubleDouble p = TwoSqr(hi);
		//	//	double p2 = p.lo;
		//	//	p2 += 2.0 * hi * lo;
		//	//	p2 += lo * lo;
		//	//	DoubleDouble s = QuickTwoSum(p.hi, p2);
		//	//	return s;
			return this * this;
		}

		#region Private methods

		// ok
		//private static DoubleDouble Split(double a) {

		//	//const int QD_BITS = 27; // (53 + 1) / 2;
		//	const double QD_SPLITTER = 134217729.0; // (1.0 * 2 ^ QD_BITS) + 1.0;
		//	const double k_pos = 268435456.0; // 2 ^ (QD_BITS + 1);
		//	const double k_neg = 1.0 / k_pos; // 0.0000000037252902984619140625; // 2.0 ^ (-QD_BITS - 1);
		//	const double QD_SPLIT_THRESH = double.MaxValue * k_neg;

		//	double hi, lo;
		//	double temp;
		//	if (Math.Abs(a) > QD_SPLIT_THRESH) {
		//		a *= k_neg; // 2 ^ (-QD_BITS - 1);
		//		temp = QD_SPLITTER * a;
		//		hi = temp - (temp - a);
		//		lo = a - hi;
		//		hi *= k_pos; // 2 ^ (QD_BITS + 1);
		//		lo *= k_pos; // 2 ^ (QD_BITS + 1);
		//	} else {
		//		temp = QD_SPLITTER * a;
		//		hi = temp - (temp - a);
		//		lo = a - hi;
		//	}
		//	return new DoubleDouble(hi, lo);
		//}

		//public static DoubleDouble Split(double a) {
		//	const double tFactor = (1 << 27) + 1;
		//	double t = tFactor * a;
		//	double high = t - (t - a);
		//	return new DoubleDouble(high, a - high);
		//}

		// ok 2
		//private static DoubleDouble TwoProd(double a, double b) {
		//	double err;
		//	double p = a * b;
		//	//if (Double.IsFinite(p)) {
		//	DoubleDouble aa = Split(a);
		//	DoubleDouble bb = Split(b);
		//	err = ((aa.hi * bb.hi - p) + aa.hi * bb.lo + aa.lo * bb.hi) + aa.lo * bb.lo;
		//	//} else {
		//	//	err = 0.0;
		//	//}
		//	return new DoubleDouble(p, err);
		//}

		//public static DoubleDouble TwoProd(double a, double b) {
		//	double product = a * b;
		//	DoubleDouble aa = Split(a);
		//	DoubleDouble bb = Split(b);
		//	return new DoubleDouble(product, aa.hi * bb.hi - product + aa.hi * bb.lo + aa.lo * bb.hi + aa.lo * bb.lo);
		//}

		// ok 3
		//private static DoubleDouble TwoSum(double a, double b) {
		//	double err;
		//	double s = a + b;
		//	//if (Double.IsFinite(s)) {
		//	double v = s - a;
		//	err = (a - (s - v)) + (b - v);
		//	//} else {
		//	//	err = 0;
		//	//}
		//	return new DoubleDouble(s, err);
		//}

		//public static DoubleDouble TwoSum(double a, double b) {
		//	double s = a + b;
		//	double v = s - a;
		//	return new DoubleDouble(s, a - (s - v) + (b - v));
		//}

		// ok 2
		//private static DoubleDouble ThreeSum(double a, double b, double c) {
		//	DoubleDouble t, x, y;
		//	t = TwoSum(a, b);
		//	x = TwoSum(c, t.hi);
		//	y = TwoSum(t.lo, x.lo);
		//	return new DoubleDouble(x.hi, y.hi);
		//}

		//// ok 2
		//private static DoubleDouble TwoSqr(double a) {
		//	double p = a * a;
		//	double err;
		//	//if (Double.IsFinite(p)) {
		//	DoubleDouble b = Split(a);
		//	err = ((b.hi * b.hi - p) + 2.0 * b.hi * b.lo) + b.lo * b.lo;
		//	//} else {
		//	//	err = 0.0;
		//	//}
		//	return new DoubleDouble(p, err);
		//}

		// ok 2
		//private static DoubleDouble QuickTwoSum(double a, double b) {
		//	double s = a + b;
		//	double err = Double.IsFinite(s) ? b - (s - a) : 0.0;
		//	return new DoubleDouble(s, err);
		//}

		//public static DoubleDouble QuickTwoSum(double a, double b) {
		//	double sum = a + b;
		//	return new DoubleDouble(sum, b - (sum - a));
		//}

		#endregion

		//public static DoubleDouble Parse(string s) {
		//	s = s.Trim();

		//	int sign = 0;
		//	int point = -1;
		//	int nd = 0;
		//	int e = 0;
		//	//bool done = false;
		//	DoubleDouble r = new DoubleDouble(0.0, 0.0);
		//	int p = 0;
		//	bool exp = false;

		//	while (/*!done &&*/ p < s.Length) {
		//		char ch = s[p];
		//		if (Char.IsDigit(ch)) {
		//			int d = ch - '0';
		//			if (exp) {
		//				e = e * 10 + d;
		//			} else {
		//				r = r * 10.0 + d;
		//			}
		//			nd++;
		//		} else {
		//			switch (ch) {
		//				case '.':
		//					if (point >= 0) throw new FormatException();
		//					point = nd;
		//					break;
		//				case '-':
		//				case '+':
		//					if (sign != 0 || nd > 0) throw new FormatException();
		//					sign = (ch == '-') ? -1 : 1;
		//					break;
		//				case 'E':
		//				case 'e':
		//					if (exp) throw new FormatException();
		//					exp = true;
		//					break;
		//					//int nread = std::swscanf(p + 1, L"%d", &e);
		//					//	done = true;
		//					//	if (nread != 1)
		//					//		return -1;
		//					//	break;
		//				default:
		//					throw new FormatException();
		//			}
		//		}
		//		p++;
		//	}
		//	if (point >= 0) {
		//		e -= nd - point;
		//	}
		//	if (e != 0) {
		//		r *= Math.Pow(10.0, e);
		//	}
		//	return sign == -1 ? -r : r;
		//}

	}

}
