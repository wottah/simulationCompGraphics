using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using micfort.GHL.Math2;

namespace Project1
{
	static class Solver
	{
		public const float Damp = 0.98f;
		private static Random rand = new Random();
		public static float GetRandom()
		{
			float result = Convert.ToSingle(((rand.Next()%2000.0f)/1000.0f) - 1.0f);
			return result;
		}

		public static void SimulationStep(List<Particle> particles, float dt)
		{
			for (int i = 0; i < particles.Count; i++)
			{
				particles[i].Position += particles[i].Velocity*dt;
				particles[i].Velocity = particles[i].Velocity*Damp + new HyperPoint<float>(GetRandom(), GetRandom())*0.005f;
			}
		}
	}
}
