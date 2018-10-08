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
        private Stack<Move> moves;
        private Stack<ulong> prevHashes;
        private Stack<int> progress;
        private ChessView chessView;
        private int maxSeconds = 5;

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
            List<Piece> pieceList = new List<Piece>();
            Piece[] pieceArray = Board.GetAllPieces(color);

            for (int i = 0; i < 16; i++)
            {
                if (pieceArray[i] != null)
                {
                    pieceList.Add(pieceArray[i]);
                }
            }

            return pieceList;
        }
                
        internal void SetUpPieces()
        {
            Board.Clear();
            moves.Clear();

            for (int i = 0; i < 8; i++)
            {
                AddPiece(1, i, new Pawn(PieceColor.Black, Board, i));
                AddPiece(6, i, new Pawn(PieceColor.White, Board, i));                
            }

            AddPiece(0, 0, new Rook(PieceColor.Black, Board, 8));
            AddPiece(0, 7, new Rook(PieceColor.Black, Board, 9));
            AddPiece(7, 0, new Rook(PieceColor.White, Board, 8));
            AddPiece(7, 7, new Rook(PieceColor.White, Board, 9));

            AddPiece(0, 1, new Knight(PieceColor.Black, Board, 10));
            AddPiece(0, 6, new Knight(PieceColor.Black, Board, 11));
            AddPiece(7, 1, new Knight(PieceColor.White, Board, 10));
            AddPiece(7, 6, new Knight(PieceColor.White, Board, 11));

            AddPiece(0, 2, new Bishop(PieceColor.Black, Board, 12));
            AddPiece(0, 5, new Bishop(PieceColor.Black, Board, 13));
            AddPiece(7, 2, new Bishop(PieceColor.White, Board, 12));
            AddPiece(7, 5, new Bishop(PieceColor.White, Board, 13));

            AddPiece(0, 3, new Queen(PieceColor.Black, Board, 14));
            AddPiece(7, 3, new Queen(PieceColor.White, Board, 14));

            AddPiece(0, 4, new King(PieceColor.Black, Board, 15));
            AddPiece(7, 4, new King(PieceColor.White, Board, 15));

            MidGame = true;

            prevHashes.Push(Board.GetHash());

            chessView.UpdateVisual(Board);
        }

        public void AddPiece(int row, int col, Piece piece)
        {
            Board.AddPiece(row, col, piece);
        }
        
        internal bool IsCheck(PieceColor color)
        {
            if (color == PieceColor.Black)
            {
                foreach (Piece p in Board.GetAllPieces(PieceColor.White))
                {
                    if (p != null)
                    {
                        foreach (Move move in p.GetPotentialMoves())
                        {
                            if (move.Capture is King) { return true; }
                        }
                    }
                }                
            }
            else
            {
                foreach (Piece p in Board.GetAllPieces(PieceColor.Black))
                {
                    if (p != null)
                    {
                        foreach (Move move in p.GetPotentialMoves())
                        {
                            if (move.Capture is King) { return true; }
                        }
                    }
                }
            }

            return false;
        }

        internal bool IsCheckSafeMove(Move move)
        {
            MovePiece(move);

            if (IsCheck(move.Piece.Color))
            {
                UnMovePiece(move);
                return false;
            }

            UnMovePiece(move);
            return true;
        }

        public List<Move> GetAllMoves(PieceColor color)
        {
            List<Move> moves = new List<Move>();

            foreach (Piece p in Board.GetAllPieces(color))
            {
                if (p != null)
                {
                    foreach (Move m in p.GetPotentialMoves())
                    {
                        if (IsCheckSafeMove(m)) { moves.Add(m); }
                    }
                }
            }

            return moves;
        }
        
        internal string WriteMove(Move move)
        {
            string moveString = move.Piece.Enum.ToString();
            moveString += ": " + Convert.ToString((char)(move.From.Row + 65)) + move.From.Col;

            if (move.Capture is Empty) { moveString += "-"; }
            else { moveString += "x"; }

            moveString += Convert.ToString((char)(move.To.Row + 65)) + move.To.Col;
            moveString += " " + move.Piece.MoveCount + " moves | ";
            moveString += "Capture: " + move.Capture.Enum;

            return moveString;
        }

        internal string PrintBoard()
        {
            string boardString = "";

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    switch(Board.GetPiece(i, j).Enum)
                    {
                        case PieceEnum.BlackBishop:
                            boardString += "BB ";
                            break;
                        case PieceEnum.BlackKing:
                            boardString += "BK ";
                            break;
                        case PieceEnum.BlackKingNoCastle:
                            boardString += "BK ";
                            break;
                        case PieceEnum.BlackKnight:
                            boardString += "BN ";
                            break;
                        case PieceEnum.BlackPawn:
                            boardString += "BP ";
                            break;
                        case PieceEnum.BlackPawnEnPassant:
                            boardString += "BP ";
                            break;
                        case PieceEnum.BlackQueen:
                            boardString += "BQ ";
                            break;
                        case PieceEnum.BlackRook:
                            boardString += "BR ";
                            break;
                        case PieceEnum.WhiteBishop:
                            boardString += "WB ";
                            break;
                        case PieceEnum.WhiteKing:
                            boardString += "WK ";
                            break;
                        case PieceEnum.WhiteKingNoCastle:
                            boardString += "WK ";
                            break;
                        case PieceEnum.WhiteKnight:
                            boardString += "WN ";
                            break;
                        case PieceEnum.WhitePawn:
                            boardString += "WP ";
                            break;
                        case PieceEnum.WhitePawnEnPassant:
                            boardString += "WP ";
                            break;
                        case PieceEnum.WhiteQueen:
                            boardString += "WQ ";
                            break;
                        case PieceEnum.WhiteRook:
                            boardString += "WR ";
                            break;
                        default:
                            boardString += "   ";
                            break;
                    }
                }

                boardString += "\n";
            }

            return boardString;
        }

        internal void MovePiece(Move move)
        {
            Position to = move.To;
            Position from = move.From;

            // Pawn promotion
            if (move.Promotion) { Board.PromotePawn(move); }
                        
            if (move.Piece is Pawn)
            {
                Pawn pawn = move.Piece as Pawn;

                if (move.EnPassantMove && pawn.Color == PieceColor.White) { Board.UpdatePieceEnum(pawn, PieceEnum.WhitePawnEnPassant); }                
                else if (move.EnPassantMove && pawn.Color == PieceColor.Black) { Board.UpdatePieceEnum(pawn, PieceEnum.BlackPawnEnPassant); }                
            }            

            if (move.Piece is King)
            {
                King king = move.Piece as King;

                if (move.Castle)
                {
                    Board.MovePiece(move.CastleMove.From, move.CastleMove.To);
                    move.CastleMove.Piece.MoveCount++;
                }

                if (king.Color == PieceColor.White)
                {
                    Board.UpdatePieceEnum(king, PieceEnum.WhiteKingNoCastle);
                }
                else if (king.Color == PieceColor.Black)
                {
                    Board.UpdatePieceEnum(king, PieceEnum.BlackKingNoCastle);
                }
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

            if (CurrentTurn == PieceColor.White)
            {
                CurrentTurn = PieceColor.Black;
                ClearPassant(PieceColor.Black);
            }
            else
            {
                CurrentTurn = PieceColor.White;
                ClearPassant(PieceColor.White);
            }
        }

        internal void ReportOutcome()
        {
            bool humanPlayer = WhiteHumanPlayer || BlackHumanPlayer;            
            PieceColor previousPlayer = (CurrentTurn == PieceColor.White)? PieceColor.Black : PieceColor.White;

            if (HasCheckmate(previousPlayer))
            {
                MidGame = false;
                if (humanPlayer) { MessageBox.Show(previousPlayer.ToString() + " wins!", "Checkmate"); }
            }

            if (IsStalemate(CurrentTurn))
            {
                MidGame = false;
                if (humanPlayer) { MessageBox.Show("Game is a draw!", "Stalemate"); }
            }

            if (IsThreeFoldRepetition())
            {
                if (humanPlayer)
                {
                    MessageBoxResult response = MessageBox.Show("Do you want to declare a draw?",
                        "Threefold Repetition", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (response == MessageBoxResult.Yes)
                    {
                        MidGame = false;
                        MessageBox.Show("Game is a draw!", "Threefold Repetition");
                    }
                }
            }

            if (IsFiveFoldRepetition())
            {
                MidGame = false;
                if (humanPlayer) { MessageBox.Show("Game is a draw!", "Fivefold Repetition"); }
            }                                   
        }

        internal void ClearPassant(PieceColor color)
        {
            foreach (Piece p in Board.GetAllPieces(color))
            {
                if (p is Pawn)
                {
                    if (p.Color == PieceColor.White) { Board.UpdatePieceEnum(p, PieceEnum.WhitePawn); }
                    else { Board.UpdatePieceEnum(p, PieceEnum.BlackPawn); }
                }
            }
        }

        internal void ShowMove()
        {
            chessView.UpdateVisual(Board);
            ReportOutcome();
            
            if (MidGame)
            {
                if ((CurrentTurn == PieceColor.White && !WhiteHumanPlayer)
                || (CurrentTurn == PieceColor.Black && !BlackHumanPlayer))
                {
                    Adam adam = new Adam(this, CurrentTurn);
                    adam.RunSimulation(maxSeconds);
                    chessView.ShowProcessingBar(maxSeconds);
                }
            }                            
        }

        public void UnMovePiece(Move move)
        {
            Position to = move.To;
            Position from = move.From;

            // Pawn demotion
            if (move.Promotion) { Board.DemotePawn(move); }

            if (move.Piece is King)
            {
                King king = move.Piece as King;
                
                if (move.Piece.MoveCount == 1)
                {
                    if (king.Color == PieceColor.White) { Board.UpdatePieceEnum(king, PieceEnum.WhiteKing); }
                    else if (king.Color == PieceColor.Black) { Board.UpdatePieceEnum(king, PieceEnum.BlackKing); }
                }

                if (move.Castle)
                {
                    Board.MovePiece(move.CastleMove.To, move.CastleMove.From);
                    move.CastleMove.Piece.MoveCount--;
                }
            }
            
            Board.MovePiece(to, from);
            move.Piece.MoveCount--;

            if (!(move.Capture is Empty)) { Board.AddPiece(move.Capture.Position.Row, move.Capture.Position.Col, move.Capture); }

            if (progress.Count > 0) { progress.Pop(); }
            if (moves.Count > 0) { moves.Pop(); }
            if (prevHashes.Count > 0) { prevHashes.Pop(); }

            if (CurrentTurn == PieceColor.White)
            {
                CurrentTurn = PieceColor.Black;
                if (moves.Count > 0 && moves.Peek().EnPassantMove)
                {
                    Board.UpdatePieceEnum(moves.Peek().Piece, PieceEnum.WhitePawnEnPassant);
                }
            }
            else
            {
                CurrentTurn = PieceColor.White;
                if (moves.Count > 0 && moves.Peek().EnPassantMove)
                {
                    Board.UpdatePieceEnum(moves.Peek().Piece, PieceEnum.BlackPawnEnPassant);
                }
            }
        }

        public bool ValidHumanMove(Piece piece, Position to)
        {
            List<Move> moves = GetAllMoves(piece.Color);

            if (MidGame && piece.Color == CurrentTurn)
            {
                foreach (Move m in moves)
                {
                    if (m.To.Equals(to) && m.From.Equals(piece.Position)) { return true; }
                }
            }

            return false;
        }

        public bool IsCheckmate(PieceColor color)
        {
            List<Move> moves = GetAllMoves(color);

            foreach (Move m in moves)
            {
                if (m.Capture is King) { return true; }
            }

            return false;
        }

        public bool HasCheckmate(PieceColor playerA)
        {
            PieceColor playerB = (playerA == PieceColor.White)? PieceColor.Black : PieceColor.White;
            bool canCaptureKing;

            foreach (Move playerBMove in GetAllMoves(playerB))
            {
                MovePiece(playerBMove);

                canCaptureKing = false;
                foreach (Move playerAMove in GetAllMoves(playerA))
                {
                    if (playerAMove.Capture is King) { canCaptureKing = true; }
                }
                
                UnMovePiece(playerBMove);                

                if (!canCaptureKing) { return false; }
            }

            return true;
        }

        public bool IsStalemate(PieceColor color)
        {
            return !IsCheck(color) && GetAllMoves(color).Count == 0;
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
                if (hashes[i] == hashes[i + 1] && hashes[i + 1] == hashes[i + 2]
                    && hashes[i + 2] == hashes[i + 3] && hashes[i + 3] == hashes[i + 4])
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
            List<Piece> whitePieces = GetAllPieces(PieceColor.White);
            List<Piece> blackPieces = GetAllPieces(PieceColor.Black);

            if (whitePieces.Count == 1)
            {
                if (blackPieces.Count == 1) { return true; }

                if (blackPieces.Count == 2)
                {
                    if (blackPieces[0] is Knight || blackPieces[1] is Knight) { return true; }

                    if (blackPieces[0] is Bishop || blackPieces[1] is Bishop) { return true; }
                }
            }

            if (blackPieces.Count == 1)
            {
                if (whitePieces.Count == 1) { return true; }

                if (whitePieces.Count == 2)
                {
                    if (whitePieces[0] is Knight || whitePieces[1] is Knight) { return true; }

                    if (whitePieces[0] is Bishop || whitePieces[1] is Bishop) { return true; }
                }
            }

            // 2 bishop on same color
            if (whitePieces.Count == 2 && blackPieces.Count == 2)
            {
                Piece whiteBishop = null;
                Piece blackBishop = null;

                if (whitePieces[0] is Bishop) { whiteBishop = whitePieces[0]; }
                else if (whitePieces[1] is Bishop) { whiteBishop = whitePieces[1]; }

                if (blackPieces[0] is Bishop) { blackBishop = blackPieces[0]; }
                else if (blackPieces[1] is Bishop) { blackBishop = blackPieces[1]; }

                if (whiteBishop != null && blackBishop != null)
                {
                    if (chessView.IsGray(whiteBishop.Position.Row, whiteBishop.Position.Col)
                        && chessView.IsGray(blackBishop.Position.Row, blackBishop.Position.Col)) { return true; }

                    if (!chessView.IsGray(whiteBishop.Position.Row, whiteBishop.Position.Col)
                        && !chessView.IsGray(blackBishop.Position.Row, blackBishop.Position.Col)) { return true; }
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