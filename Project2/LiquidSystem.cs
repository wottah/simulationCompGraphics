using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using micfort.GHL.Math2;
using OpenTK.Graphics.OpenGL;

namespace Project2
{
	enum Visualization
	{
		Velocity,
		Density,
		Both
	}

	class LiquidSystem: IDrawable
	{
		public int N = 64;
		//public float dt = 0.1f;
		public float diff = 0.0f;
		public float visc = 0.0f;

		public HyperPoint<float>[] velField;
		public HyperPoint<float>[] helpVectors;
		public HyperPoint<float>[] force;
		public float[] densityField;
		public float[] helpScalers;
		public float[] helpScalers2;
		public float[] sources;
		private Visualization _visualization = Visualization.Density;

		public Visualization Visualization
		{
			get { return _visualization; }
			set { _visualization = value; }
		}

		public void ClearData()
		{
			int size = (N + 2) * (N + 2);

			for (int i = 0; i < size; i++)
			{
				velField[i] = new HyperPoint<float>(0, 0);
				force[i] = new HyperPoint<float>(2);
				densityField[i] = 0f;
				sources[i] = 0f;

				helpVectors[i] = new HyperPoint<float>(2);
				helpScalers[i] = 0f;
				helpScalers2[i] = 0f;
			}
			densityField[IX(N / 2, N / 2)] = 1;
			//densityField[IX(1, 1)] = 1;
			velField[IX(1, 1)] = new HyperPoint<float>(0.02f, 0.03f);
			Random rand = new Random();

			for (int i = 1; i <= N; i++)
			{
				for (int j = 1; j <= N; j++)
				{
					velField[IX(i, j)] = new HyperPoint<double>(rand.NextDouble() * 0.1, rand.NextDouble() * 0.1).ConvertTo<float>();
				}
			}
		}

		public void AllocateData()
		{
			int size = (N + 2)*(N + 2);
			velField = new HyperPoint<float>[size];
			force = new HyperPoint<float>[size];
			densityField = new float[size];
			sources= new float[size];

			helpVectors = new HyperPoint<float>[size];
			helpScalers = new float[size];
			helpScalers2 = new float[size];

			ClearData();
		}

		#region Density calculations

		public void AddSourceDensity(float dt)
		{
			int size = (N + 2)*(N + 2);
			for (int i = 0; i < size; i++) densityField[i] += dt*sources[i];
		}

		public void DiffuseDensity(float diff, float dt, int b)
		{
			//calculate from helpScalers to densityField
			Swap(ref densityField, ref helpScalers);

			int size = (N + 2) * (N + 2);
			for (int i = 0; i < size; i++) densityField[i] = 0;

			float a = dt*diff*N*N;
			LinSolve(b, densityField, helpScalers, a, 1 + 4*a);
		}

		public void AdvectDensity(int b, float dt)
		{
			//calculate from helpScalers to densityField
			Swap(ref densityField, ref helpScalers);

			int i, j, i0, j0, i1, j1;
			float s0, t0, s1, t1, dt0;

			int size = (N + 2) * (N + 2);
			for (i = 0; i < size; i++) densityField[i] = 0;

			dt0 = dt*N;
			for (i = 1; i <= N; i++)
			{
				for (j = 1; j <= N; j++)
				{
					HyperPoint<float> pos = new HyperPoint<float>(i, j) - dt0*velField[IX(i, j)];

					if (pos.X < 0.5f) pos.X = 0.5f;
					if (pos.X > N + 0.5f) pos.X = N + 0.5f;
					i0 = (int) pos.X;
					i1 = i0 + 1;

					if (pos.Y < 0.5f) pos.Y = 0.5f;
					if (pos.Y > N + 0.5f) pos.Y = N + 0.5f;
					j0 = (int) pos.Y;
					j1 = j0 + 1;

					s1 = pos.X - i0;
					s0 = 1 - s1;
					t1 = pos.Y - j0;
					t0 = 1 - t1;
					densityField[IX(i, j)] = s0*(t0*helpScalers[IX(i0, j0)] + t1*helpScalers[IX(i0, j1)]) +
					                         s1*(t0*helpScalers[IX(i1, j0)] + t1*helpScalers[IX(i1, j1)]);
				}
			}
			SetBnd(b, densityField);
		}

		public void CalculateDensity(float dt, float diff)
		{
			AddSourceDensity(dt);
			DiffuseDensity(diff, dt, 0);
			AdvectDensity(0, dt);
		}

		#endregion

		#region Velocity calculations

		public void AddSourceVelocity(float dt)
		{
			int size = (N + 2) * (N + 2);
			for (int i = 0; i < size; i++) velField[i] += dt * force[i];
		}

