using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Project1
{
	public interface IForce
	{
		/// <summary>
		/// draw the force if neccesary
		/// </summary>
		void Draw();
		/// <summary>
		/// Calculate the force for a certain time step
		/// </summary>
		/// <param name="time">time in seconds</param>
		void Step(float time);
	}
}
