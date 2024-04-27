using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossword
{
    public static class Utils
    {
        public static void WritePuzzle(char[][] puzzle)
        {
            for (int i = 0; i < puzzle.Length; i++)
            {
                for (int j = 0; j < puzzle[i].Length; j++)
                {
                    Console.Write(puzzle[i][j]);
                }
                Console.WriteLine();
            }
        }
    }
}
