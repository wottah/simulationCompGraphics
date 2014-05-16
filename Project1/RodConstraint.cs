using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using micfort.GHL.Math2;
using OpenTK.Graphics.OpenGL;

namespace Project1
{
	class RodConstraint: IDrawableConstraint2
	{
		private readonly Particle _p1;
		private readonly Particle _p2;
		private readonly float _dist;

		public RodConstraint(Particle p1, Particle p2, float dist)
		{
			_p1 = p1;
			_p2 = p2;
			_dist = dist;
		}

		public void Draw()
		{
			GL.Begin(BeginMode.Lines);
			GL.Color3(0f, 1f, 0f);
			GL.Vertex2(_p1.Position[0], _p1.Position[1]);
			GL.Vertex2(_p2.Position[0], _p2.Position[1]); 
			GL.End();
		}

		#region Implementation of IConstraint2

		public float Calculate(HyperPoint<float> q)
		{
			float x = _p1.Position.X - _p2.Position.X;
			float y = _p1.Position.Y - _p2.Position.Y;
			return x*x + y*y - _dist*_dist;
		}

		public float CalculateTD(HyperPoint<float> q, HyperPoint<float> qDot)
		{
			float x = _p1.Position.X - _p2.Position.X;
			float y = _p1.Position.Y - _p2.Position.Y;
			float xDot = _p1.Velocity.X - _p2.Velocity.X;
			float yDot = _p1.Velocity.Y - _p2.Velocity.Y;
			return 2*x*xDot + 2*y*yDot;
		}

		public List<ResultingConstraint> CalculateQD()
		{
			return new List<ResultingConstraint>()
				       {
					       new ResultingConstraint()
						       {
							       Constraint = 2*(_p1.Position-_p2.Position),
								   Particle = _p1
						       },
					       new ResultingConstraint()
						       {
							       Constraint = -2*(_p1.Position-_p2.Position),
								   Particle = _p2
						       }
				       };
		}

		public List<ResultingConstraint> CalculateQDTD()
		{
			return new List<ResultingConstraint>()
				       {
					       new ResultingConstraint()
						       {
							       Constraint = 2*(_p1.Velocity-_p2.Velocity),
								   Particle = _p1
						       },
					       new ResultingConstraint()
						       {
							       Constraint = -2*(_p1.Velocity-_p2.Velocity),
								   Particle = _p2
						       }
				       };
		}

		#endregion
	}
}
