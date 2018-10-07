using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ChessGame.Pieces;

namespace ChessGame
{
    internal class GameEngine
    {
        public Board Board { get; set; }        
        public PieceColor CurrentTurn { get; set; }
        public bool BlackHumanPlayer { get; set; }
        public bool WhiteHumanPlayer { get; set; }
        public bool MidGame { get; set; }
        public bool GameOver { get; set; }
        private Stack<Move> moves;
        private Stack<ulong> prevHashes;
        private Stack<int> progress;
        private ChessView chessView;
        private delegate void ShowMoveDelegate(Move move);

        public GameEngine(ChessView main)
        {
            Board = new Board();
            moves = new Stack<Move>();
            prevHashes = new Stack<ulong>();
            CurrentTurn = PieceColor.White;
            WhiteHumanPlayer = true;
            progress = new Stack<int>();
            chessView = main;
        }

        public List<Piece> GetAllPieces(PieceColor color)
        {
            return Board.GetAllPieces(color);
        }
                
        internal void SetUpPieces()
        {
            GameOver = false;
            Board.Clear();
            moves.Clear();
            chessView.ClearPieces();

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Board.AddPiece(i, j, new Empty(i, j, Board));
                }
            }

            for (int i = 0; i < 8; i++)
            {
                AddPiece(1, i, new Pawn(PieceColor.Black, Board));
                AddPiece(6, i, new Pawn(PieceColor.White, Board));                
            }

            AddPiece(0, 0, new Rook(PieceColor.Black, Board));
            AddPiece(0, 7, new Rook(PieceColor.Black, Board));
            AddPiece(7, 0, new Rook(PieceColor.White, Board));
            AddPiece(7, 7, new Rook(PieceColor.White, Board));

            AddPiece(0, 1, new Knight(PieceColor.Black, Board));
            AddPiece(0, 6, new Knight(PieceColor.Black, Board));
            AddPiece(7, 1, new Knight(PieceColor.White, Board));
            AddPiece(7, 6, new Knight(PieceColor.White, Board));

            AddPiece(0, 2, new Bishop(PieceColor.Black, Board));
            AddPiece(0, 5, new Bishop(PieceColor.Black, Board));
            AddPiece(7, 2, new Bishop(PieceColor.White, Board));
            AddPiece(7, 5, new Bishop(PieceColor.White, Board));

            AddPiece(0, 3, new Queen(PieceColor.Black, Board));
            AddPiece(7, 3, new Queen(PieceColor.White, Board));

            AddPiece(0, 4, new King(PieceColor.Black, Board));
            AddPiece(7, 4, new King(PieceColor.White, Board));

            MidGame = true;

