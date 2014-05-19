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
			float x = particles[_p1].Position.X - particles[_p2].Position.X;
			float y = particles[_p2].Position.Y - particles[_p2].Position.Y;
			return x*x + y*y - _dist*_dist;
		}

		public float CalculateTD(List<Particle> particles)
		{
			float x = particles[_p1].Position.X - particles[_p2].Position.X;
			float y = particles[_p1].Position.Y - particles[_p2].Position.Y;
			float xDot = particles[_p1].Velocity.X - particles[_p2].Velocity.X;
			float yDot = particles[_p1].Velocity.Y - particles[_p2].Velocity.Y;
			return 2*x*xDot + 2*y*yDot;
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
