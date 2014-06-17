using System.Collections.Generic;

namespace Project2.Particles
{
	class ViscousDragForce: IForce
	{
		private readonly List<int> _particles;
		private readonly float _drag;

		public ViscousDragForce(List<int> particles, float drag)
		{
			_particles = particles;
			_drag = drag;
		}

		#region Implementation of IForce

		/// <summary>
		/// Calculate the force
		/// </summary>
		public void CalculateForce(List<Particle> particles)
		{
			_particles.ForEach(x => particles[x].ForceAccumulator -= _drag * particles[x].Velocity);
		}

		#endregion
	}
}
