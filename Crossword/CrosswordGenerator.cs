using System;
using System.Collections.Generic;
using System.Linq;

namespace Crossword
{
    class CrosswordGenerator
    {
        private const char Empty = '-';

        private const char Black = '#';

        private WordFilter WordFilter { get; }

        private Random Random { get; }

        private uint Width { get; }

        private uint Height { get; }

        public CrosswordGenerator(WordFilter wordFilter, uint width, uint height)
        {
            WordFilter = wordFilter;
            Random = new Random();
            Width = width;
            Height = height;
        }

        public char[][] GetStartPuzzle(params SquareValue[] existing)
        {
            var context = InitializeContext(existing);
            return context.Puzzle;
        }

        public char[][] GenerateCrossword(params SquareValue[] existing)
        {
            var context = InitializeContext(existing);

            var usedWords = new HashSet<string>();

            if (!FillGrid(context, usedWords))
            {
                return null;
            }

            return context.Puzzle;
        }

        private GenerationContext InitializeContext(SquareValue[] existing)
        {
            var context = new GenerationContext(Width, Height);

            PopulateInitialValues(existing, context);

            PopulateWordStartPositions(context);

            InitializeCriteria(context);

            return context;
        }

        private void PopulateInitialValues(SquareValue[] existing, GenerationContext context)
        {
            for (int i = 0; i < existing.Length; i++)
            {
                var squareValue = existing[i];

                if (squareValue.Coordinate.I > Height)
                {
                    throw new Exception($"Provided i coordinate {squareValue.Coordinate.I} is greater than height {Height}.");
                }

                if (squareValue.Coordinate.J > Width)
                {
                    throw new Exception($"Provided j coordinate {squareValue.Coordinate.J} is greater than width {Width}.");
                }

                if (squareValue.Value != Black && squareValue.Value != Empty && (squareValue.Value < 'A' && squareValue.Value > 'Z'))
                {
                    throw new Exception($"Provided value for {squareValue.Coordinate.I},{squareValue.Coordinate.J}: {squareValue.Value} is not valid.");
                }

                context.Puzzle[squareValue.Coordinate.I][squareValue.Coordinate.J] = squareValue.Value;
            }
        }

        private void PopulateWordStartPositions(GenerationContext context)
        {
            for (int y = 0; y < Height; y++)
            {
                bool inWord = false;
                int currentStart = -1;

                for (int x = 0; x < Width; x++)
                {
                    if (context.Puzzle[y][x] == Black)
                    {
                        inWord = false;
                        context.HorizontalWordStarts[y][x] = -1;
                    }
                    else
                    {
                        if (!inWord)
                        {
                            inWord = true;
                            currentStart = x;
                        }
                        context.HorizontalWordStarts[y][x] = currentStart;
                    }
                }
            }

            for (int x = 0; x < Width; x++)
            {
                bool inWord = false;
                int currentStart = -1;

                for (int y = 0; y < Height; y++)
                {
                    if (context.Puzzle[y][x] == Black)
                    {
                        inWord = false;
                        context.VerticalWordStarts[y][x] = -1;
                    }
                    else
                    {
                        if (!inWord)
                        {
                            inWord = true;
                            currentStart = y;
                        }
                        context.VerticalWordStarts[y][x] = currentStart;
                    }
                }
            }
        }

        private void InitializeCriteria(GenerationContext context)
        {
            for (int y = 0; y < Height; y++)
            {
                List<LetterCriterion> letterCriteria = null;
                uint currentWordLength = 0;

                for (int x = 0; x < Width; x++)
                {
                    if (context.HorizontalWordStarts[y][x] == -1)
                    {
                        if (currentWordLength > 0)
                        {
                            var wordCriteria = new WordCriteria(currentWordLength, letterCriteria.ToArray());

                            for (int i = x - (int)currentWordLength; i < x; i++)
                            {
                                context.HorizontalCriteria[y][i] = wordCriteria;
                            }
                        }
                        letterCriteria = null;
                        currentWordLength = 0;
                        continue;
                    }

                    if (letterCriteria == null)
                    {
                        letterCriteria = new List<LetterCriterion>();
                    }

                    currentWordLength++;

                    letterCriteria.Add(new LetterCriterion((uint)(x - context.HorizontalWordStarts[y][x]), context.Puzzle[y][x]));
                }

                if (currentWordLength > 0)
                {
                    var wordCriteria = new WordCriteria(currentWordLength, letterCriteria.ToArray());

                    for (var i = Width - currentWordLength; i < Width; i++)
                    {
                        context.HorizontalCriteria[y][i] = wordCriteria;
                    }
                }
            }

            for (int x = 0; x < Width; x++)
            {
                List<LetterCriterion> letterCriteria = null;
                uint currentWordLength = 0;

                for (int y = 0; y < Height; y++)
                {
                    if (context.VerticalWordStarts[y][x] == -1)
                    {
                        if (currentWordLength > 0)
                        {
                            var wordCriteria = new WordCriteria(currentWordLength, letterCriteria.ToArray());

                            for (int i = y - (int)currentWordLength; i < y; i++)
                            {
                                context.VerticalCriteria[i][x] = wordCriteria;
                            }
                        }
                        letterCriteria = null;
                        currentWordLength = 0;
                        continue;
                    }

                    if (letterCriteria == null)
                    {
                        letterCriteria = new List<LetterCriterion>();
                    }

                    currentWordLength++;

                    letterCriteria.Add(new LetterCriterion((uint)(y - context.VerticalWordStarts[y][x]), context.Puzzle[y][x]));
                }

                if (currentWordLength > 0)
                {
                    var wordCriteria = new WordCriteria(currentWordLength, letterCriteria.ToArray());

                    for (var i = Height - currentWordLength; i < Height; i++)
                    {
                        context.VerticalCriteria[i][x] = wordCriteria;
                    }
                }
            }
        }

