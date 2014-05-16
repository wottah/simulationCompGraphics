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
        private  Particle _p1;
		private  Particle _p2;
		private readonly float _r;
        private readonly float _ks;
        private readonly float _kd;
        private bool _isEnabled;

        public MouseSpringForce(Particle p1, HyperPoint<float> mousePos, float r, float ks, float kd, bool isEnabled)
		{
			_p1 = p1;
			_p2 = new Particle(mousePos);
			_r = r;
			_ks = ks;
			_kd = kd;
            _isEnabled = isEnabled;
		}

        public Particle Particle
        {
            get { return _p1; }
            set { _p1 = value; }
        }

        public HyperPoint<float> MousePos
        {
            get { return _p2.Position; }
            set { _p2.Position = value; }
        }

        public void Enable()
        {
            _isEnabled = true;
        }

        public void Disable()
        {
            _isEnabled = false;
        }

		public void Draw()
		{
            if (_isEnabled)
            {
                GL.Begin(BeginMode.Lines);
                GL.Color3(0.8f, 0.7f, 0.6f);
                GL.Vertex2(_p1.Position[0], _p1.Position[1]);
                GL.Color3(0.8f, 0.7f, 0.6f);
                GL.Vertex2(_p2.Position[0], _p2.Position[1]);
                GL.End();
            }
		}

        public void CalculateForce()
        {
            if (_isEnabled)
            {
                HyperPoint<float> _l = _p1.Position - _p2.Position;
                HyperPoint<float> _lDot = _p1.Velocity - _p2.Velocity;
                float _lAbs = _l.GetLength();
                if (_lAbs != 0)
                {
                    HyperPoint<float> _springforce = (_l / _lAbs) * ((_lAbs - _r) * _ks + (HyperPoint<float>.DotProduct(_lDot, _l) / _lAbs) * _kd);
                    _p1.ForceAccumulator -= _springforce;
                }
            }
           
        }
    }
}
