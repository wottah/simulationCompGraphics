using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Project1
{
	class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			int N;
			float dt;
			float d;
			if (args.Length == 0)
			{
				N = 64;
				dt = 0.001f;
				d = 5.0f;
				
			}
			else
			{
				N = int.Parse(args[0]);
				dt = int.Parse(args[1]);
				d = int.Parse(args[2]);
			}
			Console.Out.WriteLine("Using parameters : N={0} dt={1} d={2}\n", N, dt, d);
			using (var game = new Game(N, dt, d))
			{
				// Run the game at 60 updates per second
				game.Run(200);
			}
		}
	}
}
