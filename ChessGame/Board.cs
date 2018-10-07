using System;
using System.Collections.Generic;
using System.Diagnostics;
using ChessGame.Pieces;

namespace ChessGame
{
    public class Board
    {
        private Piece[,] board;
        private ulong hash;

        private List<Piece> WhitePieces { get; set; }
        private List<Piece> BlackPieces { get; set; }    
        
        public Board()
        {
            board = new Piece[8, 8];
            WhitePieces = new List<Piece>();
            BlackPieces = new List<Piece>();
            
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    for (int k = 0; k < 17; k++)
                    {
                        board[i, j] = new Empty(i, j, this);
                    }                    
                }
            }

            hash = 0;
        }

        public Piece GetPiece(int row, int col)
        {
            return board[row, col];
        }

        public List<Piece> GetAllPieces(PieceColor color)
        {
            if (color == PieceColor.White)
            {
                return WhitePieces;
            }
            if (color == PieceColor.Black)
            {
                return BlackPieces;
            }

            return null;
        }

        public void AddPiece(int row, int col, Piece p)
        {
            RemovePiece(row, col);
            board[row, col] = p;
            p.Position = new Position(row, col);

            if (p.Color == PieceColor.White)
            {
                WhitePieces.Add(p);

                if (WhitePieces.Count > 16)
                {
                    Debug.WriteLine("White Piece Count > 16! ");
                }

            }
            else if (p.Color == PieceColor.Black)
            {
                BlackPieces.Add(p);

                if (WhitePieces.Count > 16)
                {
                    Debug.WriteLine("Black Piece Count > 16!");
                }
            }
                        
            hash = hash ^ BoardHash.Hashes[(int)PieceEnum.EmptyPosition, row, col];
            hash = hash ^ BoardHash.Hashes[(int)p.Enum, row, col];
        }

        internal void MovePiece(Position from, Position to)
        {
            Piece fromPiece = board[from.Row, from.Col];
            RemovePiece(from.Row, from.Col);
            AddPiece(to.Row, to.Col, fromPiece);
        }

        internal void RemovePiece(int row, int col)
        {
            Piece p = board[row, col];
            hash = hash ^ BoardHash.Hashes[(int)p.Enum, row, col];

            if (p.Color == PieceColor.White)
            {
                WhitePieces.Remove(p);
            }
            else if (p.Color == PieceColor.Black)
            {
                BlackPieces.Remove(p);
            }

            board[row, col] = new Empty(row, col, this);
            hash = hash ^ BoardHash.Hashes[(int)PieceEnum.EmptyPosition, row, col];
        }

        internal void Clear()
        {
            hash = 0;
            WhitePieces.Clear();
            BlackPieces.Clear();

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    board[i, j] = new Empty(i, j, this);
                    hash = hash ^ BoardHash.Hashes[(int)PieceEnum.EmptyPosition, i, j];
                }
            }
        }
                
        internal void PromotePawn(Move move)
        {
            RemovePiece(move.From.Row, move.From.Col);
            AddPiece(move.From.Row, move.From.Col, move.PromotedTo);

            //Debug.WriteLine(move.Piece.Color.ToString() + " Pawn Promoted");
        }
        
        internal void DemotePawn(Move move)
        {
            RemovePiece(move.To.Row, move.To.Col);
            AddPiece(move.To.Row, move.To.Col, move.PromotedFrom);
            
            //Debug.WriteLine(move.Piece.Color.ToString() + " Piece Demoted\n");
        }

        internal ulong GetHash()
        {
            return hash;
        }
    }    
}
