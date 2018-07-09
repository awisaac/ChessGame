using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ChessGame.Pieces
{
    class Pawn : Piece
    {
        public bool enPassant { get; set; }

        public Pawn(PieceColor color, Board b) : base(color, b)
        {
            enPassant = false;

            if (color == PieceColor.Black)
            {
                Image.Source = new BitmapImage(new Uri("../Images/black_pawn.png", UriKind.RelativeOrAbsolute));
                Enum = PieceEnum.BlackPawn;
            }
            else
            {
                Image.Source = new BitmapImage(new Uri("../Images/white_pawn.png", UriKind.RelativeOrAbsolute));
                Enum = PieceEnum.WhitePawn;
            }
        }

        internal override List<Move> GetPotentialMoves()
        {
            List<Move> moves = new List<Move>();
            Position p = Board.GetPosition(this);
            Piece toPiece;

            if (Color == PieceColor.White)
            {
                if (p.Row > 0)
                {
                    // One ahead
                    toPiece = Board.GetPiece(p.Row - 1, p.Col);
                    if (toPiece is Empty)
                    {
                        if (Board.GetPosition(toPiece).Row == 0)
                        {
                            addPromotions(moves, toPiece);
                        }
                        else
                        {
                            moves.Add(new Move(p, Board.GetPosition(toPiece), (Piece)this, toPiece, false));
                        }                        
                    }

                    // Two ahead

                    if (p.Row > 1)
                    {
                        toPiece = Board.GetPiece(p.Row - 2, p.Col);
                        if (p.Row == 6 && toPiece is Empty)
                        {
                            moves.Add(new Move(p, Board.GetPosition(toPiece), this, toPiece, false));
                        }
                    }

                    // Move left capture
                    if (p.Col > 0)
                    {
                        toPiece = Board.GetPiece(p.Row - 1, p.Col - 1);
                        if (toPiece.Color == PieceColor.Black)
                        {
                            if (Board.GetPosition(toPiece).Row == 0)
                            {
                                addPromotions(moves, toPiece);
                            }
                            else
                            {
                                moves.Add(new Move(p, Board.GetPosition(toPiece), this, toPiece, false));
                            }
                        }

                        toPiece = Board.GetPiece(p.Row, p.Col - 1);
                        if (toPiece.Color == PieceColor.Black && toPiece is Pawn && ((Pawn)toPiece).enPassant)
                        {                            
                            moves.Add(new Move(p, new Position(p.Row - 1, p.Col - 1), this, toPiece, false));                            
                        }
                    }                        

                    // Move right capture
                    if (p.Col < 7)
                    {
                        toPiece = Board.GetPiece(p.Row - 1, p.Col + 1);
                        if (toPiece.Color == PieceColor.Black)
                        {
                            if (Board.GetPosition(toPiece).Row == 0)
                            {
                                addPromotions(moves, toPiece);
                            }
                            else
                            {
                                moves.Add(new Move(p, new Position(p.Row - 1, p.Col + 1), this, toPiece, false));
                            }
                        }

                        toPiece = Board.GetPiece(p.Row, p.Col + 1);
                        if (toPiece.Color == PieceColor.Black && toPiece is Pawn && ((Pawn)toPiece).enPassant)
                        {
                            moves.Add(new Move(p, new Position(p.Row - 1, p.Col + 1), this, toPiece, false));
                        }
                    }
                }
            }

            else
            {
                if (p.Row < 7)
                {
                    // One ahead
                    toPiece = Board.GetPiece(p.Row + 1, p.Col);
                    if (toPiece is Empty)
                    {
                        if (Board.GetPosition(toPiece).Row == 7)
                        {
                            addPromotions(moves, toPiece);
                        }
                        else
                        {
                            moves.Add(new Move(p, Board.GetPosition(toPiece), (Piece)this, toPiece, false));
                        }
                    }

                    // Two ahead                    
                    if (p.Row < 6)
                    {
                        toPiece = Board.GetPiece(p.Row + 2, p.Col);
                        if (p.Row == 1 && toPiece is Empty)
                        {
                            moves.Add(new Move(p, Board.GetPosition(toPiece), this, toPiece, false));
                        }
                    }

                    // Move left capture
                    if (p.Col > 0)
                    {
                        toPiece = Board.GetPiece(p.Row + 1, p.Col - 1);
                        if (toPiece.Color == PieceColor.White)
                        {
                            if (Board.GetPosition(toPiece).Row == 7)
                            {
                                addPromotions(moves, toPiece);
                            }
                            else
                            {
                                moves.Add(new Move(p, new Position(p.Row + 1, p.Col - 1), this, toPiece, false));
                            }
                        }

                        toPiece = Board.GetPiece(p.Row, p.Col - 1);
                        if (toPiece.Color == PieceColor.White && toPiece is Pawn && ((Pawn)toPiece).enPassant)
                        {
                            moves.Add(new Move(p, new Position(p.Row + 1, p.Col - 1), this, toPiece, false));
                        }
                    }

                    // Move right capture
                    if (p.Col < 7)
                    {
                        toPiece = Board.GetPiece(p.Row + 1, p.Col + 1);
                        if (toPiece.Color == PieceColor.White)
                        {
                            if (Board.GetPosition(toPiece).Row == 7)
                            {
                                addPromotions(moves, toPiece);
                            }
                            else
                            {
                                moves.Add(new Move(p, new Position(p.Row + 1, p.Col + 1), this, toPiece, false));
                            }
                        }

                        toPiece = Board.GetPiece(p.Row, p.Col + 1);
                        if (toPiece.Color == PieceColor.White && toPiece is Pawn && ((Pawn)toPiece).enPassant)
                        {
                            moves.Add(new Move(p, new Position(p.Row + 1, p.Col + 1), this, toPiece, false));
                        }
                    }
                }
            }

            return moves;
        }

        private void addPromotions(List<Move> moves, Piece toPiece)
        {
            Queen queen = new Queen(Color, Board);
            Bishop bishop = new Bishop(Color, Board);
            Knight knight = new Knight(Color, Board);
            Rook rook = new Rook(Color, Board);

            moves.Add(new Move(Board.GetPosition(this), Board.GetPosition(toPiece), queen, toPiece, true));
            moves.Add(new Move(Board.GetPosition(this), Board.GetPosition(toPiece), bishop, toPiece, true));
            moves.Add(new Move(Board.GetPosition(this), Board.GetPosition(toPiece), knight, toPiece, true));
            moves.Add(new Move(Board.GetPosition(this), Board.GetPosition(toPiece), rook, toPiece, true));
        }
    }
}
