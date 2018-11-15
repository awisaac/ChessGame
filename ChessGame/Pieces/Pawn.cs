using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ChessGame.Pieces
{
    public class Pawn : Piece
    {
        internal Pawn(PieceColor color, Board b, int index) : base(color, b, index)
        {
            if (Color == PieceColor.Black)
            {
                Enum = PieceEnum.BlackPawn;
            }
            else
            {
                Enum = PieceEnum.WhitePawn;
            }
        }

        public bool EnPassant()
        {
            return Enum == PieceEnum.BlackPawnEnPassant || Enum == PieceEnum.WhitePawnEnPassant;
        }

        internal override List<Move> GetPotentialMoves()
        {
            List<Move> moves = new List<Move>();
            Piece toPiece;

            if (Color == PieceColor.White)
            {
                if (Position.Row > 0)
                {
                    // One ahead
                    toPiece = GameBoard.GetPiece(Position.Row - 1, Position.Col);
                    if (toPiece is Empty)
                    {
                        if (toPiece.Position.Row == 0)
                        {
                            AddPromotions(moves, toPiece);
                        }
                        else
                        {
                            moves.Add(new Move(Position, toPiece.Position, this, toPiece));
                        }                        
                    }

                    // Two ahead

                    if (Position.Row == 6)
                    {
                        toPiece = GameBoard.GetPiece(Position.Row - 2, Position.Col);
                        if (toPiece is Empty && GameBoard.GetPiece(Position.Row - 1, Position.Col) is Empty)
                        {
                            moves.Add(new Move(Position, toPiece.Position, this, toPiece));
                        }
                    }

                    // Move left capture
                    if (Position.Col > 0)
                    {
                        toPiece = GameBoard.GetPiece(Position.Row - 1, Position.Col - 1);
                        if (toPiece.Color == PieceColor.Black)
                        {
                            if (toPiece.Position.Row == 0)
                            {
                                AddPromotions(moves, toPiece);
                            }
                            else
                            {
                                moves.Add(new Move(Position, toPiece.Position, this, toPiece));
                             }
                        }

                        toPiece = GameBoard.GetPiece(Position.Row, Position.Col - 1);
                        if (toPiece.Color == PieceColor.Black && toPiece is Pawn && ((Pawn)toPiece).EnPassant())
                        {
                            moves.Add(new Move(Position, new Position(Position.Row - 1, Position.Col - 1), this, toPiece) { EnPassantMove = true });      
                        }
                    }                        

                    // Move right capture
                    if (Position.Col < 7)
                    {
                        toPiece = GameBoard.GetPiece(Position.Row - 1, Position.Col + 1);
                        if (toPiece.Color == PieceColor.Black)
                        {
                            if (toPiece.Position.Row == 0)
                            {
                                AddPromotions(moves, toPiece);
                            }
                            else
                            {
                                moves.Add(new Move(Position, toPiece.Position, this, toPiece));
                            }
                        }

                        toPiece = GameBoard.GetPiece(Position.Row, Position.Col + 1);
                        if (toPiece.Color == PieceColor.Black && toPiece is Pawn && ((Pawn)toPiece).EnPassant())
                        {
                            moves.Add(new Move(Position, new Position(Position.Row - 1, Position.Col + 1), this, toPiece) { EnPassantMove = true });
                        }
                    }
                }
            }

            else
            {
                if (Position.Row < 7)
                {
                    // One ahead
                    toPiece = GameBoard.GetPiece(Position.Row + 1, Position.Col);
                    if (toPiece is Empty)
                    {
                        if (toPiece.Position.Row == 7)
                        {
                            AddPromotions(moves, toPiece);
                        }
                        else
                        {
                            moves.Add(new Move(Position, toPiece.Position, this, toPiece));
                        }
                    }

                    // Two ahead                    
                    if (Position.Row == 1)
                    {
                        toPiece = GameBoard.GetPiece(Position.Row + 2, Position.Col);
                        if (toPiece is Empty && GameBoard.GetPiece(Position.Row + 1, Position.Col) is Empty)
                        {
                            moves.Add(new Move(Position, toPiece.Position, this, toPiece));
                        }
                    }

                    // Move left capture
                    if (Position.Col > 0)
                    {
                        toPiece = GameBoard.GetPiece(Position.Row + 1, Position.Col - 1);
                        if (toPiece.Color == PieceColor.White)
                        {
                            if (toPiece.Position.Row == 7)
                            {
                                AddPromotions(moves, toPiece);
                            }
                            else
                            {
                                moves.Add(new Move(Position, toPiece.Position, this, toPiece));
                            }
                        }

                        toPiece = GameBoard.GetPiece(Position.Row, Position.Col - 1);
                        if (toPiece.Color == PieceColor.White && toPiece is Pawn && ((Pawn)toPiece).EnPassant())
                        {
                            moves.Add(new Move(Position, new Position(Position.Row + 1, Position.Col - 1), this, toPiece) { EnPassantMove = true });
                        }
                    }

                    // Move right capture
                    if (Position.Col < 7)
                    {
                        toPiece = GameBoard.GetPiece(Position.Row + 1, Position.Col + 1);
                        if (toPiece.Color == PieceColor.White)
                        {
                            if (toPiece.Position.Row == 7)
                            {
                                AddPromotions(moves, toPiece);
                            }
                            else
                            {
                                moves.Add(new Move(Position, new Position(Position.Row + 1, Position.Col + 1), this, toPiece));
                            }
                        }

                        toPiece = GameBoard.GetPiece(Position.Row, Position.Col + 1);
                        if (toPiece.Color == PieceColor.White && toPiece is Pawn && ((Pawn)toPiece).EnPassant())
                        {
                            moves.Add(new Move(Position, new Position(Position.Row + 1, Position.Col + 1), this, toPiece) { EnPassantMove = true });
                        }
                    }
                }
            }

            return moves;
        }

        private void AddPromotions(List<Move> moves, Piece toPiece)
        {
            Move move = new Move(Position, toPiece.Position, this, toPiece) { Promotion = true };
            move.AddPromotion(this, new Queen(Color, GameBoard, Index));
            moves.Add(move); 

            move = new Move(Position, toPiece.Position, this, toPiece) { Promotion = true };
            move.AddPromotion(this, new Bishop(Color, GameBoard, Index));
            moves.Add(move);

            move = new Move(Position, toPiece.Position, this, toPiece) { Promotion = true };
            move.AddPromotion(this, new Knight(Color, GameBoard, Index));
            moves.Add(move); 

            move = new Move(Position, toPiece.Position, this, toPiece) { Promotion = true };
            move.AddPromotion(this, new Rook(Color, GameBoard, Index));
            moves.Add(move);            
        }
    }
}
