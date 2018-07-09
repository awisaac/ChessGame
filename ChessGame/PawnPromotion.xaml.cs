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
using System.Windows.Shapes;
using ChessGame.Pieces;

namespace ChessGame
{
    /// <summary>
    /// Interaction logic for TaskDialog.xaml
    /// </summary>
    public partial class PawnPromotion : Window
    {
        public PieceType SelectedPiece { get; set; }

        public PawnPromotion()
        {
            InitializeComponent();
            SelectedPiece = PieceType.Queen;
        }

        public void selectionMade(object sender, EventArgs e)
        {
            if ((bool)queenPromote.IsChecked)
            {
                SelectedPiece = PieceType.Queen;
            }
            else if ((bool)bishopPromote.IsChecked)
            {
                SelectedPiece = PieceType.Bishop;
            }
            else if ((bool)rookPromote.IsChecked)
            {
                SelectedPiece = PieceType.Rook;
            }
            else
            {
                SelectedPiece = PieceType.Knight;
            }
        }

        public void ok_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
