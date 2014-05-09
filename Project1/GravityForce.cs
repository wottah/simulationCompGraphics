using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using micfort.GHL.Math2;

namespace Project1
{
	class GravityForce : IDrawableForce
	{
		private readonly List<Particle> _particles;
		private readonly HyperPoint<float> _g;

		public GravityForce(List<Particle> particles, HyperPoint<float> g)
		{
			_particles = particles;
			_g = g;
		}

		#region Implementation of IDrawable

		public void Draw()
		{
			
			GL.Begin(BeginMode.Lines);
			foreach (Particle p in _particles)
			{
				GL.Color3(0.8f, 0.7f, 0.6f);
				GLMath2.Vertex2(p.Position);
				GL.Color3(0.8f, 0.7f, 0.6f);
				GLMath2.Vertex2(p.Position + _g);
			}
			GL.End();
		}

		#endregion

		#region Implementation of IForce

		/// <summary>
		/// Calculate the force
		/// </summary>
		public void CalculateForce()
		{
			_particles.ForEach(x => x.ForceAccumulator += _g);
		}

		#endregion
	}
}
