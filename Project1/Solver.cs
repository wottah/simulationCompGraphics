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
			particles.ForEach(x => x.ForceAccumulator = new HyperPoint<float>(0f, 0f));
			Forces.ForEach(x => x.CalculateForce());
			particles.ForEach(x => x.Velocity = x.Velocity*Damp + x.ForceAccumulator*dt);
			particles.ForEach(x => x.Position += x.Velocity*dt);
		}
	}
}
