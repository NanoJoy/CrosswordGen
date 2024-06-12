using Crossword;

namespace CrossWeb.Providers
{
    public interface IWordFilterProvider
    {
        public WordFilter Filter { get; }
    }
}
