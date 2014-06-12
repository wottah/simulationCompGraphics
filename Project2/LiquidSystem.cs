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

		public float[] u;
		public float[] v;
		public float[] uForce;
		public float[] vForce;
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
				u[i] = 0; v[i] = 0;
				uForce[i] = 0; vForce[i] = 0;
				densityField[i] = 0f;
				sources[i] = 0f;

				helpScalers[i] = 0f;
				helpScalers2[i] = 0f;
			}
			//densityField[IX(N / 2, N / 2)] = 1;
			//densityField[IX(1, 1)] = 1;
			//densityField[IX(1, 2)] = 1;
			//densityField[IX(2, 2)] = 1;
			//densityField[IX(3, 3)] = 1;
			Random rand = new Random();

			for (int i = 0; i < 1000; i++)
			{
				densityField[IX(rand.Next(1, N + 1), rand.Next(1, N + 1))] = 1;
			}

			for (int i = 1; i <= N; i++)
			{
				for (int j = 1; j <= N; j++)
				{
					u[IX(i, j)] = Convert.ToSingle(rand.NextDouble()*0.4-0.2);
					v[IX(i, j)] = Convert.ToSingle(rand.NextDouble()*0.4-0.2);
				}
			}
		}

		public void AllocateData()
		{
			int size = (N + 2)*(N + 2);
			u = new float[size];
			v = new float[size];
			uForce = new float[size];
			vForce = new float[size];
			densityField = new float[size];
			sources= new float[size];

			helpScalers = new float[size];
			helpScalers2 = new float[size];

			ClearData();
		}

		#region Density calculations

		public void AddSource(float dt, float[] x, float[] s)
		{
			int size = (N + 2)*(N + 2);
			for (int i = 0; i < size; i++) x[i] += dt*s[i];
		}

		public void Diffuse(float diff, float dt, int b, float[] output, float[] input)
		{
			//int size = (N + 2) * (N + 2);
			//for (int i = 0; i < size; i++) output[i] = input[i];

			float a = dt*diff*N*N;
			LinSolve(b, output, input, a, 1 + 4*a);
		}

		public void Advect(int b, float dt, float[] output, float[] input, float[] u, float[] v)
		{
			float[] d = output;
			float[] d0 = input;

			int i, j, i0, j0, i1, j1;
			float s0, t0, s1, t1, dt0, x, y;

			dt0 = dt*N;
			for (i = 1; i <= N; i++)
			{
				for (j = 1; j <= N; j++)
				{
					x = i - dt0 * u[IX(i, j)];
					y = j - dt0 * v[IX(i, j)];

					if (x < 0.5f) x = 0.5f;
					if (x > N + 0.5f) x = N + 0.5f;
					i0 = (int) x;
					i1 = i0 + 1;

					if (y < 0.5f) y = 0.5f;
					if (y > N + 0.5f) y = N + 0.5f;
					j0 = (int) y;
					j1 = j0 + 1;

					s1 = x - i0;
					s0 = 1 - s1;
					t1 = y - j0;
					t0 = 1 - t1;
					d[IX(i, j)] = s0*(t0*d0[IX(i0, j0)] + t1*d0[IX(i0, j1)]) +
					              s1*(t0*d0[IX(i1, j0)] + t1*d0[IX(i1, j1)]);
				}
			}
			SetBnd(b, densityField);
		}

		public void CalculateDensity(float dt, float diff)
		{
			AddSource(dt, densityField, sources);
			Diffuse(diff, dt, 0, helpScalers, densityField);
			Advect(0, dt, densityField, helpScalers, u, v);
		}

		#endregion

		#region Velocity calculations

		public void Project()
		{
			int i, j;

			for ( i=1 ; i<=N ; i++ )
			{
				for (j = 1; j <= N; j++)
				{
					helpScalers[IX(i, j)] = -0.5f * (u[IX(i + 1, j)] - u[IX(i - 1, j)] + v[IX(i, j + 1)] - v[IX(i, j - 1)]) / N;
					helpScalers2[IX(i, j)] = 0;
				}
			}
			SetBnd(0, helpScalers); SetBnd(0, helpScalers2);

			LinSolve(0, helpScalers2, helpScalers, 1, 4);

			for ( i=1 ; i<=N ; i++ )
			{
				for (j = 1; j <= N; j++)
				{
					u[IX(i, j)] -= 0.5f * N * (helpScalers2[IX(i + 1, j)] - helpScalers2[IX(i - 1, j)]);
					v[IX(i, j)] -= 0.5f * N * (helpScalers2[IX(i, j + 1)] - helpScalers2[IX(i, j - 1)]);
				}
			}
			SetBnd(1, u);
			SetBnd(2, v);
		}

		public void CalculateVelocity(float dt, float visc)
		{
			AddSource(dt, u, uForce);
			AddSource(dt, v, vForce);

			Diffuse(visc, dt, 1, helpScalers, u);
			Diffuse(visc, dt, 1, helpScalers2, v);
			Swap(ref helpScalers, ref u);
			Swap(ref helpScalers2, ref v);
			Project();
			Advect(1, dt, helpScalers, u, u, v);
			Advect(2, dt, helpScalers2, v, u, v);
			Swap(ref helpScalers, ref u);
			Swap(ref helpScalers2, ref v);
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
					GL.Vertex2(x + u[IX(i, j)], y + v[IX(i, j)]);
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
