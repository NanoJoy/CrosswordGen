using System;
using System.IO;
using System.Text;

namespace Crossword
{
    public static class Utils
    {
        private static TextWriter TextWriter { get; set; }

        public static void WritePuzzle(char[][] puzzle)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < puzzle.Length; i++)
            {
                for (int j = 0; j < puzzle[i].Length; j++)
                {
                    builder.Append(puzzle[i][j]);
                }
                builder.AppendLine();
            }

            var puz = builder.ToString();

            TextWriter?.WriteLine(puz);
            Console.WriteLine(puz);
        }

        public static void SetOutputFile(Stream stream)
        {
            if (TextWriter != null)
            {
                TextWriter.Close();
            }

            TextWriter = new StreamWriter(stream);
        }
    }
}
