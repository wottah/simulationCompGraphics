using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using micfort.GHL.Math2;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Project1
{
	class Game: GameWindow
	{
		private int N;
		private float dt, d;
		private bool dsim;
		private bool dump_frames;
		private int frame_number;

		// static Particle *pList;
		private List<Particle> particles;
		private SolverEnvironment _solverEnvironment;
        private MouseSpringForce mouseForce;
        private Matrix<float> mouseTranslation;

		private string systemName = string.Empty;
		private List<IDrawable> drawables;
		private float SpeedUp = 0.25f;

		private HyperPoint<float> VirtualScreenSize;


		/*
		----------------------------------------------------------------------
		free/clear/allocate simulation data
		----------------------------------------------------------------------
		*/

		#region free/clear/allocate simulation data

		private void ClearData()
		{
			particles.ForEach(x => x.reset());
		}

		private void InitSystem(Action createSystem)
		{
			dsim = false;
			dump_frames = false;

			particles = new List<Particle>();
			drawables = new List<IDrawable>();
			
			_solverEnvironment = new SolverEnvironment();
			_solverEnvironment.Solver = new EulerSolver();

			mouseForce = new MouseSpringForce(0, new HyperPoint<float>(1f, 1f), 0.02f, 100f, 1f, false);
			Add(mouseForce);

			createSystem();

			ClearData();
			UpdateHeader();
		}

		private void CreateParticleScene()
		{
			systemName = "Particle";

			AddParticle(new Particle(new HyperPoint<float>(1f, 0)) { Color = new HyperPoint<float>(0, 0, 1) });
			AddParticle(new Particle(new HyperPoint<float>(0.8f, 0)) { Color = new HyperPoint<float>(0, 1, 1) });
			AddParticle(new Particle(new HyperPoint<float>(0.25f, 0)) { Color = new HyperPoint<float>(1, 0, 1) });
			AddParticle(new Particle(new HyperPoint<float>(0, 1f)));
			AddParticle(new Particle(new HyperPoint<float>(0, 0.75f)));

			List<int> particleIndexes = particles.ConvertAll(x => x.Index);

			Add(new SpringForce(0, 1, 0.5f, 1f, 1));
			Add(new SpringForce(1, 2, 0.5f, 1f, 1));
			Add(new ViscousDragForce(particleIndexes, 0.4f));
			Add(new GravityForce(particleIndexes, new HyperPoint<float>(0, -10f)));

			Add(new CircularWireConstraint(0, new HyperPoint<float>(0f, 0f), 1));
			Add(new CircularWireConstraint(1, new HyperPoint<float>(0.3f, 0f), 0.5f));
			Add(new HorizontalWireConstraint(3, 1f));
			Add(new RodConstraint(3, 4, 0.25f));
		}

		private void CreateCloth()
		{
			systemName = "Cloth";

			float dist = 0.3f;
			float tc = 1.1f;
			float sheardist = (float) Math.Sqrt(2*(dist*dist));
			int clothwidth = 8;
			int clothheigth = clothwidth;
			HyperPoint<float> offsetx = new HyperPoint<float>(dist, 0.0f);
			HyperPoint<float> offsety = new HyperPoint<float>(0.0f, -dist);
			HyperPoint<float> startpos = new HyperPoint<float>(-clothwidth*(dist/2), 1.5f);
			//create clothmesh and embedded lists
			List<List<Particle>> clothmesh = new List<List<Particle>>();
			for (int ii = 0; ii < clothwidth; ii++) clothmesh.Insert(ii, new List<Particle>());
			for (int i = 0; i < clothwidth; i++)
			{
				for (int j = 0; j < clothheigth; j++)
				{
					HyperPoint<float> _pos = startpos + offsetx*i + offsety*j;
					Particle _p = new Particle(_pos, 1);

					//addparticle
					clothmesh[i].Add(_p);
					AddParticle(_p);
				}
			}

			for (int i = 0; i < clothwidth; i++)
			{
				for (int j = 0; j < clothheigth; j++)
				{
					Particle _p = clothmesh[i][j];
					//add forces!
					//structural?
					if (i > 0)
					{
						Add(new SpringForce(_p.Index, clothmesh[i - 1][j].Index, dist, 50f, 1f));
						Add(new RodConstraint(_p.Index, clothmesh[i - 1][j].Index, dist*tc));
					}
					if (j > 0)
					{
						Add(new SpringForce(_p.Index, clothmesh[i][j - 1].Index, dist, 50f, 1f));
						Add(new RodConstraint(_p.Index, clothmesh[i][j - 1].Index, dist*tc));
					}
					//shear?
					if (j > 0)
					{
						if (i > 0)
						{
							Add(new SpringForce(_p.Index, clothmesh[i - 1][j - 1].Index, sheardist, 30, 1f));
							Add(new RodConstraint(_p.Index, clothmesh[i - 1][j - 1].Index, sheardist*tc));
						}
						if (i < clothwidth - 1)
						{
							Add(new SpringForce(_p.Index, clothmesh[i + 1][j - 1].Index, sheardist, 30, 1f));
							Add(new RodConstraint(_p.Index, clothmesh[i + 1][j - 1].Index, sheardist*tc));
						}
					}

					//flexion?
					if (i > 1) Add(new SpringForce(_p.Index, clothmesh[i - 2][j].Index, 2*dist, 10f, 1f));
					if (j > 1) Add(new SpringForce(_p.Index, clothmesh[i][j - 2].Index, 2*dist, 10f, 1f));
				}
			}
			//create hangup particles
			//Add(new MouseSpringForce(clothmesh[0][0],clothmesh[0][0].Position,0.1f,1000f,30f,true));
			//Add(new MouseSpringForce(clothmesh[clothmesh.Count-1][0], clothmesh[clothmesh.Count-1][0].Position, 0.1f, 1000f, 30f, true));
			Add(new HorizontalWireConstraint(clothmesh[0][0].Index, clothmesh[0][0].Position.Y));
			Add(new HorizontalWireConstraint(clothmesh[Convert.ToInt32(Math.Floor((clothmesh.Count - 1)/2f))][0].Index,
			                                 clothmesh[0][0].Position.Y));
			Add(new HorizontalWireConstraint(clothmesh[clothmesh.Count - 1][0].Index, clothmesh[0][0].Position.Y));
		}

		private void CreateHair()
		{
			systemName = "Hair";

			double beta = Math.PI/2;
			HyperPoint<float> startPosition = new HyperPoint<float>(0, 1f);
			HyperPoint<float> unitSize = new HyperPoint<float>(0, -0.2f);
			HyperPoint<float> diffLeftRight = new HyperPoint<float>(Convert.ToSingle(Math.Cos(beta/2)*unitSize.GetLength()), 0f);
			float springDist = Convert.ToSingle(2*unitSize.GetLength()*Math.Sin(beta));
			int particleCount = 10;
			List<Particle> hair = new List<Particle>();
			for (int i = 0; i < particleCount; i++)
			{
				Particle p = new Particle(startPosition + i*unitSize);
				AddParticle(p);
				hair.Add(p);
			}

			for (int i = 0; i < particleCount-2; i++)
			{
				SpringForce sp = new SpringForce(i, i + 2, springDist, 5000, 1);
				Add(sp);
			}
			for (int i = 0; i < particleCount - 1; i++)
			{
				RodConstraint rc = new RodConstraint(i, i + 1, unitSize.GetLength());
				Add(rc);
			}
			for (int i = 0; i < particleCount; i++)
			{
				if(i % 2 == 0)
				{
					hair[i].ConstructPos = hair[i].ConstructPos + diffLeftRight/2;
				}
				else
				{
					hair[i].ConstructPos = hair[i].ConstructPos - diffLeftRight/2;
				}
				hair[i].Position = hair[i].ConstructPos;
			}

			Add(new CircularWireConstraint(hair[0].Index, startPosition, 0));

			List<int> particleIndexes = hair.ConvertAll(x => x.Index);
			Add(new ViscousDragForce(particleIndexes, 1f));
			Add(new GravityForce(particleIndexes, new HyperPoint<float>(0, -0.1f)));
		}

		private void UpdateHeader()
		{
			this.Title = string.Format("{3} system (dt={0}, solver={1}, speedup={2})", dt, _solverEnvironment.Solver.Name,
			                           SpeedUp, systemName);
		}

		private void UpdateMouseMatrix()
		{
			Matrix<float> translate = Matrix<float>.Translate(-1, -1);
			Matrix<float> resize = Matrix<float>.Resize(new HyperPoint<float>(1f / (Width / 2), 1f / (Height / 2)));
			Matrix<float> mirror = Matrix<float>.Resize(new HyperPoint<float>(1, -1));
			Matrix<float> virtualScreenSizeMatrix = Matrix<float>.Resize(VirtualScreenSize);
			mouseTranslation = virtualScreenSizeMatrix * mirror * translate * resize;
		}


		#endregion

		#region OpenGL specific drawing routines

		private void PreDisplay()
		{
			GL.Viewport(0, 0, Width, Height);
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Ortho(-VirtualScreenSize.X, VirtualScreenSize.X, -VirtualScreenSize.Y, VirtualScreenSize.Y, -1, 1);
			GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
			GL.Clear(ClearBufferMask.ColorBufferBit);
		}

		private void PostDisplay()
		{
			// Write frames if necessary.
			if (dump_frames)
			{
				const int FrameInterval = 1;
				if ((frame_number%FrameInterval) == 0)
				{
					using (Bitmap bmp = new Bitmap(Width, Height))
					{
						BitmapData data =
							bmp.LockBits(this.ClientRectangle, ImageLockMode.WriteOnly,
							             PixelFormat.Format24bppRgb);
						GL.ReadPixels(0, 0, Width, Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
						bmp.UnlockBits(data);

						bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

						if (!Directory.Exists("snapshots"))
							Directory.CreateDirectory("snapshots");

						string filename = string.Format("snapshots/img{0}.png", Convert.ToSingle(frame_number)/FrameInterval);
						bmp.Save(filename);
						Console.Out.WriteLine("Output snapshot: {0}", Convert.ToSingle(frame_number)/FrameInterval);
					}
				}
			}
			frame_number++;

			SwapBuffers();
		}

		private void DrawDrawables()
		{
			drawables.ForEach(x => x.Draw(particles));
		}

		#endregion

		#region Helper methods

		private void AddParticle(Particle p)
		{
			p.Index = particles.Count;
			particles.Add(p);
			Add(p);
		}

		private void Add(IDrawable p)
		{
			drawables.Add(p);
		}

		private void Add(IDrawableForce dp)
		{
			Add((IDrawable) dp);
			Add((IForce) dp);
		}

		private void Add(IForce dp)
		{
			_solverEnvironment.Forces.Add(dp);
		}

		private void Add(IDrawableConstraint dp)
		{
			Add((IConstraint) dp);
			Add((IDrawable) dp);
		}

		private void Add(IConstraint dp)
		{
			_solverEnvironment.Constraints.Add(dp);
		}

		#endregion

		#region callback routines

		private void OnLoad(object sender, EventArgs eventArgs)
		{
			// setup settings, load textures, sounds
			VSync = VSyncMode.On;

			VirtualScreenSize = new HyperPoint<float>(2, 2);

			UpdateMouseMatrix();

			GL.Enable(EnableCap.LineSmooth);
			GL.Enable(EnableCap.PolygonSmooth);
		}

		private void OnResize(object sender, EventArgs eventArgs)
		{
			UpdateMouseMatrix();
		}

		private void OnRenderFrame(object sender, FrameEventArgs frameEventArgs)
		{
			PreDisplay();

			DrawDrawables();

			PostDisplay();
		}

		private void OnUpdateFrame(object sender, FrameEventArgs frameEventArgs)
		{
			if (dsim)
			{
				mouseForce.MousePos =
					((HyperPoint<float>) (mouseTranslation*new HyperPoint<float>(Mouse.X, Mouse.Y, 1))).GetLowerDim(2);
				int steps = Math.Max(1, Convert.ToInt32(Math.Round(1 / dt * SpeedUp)));
				for (int i = 0; i < steps; i++)
				{
					_solverEnvironment.SimulationStep(particles, dt);
				}
			}
			else
			{
				//todo reset
			}

			// add game logic, input handling
			if (Keyboard[Key.Escape])
			{
				Exit();
			}
		}

		private void OnMouseUp(object sender, MouseButtonEventArgs e)
		{
			mouseForce.Disable();
		}

		private void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			float _dist = 0.06f;
			HyperPoint<float> _mousepos =
				((HyperPoint<float>) (mouseTranslation*new HyperPoint<float>(e.X, e.Y, 1))).GetLowerDim(2);
			foreach (Particle _p in particles)
			{
				if ((_mousepos - _p.Position).GetLengthSquared() < _dist*_dist)
				{
					mouseForce.MousePos = _mousepos;
					mouseForce.Particle = _p.Index;
					mouseForce.Enable();
				}
			}

		}

		private void OnKeyUp(object sender, KeyboardKeyEventArgs keyboardKeyEventArgs)
		{

		}

		private void OnKeyDown(object sender, KeyboardKeyEventArgs keyboardKeyEventArgs)
		{
			switch (keyboardKeyEventArgs.Key)
			{
				case Key.C:
					ClearData();
					break;

				case Key.D:
					dump_frames = !dump_frames;
					break;

				case Key.Q:
					Exit();
					break;

				case Key.Space:
					dsim = !dsim;
					break;
				
				case Key.Up:
					dt *= 2;
					UpdateHeader();
					break;

				case Key.Down:
					dt /= 2;
					UpdateHeader();
					break;

				case Key.Left:
					SpeedUp /= 2;
					UpdateHeader();
					break;

				case Key.Right:
					SpeedUp *= 2;
					UpdateHeader();
					break;

				case Key.Number1:
					_solverEnvironment.Solver = new EulerSolver();
					UpdateHeader();
					break;
				case Key.Number2:
					_solverEnvironment.Solver = new MidPointSolver();
					UpdateHeader();
					break;
				case Key.Number3:
					_solverEnvironment.Solver = new RKSolver();
					UpdateHeader();
					break;
				case Key.Number4:
					_solverEnvironment.Solver = new VerletSolver();
					UpdateHeader();
					break;

				case Key.F1:
					InitSystem(CreateParticleScene);
					break;
				case Key.F2:
					InitSystem(CreateCloth);
					break;
				case Key.F3:
					InitSystem(CreateHair);
					break;
			}
		}

		#endregion



		public Game(int n, float dt, float d)
		{
			this.N = n;
			this.dt = dt;
			this.d = d;

			dsim = false;
			dump_frames = false;
			frame_number = 0;

			InitSystem(CreateParticleScene);

			this.Load += OnLoad;
			this.Resize += OnResize;
			this.UpdateFrame += OnUpdateFrame;
			this.RenderFrame += OnRenderFrame;
			this.KeyDown += OnKeyDown;
			this.KeyUp += OnKeyUp;
            this.Mouse.ButtonDown += OnMouseDown;
            this.Mouse.ButtonUp += OnMouseUp;
		}
    }
}