		public void DiffuseVelocity(float diff, float dt, HyperPoint<int> b)
		{
			//calculate from helpScalers to densityField
			Swap(ref velField, ref helpVectors);

			int size = (N + 2) * (N + 2);
			for (int i = 0; i < size; i++) velField[i] = new HyperPoint<float>(0f, 0f);

			float a = dt * diff * N * N;
			LinSolve(b, velField, helpVectors, a, 1 + 4 * a);
		}

		public void AdvectVelocity(HyperPoint<int> b, float dt)
		{
			//calculate from helpScalers to densityField
			Swap(ref velField, ref helpVectors);

			int i, j, i0, j0, i1, j1;
			float s0, t0, s1, t1, dt0;

			int size = (N + 2) * (N + 2);
			for (i = 0; i < size; i++) velField[i] = new HyperPoint<float>(0, 0);

			dt0 = dt * N;
			for (i = 1; i <= N; i++)
			{
				for (j = 1; j <= N; j++)
				{
					HyperPoint<float> pos = new HyperPoint<float>(i, j) - dt0 * helpVectors[IX(i, j)];

					if (pos.X < 0.5f) pos.X = 0.5f;
					if (pos.X > N + 0.5f) pos.X = N + 0.5f;
					i0 = (int)pos.X;
					i1 = i0 + 1;

					if (pos.Y < 0.5f) pos.Y = 0.5f;
					if (pos.Y > N + 0.5f) pos.Y = N + 0.5f;
					j0 = (int)pos.Y;
					j1 = j0 + 1;

					s1 = pos.X - i0;
					s0 = 1 - s1;
					t1 = pos.Y - j0;
					t0 = 1 - t1;
					velField[IX(i, j)] = s0*(t0*helpVectors[IX(i0, j0)] + t1*helpVectors[IX(i0, j1)]) +
					                     s1*(t0*helpVectors[IX(i1, j0)] + t1*helpVectors[IX(i1, j1)]);
				}
			}
			SetBnd(b, velField);
		}

		public void Project()
		{
			int i, j;

			for ( i=1 ; i<=N ; i++ )
			{
				for (j = 1; j <= N; j++)
				{
					helpScalers[IX(i, j)] = -0.5f * (velField[IX(i + 1, j)].X - velField[IX(i - 1, j)].X + velField[IX(i, j + 1)].Y - velField[IX(i, j - 1)].Y) / N;
					helpScalers2[IX(i, j)] = 0;
				}
			}
			SetBnd(0, helpScalers); SetBnd(0, helpScalers2);

			LinSolve(0, helpScalers2, helpScalers, 1, 4);

			for ( i=1 ; i<=N ; i++ )
			{
				for (j = 1; j <= N; j++)
				{
					velField[IX(i, j)].X -= 0.5f * N * (helpScalers2[IX(i + 1, j)] - helpScalers2[IX(i - 1, j)]);
					velField[IX(i, j)].Y -= 0.5f * N * (helpScalers2[IX(i, j + 1)] - helpScalers2[IX(i, j - 1)]);
				}
			}
			SetBnd(new HyperPoint<int>(1, 2), velField);
		}

		public void CalculateVelocity(float dt, float visc)
		{
			AddSourceVelocity(dt);
			DiffuseVelocity(visc, dt, new HyperPoint<int>(1, 2));
			Project();
			AdvectVelocity(new HyperPoint<int>(1, 2), dt);
			Project();
		}

		#endregion

		#region Linear solver

		private void LinSolve(int b, float[] x, float[] x0, float a, float c)
		{
			int i, j, k;

			for (k = 0; k < 20; k++)
			{
				for (i = 1; i <= N; i++)
				{
					for (j = 1; j <= N; j++)
					{
						x[IX(i, j)] = (x0[IX(i, j)] + a*(x[IX(i - 1, j)] + x[IX(i + 1, j)] + x[IX(i, j - 1)] + x[IX(i, j + 1)]))/c;
					}
				}
				SetBnd(b, x);
			}
		}

		private void LinSolve(HyperPoint<int> b, HyperPoint<float>[] x, HyperPoint<float>[] x0, float a, float c)
		{
			int i, j, k;

			for (k = 0; k < 20; k++)
			{
				for (int l = 0; l < b.Dim; l++)
				{
					for (i = 1; i <= N; i++)
					{
						for (j = 1; j <= N; j++)
						{
							x[IX(i, j)].p[l] = (x0[IX(i, j)].p[l] + a * (x[IX(i - 1, j)].p[l] + x[IX(i + 1, j)].p[l] + x[IX(i, j - 1)].p[l] + x[IX(i, j + 1)].p[l])) / c;
						}
					}
				}
				
				SetBnd(b, x);
			}
		}

		#endregion

