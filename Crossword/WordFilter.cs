using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Crossword
{
    class WordFilter
    {
        private const uint NumLetters = 5;

        private LetterFilter[] LetterFilters { get; }

        private List<string> AllWords { get; }

        public WordFilter(string filePath)
        {
            LetterFilters = new LetterFilter[NumLetters];

            for (uint i = 0; i < NumLetters; i++)
            {
                LetterFilters[i] = new LetterFilter(i);
            }

            AllWords = new List<string>();

            using var stream = File.OpenRead(filePath);

            using var streamReader = new StreamReader(stream);

            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadLine();

                if (TryGetWord(line, out var word))
                {
                    AllWords.Add(word);

                    for (int i = 0; i < NumLetters; i++)
                    {
                        LetterFilters[i].AddWord(word);
                    }
                }
            }
        }

        public List<string> GetMatchingWords(LetterCriterion[] criteria)
        {
            if (criteria.Length == 0)
            {
                return AllWords;
            }

            foreach (var criterion in criteria)
            {
                ValidateCriterion(criterion);
            }

            var matches = LetterFilters[criteria[0].Position].GetMatchingWords(criteria[0].Letter);

            for (int i = 1; i < criteria.Length; i++)
            {
                var criterion = criteria[i];

                matches = matches.Where(w => LetterFilters[criterion.Position].CheckWord(w, criterion.Letter));
            }

            return matches.ToList();
        }

        public bool HasMatchingWords(LetterCriterion[] criteria)
        {
            if (criteria.Length == 0)
            {
                return true;
            }

            foreach (var criterion in criteria)
            {
                ValidateCriterion(criterion);
            }

            var matches = LetterFilters[criteria[0].Position].GetMatchingWords(criteria[0].Letter);

            if (!matches.Any())
            {
                return false;
            }

            for (int i = 1; i < criteria.Length; i++)
            {
                var criterion = criteria[i];

                matches = matches.Where(w => LetterFilters[criterion.Position].CheckWord(w, criterion.Letter));

                if (!matches.Any())
                {
                    return false;
                }
            }

            return true;
        }

        public bool HasMatchingWord(string word)
        {
            LetterCriterion[] criteria = new LetterCriterion[word.Length];
            uint i = 0;

            foreach (var c in word)
            {
                criteria[i] = new LetterCriterion(i, c);
                i++;
            }

            return HasMatchingWords(criteria);
        }

        private static void ValidateCriterion(LetterCriterion criterion)
        {
            if (criterion.Letter < 'A' || criterion.Letter > 'Z')
            {
                throw new Exception($"Invalid letter for criterion: '{criterion.Letter}'.");
            }
            if (criterion.Position >= NumLetters)
            {
                throw new Exception($"Criterion letter position {criterion.Position} is invalid: {criterion.Position}.");
            }
        }

        private static bool TryGetWord(string line, out string word)
        {
            var split = line.Split(';');

            word = split[0];

            if (word.Length != NumLetters || word.Any(c => c < 'A' || c > 'Z'))
            {
                word = null;
                return false;
            }

            return true;
        }
    }
}
