using System;
using System.Collections.Generic;

namespace Crossword
{
    class GenerationContext
    {
        public char[][] Puzzle { get; }

        public int[][] VerticalWordStarts { get; }

        public int[][] HorizontalWordStarts { get; }

        public WordCriteria[][] VerticalCriteria { get; }

        public WordCriteria[][] HorizontalCriteria { get; }

        public HashSet<string> UsedWords { get; }

        public GenerationContext(int width, int height)
        {
            Puzzle = new char[height][];
            VerticalWordStarts = new int[height][];
            HorizontalWordStarts = new int[height][];
            VerticalCriteria = new WordCriteria[height][];
            HorizontalCriteria = new WordCriteria[height][];
            UsedWords = new HashSet<string>();

            for (int i = 0; i < height; i++)
            {
                Puzzle[i] = new char[width];
                VerticalWordStarts[i] = new int[width];
                HorizontalWordStarts[i] = new int[width];
                VerticalCriteria[i] = new WordCriteria[width];
                HorizontalCriteria[i] = new WordCriteria[width];
            }
        }

        /*public GenerationContext Copy()
        {
            var width = Puzzle[0].Length;
            var height = Puzzle.Length;
            var copy = new GenerationContext(width, height);

            foreach (var usedWord in UsedWords)
            {
                copy.UsedWords.Add(usedWord);
            }

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    copy.Puzzle[i][j] = Puzzle[i][j];
                    copy.VerticalWordStarts[i][j] = VerticalWordStarts[i][j];
                    copy.HorizontalWordStarts[i][j] = HorizontalWordStarts[i][j];
                    copy.VerticalCriteria[i][j] = VerticalCriteria[i][j]?.Copy();
                    copy.HorizontalCriteria[i][j] = HorizontalCriteria[i][j]?.Copy();
                }
            }

            return copy;
        }*/
    }
}
