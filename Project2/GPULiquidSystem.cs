using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project1;
using micfort.GHL.Math2;
using OpenTK.Graphics.OpenGL;
using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;

namespace Project2
{
	class GPULiquidSystem: IDrawable
	{
		public int N = 64;

		public const byte resolveEmpty = 0;
		public const byte resolceCopy = 0;
		public const byte resolveInvert = 0;

		[Cudafy]
        public struct bnd
        {
            public byte res;
            public byte source;
        }

		public float[] uForce;
		public float[] vForce;
		public float[] sources;
		public float[] u;
		public float[] v;
		public float[] densityField;

		private bnd[] dev_bIndexu;
		private bnd[] dev_bIndexv;
		private bnd[] dev_bIndexd;
		private float[] dev_u;
		private float[] dev_v;
		private float[] dev_uForce;
		private float[] dev_vForce;
		private float[] dev_densityField;
		private float[] dev_helpScalers;
		private float[] dev_helpScalers2;
		private float[] dev_sources;
		private Visualization _visualization = Visualization.Density;
		private HyperPoint<float> oldPosition = new HyperPoint<float>(0, 0);

		private GPGPU _gpu;

		public GPULiquidSystem()
		{
			CudafyTranslator.Language = eLanguage.OpenCL;
			CudafyModule km = CudafyTranslator.Cudafy(typeof(GPULiquidSystem));
			System.IO.File.WriteAllText("LiquidSystem.cl", km.SourceCode);

			_gpu = CudafyHost.GetDevice(eGPUType.OpenCL, 0);
			_gpu.LoadModule(km);
		}

		public Visualization Visualization
		{
			get { return _visualization; }
			set { _visualization = value; }
		}

		public void AllocateData()
		{
			int size = (N + 2) * (N + 2);
			uForce = new float[size];
			vForce = new float[size];
			sources = new float[size];
			u = new float[size];
			v = new float[size];
			densityField = new float[size];

			dev_u = _gpu.Allocate<float>(size);
			dev_v = _gpu.Allocate<float>(size);
			dev_densityField = _gpu.Allocate<float>(size);
			dev_bIndexu = _gpu.Allocate<bnd>(size);
			dev_bIndexv = _gpu.Allocate<bnd>(size);
			dev_bIndexd = _gpu.Allocate<bnd>(size);
			dev_uForce = _gpu.Allocate<float>(size);
			dev_vForce = _gpu.Allocate<float>(size);
			dev_sources = _gpu.Allocate<float>(size);

			dev_helpScalers = _gpu.Allocate<float>(size);
			dev_helpScalers2 = _gpu.Allocate<float>(size);

			ClearData();
		}

		public void ClearData()
		{
			int size = (N + 2) * (N + 2);

			_gpu.Set(dev_u);
			_gpu.Set(dev_v);
			_gpu.Set(dev_uForce);
			_gpu.Set(dev_vForce);
			_gpu.Set(dev_densityField);
			_gpu.Set(dev_sources);
			_gpu.Set(dev_helpScalers);
			_gpu.Set(dev_helpScalers2);

			for (int i = 0; i < size; i++)
			{
				uForce[i] = 0; 
				vForce[i] = 0;
				sources[i] = 0f;

				u[i] = 0;
				v[i] = 0;
				densityField[i] = 0;
			}

            FillBoundryIndexes();
		}

