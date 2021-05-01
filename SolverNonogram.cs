using System;
using System.Collections.Generic;
using System.Text;

namespace NonogramGame
{
	class SolverNonogram : PlayerNonogram
	{
		Stack<Tuple<bool[,], bool[,], int, int>> stateStack;
		
		public SolverNonogram(int size) : base(size)
		{
			stateStack = new Stack<Tuple<bool[,], bool[,], int, int>>();
		}

		public void ClearStateStack()
		{
			stateStack.Clear();
		}

		public void StepSolve()
		{
			// Save Copy of the board before step solve
			bool[,] oldMark = (bool[,])markBoard.Clone();
			bool[,] oldCross = (bool[,])crossBoard.Clone();
			
			// Step Solve Rows
			for(int i = 0; i < markBoard.GetLength(0); i++)
			{
				StepSolveRow(i);
			}

			// Step Solve Columns
			for(int j = 0; j < markBoard.GetLength(0); j++)
			{
				StepSolveColumn(j);
			}
			
			// Check if board was changed
			for(int i = 0; i < markBoard.GetLength(0); i++)
			{
				for(int j = 0; j < markBoard.GetLength(0); j++)
				{
					if(oldMark[i, j] != markBoard[i, j] || oldCross[i, j] != crossBoard[i, j]) return;
				}
			}

			// Branch of a solution if a step solve doesn't produce a unique solution alone
			BranchSolve();
		}

		public void BranchSolve()
		{
			// Find an empty cell and fill it in
			for(int i = 0; i < markBoard.GetLength(0); i++)
			{
				for(int j = 0; j < markBoard.GetLength(0); j++)
				{
					if(markBoard[i, j] == false && crossBoard[i, j] == false)
					{
						// Save the board state to the stack
						Tuple<bool[,], bool[,], int, int> addState;
						addState = new Tuple<bool[,], bool[,], int, int>(
							(bool[,])markBoard.Clone(),
							(bool[,])crossBoard.Clone(), 
							i, j);
						stateStack.Push(addState);

						// Mark a cell in
						markBoard[i, j] = true;
						return;
					}
				}
			}

			// If we cannot make anymore moves, check if we actually solved it
			if(IsSolved()) return;

			// Check if the stack is non-empty
			if(stateStack.Count == 0) throw new Exception("Nonogram appears to be unsolvable");

			// Return to previous state, and mark current space as a cross
			Tuple<bool[,], bool[,], int, int> state = stateStack.Pop();
			//markBoard = state.Item1;
			//crossBoard = state.Item2;
			
			for(int i = 0; i < markBoard.GetLength(0); i++)
			{
				for(int j = 0; j < markBoard.GetLength(0); j++)
				{
					markBoard[i, j] = state.Item1[i, j];
					crossBoard[i, j] = state.Item2[i, j];
				}
			}
			crossBoard[state.Item3, state.Item4] = true;
		}

		public void StepSolve(int step)
		{
			if(step % 2 == 0)
			{
				for(int i = 0; i < markBoard.GetLength(0); i++)
				{
					StepSolveRow(i);
				}
			}
			else
			{
				for(int j = 0; j < markBoard.GetLength(0); j++)
				{
					StepSolveColumn(j);
				}
			}
		}

		public bool[] BoolFill(bool[] boolArray, int start, int length)
		{
			bool[] newArray = (bool[])boolArray.Clone();

			for(int i = start; i < start + length; i++)
			{
				newArray[i] = true;
			}

			return newArray;
		}

		#region Horizontal Solve

		public void StepSolveRow(int i)
		{
			// STEP 1: Generate all the possible solutions for that row without consider the board state
			List<bool[]> solutions = GetRowSolutions(i);

			// STEP 2: Remove all solutions that would contradict the board
			if(solutions == null) return;

			for(int j = 0; j < markBoard.GetLength(0); j++)
			{
				// Scan the solutions
				bool[][] solutionsArray = solutions.ToArray();
				foreach(bool[] solution in solutionsArray)
				{
					if(solution[j] && crossBoard[i, j] || !solution[j] && markBoard[i, j]) solutions.Remove(solution); 
				}
			}

			// STEP 3: Update the board for tiles that are always marked or crossed from solutions
			if(solutions == null) return;

			bool[] hasMark = new bool[markBoard.GetLength(0)];
			bool[] hasBlank = new bool[markBoard.GetLength(0)];

			for(int j = 0; j < markBoard.GetLength(0); j++)
			{
				// Scan the solutions
				foreach(bool[] solution in solutions)
				{
					if(solution[j]) hasMark[j] = true;
					else hasBlank[j] = true;

					if(hasMark[j] && hasBlank[j]) break;
				}

				// Determine board state
				if(!hasMark[j] && !markBoard[i, j]) crossBoard[i, j] = true;
				if(!hasBlank[j] && !crossBoard[i, j]) markBoard[i, j] = true;
			}
		}
		
		public List<bool[]> GetRowSolutions(int i)
		{
			bool[] prevState = new bool[markBoard.GetLength(0)];
			//for(int j = 0; j < markBoard.GetLength(0); j++) prevState[j] = false;
			return GetRowSolutions(i, 0, prevState, 0);
		}

