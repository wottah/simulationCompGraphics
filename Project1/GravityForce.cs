using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using micfort.GHL.Math2;

namespace Project1
{
	class GravityForce: IForce
	{
		private readonly Particle _p;
		private readonly HyperPoint<float> _g;

		public GravityForce(Particle p, HyperPoint<float> g)
		{
			_p = p;
			_g = g;
		}

		#region Implementation of Force

		public void Draw()
		{
			GL.Begin(BeginMode.Lines);
			GL.Color3(0.8f, 0.7f, 0.6f);
			GLMath2.Vertex2(_p.Position);
			GL.Color3(0.8f, 0.7f, 0.6f);
			GLMath2.Vertex2(_p.Position+_p.Velocity);
			GL.End();
		}

		public void Step(float time)
		{
			_p.Velocity = _p.Velocity + _g*time;
		}

		#endregion
	}
}
