using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ChessGame.Pieces
{
    class Knight : Piece
    {
        public Knight(PieceColor color, Board b) : base(color, b)
        {
            if (color == PieceColor.Black)
            {
                Image.Source = new BitmapImage(new Uri("../Images/black_knight.png", UriKind.RelativeOrAbsolute));
                Enum = PieceEnum.BlackKnight;
            }
            else
            {
                Image.Source = new BitmapImage(new Uri("../Images/white_knight.png", UriKind.RelativeOrAbsolute));
                Enum = PieceEnum.WhiteKnight;
            }
        }

        internal override List<Move> GetPotentialMoves()
        {
            List<Move> moves = new List<Move>();
            Position p = Board.GetPosition(this);
            int row = p.Row;
            int col = p.Col;

            if (row < 6 && col > 0 && Board.GetPiece(row + 2, col - 1).Color != Color)
            {
                moves.Add(new Move(p, new Position(row + 2, col - 1), this, Board.GetPiece(row + 2, col - 1), false));
            }

            if (row < 6 && col < 7 && Board.GetPiece(row + 2, col + 1).Color != Color)
            {
                moves.Add(new Move(p, new Position(row + 2, col + 1), this, Board.GetPiece(row + 2, col + 1), false));
            }

            if (row > 1 && col > 0 && Board.GetPiece(row - 2, col - 1).Color != Color)
            {
                moves.Add(new Move(p, new Position(row - 2, col - 1), this, Board.GetPiece(row - 2, col - 1), false));
            }

            if (row > 1 && col < 7 && Board.GetPiece(row - 2, col + 1).Color != Color)
            {
                moves.Add(new Move(p, new Position(row - 2, col + 1), this, Board.GetPiece(row - 2, col + 1), false));
            }

            if (row < 7 && col > 1 && Board.GetPiece(row + 1, col - 2).Color != Color)
            {
                moves.Add(new Move(p, new Position(row + 1, col - 2), this, Board.GetPiece(row + 1, col - 2), false));
            }

            if (row < 7 && col < 6 && Board.GetPiece(row + 1, col + 2).Color != Color)
            {
                moves.Add(new Move(p, new Position(row + 1, col + 2), this, Board.GetPiece(row + 1, col + 2), false));
            }

            if (row > 0 && col > 1 && Board.GetPiece(row - 1, col - 2).Color != Color)
            {
                moves.Add(new Move(p, new Position(row - 1, col - 2), this, Board.GetPiece(row - 1, col - 2), false));
            }

            if (row > 0 && col < 6 && Board.GetPiece(row - 1, col + 2).Color != Color)
            {
                moves.Add(new Move(p, new Position(row - 1, col + 2), this, Board.GetPiece(row - 1, col + 2), false));
            }

            return moves;
        }
    }
}
