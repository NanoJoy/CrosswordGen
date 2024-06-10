using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossword
{
    public record LetterCriterion
    {
        public int Position { get; }

        public char Letter { get; set; }

        public LetterCriterion(int position, char letter)
        {
            Position = position;
            Letter = letter;
        }
    }
}
