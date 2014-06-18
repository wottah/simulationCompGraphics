using System.Collections.Generic;
using micfort.GHL.Math2;

namespace Project2.Particles
{
	public class SolverEnvironment
	{
		public ISolver Solver { get; set; }
		public List<IForce> Forces { get; private set; }
		public List<IConstraint> Constraints { get; private set; }

		private ParticleSimulation simulation = new ParticleSimulation()
			                                        {
														ConstraintsEpsilon = 1 / 100000000f,
														Steps = 1000,
														SpringConstant = 1,
														SpringDemping = 1
			                                        };

		public SolverEnvironment()
		{
			Forces = new List<IForce>();
			Constraints = new List<IConstraint>();
			Solver = new EulerSolver();
		}

		public void SimulationStep(List<Particle> particles, float dt)
		{
			simulation.Constraints = Constraints;
			simulation.Forces = Forces;
			Solver.SimulationStep(simulation, particles, dt);
		}
	}

	public interface ISolver
	{
		void SimulationStep(IBackBox bb, List<Particle> particles, float dt);
		string Name { get; }
	}

	public class EulerSolver : ISolver
	{
		public void SimulationStep(IBackBox bb, List<Particle> particles, float dt)
		{
			bb.Execute(particles, dt);
			particles.ForEach(x => x.Position += x.Velocity * dt);
			particles.ForEach(x => x.Rotation += x.AngularVelocity*dt);
		}

		public string Name
		{
			get { return "Euler"; }
		}
	}

	public interface IBackBox
	{
		void Execute(List<Particle> particles, float dt);
	}

	public class ParticleSimulation : IBackBox
	{
		private float _constraintsEpsilon = 1/10f;

		public List<IForce> Forces { get; set; }
		public List<IConstraint> Constraints { get; set; }
		public float ConstraintsEpsilon
		{
			get { return _constraintsEpsilon; }
			set { _constraintsEpsilon = value; }
		}

		public float SpringConstant { get; set; }
		public float SpringDemping { get; set; }

		public int Steps { get; set; }

		public void Execute(List<Particle> particles, float dt)
		{
			particles.ForEach(x => x.ForceAccumulator = new HyperPoint<float>(0f, 0f));
			particles.ForEach(x => x.AngularForce = 0f);
			Forces.ForEach(x => x.CalculateForce(particles));
			DoConstraints(Constraints, particles, dt, SpringConstant, SpringDemping);
			particles.ForEach(x => x.Velocity += (x.ForceAccumulator + x.ForceConstraint) / x.Massa * dt);
			particles.ForEach(x => x.AngularVelocity += x.AngularForce * dt);
		}

		private void DoConstraints(List<IConstraint> constraints, List<Particle> particles, float dt, float ks, float kd)
		{
			ImplicitMatrix<float> W = ImplicitMatrix<float>.Identity(particles.Count * 2);
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

			ImplicitMatrix<float> J = JacobianMatrix.Create(particles, constraints);
			ImplicitMatrix<float> JDot = JacobianMatrix.CreateDerivative(particles, constraints);
			ImplicitMatrix<float> JT = J.Transpose();

			ImplicitMatrix<float> A = J * W * JT;
			HyperPoint<float> b1 = -JDot * qDot;
			HyperPoint<float> b2 = J * W * Q;
			HyperPoint<float> b3 = ks * c;
			HyperPoint<float> b4 = kd * cDot;
			HyperPoint<float> b = b1 - b2 - b3 - b4;
			HyperPoint<float> x;
			LinearSolver.ConjGrad(b.Dim, A, out x, b, ConstraintsEpsilon, Steps);

			QDak = JT * x;

			for (int i = 0; i < particles.Count; i++)
			{
				particles[i].ForceConstraint = new HyperPoint<float>(QDak[i * 2 + 0], QDak[i * 2 + 1]);
			}
		} 
	}
}
