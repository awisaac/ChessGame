﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using ChessGame.Pieces;

namespace ChessGame
{
    public class Board
    {
        private Piece[,] board;
        public long Hash { get; set; }
        private GameEngine Engine;

        private Piece[] WhitePieces { get; set; }
        private Piece[] BlackPieces { get; set; }
        
        internal Board(GameEngine engine)
        {
            board = new Piece[8, 8];
            WhitePieces = new Piece[16];
            BlackPieces = new Piece[16];
            Engine = engine;
            
            Hash = 0;
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

            Hash = Hash ^ BoardHash.Hashes[(int)PieceEnum.EmptyPosition, row, col];
            Hash = Hash ^ BoardHash.Hashes[(int)p.Enum, row, col];
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

            Hash = Hash ^ BoardHash.Hashes[(int)p.Enum, row, col];

            if (board[row, col].Color == PieceColor.White) { WhitePieces[p.Index] = null; }
            else if (board[row, col].Color == PieceColor.Black) { BlackPieces[p.Index] = null; }

            board[row, col] = new Empty(row, col, this);
            Hash = Hash ^ BoardHash.Hashes[(int)PieceEnum.EmptyPosition, row, col];
        }

        internal void Clear()
        {
            Hash = 0;
            WhitePieces = new Piece[16];
            BlackPieces = new Piece[16];

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    board[i, j] = new Empty(i, j, this);
                    Hash = Hash ^ BoardHash.Hashes[(int)PieceEnum.EmptyPosition, i, j];
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

        internal void UpdatePieceEnum(Piece p, PieceEnum pieceEnum)
        {
            Hash = Hash ^ BoardHash.Hashes[(int)p.Enum, p.Position.Row, p.Position.Col];
            p.Enum = pieceEnum;
            Hash = Hash ^ BoardHash.Hashes[(int)p.Enum, p.Position.Row, p.Position.Col];
        }

        internal King GetKing(PieceColor color)
        {
            if (color == PieceColor.White)
            {
                return WhitePieces[15] as King;
            }
            else
            {
                return BlackPieces[15] as King;
            }
        }        
    }    
}
