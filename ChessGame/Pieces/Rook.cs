using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ChessGame.Pieces
{
    class Rook : Piece
    {
        public Rook(PieceColor color, Board b) : base(color, b)
        {
            if (color == PieceColor.Black)
            {
                Image.Source = new BitmapImage(new Uri("../Images/black_rook.png", UriKind.RelativeOrAbsolute));
                Enum = PieceEnum.BlackRook;
            }
            else
            {
                Image.Source = new BitmapImage(new Uri("../Images/white_rook.png", UriKind.RelativeOrAbsolute));
                Enum = PieceEnum.WhiteRook;
            }
        }
        
        internal override List<Move> GetPotentialMoves()
        {
            List<Move> moves = new List<Move>();
            Position p = Board.GetPosition(this);
            int row = p.Row + 1;
            int col = p.Col;

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
