using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using micfort.GHL.Math2;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Project1
{
	public class Particle
	{
		private HyperPoint<float> _constructPos;
		private HyperPoint<float> _position;
		private HyperPoint<float> _velocity;

		public HyperPoint<float> ConstructPos
		{
			get { return _constructPos; }
			set { _constructPos = value; }
		}

		public HyperPoint<float> Position
		{
			get { return _position; }
			set { _position = value; }
		}

		public HyperPoint<float> Velocity
		{
			get { return _velocity; }
			set { _velocity = value; }
		}

		public Particle(HyperPoint<float> constructPos)
		{
			_constructPos = constructPos;
			_position = new HyperPoint<float>(0, 0);
			_velocity = new HyperPoint<float>(0, 0);
		}

		public void reset()
		{
			_position = _constructPos;
			_velocity = new HyperPoint<float>(0f, 0f);
		}

		public void draw()
		{
			const double h = 0.03;
			GL.Color3(1f, 1f, 1f);
			GL.Begin(BeginMode.Quads);
			GL.Vertex2(_position[0] - h / 2.0, _position[1] - h / 2.0);
			GL.Vertex2(_position[0] + h / 2.0, _position[1] - h / 2.0);
			GL.Vertex2(_position[0] + h / 2.0, _position[1] + h / 2.0);
			GL.Vertex2(_position[0] - h / 2.0, _position[1] + h / 2.0);
			GL.End();
		}
	}
}