		#region Boundary conditions

		private void SetBnd(int b, float[] x)
		{
			int i;

			for (i = 1; i <= N; i++)
			{
				x[IX(0, i)] = b == 1 ? -x[IX(1, i)] : x[IX(1, i)];
				x[IX(N + 1, i)] = b == 1 ? -x[IX(N, i)] : x[IX(N, i)];
				x[IX(i, 0)] = b == 2 ? -x[IX(i, 1)] : x[IX(i, 1)];
				x[IX(i, N + 1)] = b == 2 ? -x[IX(i, N)] : x[IX(i, N)];
			}
			x[IX(0, 0)] = 0.5f*(x[IX(1, 0)] + x[IX(0, 1)]);
			x[IX(0, N + 1)] = 0.5f*(x[IX(1, N + 1)] + x[IX(0, N)]);
			x[IX(N + 1, 0)] = 0.5f*(x[IX(N, 0)] + x[IX(N + 1, 1)]);
			x[IX(N + 1, N + 1)] = 0.5f*(x[IX(N, N + 1)] + x[IX(N + 1, N)]);
		}

		private void SetBnd(HyperPoint<int> b, HyperPoint<float>[] x)
		{
			for (int k = 0; k < b.Dim; k++)
			{
				int i;

				for (i = 1; i <= N; i++)
				{
					x[IX(0, i)][k] = b[k] == 1 ? -x[IX(1, i)][k] : x[IX(1, i)][k];
					x[IX(N + 1, i)][k] = b[k] == 1 ? -x[IX(N, i)][k] : x[IX(N, i)][k];
					x[IX(i, 0)][k] = b[k] == 2 ? -x[IX(i, 1)][k] : x[IX(i, 1)][k];
					x[IX(i, N + 1)][k] = b[k] == 2 ? -x[IX(i, N)][k] : x[IX(i, N)][k];
				}
				x[IX(0, 0)][k] = 0.5f*(x[IX(1, 0)][k] + x[IX(0, 1)][k]);
				x[IX(0, N + 1)][k] = 0.5f*(x[IX(1, N + 1)][k] + x[IX(0, N)][k]);
				x[IX(N + 1, 0)][k] = 0.5f*(x[IX(N, 0)][k] + x[IX(N + 1, 1)][k]);
				x[IX(N + 1, N + 1)][k] = 0.5f*(x[IX(N, N + 1)][k] + x[IX(N + 1, N)][k]);
			}
		}

		#endregion


		#region Helper methods

		private int IX(int x, int y)
		{
			return (x) + (N + 2)*(y);
		}

		private void Swap<T>(ref T[] a, ref T[] b)
		{
			T[] c = a;
			a = b;
			b = c;
		}

		#endregion

		#region Implementation of IDrawable

		/// <summary>
		/// draw the force if neccesary
		/// </summary>
		public void Draw()
		{
			switch (Visualization)
			{
				case Visualization.Density:
					DrawDensity();
					break;
				case Visualization.Velocity:
					DrawVelocity();
					break;
				case Visualization.Both:
					DrawDensity();
					DrawVelocity();
					break;
			}
		}

		private void DrawVelocity()
		{
			int i, j;
			float x, y, h;

			h = 1.0f / (N+2);

			GL.Color3(1f, 0f, 1f);
			GL.LineWidth(1.0f);

			GL.Begin(PrimitiveType.Lines);

			for (i = 1; i <= N; i++)
			{
				x = (i + 0.5f) * h;
				for (j = 1; j <= N; j++)
				{
					y = (j + 0.5f) * h;

					GL.Vertex2(x, y);
					GL.Vertex2(x + velField[IX(i, j)].X, y + velField[IX(i, j)].Y);
				}
			}

			GL.End();
		}

		private void DrawDensity()
		{
			int i, j;
			float x, y, h, d00, d01, d10, d11;

			h = 1.0f / (N+2);

			GL.Begin(PrimitiveType.Quads);

			for (i = 0; i <= N; i++)
			{
				x = (i + 0.5f) * h;
				for (j = 0; j <= N; j++)
				{
					y = (j + 0.5f) * h;

					d00 = densityField[IX(i, j)];
					d01 = densityField[IX(i, j + 1)];
					d10 = densityField[IX(i + 1, j)];
					d11 = densityField[IX(i + 1, j + 1)];

					GL.Color3(d00, d00, d00); GL.Vertex2(x, y);
					GL.Color3(d10, d10, d10); GL.Vertex2(x + h, y);
					GL.Color3(d11, d11, d11); GL.Vertex2(x + h, y + h);
					GL.Color3(d01, d01, d01); GL.Vertex2(x, y + h);
				}
			}

			GL.End();
		}

		#endregion


	}
}
