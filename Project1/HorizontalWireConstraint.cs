using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using micfort.GHL.Math2;
using OpenTK.Graphics.OpenGL;

namespace Project1
{
	
	class HorizontalWireConstraint : IDrawableConstraint
	{
		private readonly int _p;
        private readonly float _linepos;

        public HorizontalWireConstraint(int p, float linepos)
		{
            _p = p;
            _linepos = linepos;
		}

		public void Draw(List<Particle> particles)
		{
            GL.Begin(BeginMode.Lines);
            GL.Color3(0.8f, 0.7f, 0.6f);
            GL.Vertex2(-1000, _linepos);
            GL.Color3(0.8f, 0.7f, 0.6f);
            GL.Vertex2(1000, _linepos);
            GL.End();
		}

		#region Implementation of IConstraint2

		public float Calculate(List<Particle> particles)
		{
			return particles[_p].Position.Y - _linepos;
		}

		public float CalculateTD(List<Particle> particles)
		{
			return particles[_p].Velocity.Y;
		}

		public List<ResultingConstraint> CalculateQD(List<Particle> particles)
		{
			return new List<ResultingConstraint>()
				       {
					       new ResultingConstraint()
						       {
							       Constraint = new HyperPoint<float>(0,1),
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
							       Constraint = new HyperPoint<float>(0,0),
								   ParticleIndex = _p
						       }
				       };
		}

		#endregion
	}
}