		public List<bool[]> GetRowSolutions(int i, int currentIndex, bool[] prevState, int hintDepth)
		{
			// Get the Hints for this row
			List<int> hints = horzHints[i];

			// Check if we reached the end of the hints
			if(hints.Count <= hintDepth)
			{
				// Just return a list with the prevState as its only element since its the only solution in this case
				List<bool[]> solution = new List<bool[]>();
				solution.Add(prevState);
				return solution;
			}

			// Check if the only hint is 0 (i.e. no marks on this row)
			if(hints[0] == 0)
			{
				// Create a false bool array
				bool[] blank = new bool[prevState.Length];
				//for(int j = 0; j < prevState.Length; j++) blank[j] = false;
				
				// Return said bool in a list
				List<bool[]> solution = new List<bool[]>();
				solution.Add(blank);
				return solution;
			}

			// Find the length of the current hint
			int hintLength = hints[hintDepth];

			// Begin loop to find all solutions that can be inserted
			List<bool[]> solutions = null;

			for(int j = currentIndex; j < prevState.Length; j++)
			{
				// If any further solutions would not fit, break the loop here
				if(j + hintLength - 1 >= prevState.Length) break;

				// Put the piece of solution into the state
				bool[] newState = BoolFill(prevState, j, hintLength);

				// Check if we can recur on further solutions
				if(j + hintLength + 1 < prevState.Length)
				{
					// Gather Solutions via Recursion
					List<bool[]> subSolutions = GetRowSolutions(i, j + hintLength + 1, newState, hintDepth + 1);

					// Combine these solutions with the main solutions
					if(solutions == null && subSolutions != null) solutions = new List<bool[]>();
					if(subSolutions != null) foreach(bool[] solution in subSolutions) solutions.Add(solution);
				}
				else if(hintDepth == hints.Count - 1)
				{
					if(solutions == null) solutions = new List<bool[]>();
					solutions.Add(newState);
				}
			}
			
			return solutions;
		}

		#endregion

		#region Vertical Solve

		public void StepSolveColumn(int j)
		{
			// STEP 1: Generate all the possible solutions for that row without consider the board state
			List<bool[]> solutions = GetColumnSolutions(j);

			// STEP 2: Remove all solutions that would contradict the board
			if(solutions == null) return;

			bool[][] solutionsArray = solutions.ToArray();
			for(int i = 0; i < markBoard.GetLength(0); i++)
			{
				// Scan the solutions
				foreach(bool[] solution in solutionsArray)
				{
					if(solution[i] && crossBoard[i, j] || !solution[i] && markBoard[i, j]) solutions.Remove(solution);
				}
			}

			// STEP 3: Update the board for tiles that are always marked or crossed from solutions
			if(solutions == null) return;

			bool[] hasMark = new bool[markBoard.GetLength(0)];
			bool[] hasBlank = new bool[markBoard.GetLength(0)];

			for(int i = 0; i < markBoard.GetLength(0); i++)
			{
				// Scan the solutions
				foreach(bool[] solution in solutions)
				{
					if(solution[i]) hasMark[i] = true;
					else hasBlank[i] = true;

					if(hasMark[i] && hasBlank[i]) break;
				}

				// Determine board state
				if(!hasMark[i] && !markBoard[i, j]) crossBoard[i, j] = true;
				if(!hasBlank[i] && !crossBoard[i, j]) markBoard[i, j] = true;
			}
		}

		public List<bool[]> GetColumnSolutions(int j)
		{
			bool[] prevState = new bool[markBoard.GetLength(0)];
			//for(int i = 0; i < markBoard.GetLength(0); i++) prevState[i] = false;
			return GetColumnSolutions(j, 0, prevState, 0);
		}

		public List<bool[]> GetColumnSolutions(int j, int currentIndex, bool[] prevState, int hintDepth)
		{
			// Get the Hints for this row
			List<int> hints = vertHints[j];

			// Check if we reached the end of the hints
			if(hints.Count <= hintDepth)
			{
				// Just return a list with the prevState as its only element since its the only solution in this case
				List<bool[]> solution = new List<bool[]>();
				solution.Add(prevState);
				return solution;
			}

			// Check if the only hint is 0 (i.e. no marks on this row)
			if(hints[0] == 0)
			{
				// Create a false bool array
				bool[] blank = new bool[prevState.Length];
				//for(int j = 0; j < prevState.Length; j++) blank[j] = false;

				// Return said bool in a list
				List<bool[]> solution = new List<bool[]>();
				solution.Add(blank);
				return solution;
			}

			// Find the length of the current hint
			int hintLength = hints[hintDepth];

			// Begin loop to find all solutions that can be inserted
			List<bool[]> solutions = null;

			for(int i = currentIndex; i < prevState.Length; i++)
			{
				// If any further solutions would not fit, break the loop here
				if(i + hintLength - 1 >= prevState.Length) break;

				// Put the piece of solution into the state
				bool[] newState = BoolFill(prevState, i, hintLength);

				// Check if we can recur on further solutions
				if(i + hintLength + 1 < prevState.Length)
				{
					// Gather Solutions via Recursion
					List<bool[]> subSolutions = GetColumnSolutions(j, i + hintLength + 1, newState, hintDepth + 1);

					// Combine these solutions with the main solutions
					if(solutions == null && subSolutions != null) solutions = new List<bool[]>();
					if(subSolutions != null) foreach(bool[] solution in subSolutions) solutions.Add(solution);
				}
				else if(hintDepth == hints.Count - 1)
				{
					if(solutions == null) solutions = new List<bool[]>();
					solutions.Add(newState);
				}
			}

			return solutions;
		}

		#endregion
	}
}