        private bool FillGrid(GenerationContext context, HashSet<string> usedWords)
        {
            Utils.WritePuzzle(context.Puzzle);
            Console.WriteLine();

            var nextEmptyCoordinates = GetNextEmptyCoordinates(context.Puzzle);

            if (nextEmptyCoordinates == null)
            {
                return true;
            }

            var i = nextEmptyCoordinates.I;
            var j = nextEmptyCoordinates.J;

            var verticalCriteria = GetVerticalCriteria(context, (int)i, (int)j);
            var horizontalCriteria = GetHorizontalCriteria(context, (int)i, (int)j);

            var doHorizontal = horizontalCriteria.criteria.NumFilledLetters >= verticalCriteria.criteria.NumFilledLetters;
            var criteria = doHorizontal ? horizontalCriteria : verticalCriteria;

            var potentialWords = ShuffleWords(WordFilter.GetMatchingWords(criteria.criteria)).Where(w => !usedWords.Contains(w)).ToList();

            foreach (var word in potentialWords)
            {
                var writtenSpaces = WriteWord(context, i, j, doHorizontal, word);
                usedWords.Add(word);

                if (DoAlternateWordsExist(context, writtenSpaces, doHorizontal) && FillGrid(context, usedWords))
                {
                    return true;
                }

                usedWords.Remove(word);
                UnwriteWord(context, writtenSpaces);
            }

            return false;
        }

        private bool DoAlternateWordsExist(GenerationContext context, Coordinate[] writtenSpaces, bool doHorizontal)
        {
            foreach (var writtenSpace in writtenSpaces)
            {
                var i = writtenSpace.I;
                var j = writtenSpace.J;

                var (_, criteria) = doHorizontal ? GetVerticalCriteria(context, (int)i, (int)j) : GetHorizontalCriteria(context, (int)i, (int)j);

                if (!WordFilter.GetMatchingWords(criteria).Any())
                {
                    return false;
                }
            }
            return true;
        }

        private static Coordinate[] WriteWord(GenerationContext context, uint i, uint j, bool doHorizontal, string word)
        {
            var changes = new List<Coordinate>();

            if (doHorizontal)
            {
                var wordStart = context.HorizontalWordStarts[i][j];
                for (int x = 0; x < word.Length; x++)
                {
                    if (context.Puzzle[i][x + wordStart] == Empty)
                    {
                        context.Puzzle[i][x + wordStart] = word[x];
                        context.HorizontalCriteria[i][j].SetLetter(x, word[x]);
                        context.VerticalCriteria[i][x + wordStart].SetLetter((int)i - context.VerticalWordStarts[i][x + wordStart], word[x]);
                        changes.Add(new Coordinate(i, (uint)(x + wordStart)));
                    }
                }
            }
            else
            {
                var wordStart = context.VerticalWordStarts[i][j];

                for (int y = 0; y < word.Length; y++)
                {
                    if (context.Puzzle[y + wordStart][j] == Empty)
                    {
                        context.Puzzle[y + wordStart][j] = word[y];
                        context.VerticalCriteria[i][j].SetLetter(y, word[y]);
                        context.HorizontalCriteria[y + wordStart][j].SetLetter((int)j - context.HorizontalWordStarts[y + wordStart][j], word[y]);
                        changes.Add(new Coordinate((uint)(y + wordStart), j));
                    }
                }
            }

            return changes.ToArray();
        }

        private static void UnwriteWord(GenerationContext context, Coordinate[] spaces)
        {
            for (int m = 0; m < spaces.Length; m++)
            {
                var i = spaces[m].I;
                var j = spaces[m].J;
                context.Puzzle[i][j] = Empty;
                var horizontalWordStart = context.HorizontalWordStarts[i][j];
                var verticalWordStart = context.VerticalWordStarts[i][j];
                context.HorizontalCriteria[i][j].SetLetter((int)j - horizontalWordStart, Empty);
                context.VerticalCriteria[i][j].SetLetter((int)i - verticalWordStart, Empty);
            }
        }

        private (uint wordStart, WordCriteria criteria) GetVerticalCriteria(GenerationContext context, int i, int j)
        {
            return ((uint)context.VerticalWordStarts[i][j], context.VerticalCriteria[i][j]);
        }

        private (uint wordStart, WordCriteria criteria) GetHorizontalCriteria(GenerationContext context, int i, int j)
        {
            return ((uint)context.HorizontalWordStarts[i][j], context.HorizontalCriteria[i][j]);
        }

        private Coordinate GetNextEmptyCoordinates(char[][] puzzle)
        {
            for (uint i = 0; i < Height; i++)
            {
                for (uint j = 0; j < Width; j++)
                {
                    if (puzzle[i][j] == Empty)
                    {
                        return new Coordinate(i, j);
                    }
                }
            }

            return null;
        }

        private string[] ShuffleWords(List<string> words)
        {
            var result = new string[words.Count];
            words.CopyTo(result);

            for (int i = 0; i < words.Count; i++)
            {
                var randomIndex = Random.Next(i, words.Count);
                var temp = result[i];
                result[i] = result[randomIndex];
                result[randomIndex] = temp;
            }

            return result;
        }
    }
}
