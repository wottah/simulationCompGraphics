using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace Project1
{
	class SpringForce
	{
		private readonly Particle _p1;
		private readonly Particle _p2;
		private readonly double _dist;
		private readonly double _ks;
		private readonly double _kd;

		public SpringForce(Particle p1, Particle p2, double dist, double ks, double kd)
		{
			_p1 = p1;
			_p2 = p2;
			_dist = dist;
			_ks = ks;
			_kd = kd;
		}

		public void Draw()
		{
			GL.Begin(BeginMode.Lines);
			GL.Color3(0.8f, 0.7f, 0.6f);
			GL.Vertex2(_p1.Position[0], _p1.Position[1]);
			GL.Color3(0.8f, 0.7f, 0.6f);
			GL.Vertex2(_p2.Position[0], _p2.Position[1]);
			GL.End();
		}
	}
}
