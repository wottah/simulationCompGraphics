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

	interface IDrawableForce : IDrawable, IForce
	{
	}

	interface IDrawableConstraint : IDrawable, IConstraint
	{
	}
}
