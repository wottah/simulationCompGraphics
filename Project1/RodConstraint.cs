using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using micfort.GHL.Math2;
using OpenTK.Graphics.OpenGL;

namespace Project1
{
	class RodConstraint
	{
		private readonly Particle _p1;
		private readonly Particle _p2;
		private readonly double _dist;

		public RodConstraint(Particle p1, Particle p2, double dist)
		{
			_p1 = p1;
			_p2 = p2;
			_dist = dist;
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
