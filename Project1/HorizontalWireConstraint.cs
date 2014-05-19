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
		private readonly Particle _p;
        private readonly float _linepos;

        public HorizontalWireConstraint(Particle p, float linepos)
		{
            _p = p;
            _linepos = linepos;
		}

		public void Draw()
		{
            GL.Begin(BeginMode.Lines);
            GL.Color3(0.8f, 0.7f, 0.6f);
            GL.Vertex2(-1000, _linepos);
            GL.Color3(0.8f, 0.7f, 0.6f);
            GL.Vertex2(1000, _linepos);
            GL.End();
		}

		#region Implementation of IConstraint2

		public float Calculate()
		{
            return _p.Position.Y - _linepos;
		}

		public float CalculateTD()
		{
            return _p.Velocity.Y;
		}

		public List<ResultingConstraint> CalculateQD()
		{
			return new List<ResultingConstraint>()
				       {
					       new ResultingConstraint()
						       {
							       Constraint = new HyperPoint<float>(0,1),
								   Particle = _p
						       }
				       };
		}

		public List<ResultingConstraint> CalculateQDTD()
		{
			return new List<ResultingConstraint>()
				       {
					       new ResultingConstraint()
						       {
							       Constraint = new HyperPoint<float>(0,0),
								   Particle = _p
						       }
				       };
		}

		#endregion
	}
}
