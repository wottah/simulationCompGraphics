using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using micfort.GHL.Math2;

namespace Project2.Particles
{
	static class GLMath2
	{
		public static void Vertex2(HyperPoint<float> p)
		{
			GL.Vertex2(p.X, p.Y);
		}

		public static void Color3(HyperPoint<float> p)
		{
			GL.Color3(p.R, p.G, p.B);
		}
	}
}
