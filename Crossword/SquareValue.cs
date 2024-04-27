namespace Crossword
{
    class SquareValue
    {
        public Coordinate Coordinate { get; }

        public char Value { get; }

        public SquareValue(uint i, uint j, char v)
        {
            Coordinate = new Coordinate(i, j);
            Value = v;
        }
    }
}
