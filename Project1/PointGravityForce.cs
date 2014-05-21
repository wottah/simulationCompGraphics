using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using micfort.GHL.Math2;

namespace Project1
{
	class PointGravityForce: IDrawableForce
	{
		private readonly List<int> _particles;
		private readonly int _p;
		private readonly HyperPoint<float> _g;

		public PointGravityForce(List<int> particles, int p, HyperPoint<float> g)
		{
			_particles = particles;
			_p = p;
			_g = g;
		}

		#region Implementation of IDrawable

		public void Draw(List<Particle> particles)
		{
			
			GL.Begin(BeginMode.Lines);
			foreach (int pIndex in _particles)
			{
				Particle p = particles[pIndex];
				GL.Color3(0.8f, 0.7f, 0.6f);
				GLMath2.Vertex2(p.Position);
				GL.Color3(0.8f, 0.7f, 0.6f);

				GLMath2.Vertex2(p.Position + calculateForce(p, particles[_p]));
			}
			GL.End();
		}

		#endregion

		#region Implementation of IForce

		/// <summary>
		/// Calculate the force
		/// </summary>
		public void CalculateForce(List<Particle> particles)
		{
			foreach (int p in _particles)
			{
				particles[p].ForceAccumulator += calculateForce(particles[p], particles[_p]);
			}
		}

		#endregion

		private HyperPoint<float> calculateForce(Particle p, Particle center )
		{
			float distance = (center.Position-p.Position).GetLength();
			if(distance == 0)
			{
				return new HyperPoint<float>(0, 0);
			}
			float delta = 0;
			for (int i = 0; i < _g.Dim; i++)
			{
				delta += Convert.ToSingle((Math.Pow(distance, i)) * _g[i]);
			}
			return (center.Position - p.Position).Normilize() * Math.Max(0, delta*p.Massa*center.Massa);
		} 
	}
}
