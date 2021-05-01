using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Input;

namespace NonogramGame
{
	class Program
	{
		static void Main(string[] args)
		{
			int size = 20;

			while(true)
			{
				SolverNonogram nonogram = new SolverNonogram(size);
				Stopwatch stopwatch = new Stopwatch();

				Console.Clear();
				PrintNonogram(nonogram);
				Console.ReadKey();

				stopwatch.Start();

				while(true)
				{
					PrintNonogram(nonogram);
					if(nonogram.IsSolved()) break;
					nonogram.StepSolve();
				}

				stopwatch.Stop();

				Console.WriteLine("Time (s): " + ((double)stopwatch.ElapsedMilliseconds)/1000.0);
				var key = Console.ReadKey();
				if(key.Key == ConsoleKey.Escape) break;
			}
		}
		
		static void HumanGame(int size)
		{
			PlayerNonogram nonogram = new PlayerNonogram(size);

			int x = 0, y = 0;

			//bool stateChange = true;

			while(true)
			{
				PrintNonogram(nonogram);
				// stateChange = false;
				
				if(nonogram.IsSolved()) break;

				Console.SetCursorPosition(2 * x + 2*(x / 5) + 3, y + (y / 5) + 1);

				ConsoleKeyInfo key = Console.ReadKey();

				// Movement
				switch(key.Key)
				{
					case ConsoleKey.RightArrow:
						if(x < size - 1) x++;
						break;
					case ConsoleKey.LeftArrow:
						if(x > 0) x--;
						break;
					case ConsoleKey.DownArrow:
						if(y < size - 1) y++;
						break;
					case ConsoleKey.UpArrow:
						if(y > 0) y--;
						break;
				}

				// Mark / Cross / Clear
				switch(key.Key)
				{
					case ConsoleKey.Z:
						nonogram.Mark(x, y);
						goto case ConsoleKey.PrintScreen;
					case ConsoleKey.X:
						nonogram.Cross(x, y);
						goto case ConsoleKey.PrintScreen;
					case ConsoleKey.C:
						nonogram.Clear(x, y);
						goto case ConsoleKey.PrintScreen;
					case ConsoleKey.PrintScreen:
						//stateChange = true;
						break;
				}
			}

			Console.WriteLine("SOLVED!!!");
			Console.ReadKey();
		}

		static void PrintNonogram(Nonogram nonogram)
		{
			// Update the console
			Console.CursorVisible = false;
			Console.SetCursorPosition(0, 0);
			//Console.Clear();
			Console.Write(nonogram.ToString());
			//Console.CursorVisible = true;
		}
	}
}
