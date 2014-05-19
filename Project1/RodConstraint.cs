using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using micfort.GHL.Math2;
using OpenTK.Graphics.OpenGL;

namespace Project1
{
	class RodConstraint: IDrawableConstraint
	{
		private readonly int _p1;
		private readonly int _p2;
		private readonly float _dist;

		public RodConstraint(int p1, int p2, float dist)
		{
			_p1 = p1;
			_p2 = p2;
			_dist = dist;
		}

		public void Draw(List<Particle> particles)
		{
			GL.Begin(BeginMode.Lines);
			GL.Color3(0f, 1f, 0f);
			GL.Vertex2(particles[_p1].Position[0], particles[_p1].Position[1]);
			GL.Vertex2(particles[_p2].Position[0], particles[_p2].Position[1]); 
			GL.End();
		}

		#region Implementation of IConstraint2

		public float Calculate(List<Particle> particles)
		{
			HyperPoint<float> distanceVector = particles[_p1].Position - particles[_p2].Position;
			return distanceVector.DotProduct(distanceVector) - _dist * _dist;
		}

		public float CalculateTD(List<Particle> particles)
		{
			HyperPoint<float> distanceVector = particles[_p1].Position - particles[_p2].Position;
			HyperPoint<float> distanceVelocityVector = particles[_p1].Velocity - particles[_p2].Velocity;
			return 2 * HyperPoint<float>.DotProduct(distanceVector, distanceVelocityVector);
		}

		public List<ResultingConstraint> CalculateQD(List<Particle> particles)
		{
			return new List<ResultingConstraint>()
				       {
					       new ResultingConstraint()
						       {
							       Constraint = 2*(particles[_p1].Position-particles[_p2].Position),
								   ParticleIndex = _p1
						       },
					       new ResultingConstraint()
						       {
							       Constraint = -2*(particles[_p1].Position-particles[_p2].Position),
								   ParticleIndex = _p2
						       }
				       };
		}

		public List<ResultingConstraint> CalculateQDTD(List<Particle> particles)
		{
			return new List<ResultingConstraint>()
				       {
					       new ResultingConstraint()
						       {
							       Constraint = 2*(particles[_p1].Velocity-particles[_p2].Velocity),
								   ParticleIndex = _p1
						       },
					       new ResultingConstraint()
						       {
							       Constraint = -2*(particles[_p1].Velocity-particles[_p2].Velocity),
								   ParticleIndex = _p2
						       }
				       };
		}

		#endregion
	}
}
