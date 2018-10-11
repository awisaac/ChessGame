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
        private GameEngine Engine;

        private Piece[] WhitePieces { get; set; }
        private Piece[] BlackPieces { get; set; }
        
        internal Board(GameEngine engine)
        {
            board = new Piece[8, 8];
            WhitePieces = new Piece[16];
            BlackPieces = new Piece[16];
            Engine = engine;
            
            hash = 0;
        }

        public Piece GetPiece(int row, int col)
        {
            return board[row, col];
        }

        public Piece[] GetAllPieces(PieceColor color)
        {
            if (color == PieceColor.White) { return WhitePieces; }
            if (color == PieceColor.Black) { return BlackPieces; }
            return null;
        }

        public void AddPiece(int row, int col, Piece p)
        {
            RemovePiece(row, col);
            board[row, col] = p;
            p.Position = new Position(row, col);

            if (p.Color == PieceColor.White) { WhitePieces[p.Index] = p; }
            else if (p.Color == PieceColor.Black) { BlackPieces[p.Index] = p; }

            hash = hash ^ BoardHash.Hashes[(int)PieceEnum.EmptyPosition, row, col];
            hash = hash ^ BoardHash.Hashes[(int)p.Enum, row, col];
        }

        internal void MovePiece(Position from, Position to)
        {
            Piece fromPiece = board[from.Row, from.Col];
            Piece toPiece = board[to.Row, to.Col];

            RemovePiece(from.Row, from.Col);
            AddPiece(to.Row, to.Col, fromPiece);
        }
        
        internal void RemovePiece(int row, int col)
        {
            Piece p = board[row, col];

            hash = hash ^ BoardHash.Hashes[(int)p.Enum, row, col];

            if (board[row, col].Color == PieceColor.White) { WhitePieces[p.Index] = null; }
            else if (board[row, col].Color == PieceColor.Black) { BlackPieces[p.Index] = null; }

            board[row, col] = new Empty(row, col, Engine, this);
            hash = hash ^ BoardHash.Hashes[(int)PieceEnum.EmptyPosition, row, col];
        }

        internal void Clear()
        {
            hash = 0;
            WhitePieces = new Piece[16];
            BlackPieces = new Piece[16];

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    board[i, j] = new Empty(i, j, Engine, this);
                    hash = hash ^ BoardHash.Hashes[(int)PieceEnum.EmptyPosition, i, j];
                }
            }
        }
        
        internal void PromotePawn(Move move)
        {
            RemovePiece(move.From.Row, move.From.Col);
            AddPiece(move.From.Row, move.From.Col, move.PromotedTo);
        }
        
        internal void DemotePawn(Move move)
        {
            RemovePiece(move.To.Row, move.To.Col);
            AddPiece(move.To.Row, move.To.Col, move.PromotedFrom);
        }

        internal ulong GetHash()
        {
            return hash;
        }

        internal void UpdatePieceEnum(Piece p, PieceEnum pieceEnum)
        {
            hash = hash ^ BoardHash.Hashes[(int)p.Enum, p.Position.Row, p.Position.Col];
            p.Enum = pieceEnum;
            hash = hash ^ BoardHash.Hashes[(int)p.Enum, p.Position.Row, p.Position.Col];
        }
    }    
}
