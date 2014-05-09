using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using micfort.GHL.Math2;

namespace Project1
{
	static class GLMath2
	{
		public static void Vertex2(HyperPoint<float> p)
		{
			GL.Vertex2(p.X, p.Y);
		}
	}
}