        public void FillBoundryIndexes()
        {
			int size = (N + 2) * (N + 2);

			bnd[] bIndexu = new bnd[size];
			bnd[] bIndexv = new bnd[size];
			bnd[] bIndexd = new bnd[size];

            //fill dev_bIndexu (boundary forces on X direction)
            int i, j;
            for (i = 0; i <= N + 1; i++)
            {
                for (j = 0; j <= N + 1; j++)
                {
                    if (i == 0)
                    {
						bIndexu[IX(i, j, N)].res = resolveInvert;
						bIndexu[IX(i, j, N)].source = 3;
                    }
                    if (i == N + 1)
                    {
						bIndexu[IX(i, j, N)].res = resolveInvert;
						bIndexu[IX(i, j, N)].source = 1;

                    }
                    if (j == 0)
                    {
						bIndexu[IX(i, j, N)].res = resolceCopy;
						bIndexu[IX(i, j, N)].source = 2;
                    }
                    if (j == N + 1)
                    {
						bIndexu[IX(i, j, N)].res = resolceCopy;
	                    bIndexu[IX(i, j, N)].source = 4;

                    }
					else bIndexu[IX(i, j, N)].res = resolveEmpty;
                }
            }
            //fill dev_bIndexv (boundary forces on Y direction)
            for (i = 0; i <= N + 1; i++)
            {
                for (j = 0; j <= N + 1; j++)
                {
                    if (j == 0)
                    {
						bIndexv[IX(i, j, N)].res = resolveInvert;
						bIndexv[IX(i, j, N)].source = 2;
                    }
                    if (j == N + 1)
                    {
						bIndexv[IX(i, j, N)].res = resolveInvert;
						bIndexv[IX(i, j, N)].source = 4;

                    }
                    if (i == 0)
                    {
						bIndexv[IX(i, j, N)].res = resolceCopy;
						bIndexv[IX(i, j, N)].source = 3;
                    }
                    if (i == N + 1)
                    {
						bIndexv[IX(i, j, N)].res = resolceCopy;
						bIndexv[IX(i, j, N)].source = 1;

                    }
					else bIndexv[IX(i, j, N)].res = resolveEmpty;
                }
            }
            //fillbIndexd (boundary forces on density field)
            for (i = 0; i <= N + 1; i++)
            {
                for (j = 0; j <= N + 1; j++)
                {
                    if (j == 0)
                    {
						bIndexd[IX(i, j, N)].res = resolceCopy;
						bIndexd[IX(i, j, N)].source = 2;
                    }
                    if (j == N + 1)
                    {
						bIndexd[IX(i, j, N)].res = resolceCopy;
						bIndexd[IX(i, j, N)].source = 4;
                    }
                    if (i == 0)
                    {
						bIndexd[IX(i, j, N)].res = resolceCopy;
						bIndexd[IX(i, j, N)].source = 3;
                    }
                    if (i == N + 1)
                    {
						bIndexd[IX(i, j, N)].res = resolceCopy;
						bIndexd[IX(i, j, N)].source = 1;
                    }
					else bIndexd[IX(i, j, N)].res = resolveEmpty;
                }
            }

	        _gpu.CopyToDevice(bIndexu, dev_bIndexu);
			_gpu.CopyToDevice(bIndexv, dev_bIndexv);
			_gpu.CopyToDevice(bIndexd, dev_bIndexd);
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
				uForce[IX(i, j, N)] = force * difference.X;
				vForce[IX(i, j, N)] = force * difference.Y;
			}


			if (right)
			{
				sources[IX(i, j, N)] = source;
			}

			oldPosition = position;
		}

		#region GPU functions

		private void CopyToGPU()
		{
			_gpu.CopyToDevice(uForce, dev_uForce);
			_gpu.CopyToDevice(vForce, dev_vForce);
			_gpu.CopyToDevice(sources, dev_sources);
		}

		private void CopyFromGPU()
		{
			_gpu.CopyFromDevice(dev_u, u);
			_gpu.CopyFromDevice(dev_v, v);
			_gpu.CopyFromDevice(dev_densityField, densityField);
		}

		[Cudafy]
		private static int CalculateThreadIndexX(GThread thread)
		{
			return thread.threadIdx.x + thread.blockIdx.x * thread.blockDim.x;
		}

		[Cudafy]
		private static int CalculateThreadIndexY(GThread thread)
		{
			return thread.threadIdx.y + thread.blockIdx.y * thread.blockDim.y;
		}

		private dim3 CreateBlockSize2()
		{
			return new dim3(8, 8);
		}

		private dim3 CreateGridSize2()
		{
			int size1 = N + 2;

			return new dim3(size1 / 8 + 1, size1 / 8 + 1);
		}

		private int CreateBlockSize1()
		{
			return 64;
		}

		private int CreateGridSize1()
		{
			int size1 = N + 2;
			int size2 = (size1) * (size1);

			return size2 / 64 + 1;
		}

		#endregion

		#region Density calculations

		[Cudafy]
		public static void AddSource(GThread thread, int N, float dt, float[] x, float[] s)
		{
			int i = CalculateThreadIndexX(thread);
			
			int size = (N + 2)*(N + 2);

			if(i >= size)
				return;

			x[i] += dt*s[i];
		}

		public void Diffuse(float diff, float dt, int b, float[] output, float[] input)
		{
			//int size = (N + 2) * (N + 2);
			//for (int i = 0; i < size; i++) output[i] = input[i];

			float a = dt*diff*N*N;
			LinSolve(b, output, input, a, 1 + 4*a);
		}

