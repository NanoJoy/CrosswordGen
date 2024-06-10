namespace Crossword
{
    public record PuzzleSpecification
    {
        public int Width { get; }

        public int Height { get; }

        public SquareValue[] ExistingValues { get; }

        public PuzzleSpecification(int width, int height, SquareValue[] existingValues)
        {
            Width = width;
            Height = height;
            ExistingValues = existingValues;
        }
    }
}
