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
            mouseForce = new MouseSpringForce(null, new HyperPoint<float>(1f, 1f), 0.02f, 100f, 1f,false);

			Matrix<float> translate = Matrix<float>.Translate(-1, -1);
			Matrix<float> resize = Matrix<float>.Resize(new HyperPoint<float>(1f / 320, 1f / 240));
			Matrix<float> mirror = Matrix<float>.Resize(new HyperPoint<float>(1, -1));
			Matrix<float> virtualScreenSizeMatrix = Matrix<float>.Resize(VirtualScreenSize);
			mouseTranslation = virtualScreenSizeMatrix * mirror * translate * resize;

			particles = new List<Particle>();
			drawables = new List<IDrawable>();
			solver = new Solver();
            
			AddParticle(new Particle(new HyperPoint<float>(1f, 0)));
			AddParticle(new Particle(new HyperPoint<float>(0.8f, 0)));
			//AddParticle(new Particle(new HyperPoint<float>(0.25f, 0)));
			
			CreateCloth();
			
            Add(mouseForce);
			
			//Add(new SpringForce(particles[0], particles[1], 0.5f, 1f, 1));
			//Add(new ViscousDragForce(particles, 0.4f));
			Add(new GravityForce(particles, new HyperPoint<float>(0, -0.1f)));

			//Add(new CircularWireConstraint2(particles[0], new HyperPoint<float>(0f, 0f), 1));
			//Add(new CircularWireConstraint2(particles[1], new HyperPoint<float>(0.3f, 0f), 0.5f));

			UpdateIndex();
			ClearData();
		}

		private void UpdateIndex()
		{
			for (int i = 0; i < particles.Count; i++)
			{
				particles[i].Index = i;
			}
		}

		/*
		----------------------------------------------------------------------
		OpenGL specific drawing routines
		----------------------------------------------------------------------
		*/
        private void CreateCloth()
        {
            float dist = 0.3f;
            float tc = 1.1f;
            float sheardist = (float)Math.Sqrt(2*(dist * dist));
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
                    HyperPoint<float> _pos = startpos + offsetx * i + offsety * j;
                    Particle _p = new Particle(_pos, 1);

                    //addparticle
                    clothmesh[i].Add(_p);
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
                        Add(new SpringForce(_p, clothmesh[i - 1][j], dist, 50f, 1f));
                        Add(new RodConstraint(_p, clothmesh[i - 1][j], dist * tc));
                    }
                    if (j > 0)
                    {
                        Add(new SpringForce(_p, clothmesh[i][j - 1], dist, 50f, 1f));
                        Add(new RodConstraint(_p, clothmesh[i][j - 1], dist * tc));
                    }
                    //shear?
                        if (j > 0)
                        {
                            if (i > 0)
                            {
                                Add(new SpringForce(_p, clothmesh[i - 1][j - 1], sheardist, 30, 1f));
                                Add(new RodConstraint(_p, clothmesh[i - 1][j - 1], sheardist * tc));
                            }
                            if (i < clothwidth - 1)
                            {
                                Add(new SpringForce(_p, clothmesh[i + 1][j - 1], sheardist, 30, 1f));
                                Add(new RodConstraint(_p, clothmesh[i + 1][j - 1], sheardist * tc));
                            }
                        }

                    //flexion?
                    if (i > 1) Add(new SpringForce(_p, clothmesh[i - 2][j], 2 * dist, 10f, 1f));
                    if (j > 1) Add(new SpringForce(_p, clothmesh[i][j - 2], 2 * dist, 10f, 1f));

                    
                }
            }
            //create hangup particles
            //Add(new MouseSpringForce(clothmesh[0][0],clothmesh[0][0].Position,0.1f,1000f,30f,true));
            //Add(new MouseSpringForce(clothmesh[clothmesh.Count-1][0], clothmesh[clothmesh.Count-1][0].Position, 0.1f, 1000f, 30f, true));
            Add(new HorizontalWireConstraint(clothmesh[0][0],clothmesh[0][0].Position.Y));
            Add(new HorizontalWireConstraint(clothmesh[Convert.ToInt32(Math.Floor((clothmesh.Count - 1)/2f))][0],clothmesh[0][0].Position.Y));
            Add(new HorizontalWireConstraint(clothmesh[clothmesh.Count - 1][0], clothmesh[0][0].Position.Y));
            foreach(List<Particle> _column in clothmesh)
            {
                foreach (Particle _part in _column)
                {
                    AddParticle(_part);
                }
            }
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

		private void Add(IConstraint2 dp)
		{
			solver.Constraints2.Add(dp);
		}

		private void Add(IDrawableConstraint2 dp)
		{
			Add((IConstraint2)dp);
			Add((IDrawable)dp);
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
				for (int i = 0; i < 10; i++)
				{
					solver.SimulationStep(particles, dt);	
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