using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Crossword
{
    class WordFilter
    {
        private const uint MaxNumLetters = 15;

        private const uint MinScoreToInclude = 60;

        private LetterFilter[] LetterFilters { get; }

        private List<string> AllWords { get; }

        private HashSet<string>[] WordsByLength { get; }

        public WordFilter(string filePath)
        {
            LetterFilters = new LetterFilter[MaxNumLetters];

            WordsByLength = new HashSet<string>[MaxNumLetters];

            for (uint i = 0; i < MaxNumLetters; i++)
            {
                LetterFilters[i] = new LetterFilter(i);
                WordsByLength[i] = new HashSet<string>();
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

                    for (int i = 0; i < MaxNumLetters; i++)
                    {
                        LetterFilters[i].AddWord(word);
                    }

                    WordsByLength[word.Length - 1].Add(word);
                }
            }
        }

        public List<string> GetMatchingWords(WordCriteria criteria)
        {
            var length = criteria.Length;
            var letterCriteria = criteria.Letters;

            foreach (var criterion in letterCriteria)
            {
                ValidateCriterion(criterion);
            }

            var matches = WordsByLength[length - 1].ToList();

            for (int i = 0; i < letterCriteria.Length; i++)
            {
                var criterion = letterCriteria[i];

                matches = matches.Where(w => LetterFilters[criterion.Position].CheckWord(w, criterion.Letter)).ToList();
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
            if (criterion.Position >= MaxNumLetters)
            {
                throw new Exception($"Criterion letter position {criterion.Position} is invalid: {criterion.Position}.");
            }
        }

        private static bool TryGetWord(string line, out string word)
        {
            var split = line.Split(';');

            word = split[0];
            var score = uint.Parse(split[1]);

            if (score < MinScoreToInclude || word.Length > MaxNumLetters || word.Any(c => c < 'A' || c > 'Z'))
            {
                word = null;
                return false;
            }

            return true;
        }
    }
}
