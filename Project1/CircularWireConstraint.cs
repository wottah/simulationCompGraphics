using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using micfort.GHL.Math2;
using OpenTK.Graphics.OpenGL;

namespace Project1
{
	class CircularWireConstraint: IDrawableConstraint
	{
		private readonly Particle _p;
		private readonly HyperPoint<float> _center;
		private readonly float _radius;

		public CircularWireConstraint(Particle p, HyperPoint<float> center, float radius)
		{
			_p = p;
			_center = center;
			_radius = radius;
		}

		public void Draw()
		{
			GL.Begin(BeginMode.LineLoop);
			GL.Color3(0f, 1f, 0f);
			for (int i = 0; i < 360; i = i + 18)
			{
				float degInRad = i * Convert.ToSingle(Math.PI) / 180;
				GL.Vertex2(_center[0] + Math.Cos(degInRad) * _radius, _center[1] + Math.Sin(degInRad) * _radius);
			}
			GL.End();
		}

		#region Implementation of IConstraint

		/// <summary>
		/// Calculate the constraint
		/// </summary>
		public void CalculateConstraint()
		{
			HyperPoint<float> f = _p.ForceAccumulator;
			HyperPoint<float> x = _p.Position;
			HyperPoint<float> xD = _p.Velocity;
			float m = _p.Massa;
			Func<HyperPoint<float>, HyperPoint<float>, float> dot = HyperPoint<float>.DotProduct;

			float xDot = dot(x, x);
			float lambda = (-dot(f, x) - m * dot(xD, xD)) / (xDot);
			HyperPoint<float> fDak = lambda*x;
			_p.ForceConstraint = fDak;

			HyperPoint<float> _l = x - (x - _center).Normilize()*_radius;
			HyperPoint<float> _lDot = xD - new HyperPoint<float>(0, 0);
			float _lAbs = _l.GetLength();
			if(_lAbs != 0)
			{
				HyperPoint<float> _springforce = (_l/_lAbs)*((_lAbs) * 100 + (HyperPoint<float>.DotProduct(_lDot, _l)/_lAbs));
				_p.ForceAccumulator -= _springforce;
			}
		}

		#endregion
	}

	class CircularWireConstraint2 : IDrawableConstraint2
	{
		private readonly Particle _p;
		private readonly HyperPoint<float> _center;
		private readonly float _radius;

		public CircularWireConstraint2(Particle p, HyperPoint<float> center, float radius)
		{
			_p = p;
			_center = center;
			_radius = radius;
		}

		public void Draw()
		{
			GL.Begin(BeginMode.LineLoop);
			GL.Color3(0f, 1f, 0f);
			for (int i = 0; i < 360; i = i + 18)
			{
				float degInRad = i * Convert.ToSingle(Math.PI) / 180;
				GL.Vertex2(_center[0] + Math.Cos(degInRad) * _radius, _center[1] + Math.Sin(degInRad) * _radius);
			}
			GL.End();
		}

		#region Implementation of IConstraint2

		public float Calculate(HyperPoint<float> q)
		{
			HyperPoint<float> translateCenter = _p.Position - _center;
			return translateCenter.DotProduct(translateCenter) - _radius*_radius;
		}

		public float CalculateTD(HyperPoint<float> q, HyperPoint<float> qDot)
		{
			return HyperPoint<float>.DotProduct(2*_p.Position - 2*_center, _p.Velocity);
		}

		public List<ResultingConstraint> CalculateQD()
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

		public List<ResultingConstraint> CalculateQDTD()
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

		#endregion
	}
}
