using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelCalculation {

	public struct Dd {

		private double hi;
		private double lo;

		public decimal ToDecimal => (decimal)hi + (decimal)lo;
		public double ToDouble => hi + lo;

		public Dd(double h, double l) {
			hi = h;
			lo = l;
		}

		public Dd(decimal d) {
			hi = (double)d;
			lo = (double)(d - (decimal)hi);
		}

		#region Private methods

		private const double _splitter = 134217729.0;

		// ok
		private static Dd TwoSum(double a, double b) {
			double s = a + b;
			double a1 = s - b;
			return new Dd(s, (a - a1) + (b - (s - a1)));
		}

		private static Dd TwoProd(double a, double b) {
			double t = _splitter * a;
			double ah = t + (a - t), al = a - ah;
			t = _splitter * b;
			double bh = t + (b - t), bl = b - bh;
			t = a * b;
			return new Dd(t, ((ah * bh - t) + ah * bl + al * bh) + al * bl);
		}

		private static Dd OneSqr(double a) {
			double t = _splitter * a;
			double ah = t + (a - t), al = a - ah;
			t = a * a;
			double hl = al * ah;
			return new Dd(t, ((ah * ah - t) + hl + hl) + al * al);
		}

		#endregion

		public static Dd operator -(Dd a) => new Dd(-a.hi, -a.lo);

		// ok
		public static Dd operator +(Dd X, Dd Y) {
			Dd S = TwoSum(X.hi, Y.hi);
			Dd E = TwoSum(X.lo, Y.lo);
			double c = S.lo + E.hi;
			double vh = S.hi + c;
			double vl = c - (vh - S.hi);
			c = vl + E.lo;
			double hi = vh + c;
			return new Dd(hi, c - (hi - vh));
		}

		public static Dd operator -(Dd X, Dd Y) {
			Dd S = TwoSum(X.hi, -Y.hi);
			Dd E = TwoSum(X.lo, -Y.lo);
			double c = S.lo + E.hi;
			double vh = S.hi + c, vl = c - (vh - S.hi);
			c = vl + E.lo;
			double hi = vh + c;
			return new Dd(hi, c - (hi - vh));
		}

		public static Dd operator *(Dd X, Dd Y) {
			Dd S = TwoProd(X.hi, Y.hi);
			double Slo = S.lo + X.hi * Y.lo + X.lo * Y.hi;
			double hi = S.hi + Slo;
			return new Dd(hi, Slo - (hi - S.hi));
		}

		public static Dd operator +(Dd X, double f) {
			Dd S = TwoSum(X.hi, f);
			double Slo = S.lo + X.lo;
			double hi = S.hi + Slo;
			return new Dd(hi, Slo - (hi - S.hi));
		}

		public static Dd operator -(Dd X, double f) {
			Dd S = TwoSum(X.hi, -f);
			double Slo = S.lo + X.lo;
			double hi = S.hi + Slo;
			return new Dd(hi, Slo - (hi - S.hi));
		}

		public static Dd operator *(Dd X, double f) {
			Dd C = TwoProd(X.hi, f);
			double cl = X.lo * f;
			double th = C.hi + cl;
			double lo = cl - (th - C.hi);
			cl = lo + C.lo;
			double hi = th + cl;
			return new Dd(hi, cl - (hi - th));
		}

		public static Dd Sqr(Dd X) {
			Dd S = OneSqr(X.hi);
			double c = X.hi * X.lo;
			double Slo = S.lo + c + c;
			double hi = S.hi + Slo;
			return new Dd(hi, Slo - (hi - S.hi));
		}

	}

}
