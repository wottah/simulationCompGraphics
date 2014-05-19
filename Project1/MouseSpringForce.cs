using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using micfort.GHL.Math2;

namespace Project1
{
    class MouseSpringForce : IDrawableForce
    {
        private int _p1;
		private HyperPoint<float> _mousePos;
		private readonly float _r;
        private readonly float _ks;
        private readonly float _kd;
        private bool _isEnabled;

        public MouseSpringForce(int p1, HyperPoint<float> mousePos, float r, float ks, float kd, bool isEnabled)
		{
			_p1 = p1;
			_mousePos = mousePos;
			_r = r;
			_ks = ks;
			_kd = kd;
            _isEnabled = isEnabled;
		}

        public int Particle
        {
            get { return _p1; }
            set { _p1 = value; }
        }

        public HyperPoint<float> MousePos
        {
			get { return _mousePos; }
			set { _mousePos = value; }
        }

        public void Enable()
        {
            _isEnabled = true;
        }

        public void Disable()
        {
            _isEnabled = false;
        }

		public void Draw(List<Particle> particles)
		{
            if (_isEnabled)
            {
                GL.Begin(BeginMode.Lines);
                GL.Color3(0.8f, 0.7f, 0.6f);
				GL.Vertex2(particles[_p1].Position[0], particles[_p1].Position[1]);
                GL.Color3(0.8f, 0.7f, 0.6f);
				GL.Vertex2(_mousePos[0], _mousePos[1]);
                GL.End();
            }
		}

		public void CalculateForce(List<Particle> particles)
        {
            if (_isEnabled)
            {
				HyperPoint<float> _l = particles[_p1].Position - _mousePos;
	            HyperPoint<float> _lDot = particles[_p1].Velocity - new HyperPoint<float>(0, 0);
                float _lAbs = _l.GetLength();
                if (_lAbs != 0)
                {
                    HyperPoint<float> _springforce = (_l / _lAbs) * ((_lAbs - _r) * _ks + (HyperPoint<float>.DotProduct(_lDot, _l) / _lAbs) * _kd);
					particles[_p1].ForceAccumulator -= _springforce;
                }
            }
           
        }
    }
}
