using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using ChessGame.Pieces;

namespace ChessGame
{
    class Adam
    {
        private GameEngine engine;
        private Node root;
        private const string connectionString = "Data Source=DESKTOP-T0C2K2N\\SQLEXPRESS;Integrated Security=True;Connect Timeout=30;" + 
                                        "Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        private int maxSeconds = 5;
        private Move bestMove;
        private Random rand;
        private DataTable dataTable;
        private SqlDataAdapter adapter;

        public Adam(GameEngine e)
        {
            engine = e;
            rand = new Random();

            if (engine.CurrentTurn == PieceColor.White) { root = new Node(null, null, PieceColor.Black); }
            else { root = new Node(null, null, PieceColor.White); }

            root.Hash = engine.Board.Hash;

            dataTable = new DataTable();

            dataTable.Columns.Add(new DataColumn("Hash", typeof(long)));
            dataTable.Columns.Add(new DataColumn("Wins", typeof(int)));
            dataTable.Columns.Add(new DataColumn("Losses", typeof(int)));
            dataTable.Columns.Add(new DataColumn("Attempts", typeof(int)));
            dataTable.Columns.Add(new DataColumn("CurrentTurn"));

            ReadFromDataBase("SELECT * FROM [ChessNodes].[dbo].[Nodes]");            
        }

        private void ReadFromDataBase(string queryString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                adapter = new SqlDataAdapter
                {
                    SelectCommand = new SqlCommand(queryString, connection)
                };
                adapter.Fill(dataTable);
            }
        }

        private void WriteToDataBase()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string deleteString = "DELETE FROM [ChessNodes].[dbo].[Nodes]";
                string insertString = "INSERT INTO [ChessNodes].[dbo].[Nodes] (Hash, Wins, Losses, Attempts, CurrentTurn) VALUES (@Hash, @Wins, @Losses, @Attempts, @CurrentTurn)";

                SqlCommand deleteCommand = new SqlCommand(deleteString, connection);
                SqlCommand insertCommand = new SqlCommand(insertString, connection);

                insertCommand.Parameters.Add("@Hash", SqlDbType.BigInt, 8, "Hash");
                insertCommand.Parameters.Add("@Wins", SqlDbType.Int, 4, "Wins");
                insertCommand.Parameters.Add("@Losses", SqlDbType.Int, 4, "Losses");
                insertCommand.Parameters.Add("@Attempts", SqlDbType.Int, 4, "Attempts");
                insertCommand.Parameters.Add("@CurrentTurn", SqlDbType.NVarChar, 5, "CurrentTurn");

                adapter.DeleteCommand = deleteCommand;
                adapter.InsertCommand = insertCommand;
                
                adapter.Update(dataTable);
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
                long hash = engine.Board.Hash;

                selected = SelectNode(root);
                expanded = ExpandNode(selected);

                if (expanded != null)
                {
                    engine.MovePiece(expanded.Move);
                    expanded.Hash = engine.Board.Hash;
                    ExpansionCheck(expanded);                    

                    result = Simulate();
                    BackPropagateResult(expanded, result);                   
                }
                else
                {
                    BackPropagateResult(selected, GameDraw()? 0:2);
                }

                if (engine.Board.Hash != hash)
                {
                    Debug.WriteLine("Adam did not put the board back how he found it!\n");
                }
                    
