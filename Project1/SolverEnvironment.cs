using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using micfort.GHL.Math2;

namespace Project1
{
	public class SolverEnvironment
	{
		public ISolver Solver { get; set; }
		public List<IForce> Forces { get; private set; }
		public List<IConstraint> Constraints { get; private set; }

		public SolverEnvironment()
		{
			Forces = new List<IForce>();
			Constraints = new List<IConstraint>();
			Solver = new EulerSolver();
		}

		public void SimulationStep(List<Particle> particles, float dt)
		{
			Solver.SimulationStep(Forces, Constraints, particles, dt);
		}
	}

	public interface ISolver
	{
		void SimulationStep(List<IForce> forces, List<IConstraint> constraints, List<Particle> particles, float dt);
		string Name { get; }
	}

	public class RKSolver : ISolver
	{
		#region Implementation of ISolver

		public void SimulationStep(List<IForce> forces, List<IConstraint> constraints, List<Particle> particles, float dt)
		{
			List<Particle> k1 = particles.ConvertAll(x => x.Clone());
			List<Particle> k2 = particles.ConvertAll(x => x.Clone());
			List<Particle> k3 = particles.ConvertAll(x => x.Clone());
			List<Particle> k4 = particles.ConvertAll(x => x.Clone());

			BlackBox(forces, constraints, k1, dt);

			for (int i = 0; i < particles.Count; i++)
			{
				k2[i].Velocity = k1[i].Velocity;
				k2[i].Position += k1[i].Velocity * dt / 2;
			}

			BlackBox(forces, constraints, k2, dt);

			for (int i = 0; i < particles.Count; i++)
			{
				k3[i].Velocity = k2[i].Velocity;
				k3[i].Position += k2[i].Velocity * dt / 2;
			}

			BlackBox(forces, constraints, k3, dt);

			for (int i = 0; i < particles.Count; i++)
			{
				k4[i].Velocity = k3[i].Velocity;
				k4[i].Position += k3[i].Velocity * dt;
			}

			BlackBox(forces, constraints, k4, dt);

			for (int i = 0; i < particles.Count; i++)
			{
				particles[i].Velocity = k1[i].Velocity/6 + k2[i].Velocity/3 + k3[i].Velocity/3 + k4[i].Velocity/6;
				particles[i].Position += particles[i].Velocity*dt;
			}
		}

		public string Name
		{
			get { return "Runge-Kutta"; }
		}

		private void BlackBox(List<IForce> forces, List<IConstraint> constraints, List<Particle> particles, float dt)
		{
			particles.ForEach(x => x.ForceAccumulator = new HyperPoint<float>(0f, 0f));
			forces.ForEach(x => x.CalculateForce(particles));
			DoConstraints(constraints, particles, dt, 100, 100);
			particles.ForEach(x => x.Velocity += (x.ForceAccumulator + x.ForceConstraint) / x.Massa * dt);
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


		#endregion
	}

	public class MidPointSolver : ISolver
	{
		#region Implementation of ISolver

		public void SimulationStep(List<IForce> forces, List<IConstraint> constraints, List<Particle> particles, float dt)
		{
			List<Particle> particlesCopy = particles.ConvertAll(x => x.Clone());

			//first calculate midpoint
			BlackBox(forces, constraints, particlesCopy, dt);
			particlesCopy.ForEach(x => x.Position += x.Velocity * (dt/2));

			//calculate forces on midpoint
			BlackBox(forces, constraints, particlesCopy, dt);

			for (int i = 0; i < particles.Count; i++)
			{
				particles[i].Velocity = particlesCopy[i].Velocity;
				particles[i].Position += particlesCopy[i].Velocity*dt;
			}
		}

		public string Name
		{
			get { return "Mid-point"; }
		}

		private void BlackBox(List<IForce> forces, List<IConstraint> constraints, List<Particle> particles, float dt)
		{
			particles.ForEach(x => x.ForceAccumulator = new HyperPoint<float>(0f, 0f));
			forces.ForEach(x => x.CalculateForce(particles));
			DoConstraints(constraints, particles, dt, 100, 100);
			particles.ForEach(x => x.Velocity += (x.ForceAccumulator + x.ForceConstraint) / x.Massa * dt);
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

		public string Name
		{
			get { return "Euler"; }
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
