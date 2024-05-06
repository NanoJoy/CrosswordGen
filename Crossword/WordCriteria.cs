
using System.Linq;

namespace Crossword
{
    class WordCriteria
    {
        private int _numFilledLetters;

        public uint Length { get; }

        public LetterCriterion[] Letters { get; }

        public int NumFilledLetters => _numFilledLetters;

        public WordCriteria(uint length, params LetterCriterion[] letters)
        {
            Length = length;
            Letters = letters;
            _numFilledLetters = letters.Count(l => l.Letter != '-');
        }

        public void SetLetter(int position, char letter)
        {
            char previous = Letters[position].Letter;

            if (previous == '-')
            {
                if (letter != '-')
                {
                    _numFilledLetters++;
                }
            }
            else if (letter == '-')
            {
                _numFilledLetters--;
            }

            Letters[position].Letter = letter;
        }
    }
}
