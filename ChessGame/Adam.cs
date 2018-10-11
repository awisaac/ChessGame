using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using ChessGame.Pieces;

namespace ChessGame
{
    class Adam
    {
        GameEngine engine;
        Node root;
        const int maxGameMoves = 50;
        int maxSeconds = 5;
        Move bestMove;
        Random rand;

        public Adam(GameEngine e)
        {
            engine = e;
            rand = new Random();

            if (engine.CurrentTurn == PieceColor.White)
            {
                root = new Node(null, null, PieceColor.Black);
            }
            else
            {
                root = new Node(null, null, PieceColor.White);
            }            
        }

        private void ShowMove()
        {
            engine.MovePiece(bestMove);
            Debug.WriteLine(engine.PrintBoard());
            Debug.WriteLine(engine.WriteMove(bestMove));
            engine.ShowMove();
        }

        public void RunSimulation(int seconds)
        {
            maxSeconds = seconds;
            Thread thread = new Thread(BeginSimulation);
            thread.Start();
        }

        public void BeginSimulation()
        {
            DateTime now = DateTime.Now;
            DateTime addedSecs = now.AddSeconds(maxSeconds);
            int result;
            Node selected;
            Node expanded;
            int count = 0;

            while (DateTime.Now < addedSecs)
            {
                ulong hash = engine.Board.GetHash();

                selected = SelectNode(root);
                expanded = ExpandNode(selected);

                if (expanded != null)
                {
                    engine.MovePiece(expanded.Move);
                    result = Simulate();
                    BackPropagateResult(expanded, result);
                }
                else
                {
                    BackPropagateResult(selected, GetEndGameResult());
                }

                if (engine.Board.GetHash() != hash)
                {
                    Debug.WriteLine("Adam did not put the board back how he found it!\n");
                }
                    
                count++;
            }

            Debug.WriteLine("Simulated " + count + " times");
            if (root.Children.Count > 0)
            {
                Node bestNode = root.Children[0];
                double bestScore = bestNode.GetScore();

                foreach (Node n in root.Children)
                {
                    if (n.GetScore() > bestScore)
                    {
                        bestScore = n.GetScore();
                        bestNode = n;
                    }
                }

                bestMove = bestNode.Move;
                ShowMove();
            }
            else
            {
                Debug.WriteLine("No moves for root");
            }
        }

        private Node SelectNode(Node n)
        {
            List<Node> children = n.Children;
            int bestIndex = 0;
            double bestScore = 0;
            int childIndex = 0;
            double childScore;
            double exploitation = 0;
            double exploration = 0;
            
            foreach (Node child in children)
            {
                exploitation = child.GetScore();

                if (child.Attempts > 0) { exploration = Math.Sqrt(Math.Log(n.Attempts) / child.Attempts); }
                else { exploration = 0; }

                childScore = exploitation + exploration;

                if (childScore > bestScore)
                {
                    bestIndex = childIndex;
                    bestScore = childScore;
                    childIndex++;
                }
            }

            if (children.Count > 0)
            {
                Node child = children[bestIndex];
                engine.MovePiece(child.Move);
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

            if (n.Color == PieceColor.White)
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
                n.AddChild(m, n, nodeColor);                
            }

            if (n.Children.Count > 0)
            {
                int selected = rand.Next(n.Children.Count);                
                Node child = n.Children[selected];
                return child;
            }
            else
            {
                return null;
            }
        }
        
        public int Simulate()
        {
            List<Piece> pieces = engine.GetAllPieces(engine.CurrentTurn);

            if (pieces.Count == 0)
            {
                Debug.WriteLine("No more pieces!");
            }
            Piece randomPiece = pieces[rand.Next(pieces.Count)];
            List<Move> moves = randomPiece.GetPotentialMoves();
            
            while (moves.Count == 0 && pieces.Count > 0)
            {
                pieces.Remove(randomPiece);

                if (pieces.Count > 0)
                {
                    randomPiece = pieces[rand.Next(pieces.Count)];
                    moves = randomPiece.GetPotentialMoves();
                }
                else
                {
                    if (engine.IsInCheck(engine.CurrentTurn))
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            if (moves.Count > 0)
            {
                int selected = rand.Next(moves.Count);
                Move move = moves[selected];
                               
                engine.MovePiece(move);
                int result = GetEndGameResult();
                if (result == 2) { result = Simulate(); }

                engine.UnMovePiece(move);
                return result;
            }
            else
            {
                if (engine.IsInCheck(engine.CurrentTurn))
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }

        private int GetEndGameResult()
        {
            if (engine.IsThreeFoldRepetition()
                || !engine.IsProgressive()
                || engine.InsufficientMaterial())
            {
                return 0;
            }

            List<Move> moves = engine.GetAllMoves(engine.CurrentTurn);

            if (moves.Count == 0)
            {
                bool inCheck = engine.IsInCheck(engine.CurrentTurn);

                if (inCheck) { return -1; }
                else { return 0; }
            }

            return 2;
        }

        public void BackPropagateResult(Node n, int result)
        {
            n.Attempts++;

            if (result == 1)
            {
                n.Wins++;
                result = -1;
            }

            else if (result == -1)
            {
                n.Losses--;
                result = 1;
            }

            Move undoMove = n.Move;

            if (undoMove != null)
            {
                engine.UnMovePiece(undoMove);
            }            

            if (n.Parent != null)
            {
                BackPropagateResult(n.Parent, result);
            }
        }
    }

    internal class Node
    {        
        public int Attempts { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }

        public Move Move { get; set; }
        public PieceColor Color { get; set; }

        public Node Parent { get; set; }
        public List<Node> Children { get; set; }

        internal Node(Move m, Node p, PieceColor c)
        {
            Attempts = 0;
            Wins = 0;
            Losses = 0;
            Children = new List<Node>();
            Move = m;
            Parent = p;
            Color = c;
        }

        internal void AddChild(Move m, Node p, PieceColor c)
        {
            Children.Add(new Node(m, p, c));
        }

        internal double GetScore()
        {
            if (Attempts > 0)
            {
                return (double)(Wins - Losses) / (double)(Attempts);
            }
            else
            {
                return 0;
            }            
        }

    }
}
