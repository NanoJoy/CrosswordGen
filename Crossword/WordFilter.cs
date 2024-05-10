﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Crossword
{
    class WordFilter
    {
        private const int MaxNumLetters = 15;

        private const int MinScoreToInclude = 50;

        private LetterFilter[] LetterFilters { get; }

        private List<string> AllWords { get; }

        private HashSet<string>[] WordsByLength { get; }

        public WordFilter(string filePath)
        {
            LetterFilters = new LetterFilter[MaxNumLetters];

            WordsByLength = new HashSet<string>[MaxNumLetters];

            for (int i = 0; i < MaxNumLetters; i++)
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

            List<string> matches = null;

            int i = letterCriteria.Length - 1;
            bool anyLettersSpecified = false;

            while (matches == null && i >= 0)
            {
                var criterion = letterCriteria[i];

                if (criterion.Letter != '-')
                {
                    anyLettersSpecified = true;
                    matches = LetterFilters[criterion.Position]
                        .GetMatchingWords(criterion.Letter)
                        .Where(w => w.Length == criteria.Length)
                        .ToList();
                }

                i--;
            }

            if (matches == null)
            {
                if (anyLettersSpecified)
                {
                    return new List<string>();
                }
                return WordsByLength[length - 1].ToList();
            }

            while (i >= 0)
            {
                var criterion = letterCriteria[i];

                if (criterion.Letter != '-')
                {
                    matches = matches.Where(w => w[criterion.Position] == criterion.Letter).ToList();
                }

                i--;
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

                if (criterion.Letter != '-')
                {
                    matches = matches.Where(w => LetterFilters[criterion.Position].CheckWord(w, criterion.Letter));
                }

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
            int i = 0;

            foreach (var c in word)
            {
                criteria[i] = new LetterCriterion(i, c);
                i++;
            }

            return HasMatchingWords(criteria);
        }

        private static void ValidateCriterion(LetterCriterion criterion)
        {
            if (criterion.Letter != '-' && (criterion.Letter < 'A' || criterion.Letter > 'Z'))
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
            var score = int.Parse(split[1]);

            if (score < MinScoreToInclude || word.Length > MaxNumLetters || word.Any(c => c < 'A' || c > 'Z'))
            {
                word = null;
                return false;
            }

            return true;
        }
    }
}
