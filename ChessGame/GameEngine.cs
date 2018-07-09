using System;
using System.Collections.Generic;
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
        private Stack<Move> moves;
        private Stack<ulong> prevHashes;
        private Stack<int> progress;
        private Canvas canvas;
        public PieceColor CurrentTurn { get; set; }
        public bool BlackHumanPlayer { get; set; }
        public bool WhiteHumanPlayer { get; set; }
        public bool MidGame { get; set; }
        

        public GameEngine(Canvas c)
        {
            Board = new Board();
            moves = new Stack<Move>();
            prevHashes = new Stack<ulong>();
            canvas = c;
            CurrentTurn = PieceColor.White;
            WhiteHumanPlayer = true;
            progress = new Stack<int>();
        }

        internal void AddPiece(int row, int col, Piece p)
        {            
            Board.AddPiece(row, col, p);
           
            Canvas.SetTop(p.Image, row * canvas.Height / 8);
            Canvas.SetLeft(p.Image, col * canvas.Width / 8);
            canvas.Children.Add(p.Image);
        }

        public void CreateBoard()
        {
            double width = canvas.Width;
            double height = canvas.Height;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Rectangle rect = new Rectangle();
                    rect.Stroke = new SolidColorBrush(Colors.Gray);
                    rect.Fill = new SolidColorBrush(Colors.Gray);

                    rect.Height = height / 8;
                    rect.Width = width / 8;

                    if (IsGray(i,j))
                    {
                        Canvas.SetLeft(rect, i * width / 8);
                        Canvas.SetTop(rect, j * height / 8);
                    }
                    else
                    {
                        Canvas.SetLeft(rect, i * width / 8 + width / 8);
                        Canvas.SetTop(rect, j * height / 8);
                    }

                    canvas.Children.Add(rect);
                }
            }
        }

        internal bool IsGray(int i, int j)
        {
            if ((i % 2 == 0 && j % 2 == 0) || (i % 2 == 1 && j % 2 == 1))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        
        internal void SetUpPieces()
        {
            Board.Clear();
            moves.Clear();
            canvas.Children.Clear();

            CreateBoard();

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Board.AddPiece(i, j, new Empty(Board));
                }
            }

            for (int i = 0; i < 8; i++)
            {
                AddPiece(1, i, new Pawn(PieceColor.Black, Board));
                AddPiece(6, i, new Pawn(PieceColor.White, Board));
                
                for (int j = 2; j < 6; j++)
                {
                    Board.AddPiece(j, i, new Empty(Board));
                }
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
        
        internal void WriteMove(Move move)
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
            System.Diagnostics.Debug.WriteLine(moveString);
        }

        internal void MovePiece(Move move)
        {
            Position to = move.To;
            Position from = move.From;

            Board.MovePiece(from, to);

            // Pawn promotion
            if (move.Promotion)
            {
                Board.PromotePiece(to.Row, to.Col, move.Piece);
            }
            
            move.Piece.MoveCount++;

            if (move.Piece is Pawn)
            {
                if (move.Piece.Color == PieceColor.White)
                {
                    Pawn pawn = move.Piece as Pawn;

                    if (move.From.Row == 6 && move.To.Row == 4)
                    {
                        Board.EnPassant(pawn, true);
                    }
                    else
                    {
                        Board.EnPassant(pawn, false);
                    }
                }

                if (move.Piece.Color == PieceColor.Black)
                {
                    Pawn pawn = move.Piece as Pawn;

                    if (move.From.Row == 1 && move.To.Row == 3)
                    {
                        Board.EnPassant(pawn, true);
                    }
                    else
                    {
                        Board.EnPassant(pawn, false);
                    }
                }
            }            

            if (move.Piece is King)
            {
                King king = move.Piece as King;

                if (move.From.Col - move.To.Col == 2)
                {
                    MovePiece(new Move(new Position(move.From.Row, 0), new Position(move.From.Row, 3),
                        Board.GetPiece(move.From.Row, 0), Board.GetPiece(move.From.Row, 3), false));
                }

                if (move.From.Col - move.To.Col == -2)
                {
                    MovePiece(new Move(new Position(move.From.Row, 7), new Position(move.From.Row, 5),
                        Board.GetPiece(move.From.Row, 7), Board.GetPiece(move.From.Row, 5), false));
                }

                Board.KingMoved(king, true);
            }

            if (!(move.Piece is Pawn) || move.Capture is Empty)
            {
                if (progress.Count > 0)
                {
                    progress.Push(progress.Peek() + 1);
                }
                else
                {
                    progress.Push(1);
                }                
            }
            else
            {
                progress.Push(0);
            }

            moves.Push(move);
            prevHashes.Push(Board.GetHash());

            if (CurrentTurn == PieceColor.White)
            {
                CurrentTurn = PieceColor.Black;
            }
            else
            {
                CurrentTurn = PieceColor.White;
            }
        }

        public void UnMovePiece(Move move)
        {
            Position to = move.To;
            Position from = move.From;

            // Pawn demotion
            if (move.Promotion)
            {
                Board.DemotePiece(to.Row, to.Col);
            }

            if (move.Piece is Pawn)
            {
                Pawn pawn = move.Piece as Pawn;

                if (move.From.Row == 1 && move.To.Row == 3)
                {
                    Board.EnPassant(pawn, false);                    
                }

                if (move.From.Row == 6 && move.To.Row == 4)
                {
                    Board.EnPassant(pawn, false);
                }
            }

            if (move.Piece is King)
            {
                King king = move.Piece as King;

                if (move.From.Col - move.To.Col == 2)
                {
                    MovePiece(new Move(new Position(move.From.Row, 3), new Position(move.From.Row, 0),
                        Board.GetPiece(move.From.Row, 3), Board.GetPiece(move.From.Row, 0), false));
                }

                if (move.From.Col - move.To.Col == -2)
                {
                    MovePiece(new Move(new Position(move.From.Row, 5), new Position(move.From.Row, 7),
                        Board.GetPiece(move.From.Row, 5), Board.GetPiece(move.From.Row, 7), false));
                }

                if (move.Piece.MoveCount == 0)
                {
                    Board.KingMoved(king, false);
                }
            }

            Board.MovePiece(to, from);
            move.Piece.MoveCount--;

            if (!(move.Capture is Empty))
            {
                Board.AddPiece(to.Row, to.Col, move.Capture);
            }            
                        
            progress.Pop();
            moves.Pop();
            prevHashes.Pop();

            if (CurrentTurn == PieceColor.White)
            {
                CurrentTurn = PieceColor.Black;
            }
            else
            {
                CurrentTurn = PieceColor.White;
            }
        }

        internal bool CheckBlock(Move move, PieceColor color)
        {
            if (IsCheck(color))
            {
                // apply move to see if removes check
                MovePiece(move);

                if (!IsCheck(color))
                {
                    UnMovePiece(move);
                    return false;
                }
                else
                {
                    UnMovePiece(move);
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
                if (m.Piece.Color == CurrentTurn
                    && m.To.equals(to)
                    && m.From.equals(Board.GetPosition(piece))
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
                if (!CheckBlock(m, color))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsStalement(PieceColor color)
        {
            if (!IsCheck(color))
            {
                List<Move> moves = GetAllMoves(color);

                foreach (Move m in moves)
                {
                    MovePiece(m);

                    if (IsCheck(color))
                    {
                        UnMovePiece(m);                        
                    }
                    else
                    {
                        UnMovePiece(m);
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
                    if (IsGray(whiteBishop.GetPosition().Row, whiteBishop.GetPosition().Col)
                        && IsGray(blackBishop.GetPosition().Row, blackBishop.GetPosition().Col))
                    {
                        return true;
                    }

                    if (!IsGray(whiteBishop.GetPosition().Row, whiteBishop.GetPosition().Col)
                        && !IsGray(blackBishop.GetPosition().Row, blackBishop.GetPosition().Col))
                    {
                        return true;
                    }
                }                   
            }

            return false;
        }

        public Piece CapturePiece(Piece capturer, Position toPosition)
        {
            Position position = Board.GetPosition(capturer);

            if (capturer is Pawn && Board.GetPiece(toPosition.Row, toPosition.Col) is Empty)
            {
                if (position.Col > 0 && Board.GetPiece(position.Row, position.Col - 1) is Pawn)
                {
                    if (((Pawn)(Board.GetPiece(position.Row, position.Col - 1))).enPassant)
                    {
                        return Board.GetPiece(position.Row, position.Col - 1);
                    }
                }

                if (position.Col < 7 && Board.GetPiece(position.Row, position.Col + 1) is Pawn)
                {
                    if (((Pawn)Board.GetPiece(position.Row, position.Col + 1)).enPassant)
                    {
                        return Board.GetPiece(position.Row, position.Col + 1);
                    }
                }
            }

            return Board.GetPiece(toPosition.Row, toPosition.Col);            
        }

        public void showMove(Move move)
        {
            if (!(move.Capture is Empty))
            {
                canvas.Children.Remove(move.Capture.Image);
            }

            if (move.Promotion 
                && ((CurrentTurn == PieceColor.White && WhiteHumanPlayer)
                || (CurrentTurn == PieceColor.Black && BlackHumanPlayer)))
            {
                PawnPromotion pawnPromotion = new PawnPromotion();
                pawnPromotion.ShowDialog();

                canvas.Children.Remove(move.Piece.Image);

                switch (pawnPromotion.SelectedPiece)
                {
                    case PieceType.Queen:
                        move.Piece = new Queen(move.Piece.Color, Board);
                        break;
                    case PieceType.Bishop:
                        move.Piece = new Bishop(move.Piece.Color, Board);
                        break;
                    case PieceType.Rook:
                        move.Piece = new Rook(move.Piece.Color, Board);
                        break;
                    case PieceType.Knight:
                        move.Piece = new Knight(move.Piece.Color, Board);
                        break;
                    default:
                        move.Piece = new Queen(move.Piece.Color, Board);
                        break;
                }

                canvas.Children.Add(move.Piece.Image);
            }

            MovePiece(move);

            Canvas.SetTop(move.Piece.Image, move.To.Row * canvas.Height / 8);
            Canvas.SetLeft(move.Piece.Image, move.To.Col * canvas.Width / 8);

            if (IsCheckmate(CurrentTurn) 
                && ((CurrentTurn == PieceColor.White && WhiteHumanPlayer)
                || (CurrentTurn == PieceColor.Black && BlackHumanPlayer)))
            {
                MessageBox.Show(CurrentTurn.ToString() + " wins!", "Checkmate");
            }

            if (IsStalement(CurrentTurn)
                && ((CurrentTurn == PieceColor.White && WhiteHumanPlayer)
                || (CurrentTurn == PieceColor.Black && BlackHumanPlayer)))
            {                
                MessageBox.Show("Game is a draw!", "Stalemate");
            }

            if (IsThreeFoldRepetition() && (WhiteHumanPlayer || BlackHumanPlayer))
            {
                MessageBoxResult response = MessageBox.Show("Do you want to declare a draw?", 
                    "Threefold Repetition", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                if (response == MessageBoxResult.Yes)
                {
                    MessageBox.Show("Game is a draw!", "Threefold Repetition");
                }
            }
            
            if (CurrentTurn == PieceColor.White & !WhiteHumanPlayer)
            {
                AlephYud computer = new AlephYud(this, CurrentTurn);
                Move computerMove = computer.RunSimulation();
                showMove(computerMove);
            }

            if (CurrentTurn == PieceColor.Black & !BlackHumanPlayer)
            {
                AlephYud computer = new AlephYud(this, CurrentTurn);
                Move computerMove = computer.RunSimulation();
                showMove(computerMove);
            } 
        }        
    }
}