using System;
using System.Collections.Generic;
using System.Text;

namespace NonogramGame
{
	class Nonogram
	{
		public bool[,] board;

		public List<int>[] horzHints;
		public List<int>[] vertHints;

		protected static char block = '\u2588';

		public Nonogram(int size)
		{
			board = new bool[size, size];

			Random rand = new Random();

			// Generate Board
			for(int i = 0; i < size; i++)
			{
				for(int j = 0; j < size; j++)
				{
					board[i,j] = Convert.ToBoolean(rand.Next(0, 2));
				}
			}

			// Generate Verical Hints
			vertHints = genVertHints();

			// Generate Horizontal Hints
			horzHints = genHorzHints();
		}

		public virtual List<int>[] genVertHints()
		{
			List<int>[] hints = new List<int>[board.GetLength(0)]; // int[board.GetLength(0)][];

			for(int j = 0; j < board.GetLength(0); j++)
			{
				bool inSeq = false;
				int length = 0;

				List<int> hint = new List<int>();

				for(int i = 0; i < board.GetLength(0); i++)
				{
					if(board[i, j])
					{
						if(!inSeq) inSeq = true;

						length++;
					}
					else if(inSeq)
					{
						inSeq = false;

						hint.Add(length);
						length = 0;
					}
				}

				if(inSeq) hint.Add(length);

				if(hint.Count == 0) hint.Add(0);

				hints[j] = hint;
			}

			return hints;
		}

		public virtual List<int>[] genHorzHints()
		{
			List<int>[] hints = new List<int>[board.GetLength(0)]; //new int[board.GetLength(0)][];

			for(int i = 0; i < board.GetLength(0); i++)
			{
				bool inSeq = false;
				int length = 0;

				List<int> hint = new List<int>();

				for(int j = 0; j < board.GetLength(0); j++)
				{
					if(board[i, j])
					{
						if(!inSeq) inSeq = true;

						length++;
					}
					else if(inSeq)
					{
						inSeq = false;

						hint.Add(length);
						length = 0;
					}
				}

				if(inSeq) hint.Add(length);

				if(hint.Count == 0) hint.Add(0);

				hints[i] = hint;
			}

			return hints;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			for(int i = 0; i < board.GetLength(0); i++)
			{
				// Print Board
				for(int j = 0; j < board.GetLength(0); j++)
				{
					if(board[i, j]) sb.Append(block + "" + block);
					else sb.Append("  ");
				}

				sb.Append(" ");

				// Print Horizontal Hints
				for(int j = 0; j < horzHints[i].Count; j++)
				{
					sb.Append(horzHints[i][j] + " ");
				}

				sb.AppendLine();
			}

			// Print Vertical Hints
			bool didPrint;
			int k = 0;
			do
			{
				didPrint = false;

				for(int i = 0; i < board.GetLength(0); i++)
				{
					if(k < vertHints[i].Count)
					{
						if(vertHints[i][k] < 10) sb.Append(" ");
						sb.Append(vertHints[i][k]);

						didPrint = true;
					}
					else sb.Append("  ");
				}

				sb.AppendLine();

				k++;
			}
			while(didPrint);

			return sb.ToString();
		}
	}
}
