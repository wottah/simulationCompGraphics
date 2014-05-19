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
		private readonly Particle _p;
		private readonly HyperPoint<float> _center;
		private readonly float _radius;

		public InCircularConstraint(Particle p, HyperPoint<float> center, float radius)
		{
			_p = p;
			_center = center;
			_radius = radius;
		}

		public void Draw()
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
		
		public bool InRadius()
		{
			HyperPoint<float> translateCenter = _p.Position - _center;
			return translateCenter.DotProduct(translateCenter) - _radius * _radius < 0;
		}

		#region Implementation of IConstraint2

		public float Calculate()
		{
			if(InRadius())
			{
				HyperPoint<float> translateCenter = _p.Position - _center;
				return translateCenter.DotProduct(translateCenter);
			}
			else
			{
				return 0f;
			}
		}

		public float CalculateTD()
		{
			if (InRadius())
			{
				return 0f;
			}
			else
			{
				return HyperPoint<float>.DotProduct(2*_p.Position - 2*_center, _p.Velocity);
			}
		}

		public List<ResultingConstraint> CalculateQD()
		{
			if (InRadius())
			{
				return new List<ResultingConstraint>()
				       {
					       new ResultingConstraint()
						       {
							       Constraint = new HyperPoint<float>(0, 0),
								   Particle = _p
						       }
				       };
			}
			else
			{
				return new List<ResultingConstraint>()
				       {
					       new ResultingConstraint()
						       {
							       Constraint = 2*_p.Position - 2*_center,
								   Particle = _p
						       }
				       };
			}
			
		}

		public List<ResultingConstraint> CalculateQDTD()
		{
			if (InRadius())
			{
				return new List<ResultingConstraint>()
					       {
						       new ResultingConstraint()
							       {
								       Constraint = new HyperPoint<float>(0, 0),
								       Particle = _p
							       }
					       };
			}
			else
			{
				return new List<ResultingConstraint>()
					       {
						       new ResultingConstraint()
							       {
								       Constraint = 2*_p.Velocity,
								       Particle = _p
							       }
					       };
			}
		}

		#endregion
	}
}
