using Crossword;

namespace CrossWeb.Providers
{
    public class WordFilterProvider : IWordFilterProvider
    {
        private WordFilter _filter;

        public WordFilter Filter
        {
            get
            {
                _filter ??= new WordFilter(@"D:\Projects\crossword\crosswordlist.txt");
                return _filter;
            }
        }
    }
}
