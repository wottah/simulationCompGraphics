using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using micfort.GHL.Math2;

namespace Project2.Particles
{
	class VelocityFieldForce: IForce
	{
		private readonly List<int> _particles;
		private readonly ContinuousField _uField;
		private readonly ContinuousField _vField;
		private readonly float _factor;

		public VelocityFieldForce(List<int> particles, ContinuousField uField, ContinuousField vField, float factor )
		{
			_particles = particles;
			_uField = uField;
			_vField = vField;
			_factor = factor;
		}

		#region Implementation of IForce

		/// <summary>
		/// Calculate the force
		/// </summary>
		public void CalculateForce(List<Particle> particles)
		{
			foreach (int p in _particles)
			{
				particles[p].ForceAccumulator += new HyperPoint<float>(_uField[particles[p].Position * _factor],
																	   _vField[particles[p].Position * _factor]);
			}
		}

		#endregion
	}
}