		[Cudafy]
		public static void Advect(GThread thread, int N, int b, float dt, float[] output, float[] input, float[] u, float[] v)
		{
			int i, j, i0, j0, i1, j1;
			float s0, t0, s1, t1, dt0, x, y;

			float[] d = output;
			float[] d0 = input;

			i = CalculateThreadIndexX(thread);
			j = CalculateThreadIndexY(thread);

			int size = N + 2;

			if (i >= size)
				return;
			if (j >= size)
				return;

			dt0 = dt*N;
			x = i - dt0 * u[IX(i, j, N)];
			y = j - dt0 * v[IX(i, j, N)];

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
			d[IX(i, j, N)] = s0*(t0*d0[IX(i0, j0, N)] + t1*d0[IX(i0, j1, N)]) +
			                 s1*(t0*d0[IX(i1, j0, N)] + t1*d0[IX(i1, j1, N)]);
		}

		public void CalculateDensity(float dt, float diff)
		{
			_gpu.Launch(CreateGridSize1(), CreateBlockSize1()).AddSource(N, dt, dev_densityField, dev_sources);
			Diffuse(diff, dt, 0, dev_helpScalers, dev_densityField);
			_gpu.Launch(CreateGridSize2(), CreateBlockSize2()).Advect(N, 0, dt, dev_densityField, dev_helpScalers, dev_u, dev_v);
			SetBnd(dev_bIndexd, dev_densityField);
		}

		#endregion

		#region Velocity calculations

		public void Project()
		{
			_gpu.Launch(CreateGridSize2(), CreateBlockSize2()).Project1(N, dev_helpScalers, dev_helpScalers2, dev_u, dev_v);

			SetBnd(dev_bIndexd, dev_helpScalers); 
			SetBnd(dev_bIndexd, dev_helpScalers2);
			LinSolve(0, dev_helpScalers2, dev_helpScalers, 1, 4);

			_gpu.Launch(CreateGridSize2(), CreateBlockSize2()).Project2(N, dev_helpScalers, dev_helpScalers2, dev_u, dev_v);

			SetBnd(dev_bIndexu, dev_u); 
			SetBnd(dev_bIndexv, dev_v);
		}

		[Cudafy]
		public static void Project1(GThread thread, int N, float[] dev_helpScalers, float[] dev_helpScalers2, float[] dev_u, float[] dev_v)
		{
			int i = CalculateThreadIndexX(thread);
			int j = CalculateThreadIndexY(thread);

			int size = N + 2;

			if (i >= size)
				return;
			if (j >= size)
				return;

			dev_helpScalers[IX(i, j, N)] = -0.5f * (dev_u[IX(i + 1, j, N)] - dev_u[IX(i - 1, j, N)] + dev_v[IX(i, j + 1, N)] - dev_v[IX(i, j - 1, N)]) / N;
			dev_helpScalers2[IX(i, j, N)] = 0;
		}

		[Cudafy]
		public static void Project2(GThread thread, int N, float[] dev_helpScalers, float[] dev_helpScalers2, float[] dev_u, float[] dev_v)
		{
			int i = CalculateThreadIndexX(thread);
			int j = CalculateThreadIndexY(thread);

			int size = N + 2;

			if (i >= size)
				return;
			if (j >= size)
				return;

			dev_u[IX(i, j, N)] -= 0.5f * N * (dev_helpScalers2[IX(i + 1, j, N)] - dev_helpScalers2[IX(i - 1, j, N)]);
			dev_v[IX(i, j, N)] -= 0.5f * N * (dev_helpScalers2[IX(i, j + 1, N)] - dev_helpScalers2[IX(i, j - 1, N)]);
		}

		public void CalculateVelocity(float dt, float visc)
		{
			_gpu.Launch(CreateGridSize1(), CreateBlockSize1()).AddSource(N, dt, dev_u, dev_uForce);
			_gpu.Launch(CreateGridSize1(), CreateBlockSize1()).AddSource(N, dt, dev_v, dev_vForce);

			Diffuse(visc, dt, 1, dev_helpScalers, dev_u);
			Diffuse(visc, dt, 2, dev_helpScalers2, dev_v);
			Swap(ref dev_helpScalers, ref dev_u);
			Swap(ref dev_helpScalers2, ref dev_v);
			Project();
			_gpu.Launch(CreateGridSize2(), CreateBlockSize2()).Advect(N, 1, dt, dev_helpScalers, dev_u, dev_u, dev_v);
			_gpu.Launch(CreateGridSize2(), CreateBlockSize2()).Advect(N, 2, dt, dev_helpScalers2, dev_v, dev_u, dev_v);
			Swap(ref dev_helpScalers, ref dev_u);
			Swap(ref dev_helpScalers2, ref dev_v);
			Project();
		}

		#endregion

		#region Linear solver

		private void LinSolve(int b, float[] x, float[] x0, float a, float c)
		{
			int k;

			for (k = 0; k < 20; k++)
			{
				_gpu.Launch(CreateGridSize2(), CreateBlockSize2()).LinSolveStep(N, b, x, x0, a, c);
				SetBnd(dev_bIndexd, x);
			}
		}

