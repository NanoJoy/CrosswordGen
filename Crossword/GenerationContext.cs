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
    }
}
