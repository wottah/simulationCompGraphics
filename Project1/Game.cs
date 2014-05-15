using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using micfort.GHL.Math2;

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
		private Solver solver;
        private MouseSpringForce mouseForce;
        private Matrix<float> mouseTranslation;

		private int win_id;
		private int win_x, win_y;
		private int[] mouse_down;
		private int[] mouse_release;
		private int[] mouse_shiftclick;
		private int omx, omy, mx, my;
		private int hmx, hmy;
		private List<IDrawable> drawables;

		private HyperPoint<float> VirtualScreenSize;


		/*
		----------------------------------------------------------------------
		free/clear/allocate simulation data
		----------------------------------------------------------------------
		*/

		private void ClearData()
		{
			particles.ForEach(x => x.reset());
		}

		private void InitSystem()
		{
			VirtualScreenSize = new HyperPoint<float>(2, 2);

			float dist = 0.2f;
			HyperPoint<float> center = new HyperPoint<float>(0, 0.5f);
			HyperPoint<float> offset = new HyperPoint<float>(dist, 0.0f);
            mouseForce = new MouseSpringForce(null, new HyperPoint<float>(1f, 1f), 0.02f, 100f, 1f);

			Matrix<float> translate = Matrix<float>.Translate(-1, -1);
			Matrix<float> resize = Matrix<float>.Resize(new HyperPoint<float>(1f / 320, 1f / 240));
			Matrix<float> mirror = Matrix<float>.Resize(new HyperPoint<float>(1, -1));
			Matrix<float> virtualScreenSizeMatrix = Matrix<float>.Resize(VirtualScreenSize);
			mouseTranslation = virtualScreenSizeMatrix * mirror * translate * resize;

			particles = new List<Particle>();
			drawables = new List<IDrawable>();
			solver = new Solver();

			AddParticle(new Particle(center + offset*0));
			AddParticle(new Particle(center + offset*1));
			AddParticle(new Particle(center + offset*2));

			Add(new GravityForce(particles, new HyperPoint<float>(0f, -1f)));
			
			Add(new SpringForce(particles[0],particles[1],0.5f,1f,1));
            Add(mouseForce);
			
			Add(new SpringForce(particles[0], particles[1], 0.5f, 1f, 1));
			Add(new SpringForce(particles[0], particles[2], 0.3f, 1f, 1));
			Add(new SpringForce(particles[1], particles[2], 0.7f, 1f, 1));
			Add(new ViscousDragForce(particles, 0.4f));

			Add(new CircularWireConstraint(particles[0], new HyperPoint<float>(0f, 0f), 1));

			ClearData();
		}

		/*
		----------------------------------------------------------------------
		OpenGL specific drawing routines
		----------------------------------------------------------------------
		*/

		private void PreDisplay()
		{
			GL.Viewport(0, 0, win_x, win_y);
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
				if((frame_number % FrameInterval) == 0)
				{
					using(Bitmap bmp = new Bitmap(Width, Height))
					{
						System.Drawing.Imaging.BitmapData data =
							bmp.LockBits(this.ClientRectangle, System.Drawing.Imaging.ImageLockMode.WriteOnly,
										 System.Drawing.Imaging.PixelFormat.Format24bppRgb);
						GL.ReadPixels(0, 0, Width, Height, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
						bmp.UnlockBits(data);

						bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

						if (!Directory.Exists("snapshots"))
							Directory.CreateDirectory("snapshots");

						string filename = string.Format("snapshots/img{0}.png", Convert.ToSingle(frame_number)/FrameInterval);
						bmp.Save(filename);
						Console.Out.WriteLine("Output snapshot: {0}", Convert.ToSingle(frame_number) / FrameInterval);
					}
				}
			}
			frame_number++;

			SwapBuffers();
		}

		private void DrawDrawables()
		{
			drawables.ForEach(x => x.Draw());
		}
		
		/*
		----------------------------------------------------------------------
		relates mouse movements to tinker toy construction
		----------------------------------------------------------------------
		*/

		/*
		----------------------------------------------------------------------
		Helper methods
		----------------------------------------------------------------------
		*/

		private void AddParticle(Particle p)
		{
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
			solver.Forces.Add(dp);
		}

		private void Add(IDrawableConstraint dp)
		{
			Add((IConstraint) dp);
			Add((IDrawable) dp);
		}

		private void Add(IConstraint dp)
		{
			solver.Constraints.Add(dp);
		}

		/*
		----------------------------------------------------------------------
		callback routines
		----------------------------------------------------------------------
		*/


		private void OnLoad(object sender, EventArgs eventArgs)
		{
			// setup settings, load textures, sounds
			VSync = VSyncMode.On;

			GL.Enable(EnableCap.LineSmooth); 
			GL.Enable(EnableCap.PolygonSmooth);
		}

		private void OnResize(object sender, EventArgs eventArgs)
		{
			GL.Viewport(0, 0, Width, Height);
			win_x = Width;
			win_y = Height;
		}

		private void OnRenderFrame(object sender, FrameEventArgs frameEventArgs)
		{
			PreDisplay();

			DrawDrawables();
			
			PostDisplay();
		}

		private void OnUpdateFrame(object sender, FrameEventArgs frameEventArgs)
		{
			if(dsim)
			{
				mouseForce.MousePos = ((HyperPoint<float>)(mouseTranslation * new HyperPoint<float>(Mouse.X, Mouse.Y, 1))).GetLowerDim(2);
				solver.SimulationStep(particles, dt);
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
            float _dist = 0.02f;
			HyperPoint<float> _mousepos = ((HyperPoint<float>)(mouseTranslation * new HyperPoint<float>(e.X, e.Y, 1))).GetLowerDim(2);
            foreach (Particle _p in particles)
            {
				if ((_mousepos - _p.Position).GetLengthSquared() < _dist * _dist)
				{
					mouseForce.MousePos = _mousepos;
                    mouseForce.Particle = _p;
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
			}
		}


		public Game(int n, float dt, float d)
		{
			this.N = n;
			this.dt = dt;
			this.d = d;

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
		}
    }
}