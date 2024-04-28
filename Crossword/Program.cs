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

            var generator = new CrosswordGenerator(wordFilter, 7, 7);

            Console.Write("Enter criteria: ");

            var existing = ParseExistingValues(Console.ReadLine());

            while (true)
            {
                var puzzle = generator.GenerateCrossword(existing);

                if (puzzle != null)
                {

                    Utils.WritePuzzle(puzzle);

                    Console.WriteLine();
                }

                Console.Write("Enter criteria: ");

                existing = ParseExistingValues(Console.ReadLine());
            }
        }

        static SquareValue[] ParseExistingValues(string existingValuesString)
        {
            if (string.IsNullOrEmpty(existingValuesString))
            {
                return Array.Empty<SquareValue>();
            }
            var strings = existingValuesString.Split(';');
            var result = new SquareValue[strings.Length];

            for (int k = 0; k < strings.Length; k++)
            {
                var s = strings[k];
                var parts = s.Split(',');
                var i = uint.Parse(parts[0]);
                var j = uint.Parse(parts[1]);
                var v = char.Parse(parts[2]);
                result[k] = new SquareValue(i, j, v);
            }

            return result;
        }
    }
}
