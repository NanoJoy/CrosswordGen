namespace Crossword
{
    record PuzzleSpecification
    {
        public uint Width { get; }

        public uint Height { get; }

        public SquareValue[] ExistingValues { get; }

        public PuzzleSpecification(uint width, uint height, SquareValue[] existingValues)
        {
            Width = width;
            Height = height;
            ExistingValues = existingValues;
        }
    }
}
