using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossword
{
    class LetterFilter
    {
        private const uint NumLetters = 26;

        private const uint AsciiLettersStart = 'A';

        private uint LetterPosition { get; }

        private HashSet<string>[] WordsByLetter { get; }

        public LetterFilter(uint letterPosition)
        {
            LetterPosition = letterPosition;
            WordsByLetter = new HashSet<string>[NumLetters];
            for (int i = 0; i < NumLetters; i++)
            {
                WordsByLetter[i] = new HashSet<string>();
            }
        }

        public void AddWord(string word)
        {
            if (word.Length - 1 < LetterPosition)
            {
                return;
            }

            var letter = word[(int)LetterPosition];

            if (letter < 'A' || letter > 'Z')
            {
                throw new Exception($"Invalid letter in word \"{word}\" at position {LetterPosition}.");
            }

            WordsByLetter[letter - AsciiLettersStart].Add(word);
        }

        public bool CheckWord(string word, char c)
        {
            if (c < 'A' || c > 'Z')
            {
                throw new Exception($"Character to check '{c}' is not a capital letter.");
            }

            return WordsByLetter[c - AsciiLettersStart].Contains(word);
        }

        public IEnumerable<string> GetMatchingWords(char letter)
        {
            return WordsByLetter[letter - AsciiLettersStart].ToArray();
        }
    }
}
