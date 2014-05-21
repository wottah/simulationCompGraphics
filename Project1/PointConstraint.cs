using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using micfort.GHL.Math2;
using OpenTK.Graphics.OpenGL;

namespace Project1
{
	class PointConstraint : IDrawableConstraint
	{
		private readonly int _p;
		private readonly HyperPoint<float> _center;

		public PointConstraint(int p, HyperPoint<float> center)
		{
			_p = p;
			_center = center;
		}

		public void Draw(List<Particle> particles)
		{

		}

		#region Implementation of IConstraint2

		public float Calculate(List<Particle> particles)
		{
			HyperPoint<float> translateCenter = particles[_p].Position - _center;
			return translateCenter.DotProduct(translateCenter);
		}

		public float CalculateTD(List<Particle> particles)
		{
			return HyperPoint<float>.DotProduct(2 * particles[_p].Position - 2 * _center, particles[_p].Velocity);
		}

		public List<ResultingConstraint> CalculateQD(List<Particle> particles)
		{
			return new List<ResultingConstraint>()
				       {
					       new ResultingConstraint()
						       {
							       Constraint = 2*particles[_p].Position - 2*_center,
								   ParticleIndex = _p
						       }
				       };
		}

		public List<ResultingConstraint> CalculateQDTD(List<Particle> particles)
		{
			return new List<ResultingConstraint>()
				       {
					       new ResultingConstraint()
						       {
							       Constraint = 2*particles[_p].Velocity,
								   ParticleIndex = _p
						       }
				       };
		}

		#endregion
	}
}
