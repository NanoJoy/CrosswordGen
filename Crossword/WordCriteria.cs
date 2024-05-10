
using System.Linq;

namespace Crossword
{
    class WordCriteria
    {
        private int _numFilledLetters;

        public int Length { get; }

        public LetterCriterion[] Letters { get; }

        public int NumFilledLetters => _numFilledLetters;

        public WordCriteria(int length, params LetterCriterion[] letters)
        {
            Length = length;
            Letters = letters.OrderBy(l => l.Position).ToArray();
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
