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

	public class VerletSolver : ISolver
	{
		private List<Particle> _previousState = null;

		#region Implementation of ISolver

		public void SimulationStep(IBackBox bb, List<Particle> particles, float dt)
		{
			List<Particle> tmp = particles.ConvertAll(x => x.Clone());
			if (_previousState == null)
			{
				bb.Execute(particles, dt);
				particles.ForEach(x => x.Position = x.Position + x.Velocity*dt + 0.5f*x.ForceComplete*dt*dt);
			}
			else
			{
				bb.Execute(particles, dt);
				for (int i = 0; i < particles.Count; i++)
				{
					particles[i].Position = 2*particles[i].Position - _previousState[i].Position +
					                        particles[i].ForceComplete*dt*dt;
				}
			}
			_previousState = tmp;
		}

		public string Name
		{
			get { return "Verlet"; }
		}

		#endregion
	}

	public class RKSolver : ISolver
	{
		#region Implementation of ISolver

		public void SimulationStep(IBackBox bb, List<Particle> particles, float dt)
		{
			List<Particle> k1 = particles.ConvertAll(x => x.Clone());
			List<Particle> k2 = particles.ConvertAll(x => x.Clone());
			List<Particle> k3 = particles.ConvertAll(x => x.Clone());
			List<Particle> k4 = particles.ConvertAll(x => x.Clone());

			bb.Execute(k1, dt);

			for (int i = 0; i < particles.Count; i++)
			{
				k2[i].Velocity = k1[i].Velocity;
				k2[i].Position += k1[i].Velocity * dt / 2;
			}

			bb.Execute(k2, dt);

			for (int i = 0; i < particles.Count; i++)
			{
				k3[i].Velocity = k2[i].Velocity;
				k3[i].Position += k2[i].Velocity * dt / 2;
			}

			bb.Execute(k3, dt);

			for (int i = 0; i < particles.Count; i++)
			{
				k4[i].Velocity = k3[i].Velocity;
				k4[i].Position += k3[i].Velocity * dt;
			}

			bb.Execute(k4, dt);

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

		#endregion
	}

	public class MidPointSolver : ISolver
	{
		#region Implementation of ISolver

		public void SimulationStep(IBackBox bb, List<Particle> particles, float dt)
		{
			List<Particle> particlesCopy = particles.ConvertAll(x => x.Clone());

			//first calculate midpoint
			bb.Execute(particlesCopy, dt);
			particlesCopy.ForEach(x => x.Position += x.Velocity * (dt/2));

			//calculate forces on midpoint
			bb.Execute(particlesCopy, dt);

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
		#endregion
	}

	public class EulerSolver : ISolver
	{
		public void SimulationStep(IBackBox bb, List<Particle> particles, float dt)
		{
			bb.Execute(particles, dt);
			particles.ForEach(x => x.Position += x.Velocity * dt);
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
			Forces.ForEach(x => x.CalculateForce(particles));
			DoConstraints(Constraints, particles, dt, SpringConstant, SpringDemping);
			particles.ForEach(x => x.Velocity += (x.ForceAccumulator + x.ForceConstraint) / x.Massa * dt);
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
