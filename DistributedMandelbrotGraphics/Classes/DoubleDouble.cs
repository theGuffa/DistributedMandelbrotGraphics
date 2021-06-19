using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedMandelbrotGraphics.Classes {

	public struct DoubleDouble {

		public double hi, lo;

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

		public static DoubleDouble operator -(DoubleDouble n) {
			return new DoubleDouble(-n.hi , - n.lo);
		}

		// ok
		public static DoubleDouble operator +(DoubleDouble a, DoubleDouble b) {
			double lo;
			(double hi, double s2) = two_sum(a.hi, b.hi);
			if (Double.IsFinite(hi)) {
				(double t1, double t2) = two_sum(a.lo, b.lo);
				(lo, t1) = two_sum(s2, t1);
				t1 += t2;
				(hi, lo, t1) = three_sum(hi, lo, t1);
			} else {
				lo = 0.0;
			}
			return new DoubleDouble(hi, lo);
		}

		// ok
		public static DoubleDouble operator +(DoubleDouble a, double b) {
			(double hi, double s2) = two_sum(a.hi, b);
			if (Double.IsFinite(hi)) {
				double lo;
				(lo, s2) = two_sum(a.lo, s2);
				(hi, lo, s2) = three_sum(hi, lo, s2);
				return new DoubleDouble(hi, lo);
			} else {
				return new DoubleDouble(hi);
			}
		}

		private static (double hi, double lo) Split(double a) {
			double hi, lo;

			//const int QD_BITS = 27; // (53 + 1) / 2;
			const double QD_SPLITTER = 134217729.0; // (1.0 * 2 ^ QD_BITS) + 1.0;
			const double k_neg = 0.0000000037252902984619140625; // 2.0 ^ (-QD_BITS - 1);
			const double k_pos = 268435456; // 2 ^ (QD_BITS + 1);
			const double QD_SPLIT_THRESH = double.MaxValue * k_neg; 

			double temp;
			if (Math.Abs(a) > QD_SPLIT_THRESH) {
				a *= k_neg; // 2 ^ (-QD_BITS - 1);
				temp = QD_SPLITTER * a;
				hi = temp - (temp - a);
				lo = a - hi;
				hi *= k_pos; // 2 ^ (QD_BITS + 1);
				lo *= k_pos; // 2 ^ (QD_BITS + 1);
			} else {
				temp = QD_SPLITTER * a;
				hi = temp - (temp - a);
				lo = a - hi;
			}
			return (hi, lo);
		}

		// ok
		private static (double p, double err) two_prod(double a, double b) {
			double err;
			double p = a * b;
			if (Double.IsFinite(p)) {
				(double a_hi, double a_lo) = Split(a);
				(double b_hi, double b_lo) = Split(b);
				err = ((a_hi * b_hi - p) + a_hi * b_lo + a_lo * b_hi) + a_lo * b_lo;
			} else {
				err = 0.0;
			}
			return (p, err);
		}

		// ok
		private static (double s, double err) two_sum(double a, double b) {
			double err;
			double s = a + b;
			if (Double.IsFinite(s)) {
				double bb = s - a;
				err = (a - (s - bb)) + (b - bb);
			} else {
				err = 0;
			}
			return (s, err);
		}

		// ok
		private static (double a, double b, double c) three_sum(double a, double b, double c) {
			double t1, t2, t3;
			(t1, t2) = two_sum(a, b);
			(a, t3) = two_sum(c, t1);
			(b, c) = two_sum(t2, t3);
			return (a, b, c);
		}

		// ok
		public static DoubleDouble Mul(DoubleDouble a, DoubleDouble b) {
			double p0, p1, p2, p3, p4, p5, p6;
			//	e powers in p = 0, 1, 1, 1, 2, 2, 2
			(p0, p1) = two_prod(a.hi, b.hi);
			if (Double.IsFinite(p0)) {
				(p2, p4) = two_prod(a.hi, b.lo);
				(p3, p5) = two_prod(a.lo, b.hi);
				p6 = a.lo * b.lo;
				//	e powers in p = 0, 1, 2, 3, 2, 2, 2
				(p1, p2, p3) = three_sum(p1, p2, p3);
				//	e powers in p = 0, 1, 2, 3, 2, 3, 4
				p2 += p4 + p5 + p6;
				(p0, p1, p2) = three_sum(p0, p1, p2);
				return new DoubleDouble(p0, p1);
			} else {
				return new DoubleDouble(p0);
			}
		}

		// ok
		public static DoubleDouble operator *(DoubleDouble a, double b) {
			(double hi, double p1) = two_prod(a.hi, b);
			if (Double.IsFinite(hi)) {
				double lo = a.lo * b;
				(hi, lo, p1) = three_sum(hi, lo, p1);
				return new DoubleDouble(hi, lo);
			} else {
				return new DoubleDouble(hi);
			}
		}

		public static DoubleDouble Mul10(DoubleDouble a, int exp) {
			return a * Math.Pow(10.0, exp);
		}

		public static DoubleDouble Div10(DoubleDouble a, int exp) {
			return a * Math.Pow(0.1, exp);
		}

		// ok
		private static (double p, double err) two_sqr(double a) {
			double p = a * a;
			double err;
			if (Double.IsFinite(p)) {
				(double hi, double lo) = Split(a);
				err = ((hi * hi - p) + 2.0 * hi * lo) + lo * lo;
			} else {
				err = 0.0;
			}
			return (p, err);
		}

		// ok
		private static (double s, double err) quick_two_sum(double a, double b) {
			double s = a + b;
			double err = Double.IsFinite(s) ? b - (s - a) : 0.0;
			return (s, err);
		}

		// ok
		public static DoubleDouble Sqr(DoubleDouble a) {
			//if (std::isnan(a)) return a;
			(double p1, double p2) = two_sqr(a.hi);
			p2 += 2.0 * a.hi * a.lo;
			p2 += a.lo * a.lo;
			(double s1, double s2) = quick_two_sum(p1, p2);
			return new DoubleDouble(s1, s2);
		}

		public static DoubleDouble Parse(string s) {
			s = s.Trim();

			int sign = 0;
			int point = -1;
			int nd = 0;
			int e = 0;
			//bool done = false;
			DoubleDouble r = new DoubleDouble(0.0, 0.0);
			int p = 0;
			bool exp = false;

			while (/*!done &&*/ p < s.Length) {
				char ch = s[p];
				if (Char.IsDigit(ch)) {
					int d = ch - '0';
					if (exp) {
						e = e * 10 + d;
					} else {
						r = r * 10.0 + d;
					}
					nd++;
				} else {
					switch (ch) {
						case '.':
							if (point >= 0) throw new FormatException();
							point = nd;
							break;
						case '-':
						case '+':
							if (sign != 0 || nd > 0) throw new FormatException();
							sign = (ch == '-') ? -1 : 1;
							break;
						case 'E':
						case 'e':
							if (exp) throw new FormatException();
							exp = true;
							break;
							//int nread = std::swscanf(p + 1, L"%d", &e);
							//	done = true;
							//	if (nread != 1)
							//		return -1;
							//	break;
						default:
							throw new FormatException();
					}
				}
				p++;
			}
			if (point >= 0) {
				e -= nd - point;
			}
			if (e != 0) {
				r *= Math.Pow(10.0, e);
			}
			return sign == -1 ? -r : r;
		}

	}

}
