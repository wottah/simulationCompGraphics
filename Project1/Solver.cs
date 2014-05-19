using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using micfort.GHL.Math2;

namespace Project1
{
	public class Solver
	{
		private EulerSolver _euler;

		public List<IForce> Forces { get; private set; }
		public List<IConstraint> Constraints { get; private set; }

		public Solver()
		{
			Forces = new List<IForce>();
			Constraints = new List<IConstraint>();
			_euler = new EulerSolver();
		}

		public void SimulationStep(List<Particle> particles, float dt)
		{
			_euler.SimulationStep(Forces, Constraints, particles, dt);
		}
	}

	public interface ISolver
	{
		void SimulationStep(List<IForce> forces, List<IConstraint> constraints, List<Particle> particles, float dt);
	}

	public class MidPointSolver : ISolver
	{
		#region Implementation of ISolver

		public void SimulationStep(List<IForce> forces, List<IConstraint> constraints, List<Particle> particles, float dt)
		{
			//EulerSolver euler = new EulerSolver();
			//List<Particle> particlesCopy = particles.ConvertAll(x => x.Clone());

			//euler.SimulationStep(forces, constraints, particles, dt / 2);
			//particles.ForEach(x => x.ForceAccumulator = new HyperPoint<float>(0f, 0f));
			//forces.ForEach(x => x.CalculateForce());
			//for (int i = 0; i < particles.Count; i++)
			//{
			//	particles.
			//}
			

		}

		#endregion
	}

	public class EulerSolver : ISolver
	{
		public void SimulationStep(List<IForce> forces, List<IConstraint> constraints, List<Particle> particles, float dt)
		{
			particles.ForEach(x => x.ForceAccumulator = new HyperPoint<float>(0f, 0f));
			forces.ForEach(x => x.CalculateForce(particles));
			DoConstraints(constraints, particles, dt, 100, 100);
			particles.ForEach(x => x.Velocity += (x.ForceAccumulator + x.ForceConstraint) / x.Massa * dt);
			particles.ForEach(x => x.Position += x.Velocity * dt);
		}

		private void DoConstraints(List<IConstraint> constraints, List<Particle> particles, float dt, float ks, float kd)
		{
			Matrix<float> W = Matrix<float>.Identity(particles.Count * 2);
			HyperPoint<float> Q = new HyperPoint<float>(particles.Count * 2);
			HyperPoint<float> q = new HyperPoint<float>(particles.Count * 2);
			HyperPoint<float> qDot = new HyperPoint<float>(particles.Count * 2);
			HyperPoint<float> QDak = new HyperPoint<float>(particles.Count * 2);
			for (int i = 0; i < particles.Count; i++)
			{
				q[i * 2 + 0] = particles[i].Position[0];
				q[i * 2 + 1] = particles[i].Position[1];
				qDot[i * 2 + 0] = particles[i].Velocity[0];
				qDot[i * 2 + 1] = particles[i].Velocity[1];
				W[i * 2 + 0, i * 2 + 0] = 1 / particles[i].Massa;
				W[i * 2 + 1, i * 2 + 1] = 1 / particles[i].Massa;
				Q[i * 2 + 0] = particles[i].ForceAccumulator[0];
				Q[i * 2 + 1] = particles[i].ForceAccumulator[1];
			}

			HyperPoint<float> c = new HyperPoint<float>(constraints.Count);
			HyperPoint<float> cDot = new HyperPoint<float>(constraints.Count);
			for (int i = 0; i < constraints.Count; i++)
			{
				c[i] = constraints[i].Calculate(particles);
				cDot[i] = constraints[i].CalculateTD(particles);
			}

			Matrix<float> J = JacobianMatrix.Create(particles, constraints);
			Matrix<float> JDot = JacobianMatrix.CreateDerivative(particles, constraints);
			Matrix<float> JT = J.Transpose();

			Matrix<float> A = J * W * JT;
			HyperPoint<float> b1 = (HyperPoint<float>)(-JDot * qDot);
			Matrix<float> b2Sub = J * W * Q;
			HyperPoint<float> b2 = (HyperPoint<float>)(b2Sub);
			HyperPoint<float> b3 = ks * c;
			HyperPoint<float> b4 = kd * cDot;
			HyperPoint<float> b = b1 - b2 - b3 - b4;
			HyperPoint<float> x;
			int steps = 100;
			LinearSolver.ConjGrad(b.Dim, A, out x, b, 1 / 10000f, ref steps);

			QDak = (HyperPoint<float>)(JT * x);

			for (int i = 0; i < particles.Count; i++)
			{
				particles[i].ForceConstraint = new HyperPoint<float>(QDak[i * 2 + 0], QDak[i * 2 + 1]);
			}
		} 

	}
}
