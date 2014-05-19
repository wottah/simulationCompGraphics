using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using micfort.GHL.Math2;
using OpenTK.Graphics.OpenGL;

namespace Project1
{
	class InCircularConstraint : IDrawableConstraint
	{
		private readonly int _p;
		private readonly HyperPoint<float> _center;
		private readonly float _radius;

		public InCircularConstraint(int p, HyperPoint<float> center, float radius)
		{
			_p = p;
			_center = center;
			_radius = radius;
		}

		public void Draw(List<Particle> particles)
		{
			GL.Begin(BeginMode.LineLoop);
			GL.Color3(0f, 0f, 1f);
			for (int i = 0; i < 360; i = i + 18)
			{
				float degInRad = i * Convert.ToSingle(Math.PI) / 180;
				GL.Vertex2(_center[0] + Math.Cos(degInRad) * _radius, _center[1] + Math.Sin(degInRad) * _radius);
			}
			GL.End();
		}

		public bool InRadius(List<Particle> particles)
		{
			HyperPoint<float> translateCenter = particles[_p].Position - _center;
			return translateCenter.DotProduct(translateCenter) - _radius * _radius < 0;
		}

		#region Implementation of IConstraint2

		public float Calculate(List<Particle> particles)
		{
			if (InRadius(particles))
			{
				HyperPoint<float> translateCenter = particles[_p].Position - _center;
				return translateCenter.DotProduct(translateCenter);
			}
			else
			{
				return 0f;
			}
		}

		public float CalculateTD(List<Particle> particles)
		{
			if (InRadius(particles))
			{
				return 0f;
			}
			else
			{
				return HyperPoint<float>.DotProduct(2 * particles[_p].Position - 2 * _center, particles[_p].Velocity);
			}
		}

		public List<ResultingConstraint> CalculateQD(List<Particle> particles)
		{
			if (InRadius(particles))
			{
				return new List<ResultingConstraint>()
				       {
					       new ResultingConstraint()
						       {
							       Constraint = new HyperPoint<float>(0, 0),
								   ParticleIndex = _p
						       }
				       };
			}
			else
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
			
		}

		public List<ResultingConstraint> CalculateQDTD(List<Particle> particles)
		{
			if (InRadius(particles))
			{
				return new List<ResultingConstraint>()
					       {
						       new ResultingConstraint()
							       {
								       Constraint = new HyperPoint<float>(0, 0),
								       ParticleIndex = _p
							       }
					       };
			}
			else
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
		}

		#endregion
	}
}
