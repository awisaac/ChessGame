using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChessGame.Pieces;

namespace ChessGame
{
    class Board
    {
        private Square[,] board;
        private ulong hash;
        List<Square>[] pieces;
        
        public Board()
        {
            board = new Square[8, 8];
            pieces = new List<Square>[17];
            
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    board[i, j] = new Square(i, j, new Empty(this));
                }
            }

            for (int i = 0; i < 17; i++)
            {
                pieces[i] = new List<Square>();
            }

            hash = 0;

            Clear();
        }

        public void AddPiece(int row, int col, Piece p)
        {
            board[row, col].Piece = p;
            pieces[(int)p.Enum].Add(board[row, col]);            

            hash = hash ^ BoardHash.Hashes[(int)PieceEnum.EmptyPosition, row, col];
            hash = hash ^ BoardHash.Hashes[(int)p.Enum, row, col];
        }

        public void MovePiece(Position from, Position to)
        {
            Piece fromPiece = GetPiece(from.Row, from.Col);
            RemovePiece(from.Row, from.Col);
            AddPiece(to.Row, to.Col, fromPiece);
        }
        
        public Piece GetPiece(int row, int col)
        {
            return board[row, col].Piece;
        }

        public Position GetPosition(Piece p)
        {
            foreach (Square s in pieces[(int)p.Enum])
            {
                if (s.Piece == p)
                {
                    return s.GetPosition();
                }
            }

            throw new Exception("No piece found.");
        }

        public void RemovePiece(int row, int col)
        {
            Piece p = GetPiece(row, col);
            pieces[(int)p.Enum].Remove(board[row, col]);            

            hash = hash ^ BoardHash.Hashes[(int)p.Enum, row, col];

            board[row, col].Piece = new Empty(this);
            pieces[(int)board[row, col].Piece.Enum].Add(board[row, col]);

            hash = hash ^ BoardHash.Hashes[(int)PieceEnum.EmptyPosition, row, col];
        }

        public void Clear()
        {
            for (int i = 0; i < 17; i++)
            {
                pieces[i].Clear();
            }
            hash = 0;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    board[i, j].Piece = new Empty(this);
                    hash = hash ^ BoardHash.Hashes[(int)PieceEnum.EmptyPosition, i, j];
                }
            }
        }

        public List<Piece> GetAllPieces(PieceColor color)
        {
            List<Piece> allPieces = new List<Piece>();

            if (color == PieceColor.White)
            {
                for (int i = 0; i < 8; i++)
                {
                    pieces[i].ForEach(s => allPieces.Add(s.Piece));
                }
            }
            else
            {
                for (int i = 8; i < 16; i++)
                {
                    pieces[i].ForEach(s => allPieces.Add(s.Piece));
                }
            }

            return allPieces;
        }

        public void PromotePiece(int row, int col, Piece p)
        {
            p.PromotedFrom = GetPiece(row, col);
            RemovePiece(row, col);
            AddPiece(row, col, p);
        }

        public void DemotePiece(int row, int col)
        {
            Piece p = GetPiece(row, col);
            RemovePiece(row, col);
            AddPiece(row, col, p.PromotedFrom);
        }

        public void EnPassant(Pawn p, Boolean enPassant)
        {
            p.enPassant = enPassant;

            Square square = pieces[(int)p.Enum].Find(s => s.Piece == p);
            pieces[(int)p.Enum].Remove(square);

            if (enPassant)
            {
                if (p.Color == PieceColor.White)
                {                    
                    p.Enum = PieceEnum.WhitePawnEnPassant;
                }
                else
                {                    
                    p.Enum = PieceEnum.BlackPawnEnPassant;
                }                
            }
            else
            {
                if (p.Color == PieceColor.White)
                {                    
                    p.Enum = PieceEnum.WhitePawn;                    
                }
                else
                {                    
                    p.Enum = PieceEnum.BlackPawn;                    
                }
            }

            pieces[(int)p.Enum].Add(square);
        }

        public void KingMoved(King k, Boolean moved)
        {
            Square square = pieces[(int)k.Enum].Find(s => s.Piece == k);

            if (moved)
            {
                if (k.Color == PieceColor.White)
                {
                    k.Enum = PieceEnum.WhiteKingNoCastle;
                }
                else
                {
                    k.Enum = PieceEnum.BlackKingNoCastle;
                }
            }

            else
            {
                if (k.Color == PieceColor.White)
                {
                    k.Enum = PieceEnum.WhiteKing;
                }
                else
                {
                    k.Enum = PieceEnum.BlackKing;
                }
            }
            
            pieces[(int)k.Enum].Add(square);            
        }        

        public ulong GetHash()
        {
            return hash;
        }
    }

    class Square
    {
        int row;
        int col;
        internal Piece Piece { get; set; }

        internal Square(int r, int c, Piece p)
        {
            row = r;
            col = c;
            Piece = p;
        }

        internal Position GetPosition()
        {
            return new Position(row, col);
        }
    }
}
