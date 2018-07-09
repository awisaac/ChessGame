using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ChessGame.Pieces
{
    class Queen : Piece
    {
        public Queen(PieceColor color, Board b) : base(color, b)
        {
            if (color == PieceColor.Black)
            {
                Image.Source = new BitmapImage(new Uri("../Images/black_queen.png", UriKind.RelativeOrAbsolute));
                Enum = PieceEnum.BlackQueen;
            }
            else
            {
                Image.Source = new BitmapImage(new Uri("../Images/white_queen.png", UriKind.RelativeOrAbsolute));
                Enum = PieceEnum.WhiteQueen;
            }            
        }

        internal override List<Move> GetPotentialMoves()
        {
            List<Move> moves = new List<Move>();
            Position p = Board.GetPosition(this);

            int row = p.Row - 1;
            int col = p.Col - 1;

            while (row >= 0 && col >= 0 && Board.GetPiece(row, col) is Empty)
            {
                moves.Add(new Move(p, new Position(row, col), this, Board.GetPiece(row, col), false));
                row--;
                col--;
            }

            if (row >= 0 && col >= 0 && Board.GetPiece(row, col).Color != Color)
            {
                moves.Add(new Move(p, new Position(row, col), this, Board.GetPiece(row, col), false));
            }

            row = p.Row - 1;
            col = p.Col + 1;

            while (row >= 0 && col <= 7 && Board.GetPiece(row, col) is Empty)
            {
                moves.Add(new Move(p, new Position(row, col), this, Board.GetPiece(row, col), false));
                row--;
                col++;
            }

            if (row >= 0 && col <= 7 && Board.GetPiece(row, col).Color != Color)
            {
                moves.Add(new Move(p, new Position(row, col), this, Board.GetPiece(row, col), false));
            }

            row = p.Row + 1;
            col = p.Col - 1;

            while (row <= 7 && col >= 0 && Board.GetPiece(row, col) is Empty)
            {
                moves.Add(new Move(p, new Position(row, col), this, Board.GetPiece(row, col), false));
                row++;
                col--;
            }

            if (row <= 7 && col >= 0 && Board.GetPiece(row, col).Color != Color)
            {
                moves.Add(new Move(p, new Position(row, col), this, Board.GetPiece(row, col), false));
            }

            row = p.Row + 1;
            col = p.Col + 1;

            while (row <= 7 && col <= 7 && Board.GetPiece(row, col) is Empty)
            {
                moves.Add(new Move(p, new Position(row, col), this, Board.GetPiece(row, col), false));
                row++;
                col++;
            }

            if (row <= 7 && col <= 7 && Board.GetPiece(row, col).Color != Color)
            {
                moves.Add(new Move(p, new Position(row, col), this, Board.GetPiece(row, col), false));
            }

            row = p.Row + 1;
            col = p.Col;

            while (row <= 7 && Board.GetPiece(row, col) is Empty)
            {
                moves.Add(new Move(p, new Position(row, col), this, Board.GetPiece(row, col), false));
                row++;
            }

            if (row <= 7 && Board.GetPiece(row, col).Color != Color)
            {
                moves.Add(new Move(p, new Position(row, col), this, Board.GetPiece(row, col), false));
            }

            row = p.Row - 1;

            while (row >= 0 && Board.GetPiece(row, col) is Empty)
            {
                moves.Add(new Move(p, new Position(row, col), this, Board.GetPiece(row, col), false));
                row--;
            }

            if (row >= 0 && Board.GetPiece(row, col).Color != Color)
            {
                moves.Add(new Move(p, new Position(row, col), this, Board.GetPiece(row, col), false));
            }

            row = p.Row;
            col = p.Col + 1;

            while (col <= 7 && Board.GetPiece(row, col) is Empty)
            {
                moves.Add(new Move(p, new Position(row, col), this, Board.GetPiece(row, col), false));
                col++;
            }

            if (col <= 7 && Board.GetPiece(row, col).Color != Color)
            {
                moves.Add(new Move(p, new Position(row, col), this, Board.GetPiece(row, col), false));
            }

            col = p.Col - 1;

            while (col >= 0 && Board.GetPiece(row, col) is Empty)
            {
                moves.Add(new Move(p, new Position(row, col), this, Board.GetPiece(row, col), false));
                col--;
            }

            if (col >= 0 && Board.GetPiece(row, col).Color != Color)
            {
                moves.Add(new Move(p, new Position(row, col), this, Board.GetPiece(row, col), false));
            }

            return moves;
        }
    }
}
