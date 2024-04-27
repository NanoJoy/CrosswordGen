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

                    CheckPuzzle(puzzle, wordFilter);

                    Console.ReadLine();
                }
            }
        }

        private static void CheckPuzzle(char[][] puzzle, WordFilter wordFilter)
        {
            Console.WriteLine("Horizontal:");

            for (int i = 0; i < puzzle.Length; i++)
            {
                var w = new string(puzzle[i]);
                Console.WriteLine($"{w} {wordFilter.HasMatchingWord(w)}");
            }

            Console.WriteLine("Vertical:");

            for (int j = 0; j < puzzle[0].Length; j++)
            {
                var b = new char[puzzle.Length];

                for (int i = 0; i < puzzle.Length; i++)
                {
                    b[i] = puzzle[i][j];
                }

                var w = new string(b);
                Console.WriteLine($"{w} {wordFilter.HasMatchingWord(w)}");
            }
        }

        private static void ConsoleFilter(WordFilter wordFilter)
        {
            var shouldContinue = true;

            while (shouldContinue)
            {
                Console.WriteLine("Enter criteria: ");

                var criteriaString = Console.ReadLine();

                Console.WriteLine();

                var criteria = GetWordCriteria(criteriaString);

                foreach (var matchingWord in wordFilter.GetMatchingWords(criteria))
                {
                    Console.WriteLine(matchingWord);
                }

                Console.WriteLine();

                Console.WriteLine("Continue?: ");

                var continueString = Console.ReadLine();

                shouldContinue = continueString == "y" || continueString == "Y";
            }
        }

        private static LetterCriterion[] GetWordCriteria(string s)
        {
            var result = new List<LetterCriterion>();

            for (int i = 0; i < s.Length; i++)
            {
                var c = s[i];

                if (c >= 'A' && c <= 'Z')
                {
                    result.Add(new LetterCriterion((uint)i, c));
                }
            }

            return result.ToArray();
        }
    }
}
