using System;
using System.Collections.Generic;
using micfort.GHL.Math2;
using OpenTK.Graphics.OpenGL;

namespace Project2.Particles
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
		private float _rotation = Convert.ToSingle(Math.PI/4);
		private float _angularVelocity = Convert.ToSingle(Math.PI/4/100);

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

		public HyperPoint<float> ConstructVel { get; set; }

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

		public float Rotation
		{
			get { return _rotation; }
			set { _rotation = value; }
		}

		public float AngularVelocity
		{
			get { return _angularVelocity; }
			set { _angularVelocity = value; }
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

		public MovingPolygon Polygon { get; set; } 

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

		public Particle(HyperPoint<float> constructPos, float massa = 1f, List<HyperPoint<float>> polygon = null)
		{
			Polygon = polygon;
			Size = 0.01f;
			Color = new HyperPoint<float>(1, 1, 1);
			_constructPos = constructPos;
			ConstructVel = new HyperPoint<float>(0, 0);
			_forceAccumulator = new HyperPoint<float>(0, 0);
			_massa = massa;
            _visible = true;
			reset();
		}

		public Particle(HyperPoint<float> constructPos, HyperPoint<float> constructVel, float massa = 1f, List<HyperPoint<float>> polygon = null)
		{
			Size = 0.01f;
			Color = new HyperPoint<float>(1, 1, 1);
			_constructPos = constructPos;
			ConstructVel = constructVel;
			Polygon = polygon;
			_forceAccumulator = new HyperPoint<float>(0, 0);
			_massa = massa;
			_visible = true;
			reset();
		}

		public void reset()
		{
			_position = _constructPos;
			_velocity = ConstructVel;
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

				if (Polygon != null)
				{
					GL.Color3(1f, 0f, 0f);
					GL.LineWidth(1.0f);

					Matrix<float> m = Matrix<float>.Identity(3);
					m[0, 0] = Convert.ToSingle(Math.Cos(Rotation)); m[0, 1] = Convert.ToSingle(-Math.Sin(Rotation));
					m[1, 0] = Convert.ToSingle(Math.Sin(Rotation)); m[1, 1] = Convert.ToSingle(Math.Cos(Rotation));
					m = Matrix<float>.Translate(Position) * m;

					Polygon.Draw(m);
				}
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
