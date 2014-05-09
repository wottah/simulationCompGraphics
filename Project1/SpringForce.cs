using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using micfort.GHL.Math2;

namespace Project1
{
	class SpringForce : IDrawableForce
	{
		private readonly Particle _p1;
		private readonly Particle _p2;
		private readonly float _r;
        private readonly float _ks;
        private readonly float _kd;

        public SpringForce(Particle p1, Particle p2, float r, float ks, float kd)
		{
			_p1 = p1;
			_p2 = p2;
			_r = r;
			_ks = ks;
			_kd = kd;
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

        public void CalculateForce()
        {
            HyperPoint<float> _l = _p1.Position - _p2.Position;
            HyperPoint<float> _lDot = _p1.Velocity - _p2.Velocity;
            float _lAbs = _l.GetLength();
            HyperPoint<float> _springforce = (_l / _lAbs)*((_lAbs - _r) * _ks + (HyperPoint<float>.DotProduct(_lDot, _l) / _lAbs) * _kd);
            _p1.ForceAccumulator -= _springforce;
            _p2.ForceAccumulator += _springforce;
           
        }
	}
}
