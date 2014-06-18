using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Project2.Particles;
using micfort.GHL.Math2;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Project2
{
	class Game: GameWindow
	{
		private float dt;
		private bool dsim;
		private bool dump_frames;
		private int frame_number;

        private Matrix<float> mouseTranslation;

		private string systemName = string.Empty;
		private List<IDrawable> drawables;
		private float SpeedUp = 0.25f;

		private HyperPoint<float> VirtualScreenSize1;
		private HyperPoint<float> VirtualScreenSize2;

		private LiquidSystem liqSystem;
		private MousePointer mousePointer;
		private List<Particle> particles;
		private SolverEnvironment _solverEnvironment;
		private ContinuousField _uField;
		private ContinuousField _vField;
		private ContinuousField _dField;


		/*
		----------------------------------------------------------------------
		free/clear/allocate simulation data
		----------------------------------------------------------------------
		*/

		#region free/clear/allocate simulation data

		private void ClearData()
		{
			liqSystem.ClearData();
			particles.ForEach(x => x.reset());
		}

		private void InitSystem()
		{
			dsim = false;
			dump_frames = false;

			drawables = new List<IDrawable>();
			particles = new List<Particle>();

			_solverEnvironment = new SolverEnvironment();
			_solverEnvironment.Solver = new EulerSolver();

			liqSystem = new LiquidSystem();
			liqSystem.AllocateData();
			liqSystem.Visualization = Visualization.Both;
			Add(liqSystem);

			_uField = new ContinuousField(liqSystem.N+2, liqSystem.N+2);
			_vField = new ContinuousField(liqSystem.N+2, liqSystem.N+2);
			_dField = new ContinuousField(liqSystem.N + 2, liqSystem.N + 2);

			AddParticle(new Particle(new HyperPoint<float>(0.5f, 0.5f), 1, new List<HyperPoint<float>>()
				                                                               {
					                                                               new HyperPoint<float>(0.05f, 0.05f),
																				   new HyperPoint<float>(-0.05f, 0.05f),
																				   new HyperPoint<float>(-0.05f, -0.05f),
																				   new HyperPoint<float>(0.05f, -0.05f),
				                                                               }));

			_solverEnvironment.Forces.Add(new VelocityFieldForce(particles.ConvertAll(x => x.Index), _uField, _vField, _dField, liqSystem.N + 2));
			_solverEnvironment.Forces.Add(new ViscousDragForce(particles.ConvertAll(x => x.Index), 0.8f));

			mousePointer = new MousePointer();
			Add(mousePointer);

			ClearData();
			UpdateHeader();
		}

		private void UpdateHeader()
		{
			this.Title = string.Format("{2} system (dt={0}, speedup={1})", dt, SpeedUp, systemName);
		}

		private void UpdateMouseMatrix()
		{
			Matrix<float> resize = Matrix<float>.Resize(new HyperPoint<float>(1f / Width, 1f / Height));
			Matrix<float> virtualScreenResize = Matrix<float>.Resize(VirtualScreenSize2 - VirtualScreenSize1);
			Matrix<float> translate = Matrix<float>.Translate(VirtualScreenSize1);
			Matrix<float> mirror = Matrix<float>.Resize(new HyperPoint<float>(1, -1));
			Matrix<float> translate2 = Matrix<float>.Translate(0, VirtualScreenSize2.Y + VirtualScreenSize1.Y);
			mouseTranslation = translate2 * mirror * translate * virtualScreenResize * resize;
		}

		private void UpdateVirtualScreenSize()
		{
			float MinSize = 1f;
			float margin = 0.1f;
			VirtualScreenSize1 = new HyperPoint<float>(-margin, -margin);
			if (MinSize / Width * Height < MinSize)
			{
				VirtualScreenSize2 = new HyperPoint<float>(MinSize / Height * Width + margin, MinSize + margin);
			}
			else
			{
				VirtualScreenSize2 = new HyperPoint<float>(MinSize + margin, MinSize / Width * Height + margin);
			}
			
			UpdateMouseMatrix();
		}

		#endregion

		#region OpenGL specific drawing routines

		private void PreDisplay()
		{
			GL.Viewport(0, 0, Width, Height);
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Ortho(VirtualScreenSize1.X, VirtualScreenSize2.X, VirtualScreenSize1.Y, VirtualScreenSize2.Y, -1, 1);
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
						string dirName = "snapshots_" + systemName;
						if (!Directory.Exists(dirName))
							Directory.CreateDirectory(dirName);

						string filename = string.Format(dirName+"/img{0}.png", Convert.ToSingle(frame_number)/FrameInterval);
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

		private void Add(IDrawable p)
		{
			drawables.Add(p);
		}

		private void AddParticle(Particle p)
		{
			p.Index = particles.Count;
			particles.Add(p);
			Add(p);
		}

		#endregion

		#region callback routines

		private void OnLoad(object sender, EventArgs eventArgs)
		{
			// setup settings, load textures, sounds
			VSync = VSyncMode.On;

			UpdateVirtualScreenSize();
			GL.Enable(EnableCap.LineSmooth);
			GL.Enable(EnableCap.PolygonSmooth);
		}

		private void OnResize(object sender, EventArgs eventArgs)
		{
			UpdateVirtualScreenSize();
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
				HyperPoint<float> mousePos = ((HyperPoint<float>)(mouseTranslation * new HyperPoint<float>(Mouse.X, Mouse.Y, 1))).GetLowerDim(2);
				
				int steps;				
				steps = Math.Max(1, Convert.ToInt32(Math.Round(1 / dt * SpeedUp / 60.0f)));
				if (dump_frames)
					steps = 1;
				for (int i = 0; i < steps; i++)
				{
					liqSystem.FillBoundryIndexes();
					particles.ForEach(liqSystem.AddBoundries);
					liqSystem.SquareBoundryInternal(0, 0, liqSystem.N + 2, liqSystem.N + 2);

					liqSystem.UI(mousePos, Mouse[MouseButton.Left], Mouse[MouseButton.Right], 5.0f, 100.0f);
					liqSystem.SimulationStep(dt, 0.000f, 0.000f);
					_uField._data = liqSystem.u;
					_vField._data = liqSystem.v;
					_dField._data = liqSystem.densityField;
					_solverEnvironment.SimulationStep(particles, dt);
					//Console.Out.WriteLine(new List<float>(liqSystem.densityField).Sum());
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
			
		}

		private void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			HyperPoint<float> _mousepos =
				((HyperPoint<float>) (mouseTranslation*new HyperPoint<float>(e.X, e.Y, 1))).GetLowerDim(2);
			

		}

		private void OnMouseMove(object sender, MouseMoveEventArgs e)
		{
			mousePointer.MousePos = ((HyperPoint<float>)(mouseTranslation * new HyperPoint<float>(e.X, e.Y, 1))).GetLowerDim(2);
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
					liqSystem.Visualization = Visualization.Density;
					break;
				case Key.Number2:
					liqSystem.Visualization = Visualization.Velocity;
					break;
				case Key.Number3:
					liqSystem.Visualization = Visualization.Both;
					break;	
			}
		}

		#endregion



		public Game(float dt)
		{
			this.dt = dt;

			dsim = false;
			dump_frames = false;
			frame_number = 0;

			InitSystem();

			this.Load += OnLoad;
			this.Resize += OnResize;
			this.UpdateFrame += OnUpdateFrame;
			this.RenderFrame += OnRenderFrame;
			this.KeyDown += OnKeyDown;
			this.KeyUp += OnKeyUp;
            this.Mouse.ButtonDown += OnMouseDown;
            this.Mouse.ButtonUp += OnMouseUp;
			this.Mouse.Move += OnMouseMove;
		}
    }
}