		[Cudafy]
		public static void LinSolveStep(GThread thread, int N, int b, float[] x, float[] x0, float a, float c)
		{
			int i, j;

			i = CalculateThreadIndexX(thread);
			j = CalculateThreadIndexY(thread);

			int size = N + 2;

			if (i >= size)
				return;
			if (j >= size)
				return;

			x[IX(i, j, N)] = (x0[IX(i, j, N)] + a * (x[IX(i - 1, j, N)] + x[IX(i + 1, j, N)] + x[IX(i, j - 1, N)] + x[IX(i, j + 1, N)])) / c;
		}

		#endregion

		#region Boundary conditions

		private void SetBnd(bnd[] b, float[] x)
		{
			_gpu.Launch(CreateGridSize2(), CreateBlockSize2()).SetBnd(N, b, x);
		}

		[Cudafy]
		public static void SetBnd(GThread thread, int N, bnd[] b, float[] x)
		{
            int i, j;

			i = CalculateThreadIndexX(thread);
			j = CalculateThreadIndexY(thread);

			int size = N + 2;

			if (i >= size)
				return;
			if (j >= size)
				return;
			
			if(i == 0 && j == 0)
			{
				x[IX(0, 0, N)] = 0.5f * (x[IX(1, 0, N)] + x[IX(0, 1, N)]);
			}
			else if (i == 0 && j == N+1)
			{
				x[IX(0, N + 1, N)] = 0.5f * (x[IX(1, N + 1, N)] + x[IX(0, N, N)]);
			}
			else if (i == N+1 && j == 0)
			{
				x[IX(N + 1, 0, N)] = 0.5f * (x[IX(N, 0, N)] + x[IX(N + 1, 1, N)]);
			}
			else if (i == N+1 && j == N+1)
			{
				x[IX(N + 1, N + 1, N)] = 0.5f * (x[IX(N, N + 1, N)] + x[IX(N + 1, N, N)]);
			}
			else
			{
				if (b[IX(i, j, N)].res == resolceCopy)
				{
					if (b[IX(i, j, N)].source == 1) x[IX(i, j, N)] = x[IX(i - 1, j, N)];
					if (b[IX(i, j, N)].source == 2) x[IX(i, j, N)] = x[IX(i, j + 1, N)];
					if (b[IX(i, j, N)].source == 3) x[IX(i, j, N)] = x[IX(i + 1, j, N)];
					if (b[IX(i, j, N)].source == 4) x[IX(i, j, N)] = x[IX(i, j - 1, N)];
				}
				if (b[IX(i, j, N)].res == resolveInvert)
				{
					if (b[IX(i, j, N)].source == 1) x[IX(i, j, N)] = -x[IX(i - 1, j, N)];
					if (b[IX(i, j, N)].source == 2) x[IX(i, j, N)] = -x[IX(i, j + 1, N)];
					if (b[IX(i, j, N)].source == 3) x[IX(i, j, N)] = -x[IX(i + 1, j, N)];
					if (b[IX(i, j, N)].source == 4) x[IX(i, j, N)] = -x[IX(i, j - 1, N)];
				}
			}
		}

		#endregion

		public void SimulationStep(float dt, float diff, float visc)
		{
			CopyToGPU();
			CalculateVelocity(dt, visc);
			CalculateDensity(dt, diff);
			CopyFromGPU();
		}


		#region Helper methods

		[Cudafy]
		public static int IX(int x, int y, int N)
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

			GL.Color3(1f, 0f, 1f);
			GL.LineWidth(1.0f);

			GL.Begin(PrimitiveType.Lines);

			for (i = 0; i <= N+1; i++)
			{
				x = (i - 0.5f) * h;
				for (j = 0; j <= N+1; j++)
				{
					y = (j - 0.5f) * h;

					GL.Vertex2(x, y);
					GL.Vertex2(x + u[IX(i, j, N)], y + v[IX(i, j, N)]);
				}
			}

			GL.End();

			GL.Begin(PrimitiveType.Quads);
			GL.Color3(0.5f, 0.5f, 0.5f);
			for (i = 0; i <= N+1; i++)
			{
				x = (i - 0.5f) * h;
				for (j = 0; j <= N+1; j++)
				{
					y = (j - 0.5f) * h;
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

					d00 = densityField[IX(i, j, N)];
					d01 = densityField[IX(i, j + 1, N)];
					d10 = densityField[IX(i + 1, j, N)];
					d11 = densityField[IX(i + 1, j + 1, N)];

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
