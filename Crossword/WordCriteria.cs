
using System.Linq;

namespace Crossword
{
    public class WordCriteria
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

        public WordCriteria Copy()
        {
            var criteriaCopy = new LetterCriterion[Letters.Length];

            for (int i = 0; i < Letters.Length; i++)
            {
                criteriaCopy[i] = new LetterCriterion(Letters[i].Position, Letters[i].Letter);
            }

            return new WordCriteria(Length, criteriaCopy);
        }
    }
}
