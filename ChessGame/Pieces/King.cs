using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ChessGame.Pieces
{
    class King : Piece
    {
        public King(PieceColor color, Board b) : base(color, b)
        {
            if (color == PieceColor.Black)
            {
                Image.Source = new BitmapImage(new Uri("../Images/black_king.png", UriKind.RelativeOrAbsolute));
                Enum = PieceEnum.BlackKing;
            }
            else
            {
                Image.Source = new BitmapImage(new Uri("../Images/white_king.png", UriKind.RelativeOrAbsolute));
                Enum = PieceEnum.WhiteKing;
            }
        }

        internal override List<Move> GetPotentialMoves()
        {
            List<Move> moves = new List<Move>();
            Position p = Board.GetPosition(this);

            int row = p.Row;
            int col = p.Col;

            if (row > 0 && col > 0 && Board.GetPiece(row - 1, col - 1).Color != Color)
            {
                moves.Add(new Move(p, new Position(row - 1, col - 1), this, Board.GetPiece(row - 1, col - 1), false));
            }

            if (row < 7 && col < 7 && Board.GetPiece(row + 1, col + 1).Color != Color)
            {
                moves.Add(new Move(p, new Position(row + 1, col + 1), this, Board.GetPiece(row + 1, col + 1), false));
            }

            if (row > 0 && col < 7 && Board.GetPiece(row - 1, col + 1).Color != Color)
            {
                moves.Add(new Move(p, new Position(row - 1, col + 1), this, Board.GetPiece(row - 1, col + 1), false));
            }

            if (col > 0 && row < 7 && Board.GetPiece(row + 1, col - 1).Color != Color)
            {
                moves.Add(new Move(p, new Position(row + 1, col - 1), this, Board.GetPiece(row + 1, col - 1), false));
            }

            if (col < 7 && Board.GetPiece(row, col + 1).Color != Color)
            {
                moves.Add(new Move(p, new Position(row, col + 1), this, Board.GetPiece(row, col + 1), false));
            }
            
            if (col > 0 && Board.GetPiece(row, col - 1).Color != Color)
            {
                moves.Add(new Move(p, new Position(row, col - 1), this, Board.GetPiece(row, col - 1), false));
            }

            if (row < 7 && Board.GetPiece(row + 1, col).Color != Color)
            {
                moves.Add(new Move(p, new Position(row + 1, col), this, Board.GetPiece(row + 1, col), false));
            }

            if (row > 0 && Board.GetPiece(row - 1, col).Color != Color)
            {
                moves.Add(new Move(p, new Position(row - 1, col), this, Board.GetPiece(row - 1, col), false));
            }

            if (MoveCount == 0 && Board.GetPiece(row, 0).MoveCount == 0 && Board.GetPiece(row, 1) is Empty
                && Board.GetPiece(row, 2) is Empty && Board.GetPiece(row, 3) is Empty)
            {
                moves.Add(new Move(p, new Position(row, col - 2), this, Board.GetPiece(row, col - 2), false));
            }

            if (MoveCount == 0 && Board.GetPiece(row, 7).MoveCount == 0 && Board.GetPiece(row, 5) is Empty
                && Board.GetPiece(row, 6) is Empty)
            {
                moves.Add(new Move(p, new Position(row, col + 2), this, Board.GetPiece(row, col + 2), false));
            }

            return moves;
        }
    }
}