                count++;
            }

            dataTable.Rows.Clear();
            UpdateDataTable(root);
            WriteToDataBase();

            Debug.WriteLine("Simulated " + count + " times");
            if (root.Children.Count > 0)
            {
                Node bestNode = root.Children[0];
                double bestScore = double.MinValue;

                Debug.WriteLine("Move\tScore\tExploitation\tCaptureBonus\tCheckBonus\tCapturePenalty");

                foreach (Node child in root.Children)
                {
                    double childScore;
                    double exploitation;
                    double exploration;
                    double checkBonus = 0.0;
                    double captureBonus = GetPieceCaptureValue(child.Move.Capture);
                    double capturePenalty = CapturePenalty(child);

                    if (child.Attempts > 0) { exploration = Math.Sqrt(Math.Log(root.Attempts) / child.Attempts); }
                    else { exploration = Math.Sqrt(Math.Log(root.Attempts)); }
                    if (child.Attempts > 2) { exploitation = child.GetScore() * Math.Sqrt(Math.Log(child.Attempts)); }
                    else { exploitation = child.GetScore(); }

                    if (child.Move.CheckBonus) { checkBonus = 0.5; }

                    childScore = exploitation + captureBonus + checkBonus + capturePenalty;

                    Debug.WriteLine(engine.WriteMove(child.Move) + "\t" + childScore + "\t" + exploitation + "\t" + captureBonus + "\t" + checkBonus + "\t" + capturePenalty);

                    if (childScore > bestScore)
                    {
                        bestScore = childScore;
                        bestNode = child;
                    }
                }

                bestMove = bestNode.Move;
                ShowMove();
            }
        }

        private double CapturePenalty(Node node)
        {
            double penalty = 0;

            foreach (Node child in node.Children)
            {
                penalty -= GetPieceCaptureValue(child.Move.Capture);                
            }

            return penalty;
        }

        private void ExpansionCheck(Node expanded)
        {
            if (dataTable.Rows.Count > 0)
            {
                string filter = "Hash = '" + expanded.Hash + "' AND CurrentTurn = '" + expanded.Color + "'";
                DataRow[] existingEntries = dataTable.Select(filter);

                if (existingEntries.Length > 0)
                {
                    expanded.Wins = Convert.ToInt32(existingEntries[0]["Wins"]);
                    expanded.Losses = Convert.ToInt32(existingEntries[0]["Losses"]);
                    expanded.Attempts = Convert.ToInt32(existingEntries[0]["Attempts"]);
                }
            }            
        }

        private void UpdateDataTable(Node n)
        {
            if (n.Attempts > 1)
            {
                DataRow newRow = dataTable.NewRow();
                newRow["Hash"] = n.Hash;
                newRow["Wins"] = n.Wins;
                newRow["Losses"] = n.Losses;
                newRow["Attempts"] = n.Attempts;
                newRow["CurrentTurn"] = n.Color;
                dataTable.Rows.Add(newRow);

                foreach (Node child in n.Children)
                {
                    UpdateDataTable(child);
                }
            }              
        }

        private Node SelectNode(Node n)
        {
            if (n.Children.Count > 0)
            {
                Node bestNode = n.Children[0];
                double bestScore = double.MinValue;
                double childScore;

                foreach (Node child in n.Children)
                {
                    childScore = CalculateMoveScore(n, child);

                    if (childScore > bestScore)
                    {
                        bestNode = child;
                        bestScore = childScore;
                    }
                }

                engine.MovePiece(bestNode.Move);
                n = SelectNode(bestNode);
                return n;
            }

            else
            {
                return n;
            }
        }

        private double CalculateMoveScore(Node parent, Node child)
        {
            double exploitation;
            double exploration;
            double checkValue = 0.0;

            if (child.Move.CheckBonus)
            {
                checkValue = 0.5;
            }

            if (child.Attempts > 0) { exploration = Math.Sqrt(Math.Log(parent.Attempts) / child.Attempts); }
            else { exploration = Math.Sqrt(Math.Log(parent.Attempts)); }

            if (child.Attempts > 2) { exploitation = child.GetScore() * Math.Sqrt(Math.Log(child.Attempts)); }
            else { exploitation = child.GetScore(); }
            
            return exploitation + exploration + GetPieceCaptureValue(child.Move.Capture) + checkValue + CapturePenalty(child);
        }

        private double GetPieceCaptureValue(Piece capture)
        {
            if (capture.Enum == PieceEnum.EmptyPosition)
            {
                return 0.0;
            }
            
            if (capture.Color == PieceColor.Black)
            {
                switch (capture.Enum)
                {
                    case PieceEnum.BlackBishop:
                        return 0.3;
                    case PieceEnum.BlackKnight:
                        return 0.3;
                    case PieceEnum.BlackPawn:
                        return 0.1;
                    case PieceEnum.BlackPawnEnPassant:
                        return 0.1;
                    case PieceEnum.BlackQueen:
                        return 0.9;
                    case PieceEnum.BlackRook:
                        return 0.5;
                    default:
                        return 0.0;
                }
            }
            else
            {
                switch (capture.Enum)
                {
                    case PieceEnum.WhiteBishop:
                        return 0.3;
                    case PieceEnum.WhiteKnight:
                        return 0.3;
                    case PieceEnum.WhitePawn:
                        return 0.1;
                    case PieceEnum.WhitePawnEnPassant:
                        return 0.1;
                    case PieceEnum.WhiteQueen:
                        return 0.9;
                    case PieceEnum.WhiteRook:
                        return 0.5;
                    default:
                        return 0.0;
                }
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
                if (!engine.WillCauseCheck(m)) { n.AddChild(m, n, nodeColor); }                
            }

            if (n.Children.Count > 0)
            {
                int selected = rand.Next(n.Children.Count);
                return n.Children[selected];     
            }
            else
            {
                return null;
            }
        }
        
        public int Simulate()
        {
            List<Piece> pieces = engine.GetAllPieces(engine.CurrentTurn);
            Piece randomPiece;
            List<Move> moves;
            Move randomMove = null;

            if (GameDraw())
            {
                return 0;
            }

            while (pieces.Count > 0)
            {
                randomPiece = pieces[rand.Next(pieces.Count)];
                moves = randomPiece.GetPotentialMoves();

                while (moves.Count > 0)
                {
                    randomMove = moves[rand.Next(moves.Count)];

                    if (engine.WillCauseCheck(randomMove)) 
                    {
                        moves.Remove(randomMove);
                    }
                    else
                    {
                        break;
                    }
                }

                if (moves.Count > 0)
                {
                    engine.MovePiece(randomMove);
                    int result = Simulate();
                    engine.UnMovePiece(randomMove);
                    return result;
                }

                pieces.Remove(randomPiece);
            }

            if (engine.IsInCheck(engine.CurrentTurn)) { return -1; }
            else { return 0; }
        }

        private bool GameDraw()
        {
            return engine.IsThreeFoldRepetition() || !engine.IsProgressive() || engine.InsufficientMaterial();
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
                n.Losses++;
                result = 1;
            }

            Move undoMove = n.Move;

            if (n.Hash == 0)
            {
                n.Hash = engine.Board.Hash;
            }

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
        public long Hash { get; set; }

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
            if (Attempts > 0) { return (double)(Wins - Losses) / (double)(Attempts); }
            else { return 0; }            
        }
    }
}
