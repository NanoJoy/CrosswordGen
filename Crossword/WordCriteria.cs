using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossword
{
    class WordCriteria
    {
        public uint Length { get; }

        public LetterCriterion[] Letters { get; }

        public WordCriteria(uint length, params LetterCriterion[] letters)
        {
            Length = length;
            Letters = letters;
        }
    }
}
