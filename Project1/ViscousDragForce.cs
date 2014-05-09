using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Project1
{
	class ViscousDragForce: IForce
	{
		private readonly List<Particle> _particles;
		private readonly float _drag;

		public ViscousDragForce(List<Particle> particles, float drag)
		{
			_particles = particles;
			_drag = drag;
		}

		#region Implementation of IForce

		/// <summary>
		/// Calculate the force
		/// </summary>
		public void CalculateForce()
		{
			_particles.ForEach(x => x.ForceAccumulator -= _drag*x.Velocity);
		}

		#endregion
	}
}
