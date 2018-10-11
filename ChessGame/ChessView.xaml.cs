using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ChessGame.Pieces;

namespace ChessGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ChessView : Window
    {
        private Image draggedImage;
        private Point mousePosition;
        private GameEngine engine;
        Piece selectedPiece;
        BackgroundWorker worker;
        Dictionary<Piece, Image> imageDictionary;
        int maxSeconds = 5;
        
        public ChessView()
        {
            InitializeComponent();            
            engine = new GameEngine(this);
            imageDictionary = new Dictionary<Piece, Image>();
            CreateBoard();
        }

        private void CreateBoard()
        {
            double width = boardCanvas.Width;
            double height = boardCanvas.Height;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Rectangle rect = new Rectangle
                    {
                        Stroke = new SolidColorBrush(Colors.Gray),
                        Fill = new SolidColorBrush(Colors.Gray),

                        Height = height / 8,
                        Width = width / 8
                    };

                    if (IsGray(i, j))
                    {
                        Canvas.SetLeft(rect, i * width / 8);
                        Canvas.SetTop(rect, j * height / 8);
                    }
                    else
                    {
                        Canvas.SetLeft(rect, i * width / 8 + width / 8);
                        Canvas.SetTop(rect, j * height / 8);
                    }

                    boardCanvas.Children.Add(rect);
                }                
            }
        }

        public bool IsGray(int i, int j)
        {
            return !((i % 2 == 0 && j % 2 == 0) || (i % 2 == 1 && j % 2 == 1));
        }

        private void CanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var image = e.Source as Image;

            if (image != null && boardCanvas.CaptureMouse())
            {
                mousePosition = e.GetPosition(boardCanvas);

                int sourceRow = (int)(8 * mousePosition.Y / boardCanvas.Height);
                int sourceCol = (int)(8 * mousePosition.X / boardCanvas.Width);
                selectedPiece = engine.Board.GetPiece(sourceRow, sourceCol);

                if (imageDictionary.ContainsKey(selectedPiece)) { draggedImage = imageDictionary[selectedPiece]; }
                Panel.SetZIndex(draggedImage, 1);
            }
        }

        private void CanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (draggedImage != null)
            {
                boardCanvas.ReleaseMouseCapture();
                
                var Position = e.GetPosition(boardCanvas);
                var offset = Position - mousePosition;
                mousePosition = Position;

                int destRow = (int)(8 * Position.Y / boardCanvas.Height);
                int destCol = (int)(8 * Position.X / boardCanvas.Width);
                Position to = new Position(destRow, destCol);

                if (engine.ValidHumanMove(selectedPiece, to))
                {
                    Position from = selectedPiece.Position;
                    Piece capture = engine.CapturePiece(selectedPiece, to);

                    bool promotion = selectedPiece is Pawn && (destRow == 7 || destRow == 0);

                    Move move = new Move(from, to, selectedPiece, capture) { Promotion = promotion };

                    if (move.Promotion)
                    {
                        PawnPromotion pawnPromotion = new PawnPromotion();
                        pawnPromotion.ShowDialog();
                        Piece promoteTo;

                        switch (pawnPromotion.SelectedPiece)
                        {
                            case PieceType.Queen:
                                promoteTo = new Queen(move.Piece.Color, engine, engine.Board, move.Piece.Index);
                                break;
                            case PieceType.Bishop:
                                promoteTo = new Bishop(move.Piece.Color, engine, engine.Board, move.Piece.Index);
                                break;
                            case PieceType.Rook:
                                promoteTo = new Rook(move.Piece.Color, engine, engine.Board, move.Piece.Index);
                                break;
                            case PieceType.Knight:
                                promoteTo = new Knight(move.Piece.Color, engine, engine.Board, move.Piece.Index);
                                break;
                            default:
                                promoteTo = new Queen(move.Piece.Color, engine, engine.Board, move.Piece.Index);
                                break;
                        }

                        move.AddPromotion(selectedPiece, promoteTo);
                    }

                    if (move.Piece is Pawn) { move.EnPassantMove = Math.Abs(move.To.Row - move.From.Row) == 2; }
                    if (move.Piece is King)
                    {
                        if (move.From.Col - move.To.Col == 2)
                        {
                            move.Castle = true;
                            move.CastleMove = new Move(new Position(move.Piece.Position.Row, 0), new Position(move.Piece.Position.Row, 3),
                                engine.Board.GetPiece(move.Piece.Position.Row, 0), engine.Board.GetPiece(move.Piece.Position.Row, 3));
                        }
                        else if (move.From.Col - move.To.Col == -2)
                        {
                            move.Castle = true;
                            move.CastleMove = new Move(new Position(move.Piece.Position.Row, 7), new Position(move.Piece.Position.Row, 5),
                                engine.Board.GetPiece(move.Piece.Position.Row, 7), engine.Board.GetPiece(move.Piece.Position.Row, 5));
                        }
                    }

                    engine.MovePiece(move);
                    Debug.WriteLine(engine.PrintBoard());
                    Debug.WriteLine(engine.WriteMove(move));
                    engine.ShowMove();
                }
                else
                {
                    Canvas.SetTop(imageDictionary[selectedPiece], selectedPiece.Position.Row * boardCanvas.Height / 8);
                    Canvas.SetLeft(imageDictionary[selectedPiece], selectedPiece.Position.Col * boardCanvas.Width / 8);
                    draggedImage.InvalidateVisual();
                }
                
                Panel.SetZIndex(draggedImage, 0);
                draggedImage = null;
            }
        }

        public void ShowProcessingBar(int seconds)
        {
            maxSeconds = seconds;
            worker = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };

            worker.ProgressChanged += new ProgressChangedEventHandler(ReportProgress);
            worker.DoWork += new DoWorkEventHandler(UpdateProgress);
            worker.RunWorkerAsync();
        }

        public void UpdateVisual(Board gameBoard)
        {
            Application.Current.Dispatcher.Invoke(delegate 
            {
                boardCanvas.Children.Clear();
                imageDictionary.Clear();
                CreateBoard();

                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        Piece piece = gameBoard.GetPiece(i, j);

                        if (!(piece is Empty)) { AddPiece(i, j, piece); }
                    }
                }

                boardCanvas.InvalidateVisual();
            });            
        }

        private void AddPiece(int row, int col, Piece p)
        {
            Image image = GetImage(p.Enum);
            imageDictionary.Add(p, image);
            Canvas.SetTop(imageDictionary[p], row * boardCanvas.Height / 8);
            Canvas.SetLeft(imageDictionary[p], col * boardCanvas.Width / 8);
            boardCanvas.Children.Add(image);
        }

        private Image GetImage(PieceEnum pieceEnum)
        {
            Image image = new Image();

            switch (pieceEnum)
            {
                case PieceEnum.BlackBishop:
                {
                    image.Source = new BitmapImage(new Uri("../Images/black_bishop.png", UriKind.RelativeOrAbsolute));
                    break;
                }
                case PieceEnum.BlackKing:
                {
                    image.Source = new BitmapImage(new Uri("../Images/black_king.png", UriKind.RelativeOrAbsolute));
                    break;
                }
                case PieceEnum.BlackKingNoCastle:
                {
                    image.Source = new BitmapImage(new Uri("../Images/black_king.png", UriKind.RelativeOrAbsolute));
                    break;
                }
                case PieceEnum.BlackKnight:
                {
                    image.Source = new BitmapImage(new Uri("../Images/black_knight.png", UriKind.RelativeOrAbsolute));
                    break;
                }
                case PieceEnum.BlackPawn:
                {
                    image.Source = new BitmapImage(new Uri("../Images/black_pawn.png", UriKind.RelativeOrAbsolute));
                    break;
                }
                case PieceEnum.BlackPawnEnPassant:
                {
                    image.Source = new BitmapImage(new Uri("../Images/black_pawn.png", UriKind.RelativeOrAbsolute));
                    break;
                }
                case PieceEnum.BlackQueen:
                {
                    image.Source = new BitmapImage(new Uri("../Images/black_queen.png", UriKind.RelativeOrAbsolute));
                    break;
                }
                case PieceEnum.BlackRook:
                {
                    image.Source = new BitmapImage(new Uri("../Images/black_rook.png", UriKind.RelativeOrAbsolute));
                    break;
                }
                case PieceEnum.WhiteBishop:
                {
                    image.Source = new BitmapImage(new Uri("../Images/white_bishop.png", UriKind.RelativeOrAbsolute));
                    break;
                }
                case PieceEnum.WhiteKing:
                {
                    image.Source = new BitmapImage(new Uri("../Images/white_king.png", UriKind.RelativeOrAbsolute));
                    break;
                }
                case PieceEnum.WhiteKingNoCastle:
                {
                    image.Source = new BitmapImage(new Uri("../Images/white_king.png", UriKind.RelativeOrAbsolute));
                    break;
                }
                case PieceEnum.WhiteKnight:
                {
                    image.Source = new BitmapImage(new Uri("../Images/white_knight.png", UriKind.RelativeOrAbsolute));
                    break;
                }
                case PieceEnum.WhitePawn:
                {
                    image.Source = new BitmapImage(new Uri("../Images/white_pawn.png", UriKind.RelativeOrAbsolute));
                    break;
                }
                case PieceEnum.WhitePawnEnPassant:
                {
                    image.Source = new BitmapImage(new Uri("../Images/white_pawn.png", UriKind.RelativeOrAbsolute));
                    break;
                }
                case PieceEnum.WhiteQueen:
                {
                    image.Source = new BitmapImage(new Uri("../Images/white_queen.png", UriKind.RelativeOrAbsolute));
                    break;
                }
                case PieceEnum.WhiteRook:
                {
                    image.Source = new BitmapImage(new Uri("../Images/white_rook.png", UriKind.RelativeOrAbsolute));
                    break;
                }
            }

            return image;
        }

        private void ReportProgress(object sender, ProgressChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                Progress_Bar.Value = e.ProgressPercentage;
            });
        }

        private void UpdateProgress(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            for (int i = 1; i <= 100; i++)
            { 
                System.Threading.Thread.Sleep(maxSeconds * 10);
                worker.ReportProgress(i * 1);                
            }
            worker.ReportProgress(0);
        }

        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (draggedImage != null)
            {
                var Position = e.GetPosition(boardCanvas);
                var offset = Position - mousePosition;
                mousePosition = Position;
                Canvas.SetLeft(draggedImage, Canvas.GetLeft(draggedImage) + offset.X);
                Canvas.SetTop(draggedImage, Canvas.GetTop(draggedImage) + offset.Y);
                draggedImage.InvalidateVisual();
            }
        }

        private void Player_Click(object sender, RoutedEventArgs e)
        {
            if (sender == HvH_MenuItem)
            {
                HvH_MenuItem.IsChecked = true;
                HvC_MenuItem.IsChecked = false;
                CvC_MenuItem.IsChecked = false;

                engine.WhiteHumanPlayer = true;
                engine.BlackHumanPlayer = true;

                White_Human_MenuItem.IsChecked = true;
                Black_Human_MenuItem.IsChecked = true;
            }

            else if (sender == HvC_MenuItem)
            {
                HvH_MenuItem.IsChecked = false;
                HvC_MenuItem.IsChecked = true;
                CvC_MenuItem.IsChecked = false;

                if (White_Human_MenuItem.IsChecked)
                {
                    Black_Human_MenuItem.IsChecked = false;
                    engine.BlackHumanPlayer = false;
                    engine.WhiteHumanPlayer = true;
                }
                else if (Black_Human_MenuItem.IsChecked)
                {
                    White_Human_MenuItem.IsChecked = false;
                    engine.BlackHumanPlayer = true;
                    engine.WhiteHumanPlayer = false;
                }
                else
                {
                    White_Human_MenuItem.IsChecked = true;                    
                    engine.BlackHumanPlayer = false;
                    engine.WhiteHumanPlayer = true;
                }
            }

            else if (sender == CvC_MenuItem)
            {
                HvH_MenuItem.IsChecked = false;
                HvC_MenuItem.IsChecked = false;
                CvC_MenuItem.IsChecked = true;

                Black_Human_MenuItem.IsChecked = false;
                White_Human_MenuItem.IsChecked = false;
                engine.BlackHumanPlayer = false;
                engine.WhiteHumanPlayer = false;
            }
        }

        private void Color_Click(object sender, RoutedEventArgs e)
        {
            if (sender == White_Human_MenuItem)
            {
                engine.WhiteHumanPlayer = White_Human_MenuItem.IsChecked;                
            }
            else if (sender == Black_Human_MenuItem)
            {
                engine.BlackHumanPlayer = Black_Human_MenuItem.IsChecked;
            }

            if (White_Human_MenuItem.IsChecked && Black_Human_MenuItem.IsChecked)
            {
                HvH_MenuItem.IsChecked = true;
                HvC_MenuItem.IsChecked = false;
                CvC_MenuItem.IsChecked = false;

                engine.WhiteHumanPlayer = true;
                engine.BlackHumanPlayer = true;
            }

            else if (White_Human_MenuItem.IsChecked)
            {
                HvH_MenuItem.IsChecked = false;
                HvC_MenuItem.IsChecked = true;
                CvC_MenuItem.IsChecked = false;

                engine.WhiteHumanPlayer = true;
                engine.BlackHumanPlayer = false;
            }

            else if (Black_Human_MenuItem.IsChecked)
            {
                HvH_MenuItem.IsChecked = false;
                HvC_MenuItem.IsChecked = true;
                CvC_MenuItem.IsChecked = false;

                engine.WhiteHumanPlayer = false;
                engine.BlackHumanPlayer = true;
            }
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            if (engine.MidGame)
            {
                // Configure the message box to be displayed
                string messageBoxText = "Do you want to start a new game?";
                string caption = "New Game";
                MessageBoxButton button = MessageBoxButton.YesNoCancel;
                MessageBoxImage icon = MessageBoxImage.Warning;

                MessageBoxResult response = MessageBox.Show(messageBoxText, caption, button, icon);

                if (response == MessageBoxResult.Yes)
                {
                    engine.SetUpPieces();
                }
            }
            else
            {
                engine.SetUpPieces();
            }
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}