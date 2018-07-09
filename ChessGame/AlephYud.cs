using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChessGame.Pieces;

namespace ChessGame
{
    class AlephYud
    {
        GameEngine engine;
        Node root;

        public AlephYud(GameEngine e, PieceColor c)
        {
            engine = e;
            root = new Node(null, null, c);
        }

        public Move RunSimulation()
        {
            DateTime now = DateTime.Now;
            DateTime addedSecs = now.AddSeconds(5);
            int result;
            Node selected;
            Node expanded;
            int count = 0;

            while (DateTime.Now < addedSecs)
            {
                selected = SelectNode(root);
                expanded = ExpandNode(selected);

                if (expanded != null)
                {
                    engine.MovePiece(expanded.GetMove());
                    result = Simulate(expanded.GetColor(), count);
                    BackPropagateResult(expanded, result);
                }
                else
                {
                    BackPropagateResult(selected, GetEndGameResult(selected.GetColor(), 0));
                }                
            }

            Node bestNode = root.GetChildren()[0];
            double bestScore = bestNode.GetScore();

            foreach (Node n in root.GetChildren())
            {
                if (n.GetScore() > bestScore)
                {
                    bestScore = n.GetScore();
                    bestNode = n;
                }
            }

            return bestNode.GetMove();
        }

        private Node SelectNode(Node n)
        {
            // for now, select randomly
            List<Node> children = n.GetChildren();

            Random rand = new Random();
            int selected = rand.Next(children.Count);

            if (n.GetChildren().Count > 0)
            {
                Node child = n.GetChildren()[selected];
                engine.MovePiece(child.GetMove());
                n = SelectNode(child);
                return n;
            }

            else
            {
                return n;
            }
        }

        private Node ExpandNode(Node n)
        {
            List<Move> moves;
            PieceColor nodeColor;

            if (n.GetColor() == PieceColor.White)
            {                
                nodeColor = PieceColor.Black;
            }
            else
            {                
                nodeColor = PieceColor.White;
            }

            moves = engine.GetAllMoves(nodeColor);

            foreach (Move m in moves)
            {
                if (!engine.CheckBlock(m, nodeColor))
                {
                    n.AddChild(m, n, nodeColor);
                }
            }

            if (n.GetChildren().Count > 0)
            {
                Random rand = new Random();
                int selected = rand.Next(n.GetChildren().Count);

                Node child = n.GetChildren()[selected];
                return child;
            }
            else
            {
                return null;
            }
        }

        private int GetEndGameResult(PieceColor color, int moveCount)
        {
            if (color == PieceColor.White)
            {
                if (engine.IsCheckmate(PieceColor.Black))
                {
                    return 1;
                }
                if (engine.IsCheckmate(PieceColor.White))
                {
                    return -1;
                }
            }

            if (color == PieceColor.Black)
            {
                if (engine.IsCheckmate(PieceColor.Black))
                {
                    return -1;
                }
                if (engine.IsCheckmate(PieceColor.White))
                {
                    return 1;
                }
            }
            if (engine.IsStalement(color) 
                || engine.IsFiveFoldRepetition() 
                || !engine.IsProgressive() 
                || engine.InsufficientMaterial()
                || moveCount > 40)
            {
                return 0;
            }

            return 2;
        }        

        public int Simulate(PieceColor color, int moveCount)
        {
            int result = GetEndGameResult(color, moveCount);

            if (moveCount > 40)
            {
                System.Diagnostics.Debug.Write("Count: " + moveCount);
            }            

            if (result <= 1)
            {
                return result;
            }

            List<Move> allMoves = engine.GetAllMoves(color);
            List<Move> validMoves = new List<Move>();
            foreach (Move m in allMoves)
            {
                if (!engine.CheckBlock(m, color))
                {
                    validMoves.Add(m);
                }
            }

            if (validMoves.Count > 0)
            {
                Random rand = new Random();
                int selected = rand.Next(validMoves.Count);
                engine.MovePiece(validMoves[selected]);

                result = Simulate(engine.CurrentTurn, moveCount + 1);
                engine.UnMovePiece(validMoves[selected]);
                return result;
            }
            else
            {
                return 0;
            }
        }

        public void BackPropagateResult(Node n, int result)
        {
            n.IncreaseAttempts();

            if (result == 1)
            {
                n.IncreaseWins();
                result = -1;
            }

            else if (result == 0)
            {
                n.IncreaseDraws();
            }

            else if (result == -1)
            {
                n.IncreaseLosses();
                result = 1;
            }

            Move undoMove = n.GetMove();

            if (undoMove != null)
            {
                engine.UnMovePiece(n.GetMove());
            }            

            if (n.GetParent() != null)
            {
                BackPropagateResult(n.GetParent(), result);
            }
        }
    }

    internal class Node
    {        
        int attempts;
        int wins;
        int losses;
        int draws;
        Move move;
        PieceColor color;

        Node parent;
        List<Node> children;

        internal Node(Move m, Node p, PieceColor c)
        {
            attempts = 0;
            wins = 0;
            losses = 0;
            draws = 0;
            children = new List<Node>();
            move = m;
            parent = p;
            color = c;
        }

        internal void AddChild(Move m, Node p, PieceColor c)
        {
            children.Add(new Node(m, p, c));
        }

        internal List<Node> GetChildren()
        {
            return children;
        }

        internal Node GetParent()
        {
            return parent;
        }

        internal Move GetMove()
        {
            return move;
        }

        internal PieceColor GetColor()
        {
            return color;
        }

        internal void IncreaseAttempts()
        {
            attempts++;
        }

        internal void IncreaseWins()
        {
            wins++;
        }

        internal void IncreaseDraws()
        {
            draws++;
        }

        internal void IncreaseLosses()
        {
            losses++;
        }

        internal double GetScore()
        {
            return (double)(wins - losses) / (double)(attempts); 
        }

    }
}
