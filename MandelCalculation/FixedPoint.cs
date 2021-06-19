using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelCalculation {

	/*
	
	A 125 bit fixed point data type that gives higher precision than a decimal, but smaller range.

	The data is stored in two long properties, and the sign in a separate boolean.
	The long properties each have 62 bits of data, leaving the sign bit unused and one bit to use as carry.

	The hi propery contains 4 bits to the left of the decimal point, and 58 bits to the right of the decimal point.
	The lo property contains the other 62 bits to the right of the decimal point.

	Layout: 00nnnn.n58 + 00n62

	As this is created for mandelbrot calculations, only addition, subtraction, multiplication and squaring is supported.
	
	*/

	public struct FixedPoint {

		private const decimal _hiMul = 288230376151711744m; // 2^58
		private const decimal _loMul = 4611686018427387904m; // 2^62

		private readonly long hi, lo;
		private readonly bool neg;

		private FixedPoint(bool neg, long hi, long lo) {
			this.neg = neg;
			this.hi = hi;
			this.lo = lo;
		}

		public FixedPoint(decimal value) {
			if (value < 0) {
				neg = true;
				value = -value;
			} else {
				neg = false;
			}
			value *= _hiMul;
			hi = (long)value;
			lo = (long)((value - hi) * _loMul + 0.5m);
		}

		public double ToDouble {
			get {
				double n = (((double)lo / (double)_loMul + (double)hi) / (double)_hiMul);
				if (neg) {
					n = -n;
				}
				return n;
			}
		}

		public decimal ToDecimal {
			get {
				decimal n = (((decimal)lo / _loMul + hi) / _hiMul);
				if (neg) {
					n = -n;
				}
				return n;
			}
		}

		//public static bool operator >(FixedPoint a, FixedPoint b) {
		//	if (a.neg == b.neg) {
		//		if (a.neg) {
		//			if (a.hi < b.hi) return true;
		//			if (a.hi > b.hi) return false;
		//			if (a.lo < b.lo) return true;
		//			return false;
		//		} else {
		//			if (a.hi > b.hi) return true;
		//			if (a.hi < b.hi) return false;
		//			if (a.lo > b.lo) return true;
		//			return false;
		//		}
		//	} else {
		//		return b.neg;
		//	}
		//}

		//public static bool operator <(FixedPoint a, FixedPoint b) {
		//	if (a.neg == b.neg) {
		//		if (a.neg) {
		//			if (a.hi > b.hi) return true;
		//			if (a.hi < b.hi) return false;
		//			if (a.lo > b.lo) return true;
		//			return false;
		//		} else {
		//			if (a.hi < b.hi) return true;
		//			if (a.hi > b.hi) return false;
		//			if (a.lo < b.lo) return true;
		//			return false;
		//		}
		//	} else {
		//		return a.neg;
		//	}
		//}

		#region Private methods

		// This method adds the absolute values and uses the sign from a.
		private static FixedPoint addAbs(FixedPoint a, FixedPoint b) {
			long lo = a.lo + b.lo;
			return new FixedPoint(a.neg, (lo >> 62) + a.hi + b.hi, lo & 0x3fffffffffffffffL);
		}

		// This method subtracts the absolute values.
		private static FixedPoint subAbs(FixedPoint a, FixedPoint b) {
			if (b.hi > a.hi || (b.hi == a.hi && b.lo > a.lo)) {
				long bhi = b.hi;
				long blo = b.lo;
				if (a.lo > blo) {
					blo += 0x4000000000000000L;
					bhi--;
				}
				return new FixedPoint(true, bhi - a.hi, blo - a.lo);
			} else {
				long ahi = a.hi;
				long alo = a.lo;
				if (b.lo > alo) {
					alo += 0x4000000000000000L;
					ahi--;
				}
				return new FixedPoint(false, ahi - b.hi, alo - b.lo);
			}
		}

		#endregion

		public static FixedPoint operator -(FixedPoint fp) => new FixedPoint(!fp.neg, fp.hi, fp.lo);

		public static FixedPoint operator +(FixedPoint a, FixedPoint b) {
			if (a.neg == b.neg) {
				// Adding two positive or two negative numbers.
				// The result is the sum of the absolute values and the sign from either.
				// a + b = |a| + |b|
				// -a + -b = -(|a| + |b|)
				return addAbs(a, b);
			} else {
				// Adding a negative to a positive is the same as subtracting the absolute of the negative from the positive
				if (a.neg) {
					// -a + b = |b| - |a|
					return subAbs(b, a);
				} else {
					// a + -b = |a| - |b|
					return subAbs(a, b);
				}
			}
		}

		public static FixedPoint operator -(FixedPoint a, FixedPoint b) {
			if (a.neg == b.neg) {
				if (a.neg) {
					// Subtracting a negative from a negative is the same as subtracting the absolutes in reverse.
					// -a - -b = |b| - |a|
					return subAbs(b, a);
				} else {
					// Subtracting a positive from a positive is the same as subtracting the absolutes.
					// a - b = |a| - |b|
					return subAbs(a, b);
				}
			} else {
				// Subtracting a positive from a negative is the same as adding the absolutes and taking the negative.
				// Subtracting a negative from a positive is the same as adding the absolutes.
				// -a - b = -(|a| + |b|)
				// a - -b = |a| + |b|
				return addAbs(a, b);
			}
		}

		public static FixedPoint operator *(FixedPoint a, FixedPoint b) {

			// Each value is split into five 28 bit components.
			// Layout: n20.n8 n28 n22+n6 n28 n28

			long a1 = a.hi >> 50;
			long a2 = (a.hi >> 22) & 0xfffffffL;
			long a3 = (a.lo >> 56) | ((a.hi & 0x3fffffL) << 6);
			long a4 = (a.lo >> 28) & 0xfffffffL;
			long a5 = a.lo & 0xfffffffL;
			long b1 = b.hi >> 50;
			long b2 = (b.hi >> 22) & 0xfffffffL;
			long b3 = (b.lo >> 56) | ((b.hi & 0x3fffffL) << 6);
			long b4 = (b.lo >> 28) & 0xfffffffL;
			long b5 = b.lo & 0xfffffffL;

			/*
			The components are multiplied as two five digit numbers in base 268435456.

			-- -- -- -- a1 a2 a3 a4 a5
			-- -- -- -- b1 b2 b3 b4 b5
			----------------------------
			-- -- -- -- 15 25 35 45 55 // 15 means a1*b5
			-- -- -- 14 24 34 44 54 --
			-- -- 13 23 33 43 53 -- --
			-- 12 22 32 42 52 -- -- --
			11 21 31 41 51 -- -- -- --
			*/

			// The last two results are not needed as they are 30 bits below the least significant bit in the result.
			//long r8 = a5 * b5;
			//long r7 = a4 * b5 + a5 * b4;
			long r6 = a3 * b5 + a4 * b4 + a5 * b3;
			long r5 = a2 * b5 + a3 * b4 + a4 * b3 + a5 * b2;
			long r4 = a1 * b5 + a2 * b4 + a3 * b3 + a4 * b2 + a5 * b1;
			long r3 = a1 * b4 + a2 * b3 + a3 * b2 + a4 * b1;
			long r2 = a1 * b3 + a2 * b2 + a3 * b1;
			long r1 = a1 * b2 + a2 * b1;
			long r0 = a1 * b1;

			// Calculate the part just beyond the last bit in the result, and add a half for rounding.

			long lolo = ((r4 & 0xffL) << 54) + ((r5 & 0xfffffffffL) << 26) + (r6 >> 2) + 0x1000000000000000L;

			// Calculate the values in the result.
			
			long rlo = ((r2 & 0x3fffL) << 48) + ((r3 & 0x3ffffffffffL) << 20) + (r4 >> 8) + (r5 >> 36) + (lolo >> 62);
			long rhi = (r0 << 42) + (r1 << 14) + (r2 >> 14) + (r3 >> 42) + (rlo >> 62);
			rlo &= 0x3fffffffffffffffL;

			return new FixedPoint(a.neg != b.neg, rhi, rlo);
		}

		public FixedPoint Sqr() {
			
			// This is the multiplication algorithm just simplified for the case a*a.
			
			long a1 = hi >> 50;
			long a2 = (hi >> 22) & 0xfffffffL;
			long a3 = (lo >> 56) | ((hi & 0x3fffffL) << 6);
			long a4 = (lo >> 28) & 0xfffffffL;
			long a5 = lo & 0xfffffffL;

			long r6 = a3 * a5 * 2 + a4 * a4;
			long r5 = (a2 * a5 + a3 * a4) * 2;
			long r4 = (a1 * a5 + a2 * a4) * 2 + a3 * a3;
			long r3 = (a1 * a4 + a2 * a3) * 2;
			long r2 = a1 * a3 * 2 + a2 * a2;
			long r1 = a1 * a2 * 2;
			long r0 = a1 * a1;

			long lolo = ((r4 & 0xffL) << 54) + ((r5 & 0xfffffffffL) << 26) + (r6 >> 2) + 0x1000000000000000L;
			long rlo = ((r2 & 0x3fffL) << 48) + ((r3 & 0x3ffffffffffL) << 20) + (r4 >> 8) + (r5 >> 36) + (lolo >> 62);
			long rhi = (r0 << 42) + (r1 << 14) + (r2 >> 14) + (r3 >> 42) + (rlo >> 62);
			rlo &= 0x3fffffffffffffffL;

			return new FixedPoint(false, rhi, rlo);
		}

	}

}
