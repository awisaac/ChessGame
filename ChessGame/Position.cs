namespace ChessGame
{
    internal class Position
    {
        public int Row { get; }
        public int Col { get; }

        public Position (int row, int col)
        {
            Row = row;
            Col = col;
        }

        public bool equals(Position p)
        {
            return Row == p.Row && Col == p.Col; 
        }
    }
}