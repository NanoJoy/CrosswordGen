using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossword
{
    class CrosswordGenerator
    {
        private const int Size = 5;

        private const char Empty = '-';

        private WordFilter WordFilter { get; }

        private Random Random { get; }

        public CrosswordGenerator(WordFilter wordFilter)
        {
            WordFilter = wordFilter;
            Random = new Random();
        }

        public char[][]? GenerateCrossword()
        {
            var result = new char[Size][];

            for (int i = 0; i < Size; i++)
            {
                result[i] = new char[Size];

                for (int j = 0; j < Size; j++)
                {
                    result[i][j] = Empty;
                }
            }

            var firstWords = ShuffleWords(WordFilter.GetMatchingWords(new LetterCriterion[0]));

            foreach (var word in firstWords)
            {
                var written = WriteWord(result, 0, 0, true, word);

                if (FillGrid(result))
                {
                    return result;
                }
                else
                {
                    UnwriteWord(result, written);
                }
            }

            return null;
        }

        private bool FillGrid(char[][] puzzle)
        {
            var (i, j) = GetNextEmptyCoordinates(puzzle);

            if (i == -1)
            {
                return AreLastWordsValid(puzzle);
            }

            var verticalCriteria = GetVerticalCriteria(puzzle, i, j);
            var horizontalCriteria = GetHorizontalCriteria(puzzle, i, j);

            var doHorizontal = horizontalCriteria.Length >= verticalCriteria.Length;
            var criteria = doHorizontal ? horizontalCriteria : verticalCriteria;

            var potentialWords = ShuffleWords(WordFilter.GetMatchingWords(criteria));

            foreach (var word in potentialWords)
            {
                var writtenSpaces = WriteWord(puzzle, i, j, doHorizontal, word);

                if (FillGrid(puzzle))
                {
                    return true;
                }

                UnwriteWord(puzzle, writtenSpaces);

                return false;
            }

            return false;
        }

        private bool AreLastWordsValid(char[][] puzzle)
        {
            var verticalCriteria = new LetterCriterion[Size];
            var horizontalCriteria = new LetterCriterion[Size];

            for (uint i = 0; i < Size; i++)
            {
                verticalCriteria[i] = new LetterCriterion(i, puzzle[i][Size - 1]);
                horizontalCriteria[i] = new LetterCriterion(i, puzzle[Size - 1][i]);
            }

            return WordFilter.HasMatchingWords(verticalCriteria) && WordFilter.HasMatchingWords(horizontalCriteria);
        }

        private static (int i, int j)[] WriteWord(char[][] puzzle, int i, int j, bool doHorizontal, string word)
        {
            var changes = new List<(int i, int j)>();

            if (doHorizontal)
            {
                for (int x = 0; x < Size; x++)
                {
                    if (puzzle[i][x] == Empty)
                    {
                        puzzle[i][x] = word[x];
                        changes.Add((i, x));
                    }
                }
            }
            else
            {
                for (int y = 0; y < Size; y++)
                {
                    if (puzzle[y][j] == Empty)
                    {
                        puzzle[y][j] = word[y];
                        changes.Add((y, j));
                    }
                }
            }

            /*Console.WriteLine();

            Utils.WritePuzzle(puzzle);*/

            return changes.ToArray();
        }

        private static void UnwriteWord(char[][] puzzle, (int i, int j)[] spaces)
        {
            for (int m = 0; m < spaces.Length; m++)
            {
                puzzle[spaces[m].i][spaces[m].j] = Empty;
            }
        }

        private static LetterCriterion[] GetVerticalCriteria(char[][] puzzle, int i, int j)
        {
            var result = new List<LetterCriterion>();

            for (int y = i - 1; y >= 0; y--)
            {
                if (puzzle[y][j] != Empty)
                {
                    result.Add(new LetterCriterion((uint)y, puzzle[y][j]));
                }
            }

            for (int y = i + 1; y < Size; y++)
            {
                if (puzzle[y][j] != Empty)
                {
                    result.Add(new LetterCriterion((uint)y, puzzle[y][j]));
                }
            }

            return result.ToArray();
        }

        private static LetterCriterion[] GetHorizontalCriteria(char[][] puzzle, int i, int j)
        {
            var result = new List<LetterCriterion>();

            for (int x = j - 1; x >= 0; x--)
            {
                if (puzzle[i][x] != Empty)
                {
                    result.Add(new LetterCriterion((uint)x, puzzle[i][x]));
                }
            }

            for (int x = j + 1; x < Size; x++)
            {
                if (puzzle[i][x] != Empty)
                {
                    result.Add(new LetterCriterion((uint)x, puzzle[i][x]));
                }
            }

            return result.ToArray();
        }

        private static (int i, int j) GetNextEmptyCoordinates(char[][] puzzle)
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (puzzle[i][j] == Empty)
                    {
                        return (i, j);
                    }
                }
            }

            return (-1, -1);
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
