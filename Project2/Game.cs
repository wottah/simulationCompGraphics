using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
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

		private HyperPoint<float> VirtualScreenSize;

		private LiquidSystem system;


		/*
		----------------------------------------------------------------------
		free/clear/allocate simulation data
		----------------------------------------------------------------------
		*/

		#region free/clear/allocate simulation data

		private void ClearData()
		{
			system.ClearData();
		}

		private void InitSystem()
		{
			dsim = false;
			dump_frames = false;

			drawables = new List<IDrawable>();

			system = new LiquidSystem();
			system.AllocateData();
			system.ClearData();
			system.Visualization = Visualization.Velocity;
			Add(system);

			ClearData();
			UpdateHeader();
		}

		private void UpdateHeader()
		{
			this.Title = string.Format("{2} system (dt={0}, speedup={1})", dt, SpeedUp, systemName);
		}

		private void UpdateMouseMatrix()
		{
			Matrix<float> translate = Matrix<float>.Translate(-1, -1);
			Matrix<float> resize = Matrix<float>.Resize(new HyperPoint<float>(1f / (Width / 2), 1f / (Height / 2)));
			Matrix<float> mirror = Matrix<float>.Resize(new HyperPoint<float>(1, -1));
			Matrix<float> virtualScreenSizeMatrix = Matrix<float>.Resize(VirtualScreenSize);
			mouseTranslation = virtualScreenSizeMatrix * mirror * translate * resize;
		}

		private void UpdateVirtualScreenSize()
		{
			float MinSize = 1f;
			if (MinSize / Width * Height < MinSize)
			{
				VirtualScreenSize = new HyperPoint<float>(MinSize / Height * Width, MinSize);
			}
			else
			{
				VirtualScreenSize = new HyperPoint<float>(MinSize, MinSize / Width * Height);
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
			GL.Ortho(0, VirtualScreenSize.X, 0, VirtualScreenSize.Y, -1, 1);
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
			drawables.ForEach(x => x.Draw());
		}

		#endregion

		#region Helper methods

		private void Add(IDrawable p)
		{
			drawables.Add(p);
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
				int steps;
				//mouseForce.MousePos =
				//	((HyperPoint<float>) (mouseTranslation*new HyperPoint<float>(Mouse.X, Mouse.Y, 1))).GetLowerDim(2);
				//steps = Math.Max(1, Convert.ToInt32(Math.Round(1 / dt * SpeedUp / 60.0f)));
				//if (dump_frames)
					steps = 1;
				for (int i = 0; i < steps; i++)
				{
					system.CalculateDensity(dt, 0f);
					system.CalculateVelocity(dt, 0f);
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

		private void OnKeyUp(object sender, KeyboardKeyEventArgs keyboardKeyEventArgs)
		{
			switch (keyboardKeyEventArgs.Key)
			{
				case Key.Z:
					int pos = system.sources.Length / 2;
					system.sources[pos] = 0;
					system.uForce[pos] = 0;
					system.vForce[pos] = 0;
					break;
			}
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

				case Key.Z:
					int pos = system.sources.Length/2;
					system.sources[pos] = 100f;
					system.uForce[pos] = 20;
					system.vForce[pos] = 20;
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
					system.Visualization = Visualization.Density;
					break;
				case Key.Number2:
					system.Visualization = Visualization.Velocity;
					break;
				case Key.Number3:
					system.Visualization = Visualization.Both;
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
		}
    }
}