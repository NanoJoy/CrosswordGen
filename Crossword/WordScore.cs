namespace Crossword
{
    public record WordScore
    {
        public string Word { get; }

        public int Score { get; }

        public WordScore(string word, int score)
        {
            Word = word;
            Score = score;
        }
    }
}
