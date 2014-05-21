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
		private int _index;
        private bool _visible;

        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

		public HyperPoint<float> ConstructPos
		{
			get { return _constructPos; }
			set { _constructPos = value; }
		}

		public HyperPoint<float> Position
		{
			get { return _position; }
			set
			{
				if (float.IsNaN(value.X) || float.IsNaN(value.Y))
				{
					throw new NaNException();
				}
				_position = value;
			}
		}

		public HyperPoint<float> Velocity
		{
			get { return _velocity; }
			set
			{
				if (float.IsNaN(value.X) || float.IsNaN(value.Y))
				{
					throw new NaNException();
				}
				_velocity = value;
			}
		}

		public HyperPoint<float> ForceConstraint
		{
			get { return _forceConstraint; }
			set
			{
				if (float.IsNaN(value.X) || float.IsNaN(value.Y))
				{
					throw new NaNException();
				}
				if (value.GetLengthSquared() > 100 * 100)
				{
					Console.Out.WriteLine("Large constraint force??");
				}
				_forceConstraint = value;
			}
		}

		public HyperPoint<float> ForceAccumulator
        {
            get { return _forceAccumulator; }
            set
            {
				if (float.IsNaN(value.X) || float.IsNaN(value.Y))
				{
					throw new NaNException();
				}
	            _forceAccumulator = value;
            }
        }

		public HyperPoint<float> ForceComplete
		{
			get
			{
				return (_forceAccumulator + _forceConstraint)/2;
			}
		} 

		public HyperPoint<float> Color { get; set; }

		public float Size { get; set; }

		public float Massa
		{
			get { return _massa; }
			set { _massa = value; }
		}

		public int Index
		{
			get { return _index; }
			set { _index = value; }
		}

		public Particle(HyperPoint<float> constructPos, float massa = 1f)
		{
			Size = 0.03f;
			Color = new HyperPoint<float>(1, 1, 1);
			_constructPos = constructPos;
			_forceAccumulator = new HyperPoint<float>(0, 0);
			_massa = massa;
            _visible = true;
			reset();
		}

		public void reset()
		{
			_position = _constructPos;
			_velocity = new HyperPoint<float>(0, 0);
			_forceConstraint = new HyperPoint<float>(0, 0);
		}

		public void Draw(List<Particle> particles)
		{
            if (_visible)
            {
                GLMath2.Color3(Color);
                GL.Begin(BeginMode.Quads);
                GL.Vertex2(_position[0] - Size / 2.0, _position[1] - Size / 2.0);
                GL.Vertex2(_position[0] + Size / 2.0, _position[1] - Size / 2.0);
                GL.Vertex2(_position[0] + Size / 2.0, _position[1] + Size / 2.0);
                GL.Vertex2(_position[0] - Size / 2.0, _position[1] + Size / 2.0);
                GL.End();
            }
		}

		public Particle Clone()
		{
			Particle p = new Particle(_constructPos, _massa)
				             {
					             _forceAccumulator = _forceAccumulator,
					             _position = _position,
					             _velocity = _velocity,
					             _forceConstraint = _forceConstraint,
					             _index = _index
				             };
			return p;
		}
	}
}
