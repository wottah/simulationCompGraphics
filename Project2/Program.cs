using System;

namespace Project2
{
	class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			using (var game = new Game(0.1f))
			{
				// Run the game at 60 updates per second
				game.Run(60.0);
			}
		}
	}
}
