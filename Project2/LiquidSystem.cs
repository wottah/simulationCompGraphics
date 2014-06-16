using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project1;
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

        public enum resolve { empty, copy, invert, zero };
        public struct bnd
        {
            public resolve res;
            public byte source;
        }

        public bnd[] bIndexu;
        public bnd[] bIndexv;
        public bnd[] bIndexd;
		public float[] u;
		public float[] v;
		public float[] uForce;
		public float[] vForce;
		public float[] densityField;
		public float[] helpScalers;
		public float[] helpScalers2;
		public float[] sources;
		private Visualization _visualization = Visualization.Density;
		private HyperPoint<float> oldPosition = new HyperPoint<float>(0, 0);

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

            FillBoundryIndexes();
		}

        public void FillBoundryIndexes()
        {
			SquareBoundryInternal(0, 0, N+2, N+2);

			SquareBoundry(10, 10, 20, 20, false);
			SquareBoundryInternal(11, 11, 18, 18);
			SquareBoundry(30, 40, 10, 10);
			SquareBoundry(40, 30, 10, 10);
        }

		public void SquareBoundryInternal(int x, int y, int w, int h)
		{
			for (int i = 0; i < w; i++)
			{
				bIndexu[IX(x + i, y)].res = resolve.copy;
				bIndexu[IX(x + i, y)].source = 2;
				bIndexv[IX(x + i, y)].res = resolve.invert;
				bIndexv[IX(x + i, y)].source = 2;
				bIndexd[IX(x + i, y)].res = resolve.copy;
				bIndexd[IX(x + i, y)].source = 2;

				bIndexu[IX(x + i, y + h - 1)].res = resolve.copy;
				bIndexu[IX(x + i, y + h - 1)].source = 4;
				bIndexv[IX(x + i, y + h - 1)].res = resolve.invert;
				bIndexv[IX(x + i, y + h - 1)].source = 4;
				bIndexd[IX(x + i, y + h - 1)].res = resolve.copy;
				bIndexd[IX(x + i, y + h - 1)].source = 4;
			}

			for (int i = 0; i < h; i++)
			{
				bIndexu[IX(x, y + i)].res = resolve.invert;
				bIndexu[IX(x, y + i)].source = 3;
				bIndexv[IX(x, y + i)].res = resolve.copy;
				bIndexv[IX(x, y + i)].source = 3;
				bIndexd[IX(x, y + i)].res = resolve.copy;
				bIndexd[IX(x, y + i)].source = 3;

				bIndexu[IX(x + w - 1, y + i)].res = resolve.invert;
				bIndexu[IX(x + w - 1, y + i)].source = 1;
				bIndexv[IX(x + w - 1, y + i)].res = resolve.copy;
				bIndexv[IX(x + w - 1, y + i)].source = 1;
				bIndexd[IX(x + w - 1, y + i)].res = resolve.copy;
				bIndexd[IX(x + w - 1, y + i)].source = 1;
			}
		}

		public void SquareBoundry(int x, int y, int w, int h, bool zeroInternal = true)
		{
			if(zeroInternal)
			{
				for (int i = 1; i < w - 1; i++)
				{
					for (int j = 1; j < h - 1; j++)
					{
						bIndexu[IX(x + i, y + j)].res = resolve.zero;
						bIndexv[IX(x + i, y + j)].res = resolve.zero;
						bIndexd[IX(x + i, y + j)].res = resolve.zero;
					}
				}
			}
			for (int i = 0; i < w; i++)
			{
				bIndexu[IX(x + i, y)].res = resolve.copy;
				bIndexu[IX(x + i, y)].source = 4;
				bIndexv[IX(x + i, y)].res = resolve.invert;
				bIndexv[IX(x + i, y)].source = 4;
				bIndexd[IX(x + i, y)].res = resolve.copy;
				bIndexd[IX(x + i, y)].source = 4;

				bIndexu[IX(x + i, y + h - 1)].res = resolve.copy;
				bIndexu[IX(x + i, y + h - 1)].source = 2;
				bIndexv[IX(x + i, y + h - 1)].res = resolve.invert;
				bIndexv[IX(x + i, y + h - 1)].source = 2;
				bIndexd[IX(x + i, y + h - 1)].res = resolve.copy;
				bIndexd[IX(x + i, y + h - 1)].source = 2;
			}

			for (int i = 0; i < h; i++)
			{
				bIndexu[IX(x, y + i)].res = resolve.invert;
				bIndexu[IX(x, y + i)].source = 1;
				bIndexv[IX(x, y + i)].res = resolve.copy;
				bIndexv[IX(x, y + i)].source = 1;
				bIndexd[IX(x, y + i)].res = resolve.copy;
				bIndexd[IX(x, y + i)].source = 1;

				bIndexu[IX(x + w - 1, y + i)].res = resolve.invert;
				bIndexu[IX(x + w - 1, y + i)].source = 3;
				bIndexv[IX(x + w - 1, y + i)].res = resolve.copy;
				bIndexv[IX(x + w - 1, y + i)].source = 3;
				bIndexd[IX(x + w - 1, y + i)].res = resolve.copy;
				bIndexd[IX(x + w - 1, y + i)].source = 3;
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
			bIndexu = new bnd[size];
			bIndexv = new bnd[size];
			bIndexd = new bnd[size];

			helpScalers = new float[size];
			helpScalers2 = new float[size];

			ClearData();
		}

		public void UI(HyperPoint<float> position, bool left, bool right, float force, float source)
		{
			int i, j, size = (N + 2) * (N + 2);

			for (i = 0; i < size; i++)
			{
				uForce[i] = vForce[i] = sources[i] = 0.0f;
			}

			if (!left && !right) return;
			
			//+1 for the first item in the line
			i = (int)(position.X * N + 1);
			j = (int)(position.Y * N + 1);

			if (i < 1 || i > N || j < 1 || j > N) return;

			HyperPoint<float> difference = oldPosition - position;
			if(difference.X != 0f) difference.X = -(difference.X/Math.Abs(difference.X));
			if(difference.Y != 0f) difference.Y = -(difference.Y/Math.Abs(difference.Y));

			if (float.IsNaN(difference.X)) throw new NaNException();
			if (float.IsNaN(difference.Y)) throw new NaNException();

			if (left)
			{
				uForce[IX(i, j)] = force * difference.X;
				vForce[IX(i, j)] = force * difference.Y;
			}


			if (right)
			{
				sources[IX(i, j)] = source;
			}

			oldPosition = position;
		}

		public void SimulationStep(float dt, float diff, float visc)
		{
			CalculateVelocity(dt, visc);
			CalculateDensity(dt, diff);
		}

		#region Density calculations

		public void AddSource(float dt, float[] x, float[] s)
		{
			int size = (N + 2)*(N + 2);
			for (int i = 0; i < size; i++) x[i] += dt*s[i];

			for (int i = 0; i < size; i++) if(float.IsNaN(x[i])) throw new NaNException();
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
					i0 = (int)x;
					i1 = i0 + 1;

					if (y < 0.5f) y = 0.5f;
					if (y > N + 0.5f) y = N + 0.5f;
					j0 = (int)y;
					j1 = j0 + 1;

					s1 = x - i0;
					s0 = 1 - s1;
					t1 = y - j0;
					t0 = 1 - t1;
					d[IX(i, j)] = s0 * (t0 * d0[IX(i0, j0)] + t1 * d0[IX(i0, j1)]) +
								  s1 * (t0 * d0[IX(i1, j0)] + t1 * d0[IX(i1, j1)]);
				}
			}
			SetBnd(bIndexd, d);
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
			SetBnd(bIndexd, helpScalers); SetBnd(bIndexd, helpScalers2);

			LinSolve(0, helpScalers2, helpScalers, 1, 4);

			for ( i=1 ; i<=N ; i++ )
			{
				for (j = 1; j <= N; j++)
				{
					u[IX(i, j)] -= 0.5f * N * (helpScalers2[IX(i + 1, j)] - helpScalers2[IX(i - 1, j)]);
					v[IX(i, j)] -= 0.5f * N * (helpScalers2[IX(i, j + 1)] - helpScalers2[IX(i, j - 1)]);
				}
			}
			SetBnd(bIndexu, u); SetBnd(bIndexv, v);
		}

		public void CalculateVelocity(float dt, float visc)
		{
			AddSource(dt, u, uForce);
			AddSource(dt, v, vForce);

			Diffuse(visc, dt, 1, helpScalers, u);
			Diffuse(visc, dt, 2, helpScalers2, v);
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
				SetBnd(bIndexd, x);
			}
		}

		#endregion

		#region Boundary conditions

		private void SetBnd(bnd[] b, float[] x)
		{
            int i, j;

            for (i = 0; i <= N + 1; i++)
            {
                for (j = 0; j <= N + 1; j++)
                {
                    if (b[IX(i, j)].res == resolve.copy)
                    {
                        if (b[IX(i, j)].source == 1) x[IX(i, j)] = x[IX(i - 1, j)];
                        if (b[IX(i, j)].source == 2) x[IX(i, j)] = x[IX(i, j + 1)];
                        if (b[IX(i, j)].source == 3) x[IX(i, j)] = x[IX(i + 1, j)];
                        if (b[IX(i, j)].source == 4) x[IX(i, j)] = x[IX(i, j - 1)];
                    }
                    if (b[IX(i, j)].res == resolve.invert)
                    {
                        if (b[IX(i, j)].source == 1) x[IX(i, j)] = -x[IX(i - 1, j)];
                        if (b[IX(i, j)].source == 2) x[IX(i, j)] = -x[IX(i, j + 1)];
                        if (b[IX(i, j)].source == 3) x[IX(i, j)] = -x[IX(i + 1, j)];
                        if (b[IX(i, j)].source == 4) x[IX(i, j)] = -x[IX(i, j - 1)];
                    }
					if (b[IX(i, j)].res == resolve.zero)
					{
						x[IX(i, j)] = 0;
					}
                }
            }

            x[IX(0, 0)] = 0.5f * (x[IX(1, 0)] + x[IX(0, 1)]);
            x[IX(0, N + 1)] = 0.5f * (x[IX(1, N + 1)] + x[IX(0, N)]);
            x[IX(N + 1, 0)] = 0.5f * (x[IX(N, 0)] + x[IX(N + 1, 1)]);
            x[IX(N + 1, N + 1)] = 0.5f * (x[IX(N, N + 1)] + x[IX(N + 1, N)]);
		}

		#endregion


		#region Helper methods

		public int IX(int x, int y)
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

			h = 1.0f / N;

			
			GL.LineWidth(1.0f);

			GL.Begin(PrimitiveType.Lines);

			for (i = 0; i <= N+1; i++)
			{
				x = (i - 0.5f) * h;
				for (j = 0; j <= N+1; j++)
				{
					y = (j - 0.5f) * h;
					if (bIndexd[IX(i, j)].res == resolve.empty)
					{
						GL.Color3(1f, 0f, 1f);
					}
					else
					{
						GL.Color3(1f, 0f, 0f);
					}
					GL.Vertex2(x, y);
					GL.Vertex2(x + u[IX(i, j)], y + v[IX(i, j)]);
				}
			}

			GL.End();

			GL.Begin(PrimitiveType.Quads);
			
			for (i = 0; i <= N+1; i++)
			{
				x = (i - 0.5f) * h;
				for (j = 0; j <= N+1; j++)
				{
					y = (j - 0.5f) * h;

					if (bIndexd[IX(i, j)].res == resolve.empty)
					{
						GL.Color3(0.5f, 0.5f, 0.5f);
					}
					else if (bIndexd[IX(i, j)].res == resolve.zero)
					{
						GL.Color3(0f, 0f, 1f);
					}
					else
					{
						GL.Color3(1f, 0f, 0f);
					}

					float quadSize = 0.002f;
					GL.Vertex2(x - quadSize, y - quadSize);
					GL.Vertex2(x + quadSize, y - quadSize);
					GL.Vertex2(x + quadSize, y + quadSize);
					GL.Vertex2(x - quadSize, y + quadSize);
				}
			}

			GL.End();
		}

		private void DrawDensity()
		{
			int i, j;
			float x, y, h, d00, d01, d10, d11;

			h = 1.0f / N;

			GL.Begin(PrimitiveType.Quads);

			for (i = 0; i <= N; i++)
			{
				x = (i - 0.5f) * h;
				for (j = 0; j <= N; j++)
				{
					y = (j - 0.5f) * h;

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
