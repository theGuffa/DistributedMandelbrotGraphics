using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelCalculation {

	public struct Compensated {

      private readonly double number;
      private readonly double error;

		public Compensated(double n, double e) {
         number = n;
         error = e;
		}

		public Compensated(decimal n) {
         number = (double)n;
         error = (double)(n - (decimal)number);
		}

      public decimal ToDecimal => (decimal)number + (decimal)error;

      public static Compensated operator +(Compensated x, Compensated n) {
         double result = x.number + n.number;
         double remainder = TwoSum(x.number, n.number, result);
         return new Compensated(result, x.error + remainder + n.error);
      }

      public static Compensated operator -(Compensated x, Compensated n) {
         double result = x.number - n.number;
         double remainder = TwoSum(x.number, -n.number, result);
         return new Compensated(result, x.error + remainder + n.error);
      }

      private static double TwoSum(double n1, double n2, double result) {
         double n22 = result - n1;
         double n11 = result - n22;
         double epsilon2 = n2 - n22;
         double epsilon1 = n1 - n11;
         double error = epsilon1 + epsilon2;
         return error;
      }

   }

}
