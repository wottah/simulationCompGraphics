using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using micfort.GHL.Math2;

namespace Project1
{
	public class Solver
	{
		public List<IForce> Forces { get; private set; }

		public const float Damp = 0.98f;

		#region Rand

		private static Random rand = new Random();

		public static float GetRandom()
		{
			float result = Convert.ToSingle(((rand.Next()%2000.0f)/1000.0f) - 1.0f);
			return result;
		}

		#endregion

		public Solver()
		{
			Forces = new List<IForce>();
		}

		public void SimulationStep(List<Particle> particles, float dt)
		{
			Forces.ForEach(x => x.Step(dt));

			for (int i = 0; i < particles.Count; i++)
			{
				particles[i].Position += particles[i].Velocity * dt;
				//particles[i].Velocity = particles[i].Velocity * Damp + new HyperPoint<float>(GetRandom(), GetRandom()) * 0.005f;
			}
		}
	}
}
