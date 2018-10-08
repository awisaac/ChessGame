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

        public Adam(GameEngine e, PieceColor c)
        {
            engine = e;
            rand = new Random();

            if (c == PieceColor.White)
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
                string boardString = engine.PrintBoard();
                ulong hash = engine.Board.GetHash();

                selected = SelectNode(root);
                expanded = ExpandNode(selected);

                if (expanded != null)
                {
                    engine.MovePiece(expanded.GetMove());
                    result = Simulate(expanded.GetColor(), 0);

                    BackPropagateResult(expanded, result);
                }
                else
                {
                    BackPropagateResult(selected, GetEndGameResult(selected.GetColor(), 0));
                }

                if (!boardString.Equals(engine.PrintBoard()) || engine.Board.GetHash() != hash)
                {
                    Debug.WriteLine("Adam did not put the board back how he found it!\n");
                    Debug.WriteLine(boardString);
                    Debug.WriteLine(engine.PrintBoard());
                }

                count++;
            }

            Debug.WriteLine("Simulated " + count + " times");

            if (root.GetChildren().Count > 0)
            {
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

                bestMove = bestNode.GetMove();
                ShowMove();
            }
            else
            {
                Debug.WriteLine("No moves for root");
            }
        }

        private Node SelectNode(Node n)
        {
            // for now, select randomly
            List<Node> children = n.GetChildren();
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
                n.AddChild(m, n, nodeColor);                
            }

            if (n.GetChildren().Count > 0)
            {
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
            if (engine.IsCheckmate(color)) { return 1; }

            if (engine.IsStalemate(color)) { return 0; }
            if (engine.IsFiveFoldRepetition()) { return 0; }
            if (!engine.IsProgressive()) { return 0; }
            if (engine.InsufficientMaterial()) { return 0; }
            if (moveCount > maxGameMoves) { return 0; }
            
            return 2;
        }        

        public int Simulate(PieceColor color, int moveCount)
        {
            int result = GetEndGameResult(color, moveCount);

            if (result < 2) { return result; }

            List<Piece> pieces = engine.GetAllPieces(color);
            Piece randomPiece;

            if (pieces.Count > 0)
            {
                randomPiece = pieces[rand.Next(pieces.Count)];
                List<Move> moves = randomPiece.GetPotentialMoves();

                if (moves.Count > 0)
                {
                    int selected = rand.Next(moves.Count);
                    engine.MovePiece(moves[selected]);

                    if (color == PieceColor.White)
                    {
                        result = Simulate(PieceColor.Black, moveCount + 1);
                    }
                    else
                    {
                        result = Simulate(PieceColor.White, moveCount + 1);
                    }

                    engine.UnMovePiece(moves[selected]);
                    return result;
                }
                else
                {
                    return 0;
                }
            }

            Debug.WriteLine(color + " has no more pieces");
            return 0;
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
                engine.UnMovePiece(undoMove);
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
