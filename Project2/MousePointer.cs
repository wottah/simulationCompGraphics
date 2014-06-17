using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using micfort.GHL.Math2;

namespace Project2
{
	class MousePointer: IDrawable
	{
		private HyperPoint<float> _mousePos = new HyperPoint<float>(0f, 0f);
		public HyperPoint<float> MousePos
		{
			get { return _mousePos; }
			set { _mousePos = value; }
		}

		#region Implementation of IDrawable

		/// <summary>
		/// draw the force if neccesary
		/// </summary>
		public void Draw(List<Particles.Particle> particles)
		{
			GL.Begin(PrimitiveType.Quads);
			GL.Color3(0, 1f, 0);
			float quadSize = 0.005f;
			GL.Vertex2(MousePos.X - quadSize, MousePos.Y - quadSize);
			GL.Vertex2(MousePos.X + quadSize, MousePos.Y - quadSize);
			GL.Vertex2(MousePos.X + quadSize, MousePos.Y + quadSize);
			GL.Vertex2(MousePos.X - quadSize, MousePos.Y + quadSize);
			GL.End();
		}

		#endregion
	}
}
