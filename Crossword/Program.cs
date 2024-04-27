using System;
using System.Collections.Generic;

namespace Crossword
{
    class Program
    {
        static void Main(string[] args)
        {
            var wordsFilePath = args[0];

            var wordFilter = new WordFilter(wordsFilePath);

            var generator = new CrosswordGenerator(wordFilter);

            while (true)
            {
                var puzzle = generator.GenerateCrossword();

                if (puzzle != null)
                {

                    Utils.WritePuzzle(puzzle);

                    Console.WriteLine();

                    Console.ReadLine();
                }
            }
        }
    }
}
