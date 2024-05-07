namespace Crossword
{
    class SquareValue
    {
        public Coordinate Coordinate { get; }

        public char Value { get; }

        public SquareValue(int i, int j, char v)
        {
            Coordinate = new Coordinate(i, j);
            Value = v;
        }
    }
}
