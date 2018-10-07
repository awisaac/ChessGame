namespace ChessGame
{
    public class Position
    {
        public int Row { get; }
        public int Col { get; }

        public Position (int row, int col)
        {
            Row = row;
            Col = col;
        }

        public bool Equals(Position p)
        {
            return Row == p.Row && Col == p.Col; 
        }
    }
}