using Crossword;
using System.Text;

namespace CrossWeb.Providers
{
    public class WordFilterProvider : IWordFilterProvider
    {
        private WordFilter _filter;

        public WordFilter Filter
        {
            get
            {
                if (_filter == null)
                {
                    var url = "https://peterbroda.me/crosswords/wordlist/lists/peter-broda-wordlist__gridtext__scored__july-25-2023.txt";
                    using var client = new HttpClient();
                    var task = client.GetStringAsync(url);
                    task.Wait();
                    var wordListContent = task.Result;
                    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(wordListContent));
                    _filter = new WordFilter(stream);
                }
                
                return _filter;
            }
        }
    }
}
