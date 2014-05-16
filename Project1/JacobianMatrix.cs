using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using micfort.GHL.Math2;

namespace Project1
{
	class JacobianMatrix
	{
		public static Matrix<float> Create(HyperPoint<float> q)
		{
			Matrix<float> output = new Matrix<float>(1, 6);
			output[0, 0] = q[0];
			output[0, 1] = q[1];
			return output;
		}

		public static Matrix<float> CreateDerivative(HyperPoint<float> qDot)
		{
			Matrix<float> output = new Matrix<float>(1, 6);
			output[0, 0] = qDot[0];
			output[0, 1] = qDot[1];
			return output;
		}

		public static Matrix<float> Create(int particleCount, List<IConstraint2> c)
		{
			Matrix<float> output = new Matrix<float>(c.Count, particleCount*2);
			for (int i = 0; i < c.Count; i++)
			{
				for (int j = 0; j < particleCount*2; j++)
				{
					output[i, j] = 0;
				}

				var constraints = c[i].CalculateQD();
				foreach (ResultingConstraint resultingConstraint in constraints)
				{
					output[i, resultingConstraint.Particle.Index*2 + 0] = resultingConstraint.Constraint[0];
					output[i, resultingConstraint.Particle.Index*2 + 1] = resultingConstraint.Constraint[1];
				}
			}
			return output;
		}

		public static Matrix<float> CreateDerivative(int particleCount, List<IConstraint2> c)
		{
			Matrix<float> output = new Matrix<float>(c.Count, particleCount * 2);
			for (int i = 0; i < c.Count; i++)
			{
				for (int j = 0; j < particleCount*2; j++)
				{
					output[i, j] = 0;
				}

				var constraints = c[i].CalculateQDTD();
				foreach (ResultingConstraint resultingConstraint in constraints)
				{
					output[i, resultingConstraint.Particle.Index * 2 + 0] = resultingConstraint.Constraint[0];
					output[i, resultingConstraint.Particle.Index * 2 + 1] = resultingConstraint.Constraint[1];
				}
			}
			return output;
		}
	}
}
