using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using micfort.GHL.Math2;

namespace Project1
{
	class JacobianMatrix
	{
		public static Matrix<float> Create(List<Particle> particles, List<IConstraint> c)
		{
			Matrix<float> output = new Matrix<float>(c.Count, particles.Count*2);
			for (int i = 0; i < c.Count; i++)
			{
				for (int j = 0; j < particles.Count * 2; j++)
				{
					output[i, j] = 0;
				}

				var constraints = c[i].CalculateQD(particles);
				foreach (ResultingConstraint resultingConstraint in constraints)
				{
					output[i, resultingConstraint.ParticleIndex*2 + 0] = resultingConstraint.Constraint[0];
					output[i, resultingConstraint.ParticleIndex*2 + 1] = resultingConstraint.Constraint[1];
				}
			}
			return output;
		}

		public static Matrix<float> CreateDerivative(List<Particle> particles, List<IConstraint> c)
		{
			Matrix<float> output = new Matrix<float>(c.Count, particles.Count * 2);
			for (int i = 0; i < c.Count; i++)
			{
				for (int j = 0; j < particles.Count * 2; j++)
				{
					output[i, j] = 0;
				}

				var constraints = c[i].CalculateQDTD(particles);
				foreach (ResultingConstraint resultingConstraint in constraints)
				{
					output[i, resultingConstraint.ParticleIndex*2 + 0] = resultingConstraint.Constraint[0];
					output[i, resultingConstraint.ParticleIndex*2 + 1] = resultingConstraint.Constraint[1];
				}
			}
			return output;
		}
	}
}
