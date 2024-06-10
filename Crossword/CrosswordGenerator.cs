using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Crossword
{
    public class CrosswordGenerator
    {
        private const char Empty = '-';

        private const char Black = '#';

        private object Lock { get; } = new object();

        private WordFilter WordFilter { get; }

        private Random Random { get; }

        private int Width { get; }

        private int Height { get; }

        private WordTryOrder WordTryOrder { get; } = WordTryOrder.Random;

        public CrosswordGenerator(WordFilter wordFilter, int width, int height, WordTryOrder trialStrategy = WordTryOrder.Random)
        {
            WordFilter = wordFilter;
            Random = new Random();
            Width = width;
            Height = height;
            WordTryOrder = trialStrategy;
        }

        public char[][] GetStartPuzzle(params SquareValue[] existing)
        {
            var context = InitializeContext(existing);
            return context.Puzzle;
        }

        public char[][] GenerateCrossword(params SquareValue[] existing)
        {
            var rootContext = InitializeContext(existing);

            var (hasNext, i, j, doHorizontal) = GetNextWordToFill(rootContext);

            if (!hasNext)
            {
                return null;
            }

            var criteria = doHorizontal ? GetHorizontalCriteria(rootContext, i, j) : GetVerticalCriteria(rootContext, i, j);

            var potentialWords = WordFilter.GetMatchingWords(criteria.criteria).Where(w => !rootContext.UsedWords.Contains(w.Word)).ToList();

            var wordsToTry = GetWordsToTry(potentialWords);

            GenerationContext winningContext = null;
            var tracker = new ParallelTracker();

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 4 };

            Parallel.ForEach(wordsToTry, parallelOptions, (word) =>
            {
                lock (Lock)
                {
                    if (winningContext != null)
                    {
                        return;
                    }
                }
                var context = new GenerationContext(Width, Height);

                PopulateInitialValues(existing, context);

                PopulateWordStartPositions(context);

                InitializeCriteria(context);

                WriteWord(context, i, j, doHorizontal, word);

                if (FillGrid(context, tracker))
                {
                    lock (Lock)
                    {
                        //Console.WriteLine($"Winning puzzle found for initial word {word}");
                        winningContext = context;
                        tracker.IsOver = true;
                    }
                }
                else
                {
                    //Console.WriteLine($"No puzzle found for initial word {word}");
                }
            });

           return winningContext?.Puzzle;
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
                int currentWordLength = 0;

                for (int x = 0; x < Width; x++)
                {
                    if (context.HorizontalWordStarts[y][x] == -1)
                    {
                        if (currentWordLength > 0)
                        {
                            var wordCriteria = new WordCriteria(currentWordLength, letterCriteria.ToArray());

                            for (int i = x - currentWordLength; i < x; i++)
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

                    letterCriteria.Add(new LetterCriterion(x - context.HorizontalWordStarts[y][x], context.Puzzle[y][x]));
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
                int currentWordLength = 0;

                for (int y = 0; y < Height; y++)
                {
                    if (context.VerticalWordStarts[y][x] == -1)
                    {
                        if (currentWordLength > 0)
                        {
                            var wordCriteria = new WordCriteria(currentWordLength, letterCriteria.ToArray());

                            for (int i = y - currentWordLength; i < y; i++)
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

                    letterCriteria.Add(new LetterCriterion(y - context.VerticalWordStarts[y][x], context.Puzzle[y][x]));
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

        private bool FillGrid(GenerationContext context, ParallelTracker tracker)
        {
            lock(Lock)
            {
                if (tracker.IsOver)
                {
                    return false;
                }
            }

            var (hasNext, i, j, doHorizontal) = GetNextWordToFill(context);

            if (!hasNext)
            {
                return true;
            }

            var criteria = doHorizontal ? GetHorizontalCriteria(context, i, j) : GetVerticalCriteria(context, i, j);

            var potentialWords = WordFilter.GetMatchingWords(criteria.criteria).Where(w => !context.UsedWords.Contains(w.Word)).ToList();

            var wordsToTry = GetWordsToTry(potentialWords);

            foreach (var word in wordsToTry)
            {

                lock (Lock)
                {
                    if (tracker.IsOver)
                    {
                        return false;
                    }
                }

                var writtenSpaces = WriteWord(context, i, j, doHorizontal, word);
                context.UsedWords.Add(word);

                if (DoAlternateWordsExist(context, writtenSpaces, doHorizontal) && FillGrid(context, tracker))
                {
                    return true;
                }

                context.UsedWords.Remove(word);
                UnwriteWord(context, writtenSpaces);
            }

            return false;
        }

        private (bool hasNext, int i, int j, bool doHorizontal) GetNextWordToFill(GenerationContext context)
        {
            bool doHorizontal = true;
            int i = -1;
            int j = -1;
            int maxCriteria = -1;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (context.Puzzle[y][x] != Empty)
                    {
                        continue;
                    }

                    if (context.HorizontalCriteria[y][x].NumFilledLetters > maxCriteria)
                    {
                        maxCriteria = context.HorizontalCriteria[y][x].NumFilledLetters;
                        i = y;
                        j = x;
                        doHorizontal = true;
                    }

                    if (context.VerticalCriteria[y][x].NumFilledLetters > maxCriteria)
                    {
                        maxCriteria = context.VerticalCriteria[y][x].NumFilledLetters;
                        i = y;
                        j = x;
                        doHorizontal = false;
                    }
                }
            }

            return (maxCriteria != -1, i, j, doHorizontal);
        }

        private bool DoAlternateWordsExist(GenerationContext context, Coordinate[] writtenSpaces, bool doHorizontal)
        {
            foreach (var writtenSpace in writtenSpaces)
            {
                var i = writtenSpace.I;
                var j = writtenSpace.J;

                var (_, criteria) = doHorizontal ? GetVerticalCriteria(context, i, j) : GetHorizontalCriteria(context, i, j);

                if (!WordFilter.GetMatchingWords(criteria).Any())
                {
                    return false;
                }
            }
            return true;
        }

        private static Coordinate[] WriteWord(GenerationContext context, int i, int j, bool doHorizontal, string word)
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
                        context.VerticalCriteria[i][x + wordStart].SetLetter(i - context.VerticalWordStarts[i][x + wordStart], word[x]);
                        changes.Add(new Coordinate(i, x + wordStart));
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
                        context.HorizontalCriteria[y + wordStart][j].SetLetter(j - context.HorizontalWordStarts[y + wordStart][j], word[y]);
                        changes.Add(new Coordinate(y + wordStart, j));
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
                context.HorizontalCriteria[i][j].SetLetter(j - horizontalWordStart, Empty);
                context.VerticalCriteria[i][j].SetLetter(i - verticalWordStart, Empty);
            }
        }

        private (int wordStart, WordCriteria criteria) GetVerticalCriteria(GenerationContext context, int i, int j)
        {
            return (context.VerticalWordStarts[i][j], context.VerticalCriteria[i][j]);
        }

        private (int wordStart, WordCriteria criteria) GetHorizontalCriteria(GenerationContext context, int i, int j)
        {
            return (context.HorizontalWordStarts[i][j], context.HorizontalCriteria[i][j]);
        }

        private string[] GetWordsToTry(List<WordScore> words)
        {
            switch (WordTryOrder)
            {
                case WordTryOrder.Random:
                    return ShuffleWords(words);
                case WordTryOrder.HighestScore:
                    return GetWordsOrderedByScore(words);
                case WordTryOrder.Balanced:
                    return GetWordsSemiShuffled(words);
            }

            return Array.Empty<string>();
        }

        private string[] ShuffleWords(List<WordScore> words)
        {
            var result = new string[words.Count];
            words.Select(w => w.Word).ToList().CopyTo(result);

            for (int i = 0; i < words.Count; i++)
            {
                var randomIndex = Random.Next(i, words.Count);
                var temp = result[i];
                result[i] = result[randomIndex];
                result[randomIndex] = temp;
            }

            return result;
        }

        private static string[] GetWordsOrderedByScore(List<WordScore> words)
        {
            return words.OrderByDescending(w => w.Score).Select(w => w.Word).ToArray();
        }

        private string[] GetWordsSemiShuffled(List<WordScore> words)
        {
            var ordered = GetWordsOrderedByScore(words);

            var shuffleWidth = Math.Min(ordered.Length / 3, 10);

            for (int i = 0; i < ordered.Length - shuffleWidth; i++)
            {
                var swapIndex = Random.Next(0, shuffleWidth);

                var temp = ordered[swapIndex];
                ordered[swapIndex] = ordered[i];
                ordered[i] = temp;
            }

            return ordered;
        }
    }
}
