using ChessGame.Pieces;

namespace ChessGame
{
    internal class Move
    {
        internal Position From { get; }
        internal Position To { get; }
        internal Piece Piece { get; set; }
        internal Piece Capture { get; }
        internal bool Promotion { get; set; }
        internal Piece PromotedFrom { get; set; }
        internal Piece PromotedTo { get; set; }
        internal bool EnPassantMove { get; set; }
        internal bool Castle { get; set; }
        internal Move CastleMove { get; set; }
        internal bool TestMove { get; set; }

        internal Move(Position from, Position to, Piece piece, Piece capture)
        {
            From = from;
            To = to;
            Piece = piece;
            Capture = capture;
        }


        internal void AddPromotion(Piece promoteFrom, Piece promoteTo)
        {
            PromotedFrom = promoteFrom;
            PromotedTo = promoteTo;           
        }
    }
}