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
        public int MaxSeconds = 30;
        private Stack<Move> moves;
        private Stack<long> prevHashes;
        private Stack<int> progress;
        private ChessView chessView;

        public GameEngine(ChessView main)
        {
            Board = new Board(this);
            moves = new Stack<Move>();
            prevHashes = new Stack<long>();
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
            prevHashes.Clear();
            progress.Clear();
            CurrentTurn = PieceColor.White;

            for (int i = 0; i < 8; i++)
            {
                Board.AddPiece(1, i, new Pawn(PieceColor.Black, Board, i));
                Board.AddPiece(6, i, new Pawn(PieceColor.White, Board, i));                
            }

            Board.AddPiece(0, 0, new Rook(PieceColor.Black, Board, 8));
            Board.AddPiece(0, 7, new Rook(PieceColor.Black, Board, 9));
            Board.AddPiece(7, 0, new Rook(PieceColor.White, Board, 8));
            Board.AddPiece(7, 7, new Rook(PieceColor.White, Board, 9));

            Board.AddPiece(0, 1, new Knight(PieceColor.Black, Board, 10));
            Board.AddPiece(0, 6, new Knight(PieceColor.Black, Board, 11));
            Board.AddPiece(7, 1, new Knight(PieceColor.White, Board, 10));
            Board.AddPiece(7, 6, new Knight(PieceColor.White, Board, 11));

            Board.AddPiece(0, 2, new Bishop(PieceColor.Black, Board, 12));
            Board.AddPiece(0, 5, new Bishop(PieceColor.Black, Board, 13));
            Board.AddPiece(7, 2, new Bishop(PieceColor.White, Board, 12));
            Board.AddPiece(7, 5, new Bishop(PieceColor.White, Board, 13));

            Board.AddPiece(0, 3, new Queen(PieceColor.Black, Board, 14));
            Board.AddPiece(7, 3, new Queen(PieceColor.White, Board, 14));

            Board.AddPiece(0, 4, new King(PieceColor.Black, Board, 15));
            Board.AddPiece(7, 4, new King(PieceColor.White, Board, 15));

            MidGame = true;

            prevHashes.Push(Board.Hash);

            chessView.UpdateVisual(Board);
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
                        moves.Add(m);
                    }
                }
            }

            return moves;
        }
        
        internal string WriteMove(Move move)
        {
            string moveString = move.Piece.Enum.ToString();
            moveString += ": " + Convert.ToString((char)(move.From.Col + 65)) + (8 - move.From.Row);

            if (move.Capture is Empty) { moveString += "-"; }
            else { moveString += "x"; }

            moveString += Convert.ToString((char)(move.To.Col + 65)) + (8 - move.To.Row);
            moveString += " " + move.Piece.MoveCount + " moves | ";
            moveString += "Capture: " + move.Capture.Enum;

            return moveString;
        }

        internal string PrintBoard()
        {
            string boardString = "  A  B  C  D  E  F  G  H\n";

            for (int i = 0; i < 8; i++)
            {
                boardString += 8 - i + " ";

                for (int j = 0; j < 8; j++)
                {
                    switch (Board.GetPiece(i, j).Enum)
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

                boardString += " " + (8 - i) +"\n";
            }

            boardString += "  A  B  C  D  E  F  G  H";

            return boardString;
        }
                
        internal void MovePiece(Move move)
        {
            Position to = move.To;
            Position from = move.From;

            if (move.Capture is King)
            {
                Debug.WriteLine("Not supposed to happen!");
            }

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
            prevHashes.Push(Board.Hash);

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

            if (IsThreatened(Board.GetKing(CurrentTurn)))
            {
                move.CheckBonus = true;
            }
        }

        internal bool IsCheckMate()
        {
            return IsThreatened(Board.GetKing(CurrentTurn)) && GetAllMoves(CurrentTurn).Count == 0;
        }

        internal bool IsStaleMate()
        {
            return !IsThreatened(Board.GetKing(CurrentTurn)) && GetAllMoves(CurrentTurn).Count == 0;
        }

        internal void ReportOutcome()
        {
            bool humanPlayer = WhiteHumanPlayer || BlackHumanPlayer;            
            PieceColor previousPlayer = (CurrentTurn == PieceColor.White)? PieceColor.Black : PieceColor.White;

            if (IsCheckMate())
            {
                MidGame = false;
                Debug.WriteLine("Checkmate: " + previousPlayer + " wins!");
                if (humanPlayer) { MessageBox.Show(previousPlayer.ToString() + " wins!", "Checkmate"); }
            }

            if (IsStaleMate())
            {
                MidGame = false;
                Debug.WriteLine("Stalemate: Game is a draw!");
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

            if (InsufficientMaterial())
            {
                MidGame = false;
                Debug.WriteLine("Insufficient Material: Game is a draw!");
                if (humanPlayer) { MessageBox.Show("Game is a draw!", "Insufficient Material"); }
            }

            if (!IsProgressive())
            {
                MidGame = false;
                Debug.WriteLine("Lack of Progress: Game is a draw!");
                if (humanPlayer) { MessageBox.Show("Game is a draw!", "Lack of Progress"); }
            }

            if (IsFiveFoldRepetition())
            {
                MidGame = false;
                Debug.WriteLine("Fivefold Repetition: Game is a draw!");
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
                    Adam adam = new Adam(this);
                    adam.RunSimulation(MaxSeconds);
                    chessView.ShowProcessingBar(MaxSeconds);
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

        internal bool WillCauseCheck(Move move)
        {
            if (move.Castle)
            {
                if (IsThreatened(Board.GetKing(move.Piece.Color))) { return true; }

                //Left castle
                if (move.From.Col - move.To.Col > 0)
                {
                    Move firstKingMove = new Move(move.From, new Position(move.From.Row, move.From.Col - 1), move.Piece, move.Capture);

                    MovePiece(firstKingMove);
                    if (IsThreatened(Board.GetKing(move.Piece.Color)))
                    {
                        UnMovePiece(firstKingMove);
                        return true;
                    }
                    UnMovePiece(firstKingMove);
                }
                //Right castle
                else
                {
                    Move firstKingMove = new Move(move.From, new Position(move.From.Row, move.From.Col + 1), move.Piece, move.Capture);

                    MovePiece(firstKingMove);
                    if (IsThreatened(Board.GetKing(move.Piece.Color)))
                    {
                        UnMovePiece(firstKingMove);
                        return true;
                    }
                    UnMovePiece(firstKingMove);
                }
            }

            MovePiece(move);
            bool isInCheck = IsThreatened(Board.GetKing(move.Piece.Color));
            UnMovePiece(move);
            return isInCheck;            
        }

        internal bool IsThreatened(Piece piece)
        {
            Position position  = piece.Position;
            int row = position.Row;
            int col = position.Col;

            while (row > 0 && col > 0 && Board.GetPiece(row - 1, col - 1) is Empty)
            {
                row--;
                col--;
            }

            if (row > 0 && col > 0 && IsThreat(piece, Board.GetPiece(row - 1, col - 1))) { return true; }

            row = position.Row;
            col = position.Col;

            while (row > 0 && col < 7 && Board.GetPiece(row - 1, col + 1) is Empty)
            {
                row--;
                col++;
            }

            if (row > 0 && col < 7 && IsThreat(piece, Board.GetPiece(row - 1, col + 1))) { return true; }

            row = position.Row;
            col = position.Col;

            while (row < 7 && col > 0 && Board.GetPiece(row + 1, col - 1) is Empty)
            {
                row++;
                col--;
            }

            if (row < 7 && col > 0 && IsThreat(piece, Board.GetPiece(row + 1, col - 1))) { return true; }

            row = position.Row;
            col = position.Col;

            while (row < 7 && col < 7 && Board.GetPiece(row + 1, col + 1) is Empty)
            {
                row++;
                col++;
            }

            if (row < 7 && col < 7 && IsThreat(piece, Board.GetPiece(row + 1, col + 1))) { return true; }

            row = position.Row;
            col = position.Col;

            while (col > 0 && Board.GetPiece(row, col - 1) is Empty)
            {
                col--;
            }

            if (col > 0 && IsThreat(piece, Board.GetPiece(row, col - 1))) { return true; }

            col = position.Col;

            while (col < 7 && Board.GetPiece(row, col + 1) is Empty)
            {
                col++;
            }

            if (col < 7 && IsThreat(piece, Board.GetPiece(row, col + 1))) { return true; }

            col = position.Col;

            while (row > 0 && Board.GetPiece(row - 1, col) is Empty)
            {
                row--;
            }

            if (row > 0 && IsThreat(piece, Board.GetPiece(row - 1, col))) { return true; }

            row = position.Row;

            while (row < 7 && Board.GetPiece(row + 1, col) is Empty)
            {
                row++;
            }

            if (row < 7 && IsThreat(piece, Board.GetPiece(row + 1, col))) { return true; }

            row = position.Row;

            if (row + 2 <= 7 && col - 1 >= 0 && IsThreat(piece, Board.GetPiece(row + 2, col - 1))) { return true; }

            if (row + 2 <= 7 && col + 1 <= 7 && IsThreat(piece, Board.GetPiece(row + 2, col + 1))) { return true; }

            if (row - 2 >= 0 && col - 1 >= 0 && IsThreat(piece, Board.GetPiece(row - 2, col - 1))) { return true; }

            if (row - 2 >= 0 && col + 1 <= 7 && IsThreat(piece, Board.GetPiece(row - 2, col + 1))) { return true; }

            if (row + 1 <= 7 && col - 2 >= 0 && IsThreat(piece, Board.GetPiece(row + 1, col - 2))) { return true; }

            if (row + 1 <= 7 && col + 2 <= 7 && IsThreat(piece, Board.GetPiece(row + 1, col + 2))) { return true; }

            if (row - 1 >= 0 && col - 2 >= 0 && IsThreat(piece, Board.GetPiece(row - 1, col - 2))) { return true; }

            if (row - 1 >= 0 && col + 2 <= 7 && IsThreat(piece, Board.GetPiece(row - 1, col + 2))) { return true; }

            return false;
        }

        private bool IsThreat(Piece threatened, Piece threat)
        {
            if (threat is Empty) { return false; }

            if (threat.Color != threatened.Color)
            {
                bool isDiagonal = Math.Abs(threatened.Position.Row - threat.Position.Row) == Math.Abs(threatened.Position.Col - threat.Position.Col);
                if (isDiagonal && (threat is Queen || threat is Bishop)) { return true; }

                bool isOrthogonal = threatened.Position.Row == threat.Position.Row || threatened.Position.Col == threat.Position.Col;
                if (isOrthogonal && (threat is Queen || threat is Rook)) { return true; }

                bool isKnightMove = (Math.Abs(threatened.Position.Row - threat.Position.Row) == 2 && Math.Abs(threatened.Position.Col - threat.Position.Col) == 1)
                        || (Math.Abs(threatened.Position.Row - threat.Position.Row) == 1 && Math.Abs(threatened.Position.Col - threat.Position.Col) == 2);
                if (isKnightMove && threat is Knight) { return true; }

                if (threat is King)
                {
                    if (Math.Abs(threatened.Position.Row - threat.Position.Row) <= 1 
                        && Math.Abs(threatened.Position.Col - threat.Position.Col) <= 1) { return true; }
                }

                if (threatened.Color == PieceColor.Black)
                {
                    return threat is Pawn && threatened.Position.Row + 1 == threat.Position.Row && Math.Abs(threatened.Position.Col - threat.Position.Col) == 1;
                }
                else
                {
                    return threat is Pawn && threatened.Position.Row - 1 == threat.Position.Row && Math.Abs(threatened.Position.Col - threat.Position.Col) == 1;
                }
            }

            return false;
        }

        public bool ValidHumanMove(Piece piece, Position to)
        {
            List<Move> moves = GetAllMoves(piece.Color);

            if (MidGame && piece.Color == CurrentTurn)
            {
                foreach (Move m in moves)
                {
                    if (m.To.Equals(to) && m.From.Equals(piece.Position) && !WillCauseCheck(m)) { return true; }
                }
            }

            return false;
        }

        public bool IsThreeFoldRepetition()
        {
            List<long> hashes = new List<long>(prevHashes);
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
            List<long> hashes = new List<long>(prevHashes);
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
            return progress.Count > 0 && progress.Peek() < 50;
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