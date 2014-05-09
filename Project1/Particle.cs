using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using micfort.GHL.Math2;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Project1
{
	public class Particle:IDrawable
	{
		private HyperPoint<float> _constructPos;
		private HyperPoint<float> _position;
		private HyperPoint<float> _velocity;
        private HyperPoint<float> _forceAccumulator;
		private float _massa;
		private HyperPoint<float> _forceConstraint;

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

		public HyperPoint<float> ForceConstraint
		{
			get { return _forceConstraint; }
			set { _forceConstraint = value; }
		}

		public HyperPoint<float> ForceAccumulator
        {
            get { return _forceAccumulator; }
            set { _forceAccumulator = value; }
        }

		public float Massa
		{
			get { return _massa; }
			set { _massa = value; }
		}

		public Particle(HyperPoint<float> constructPos, float massa = 1f)
		{
			_constructPos = constructPos;
			_forceAccumulator = new HyperPoint<float>(0, 0);
			_massa = massa;
			reset();
		}

		public void reset()
		{
			_position = _constructPos;
			_velocity = new HyperPoint<float>(0, 0);
			_forceConstraint = new HyperPoint<float>(0, 0);
		}

		public void Draw()
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
