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
		public List<IConstraint> Constraints { get; private set; }

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
			Constraints = new List<IConstraint>();
		}

		public void SimulationStep(List<Particle> particles, float dt)
		{
			particles.ForEach(x => x.ForceAccumulator = new HyperPoint<float>(0f, 0f));
			Forces.ForEach(x => x.CalculateForce());
			Constraints.ForEach(x => x.CalculateConstraint());
			particles.ForEach(x => x.Velocity += (x.ForceAccumulator+x.ForceConstraint)/x.Massa*dt);
			particles.ForEach(x => x.Position += x.Velocity*dt);
		}
	}
}
