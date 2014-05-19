using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using micfort.GHL.Math2;

namespace Project1
{
	public interface IForce
	{
		/// <summary>
		/// Calculate the force
		/// </summary>
		void CalculateForce(List<Particle> particles);
	}

	public interface IDrawable
	{
		/// <summary>
		/// draw the force if neccesary
		/// </summary>
		void Draw(List<Particle> particles);
	}

	public struct ResultingConstraint
	{
		public int ParticleIndex;
		public HyperPoint<float> Constraint;
	}

	public interface IConstraint
	{
		float Calculate(List<Particle> particles);
		float CalculateTD(List<Particle> particles);
		List<ResultingConstraint> CalculateQD(List<Particle> particles);
		List<ResultingConstraint> CalculateQDTD(List<Particle> particles);
	}

	interface IDrawableForce : IDrawable, IForce
	{
	}

	interface IDrawableConstraint : IDrawable, IConstraint
	{
	}
}
