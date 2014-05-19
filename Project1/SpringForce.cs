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
		private readonly int _p1;
		private readonly int _p2;
		private readonly float _r;
        private readonly float _ks;
        private readonly float _kd;

		public SpringForce(int p1, int p2, float r, float ks, float kd)
		{
			_p1 = p1;
			_p2 = p2;
			_r = r;
			_ks = ks;
			_kd = kd;
		}

		public void Draw(List<Particle> particles)
		{
			Particle p1 = particles[_p1];
			Particle p2 = particles[_p2];
			GL.Begin(BeginMode.Lines);
			GL.Color3(0.8f, 0.7f, 0.6f);
			GL.Vertex2(p1.Position[0], p1.Position[1]);
			GL.Color3(0.8f, 0.7f, 0.6f);
			GL.Vertex2(p2.Position[0], p2.Position[1]);
			GL.End();
		}

		public void CalculateForce(List<Particle> particles)
		{
			Particle p1 = particles[_p1];
			Particle p2 = particles[_p2];
			HyperPoint<float> _l = p1.Position - p2.Position;
			HyperPoint<float> _lDot = p1.Velocity - p2.Velocity;
            float _lAbs = _l.GetLength();
            HyperPoint<float> _springforce = (_l / _lAbs)*((_lAbs - _r) * _ks + (HyperPoint<float>.DotProduct(_lDot, _l) / _lAbs) * _kd);
			p1.ForceAccumulator -= _springforce;
			p2.ForceAccumulator += _springforce;
           
        }
	}
}
