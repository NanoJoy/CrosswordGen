using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossword
{
    class CrosswordGenerator
    {
        private const char Empty = '-';

        private const char Black = '#';

        private WordFilter WordFilter { get; }

        private Random Random { get; }

        private int Width { get; }

        private int Height { get; }

        public CrosswordGenerator(WordFilter wordFilter, int width, int height)
        {
            WordFilter = wordFilter;
            Random = new Random();
            Width = width;
            Height = height;
        }

        public char[][] GenerateCrossword(params SquareValue[] existing)
        {
            var result = new char[Height][];

            // Fill empty.
            for (int i = 0; i < Height; i++)
            {
                result[i] = new char[Width];

                for (int j = 0; j < Width; j++)
                {
                    result[i][j] = Empty;
                }
            }
            
            // Fill existing values.
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

                result[squareValue.Coordinate.I][squareValue.Coordinate.J] = squareValue.Value;
            }

            var usedWords = new HashSet<string>();

            if (!FillGrid(result, usedWords))
            {
                return null;
            }

            return result;
        }

        private bool FillGrid(char[][] puzzle, HashSet<string> usedWords)
        {
            var nextEmptyCoordinates = GetNextEmptyCoordinates(puzzle);

            if (nextEmptyCoordinates == null)
            {
                return true;
            }

            var i = nextEmptyCoordinates.I;
            var j = nextEmptyCoordinates.J;

            var verticalCriteria = GetVerticalCriteria(puzzle, (int)i, (int)j);
            var horizontalCriteria = GetHorizontalCriteria(puzzle, (int)i, (int)j);

            var doHorizontal = horizontalCriteria.criteria.Letters.Length >= verticalCriteria.criteria.Letters.Length;
            var criteria = doHorizontal ? horizontalCriteria : verticalCriteria;

            var potentialWords = ShuffleWords(WordFilter.GetMatchingWords(criteria.criteria)).Where(w => !usedWords.Contains(w)).ToList();

            foreach (var word in potentialWords)
            {
                var writtenSpaces = WriteWord(puzzle, i, j, criteria.wordStart, doHorizontal, word);
                usedWords.Add(word);

                if (DoAlternateWordsExist(puzzle, writtenSpaces, doHorizontal) && FillGrid(puzzle, usedWords))
                {
                    return true;
                }

                usedWords.Remove(word);
                UnwriteWord(puzzle, writtenSpaces);
            }

            return false;
        }

        private bool DoAlternateWordsExist(char[][] puzzle, Coordinate[] writtenSpaces, bool doHorizontal)
        {
            foreach (var writtenSpace in writtenSpaces)
            {
                var i = writtenSpace.I;
                var j = writtenSpace.J;

                var (_, criteria) = doHorizontal ? GetVerticalCriteria(puzzle, (int)i, (int)j) : GetHorizontalCriteria(puzzle, (int)i, (int)j);

                if (!WordFilter.GetMatchingWords(criteria).Any())
                {
                    return false;
                }
            }
            return true;
        }

        private static Coordinate[] WriteWord(char[][] puzzle, uint i, uint j, uint wordStart, bool doHorizontal, string word)
        {
            var changes = new List<Coordinate>();

            if (doHorizontal)
            {
                for (uint x = 0; x < word.Length; x++)
                {
                    if (puzzle[i][x + wordStart] == Empty)
                    {
                        puzzle[i][x + wordStart] = word[(int)x];
                        changes.Add(new Coordinate(i, x + wordStart));
                    }
                }
            }
            else
            {
                for (uint y = 0; y < word.Length; y++)
                {
                    if (puzzle[y + wordStart][j] == Empty)
                    {
                        puzzle[y + wordStart][j] = word[(int)y];
                        changes.Add(new Coordinate(y + wordStart, j));
                    }
                }
            }

            return changes.ToArray();
        }

        private static void UnwriteWord(char[][] puzzle, Coordinate[] spaces)
        {
            for (int m = 0; m < spaces.Length; m++)
            {
                puzzle[spaces[m].I][spaces[m].J] = Empty;
            }
        }

        private (uint wordStart, WordCriteria criteria) GetVerticalCriteria(char[][] puzzle, int i, int j)
        {
            var result = new List<LetterCriterion>();

            int y;

            for (y = i - 1; y >= 0; y--)
            {
                if (puzzle[y][j] == Black)
                {
                    break;
                }
            }

            var lowerBound = y;

            for (y = lowerBound + 1; y < Height; y++)
            {
                if (puzzle[y][j] == Black)
                {
                    break;
                }
                if (puzzle[y][j] != Empty)
                {
                    result.Add(new LetterCriterion((uint)(y - (lowerBound + 1)), puzzle[y][j]));
                }
            }

            var upperBound = y;

            return ((uint)(lowerBound + 1), new WordCriteria((uint)(upperBound - lowerBound - 1), result.ToArray()));
        }

        private (uint wordStart, WordCriteria criteria) GetHorizontalCriteria(char[][] puzzle, int i, int j)
        {
            var result = new List<LetterCriterion>();

            int x;

            for (x = j - 1; x >= 0; x--)
            {
                if (puzzle[i][x] == Black)
                {
                    break;
                }
            }

            var lowerBound = x;

            for (x = lowerBound + 1; x < Width; x++)
            {
                if (puzzle[i][x] == Black)
                {
                    break;
                }
                if (puzzle[i][x] != Empty)
                {
                    result.Add(new LetterCriterion((uint)(x - (lowerBound + 1)), puzzle[i][x]));
                }
            }

            var upperBound = x;

            return ((uint)(lowerBound + 1), new WordCriteria((uint)(upperBound - lowerBound - 1), result.ToArray()));
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
