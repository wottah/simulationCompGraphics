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
		void CalculateForce();
	}

	public interface IConstraint
	{
		/// <summary>
		/// Calculate the constraint
		/// </summary>
		void CalculateConstraint();
	}

	public interface IDrawable
	{
		/// <summary>
		/// draw the force if neccesary
		/// </summary>
		void Draw();
	}

	public struct ResultingConstraint
	{
		public Particle Particle;
		public HyperPoint<float> Constraint;
	}

	public interface IConstraint2
	{
		float Calculate();
		float CalculateTD();
		List<ResultingConstraint> CalculateQD();
		List<ResultingConstraint> CalculateQDTD();
	}

	interface IDrawableForce : IDrawable, IForce
	{
	}

	interface IDrawableConstraint : IDrawable, IConstraint
	{
	}

	interface IDrawableConstraint2 : IDrawable, IConstraint2
	{
	}
}
