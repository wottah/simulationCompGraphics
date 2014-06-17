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
		private readonly ContinuousField _dField;
		private readonly float _positionFactor;

		public VelocityFieldForce(List<int> particles, ContinuousField uField, ContinuousField vField, ContinuousField dField, float positionFactor)
		{
			_particles = particles;
			_uField = uField;
			_vField = vField;
			_dField = dField;
			_positionFactor = positionFactor;
		}

		#region Implementation of IForce

		/// <summary>
		/// Calculate the force
		/// </summary>
		public void CalculateForce(List<Particle> particles)
		{
			foreach (int p in _particles)
			{
				HyperPoint<float> position = particles[p].Position*_positionFactor;
				particles[p].ForceAccumulator += new HyperPoint<float>(_uField[position],
																	   _vField[position]) *  Math.Min(_dField[position], 1.0f);
			}
		}

		#endregion
	}
}
