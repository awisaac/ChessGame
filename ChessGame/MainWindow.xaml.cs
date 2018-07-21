using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChessGame.Pieces;

namespace ChessGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Image draggedImage;
        private Point mousePosition;
        private GameEngine engine;
        Piece selectedPiece;

        public MainWindow()
        {
            InitializeComponent();            
            engine = new GameEngine(boardCanvas);
            engine.CreateBoard();
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

                draggedImage = selectedPiece.Image;
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
                    Position from = engine.Board.GetPosition(selectedPiece);
                    Piece capture = engine.CapturePiece(selectedPiece, to);

                    bool promotion = selectedPiece is Pawn && (destRow == 7 || destRow == 0);

                    Move move = new Move(from, to, selectedPiece, capture, promotion);
                    engine.ShowMove(move);
                }
                else
                {
                    Canvas.SetTop(selectedPiece.Image, engine.Board.GetPosition(selectedPiece).Row * boardCanvas.Height / 8);
                    Canvas.SetLeft(selectedPiece.Image, engine.Board.GetPosition(selectedPiece).Col * boardCanvas.Width / 8);
                    draggedImage.InvalidateVisual();
                }
                
                Panel.SetZIndex(draggedImage, 0);
                draggedImage = null;
            }
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
