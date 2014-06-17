using System;
using micfort.GHL.Math2;

namespace Project2.Particles
{
	class LinearSolver
	{
		public const int MaxSteps = 100;

		public static float ConjGrad(int n, ImplicitMatrix<float> A, out HyperPoint<float> x, HyperPoint<float> b, float epsilon, int steps)
		{
			int i, iMax;
			float alpha, beta, rSqrLen, rSqrLenOld, u;

			HyperPoint<float> r = new HyperPoint<float>(n);
			HyperPoint<float> d = new HyperPoint<float>(n);
			HyperPoint<float> t = new HyperPoint<float>(n);
			HyperPoint<float> temp = new HyperPoint<float>(n);

			x = b;

			r = b;
			temp = A * x;
			r = r - temp;

			rSqrLen = r.GetLengthSquared();

			d = r;

			iMax = steps != 0 ? steps : MaxSteps;
			i = 0;

			if(rSqrLen > epsilon)
			{
				while (i< iMax)
				{
					i++;
					t = A * d;
					u = HyperPoint<float>.DotProduct(d, t);
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
						temp = A * x;
						r = r - temp;
					}

					rSqrLenOld = rSqrLen;
					rSqrLen = r.GetLengthSquared();

					// Converged! Let's get out of here
					if(rSqrLen <= epsilon)
					{
						break;
					}

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
