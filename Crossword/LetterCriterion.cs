using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossword
{
    record LetterCriterion
    {
        public uint Position { get; }

        public char Letter { get; set; }

        public LetterCriterion(uint position, char letter)
        {
            Position = position;
            Letter = letter;
        }
    }
}
