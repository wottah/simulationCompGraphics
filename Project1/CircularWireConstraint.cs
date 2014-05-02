using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using micfort.GHL.Math2;
using OpenTK.Graphics.OpenGL;

namespace Project1
{
	class CircularWireConstraint
	{
		private readonly Particle _p;
		private readonly HyperPoint<float> _center;
		private readonly double _radius;

		public CircularWireConstraint(Particle p, HyperPoint<float> center, double radius)
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
	}
}
