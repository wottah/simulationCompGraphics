using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using micfort.GHL.Math2;

namespace Project1
{
	class LinearSolver
	{
		public const int MaxSteps = 100;

		public static double ConjGrad(int n, Matrix<double> A, HyperPoint<double> x, HyperPoint<double> b, double epsilon, ref int steps)
		{
			int i, iMax;
			double alpha, beta, rSqrLen, rSqrLenOld, u;

			HyperPoint<double> r = new HyperPoint<double>(n);
			HyperPoint<double> d = new HyperPoint<double>(n);
			HyperPoint<double> t = new HyperPoint<double>(n);
			HyperPoint<double> temp = new HyperPoint<double>(n);

			x = b;

			r = b;
			temp = (HyperPoint<double>)(A*x);
			r = r - temp;

			rSqrLen = r.GetLengthSquared();

			d = r;

			i = 0;
			iMax = steps != 0 ? steps : MaxSteps;

			if(rSqrLen < epsilon)
			{
				while (i< iMax)
				{
					i++;
					t = (HyperPoint<double>) (A*d);
					u = HyperPoint<double>.DotProduct(d, t);
					if(u == 0)
					{
						Console.Out.WriteLine("(SolveConjGrad) d'Ad = 0\n");
						break;
					}
					// How far should we go?
					alpha = rSqrLen/u;

					// Take a step along direction d
					temp = d;
					temp = temp*alpha;
					x = x + temp;

					if ((i & 0x3F) != 0)
					{
						temp = t;
						temp = temp*alpha;
						r = r - temp;
					}
					else
					{
						// For stability, correct r every 64th iteration
						r = b;
						temp = (HyperPoint<double>) (A*x);
						r = r - temp;
					}

					rSqrLenOld = rSqrLen;
					rSqrLen = r.GetLengthSquared();

					// Converged! Let's get out of here
					if(rSqrLen <= epsilon)
						break;

					// Change direction: d = r + beta * d
					beta = rSqrLen / rSqrLenOld;
					d = d*beta;
					d = d + r;
				}
			}

			steps = i;
			return rSqrLen;
		}
	}
}
