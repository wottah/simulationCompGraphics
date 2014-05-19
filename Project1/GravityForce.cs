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
		private readonly List<int> _particles;
		private readonly HyperPoint<float> _g;

		public GravityForce(List<int> particles, HyperPoint<float> g)
		{
			_particles = particles;
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
				GLMath2.Vertex2(p.Position + _g);
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
			_particles.ForEach(x => particles[x].ForceAccumulator += _g);
		}

		#endregion
	}
}