            prevHashes.Push(Board.GetHash());
        }

        public void AddPiece(int row, int col, Piece piece)
        {
            Board.AddPiece(row, col, piece);
            chessView.AddPiece(row, col, piece);
        }

        
        internal bool IsCheck(PieceColor color)
        {
            if (color == PieceColor.Black)
            {
                List<Move> moves = GetAllMoves(PieceColor.White);

                foreach (Move move in moves)
                {
                    if (move.Capture is King)
                    {
                        return true;
                    }
                }

                return false;
            }

            else
            {
                List<Move> moves = GetAllMoves(PieceColor.Black);

                foreach (Move move in moves)
                {
                    if (move.Capture is King)
                    {
                        return true;
                    }
                }
                
                return false;
            }
        }

        public List<Move> GetAllMoves(PieceColor color)
        {
            List<Move> moves = new List<Move>();

            foreach (Piece p in Board.GetAllPieces(color))
            {
                foreach (Move m in p.GetPotentialMoves())
                {
                    moves.Add(m);
                }
            }

            return moves;
        }
        
        internal string WriteMove(Move move)
        {
            string moveString = move.Piece.Enum.ToString();
            moveString += ": " + Convert.ToString((char)(move.From.Row + 65)) + move.From.Col;

            if (move.Capture is Empty)
            {
                moveString += "-";
            }
            else
            {
                moveString += "x";
            }
            moveString += Convert.ToString((char)(move.To.Row + 65)) + move.To.Col;

            moveString += " " + move.Piece.MoveCount + " moves";
            return moveString;
        }

        internal void ShowMove(Move move)
        {
            chessView.MovePiece(move);
        }

        internal void MovePiece(Move move, bool inSimulation)
        {
            Position to = move.To;
            Position from = move.From;

            // Pawn promotion
            if (move.Promotion) { Board.PromotePawn(move); }
                        
            if (move.Piece is Pawn)
            {
                Pawn pawn = move.Piece as Pawn;

                if (move.From.Row == 6 && move.To.Row == 4) { pawn.Enum = PieceEnum.WhitePawnEnPassant; }        
                if (move.From.Row == 1 && move.To.Row == 3) { pawn.Enum = PieceEnum.BlackPawnEnPassant; }                
            }            

            if (move.Piece is King)
            {
                King king = move.Piece as King;

                if (move.From.Col - move.To.Col == 2)
                {
                    MovePiece(new Move(new Position(move.From.Row, 0), new Position(move.From.Row, 3),
                        Board.GetPiece(move.From.Row, 0), Board.GetPiece(move.From.Row, 3), false), inSimulation);
                }

                if (move.From.Col - move.To.Col == -2)
                {
                    MovePiece(new Move(new Position(move.From.Row, 7), new Position(move.From.Row, 5),
                        Board.GetPiece(move.From.Row, 7), Board.GetPiece(move.From.Row, 5), false), inSimulation);
                }

                if (king.Color == PieceColor.White) { king.Enum = PieceEnum.WhiteKingNoCastle; }
                else if (king.Color == PieceColor.Black) { king.Enum = PieceEnum.BlackKingNoCastle; }
            }

            if (!(move.Piece is Pawn) || move.Capture is Empty)
            {
                if (progress.Count > 0) { progress.Push(progress.Peek() + 1); }
                else { progress.Push(1); }
            }
            else { progress.Push(0); }

            if (!(move.Capture is Empty)) { Board.RemovePiece(move.Capture.Position.Row, move.Capture.Position.Col); }
            Board.MovePiece(from, to);
            move.Piece.MoveCount++;

            moves.Push(move);
            prevHashes.Push(Board.GetHash());

            if (CurrentTurn == PieceColor.White) { CurrentTurn = PieceColor.Black; }
            else { CurrentTurn = PieceColor.White; }

            if (!inSimulation) { ReportToUser(); }
        }

        internal void ReportToUser()
        {
            if (((CurrentTurn == PieceColor.White && WhiteHumanPlayer)
                || (CurrentTurn == PieceColor.Black && BlackHumanPlayer)))
            {
                if (IsCheckmate(CurrentTurn))
                {
                    GameOver = true;
                    MessageBox.Show(CurrentTurn.ToString() + " wins!", "Checkmate");
                }

                if (IsStalemate(CurrentTurn)
                    && ((CurrentTurn == PieceColor.White && WhiteHumanPlayer)
                    || (CurrentTurn == PieceColor.Black && BlackHumanPlayer)))
                {
                    GameOver = true;
                    MessageBox.Show("Game is a draw!", "Stalemate");
                }

                if (IsThreeFoldRepetition() && (WhiteHumanPlayer || BlackHumanPlayer))
                {
                    MessageBoxResult response = MessageBox.Show("Do you want to declare a draw?",
                        "Threefold Repetition", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                    if (response == MessageBoxResult.Yes)
                    {
                        GameOver = true;
                        MessageBox.Show("Game is a draw!", "Threefold Repetition");
                    }
                }
            }            

            if (!GameOver)
            {
                if ((CurrentTurn == PieceColor.White && !WhiteHumanPlayer)
                || (CurrentTurn == PieceColor.Black && !BlackHumanPlayer))
                {
                    Adam adam = new Adam(this, CurrentTurn);
                    adam.RunSimulation();
                }
            }                            
        }

        public void UnMovePiece(Move move, bool inSimulation)
        {
            Position to = move.To;
            Position from = move.From;

            // Pawn demotion
            if (move.Promotion) { Board.DemotePawn(move); }

            if (move.Piece is Pawn)
            {
                Pawn pawn = move.Piece as Pawn;

                if (move.From.Row == 6 && move.To.Row == 4) { pawn.Enum = PieceEnum.WhitePawn; }
                else if (move.From.Row == 1 && move.To.Row == 3) { pawn.Enum = PieceEnum.BlackPawn; }
            }

            if (move.Piece is King)
            {
                King king = move.Piece as King;

                if (move.From.Col - move.To.Col == 2)
                {
                    MovePiece(new Move(new Position(move.From.Row, 3), new Position(move.From.Row, 0),
                        Board.GetPiece(move.From.Row, 3), Board.GetPiece(move.From.Row, 0), false), inSimulation);
                }

                if (move.From.Col - move.To.Col == -2)
                {
                    MovePiece(new Move(new Position(move.From.Row, 5), new Position(move.From.Row, 7),
                        Board.GetPiece(move.From.Row, 5), Board.GetPiece(move.From.Row, 7), false), inSimulation);
                }

                if (move.Piece.MoveCount == 0)
                {
                    if (king.Color == PieceColor.White) { king.Enum = PieceEnum.WhiteKing; }                    
                    else if (king.Color == PieceColor.Black) { king.Enum = PieceEnum.BlackKing; }
                }
            }

            Board.MovePiece(to, from);
            move.Piece.MoveCount--;

            if (!(move.Capture is Empty)) { Board.AddPiece(move.Capture.Position.Row, move.Capture.Position.Col, move.Capture); }
                       
            progress.Pop();
            moves.Pop();
            prevHashes.Pop();

            if (CurrentTurn == PieceColor.White) { CurrentTurn = PieceColor.Black; }
            else { CurrentTurn = PieceColor.White; }
        }

        internal bool CheckBlock(Move move, PieceColor color)
        {
            if (IsCheck(color))
            {
                // apply move to see if removes check
                MovePiece(move, false);

                if (!IsCheck(color))
                {
                    UnMovePiece(move, false);
                    return false;
                }
                else
                {
                    UnMovePiece(move, false);
                    return true;
                }
            }

            return false;
        }

        public bool ValidHumanMove(Piece piece, Position to)
        {
            List<Move> moves = piece.GetPotentialMoves();

            foreach (Move m in moves)
            {
                if (!GameOver
                    && m.Piece.Color == CurrentTurn
                    && m.To.Equals(to)
                    && m.From.Equals(piece.Position)
                    && !CheckBlock(m, piece.Color))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsCheckmate(PieceColor color)
        {
            List<Move> moves = GetAllMoves(color);

            foreach (Move m in moves)
            {
                if (m.Capture is King)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsStalemate(PieceColor color)
        {
            if (!IsCheck(color))
            {
                List<Move> moves = GetAllMoves(color);

                foreach (Move m in moves)
                {
                    MovePiece(m, true);

                    if (IsCheck(color))
                    {
                        UnMovePiece(m, true);                        
                    }
                    else
                    {
                        UnMovePiece(m, true);
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public bool IsThreeFoldRepetition()
        {
            List<ulong> hashes = new List<ulong>(prevHashes);
            hashes.Sort();

            for (int i = 0; i < hashes.Count - 2; i++)
            {
             if (hashes[i] == hashes[i + 1] && hashes[i + 1] == hashes[i + 2])
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsFiveFoldRepetition()
        {
            List<ulong> hashes = new List<ulong>(prevHashes);
            hashes.Sort();

            for (int i = 0; i < hashes.Count - 4; i++)
            {
                if (hashes[i] == hashes[i + 1] 
                    && hashes[i + 1] == hashes[i + 2]
                    && hashes[i + 2] == hashes[i + 3]
                    && hashes[i + 3] == hashes[i + 4])
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsProgressive()
        {
            return progress.Peek() < 50;
        }

        public bool InsufficientMaterial()
        {
            List<Piece> whitePieces = Board.GetAllPieces(PieceColor.White);
            List<Piece> blackPieces = Board.GetAllPieces(PieceColor.Black);

            if (whitePieces.Count == 1)
            {
                if (blackPieces.Count == 1)
                {
                    return true;
                }

                if (blackPieces.Count == 2)
                {
                    if (blackPieces[0] is Knight || blackPieces[1] is Knight)
                    {
                        return true;
                    }

                    if (blackPieces[0] is Bishop || blackPieces[1] is Bishop)
                    {
                        return true;
                    }
                }
            }

            if (blackPieces.Count == 1)
            {
                if (whitePieces.Count == 1)
                {
                    return true;
                }

                if (whitePieces.Count == 2)
                {
                    if (whitePieces[0] is Knight || whitePieces[1] is Knight)
                    {
                        return true;
                    }

                    if (whitePieces[0] is Bishop || whitePieces[1] is Bishop)
                    {
                        return true;
                    }
                }
            }

            // 2 bishop on same color
            if (whitePieces.Count == 2 && blackPieces.Count == 2)
            {
                Piece whiteBishop = null;
                Piece blackBishop = null;

                if (whitePieces[0]is Bishop)
                {
                    whiteBishop = whitePieces[0];
                }
                else if (whitePieces[1] is Bishop)
                {
                    whiteBishop = whitePieces[1];
                }

                if (blackPieces[0] is Bishop)
                {
                    blackBishop = blackPieces[0];
                }
                else if (blackPieces[1] is Bishop)
                {
                    blackBishop = blackPieces[1];
                }

                if (whiteBishop != null && blackBishop != null)
                {
                    if (chessView.IsGray(whiteBishop.Position.Row, whiteBishop.Position.Col)
                        && chessView.IsGray(blackBishop.Position.Row, blackBishop.Position.Col))
                    {
                        return true;
                    }

                    if (!chessView.IsGray(whiteBishop.Position.Row, whiteBishop.Position.Col)
                        && !chessView.IsGray(blackBishop.Position.Row, blackBishop.Position.Col))
                    {
                        return true;
                    }
                }                   
            }

            return false;
        }

        public Piece CapturePiece(Piece capturer, Position toPosition)
        {
            Position position = capturer.Position;

            if (capturer is Pawn && Board.GetPiece(toPosition.Row, toPosition.Col) is Empty)
            {
                if (position.Col > 0 && Board.GetPiece(position.Row, position.Col - 1) is Pawn)
                {
                    if (((Pawn)(Board.GetPiece(position.Row, position.Col - 1))).EnPassant())
                    {
                        return Board.GetPiece(position.Row, position.Col - 1);
                    }
                }

                if (position.Col < 7 && Board.GetPiece(position.Row, position.Col + 1) is Pawn)
                {
                    if (((Pawn)Board.GetPiece(position.Row, position.Col + 1)).EnPassant())
                    {
                        return Board.GetPiece(position.Row, position.Col + 1);
                    }
                }
            }

            return Board.GetPiece(toPosition.Row, toPosition.Col);            
        }      
    }
}