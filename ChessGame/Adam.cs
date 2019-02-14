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
            dataTable.Columns.Add(new DataColumn("Advantages", typeof(int)));
            dataTable.Columns.Add(new DataColumn("Disadvantages", typeof(int)));

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
                string insertString = "INSERT INTO [ChessNodes].[dbo].[Nodes] (Hash, Wins, Losses, Attempts, CurrentTurn, Advantages, Disadvantages) " +
                    "VALUES (@Hash, @Wins, @Losses, @Attempts, @CurrentTurn, @Advantages, @Disadvantages)";

                SqlCommand deleteCommand = new SqlCommand(deleteString, connection);
                SqlCommand insertCommand = new SqlCommand(insertString, connection);

                insertCommand.Parameters.Add("@Hash", SqlDbType.BigInt, 8, "Hash");
                insertCommand.Parameters.Add("@Wins", SqlDbType.Int, 4, "Wins");
                insertCommand.Parameters.Add("@Losses", SqlDbType.Int, 4, "Losses");
                insertCommand.Parameters.Add("@Attempts", SqlDbType.Int, 4, "Attempts");
                insertCommand.Parameters.Add("@CurrentTurn", SqlDbType.NVarChar, 5, "CurrentTurn");
                insertCommand.Parameters.Add("@Advantages", SqlDbType.Int, 4, "Advantages");
                insertCommand.Parameters.Add("@Disadvantages", SqlDbType.Int, 4, "Disadvantages");

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

                    result = Simulate(0);

                    BackPropagateResult(expanded, result);                                                           
                }
                else if (GameDraw())
                {
                    BackPropagateResult(selected, 0);
                }
                else
                {
                    BackPropagateResult(selected, 2);
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

                Debug.WriteLine("Move\tScore\tAttempts");

                foreach (Node child in root.Children)
                {
                    double childScore;
                    double exploitation;
                    double checkBonus = 0.0;
                    double captureBonus = GetPieceValue(child.Move.Capture);
                    double capturePenalty = CapturePenalty(child);

                    if (child.Attempts > 2) { exploitation = child.GetScore() * Math.Sqrt(Math.Log(child.Attempts)); }
                    else { exploitation = child.GetScore(); }

                    if (child.Move.CheckBonus) { checkBonus = 0.2; }

                    childScore = exploitation + captureBonus + checkBonus + capturePenalty;

                    Debug.WriteLine(engine.WriteMove(child.Move) + "\t" + childScore + "\t" + child.Attempts);

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
                penalty -= GetPieceValue(child.Move.Capture);                
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
                    expanded.Advantages = Convert.ToInt32(existingEntries[0]["Advantages"]);
                    expanded.Disadvantages = Convert.ToInt32(existingEntries[0]["Disadvantages"]);    
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
                newRow["Advantages"] = n.Advantages;
                newRow["Disadvantages"] = n.Disadvantages;
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

            if (child.Attempts > 0) { exploration = Math.Sqrt(Math.Log(parent.Attempts) / child.Attempts); }
            else { exploration = Math.Sqrt(Math.Log(parent.Attempts)); }

            if (child.Attempts > 2) { exploitation = child.GetScore() * Math.Sqrt(Math.Log(child.Attempts)); }
            else { exploitation = child.GetScore(); }
            
            return exploitation + exploration;
        }

        private double GetPieceValue(Piece piece)
        {
            if (piece.Enum == PieceEnum.EmptyPosition)
            {
                return 0.0;
            }
            
            if (piece.Color == PieceColor.Black)
            {
                switch (piece.Enum)
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
                switch (piece.Enum)
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
        
        public int Simulate(int moveCount)
        {
            List<Piece> pieces = engine.GetAllPieces(engine.CurrentTurn);
            Piece randomPiece;
            List<Move> moves;
            Move randomMove = null;

            if (moveCount > 20)
            {
                return EarlyOutcome();
            }

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
                    int result = Simulate(moveCount + 1);
                    engine.UnMovePiece(randomMove);
                    return result;
                }

                pieces.Remove(randomPiece);
            }

            if (engine.IsThreatened(engine.Board.GetKing(engine.CurrentTurn))) { return -2; }
            else { return 0; }
        }

        private int EarlyOutcome()
        {
            double currentPlayerScore = 0;
            double nextPlayerScore = 0;
            PieceColor nextPlayer = engine.CurrentTurn == PieceColor.White? PieceColor.Black : PieceColor.White;
            
            foreach (Move m in engine.GetAllMoves(engine.CurrentTurn))
            {
                currentPlayerScore += GetPieceValue(m.Piece) + GetPieceValue(m.Capture);

                if (m.Piece is Pawn)
                {
                    if (engine.CurrentTurn == PieceColor.White)
                    {
                        currentPlayerScore += (6 - m.Piece.Position.Row) * 0.1;
                    }
                    else
                    {
                        currentPlayerScore += (m.Piece.Position.Row - 1) * 0.1;
                    }
                }
            }

            foreach(Move m in engine.GetAllMoves(nextPlayer))
            {
                nextPlayerScore += GetPieceValue(m.Piece) + GetPieceValue(m.Capture);

                if (m.Piece is Pawn)
                {
                    if (nextPlayer == PieceColor.White)
                    {
                        nextPlayerScore += (6 - m.Piece.Position.Row) * 0.1;
                    }
                    else
                    {
                        nextPlayerScore += (m.Piece.Position.Row - 1) * 0.1;
                    }
                }
            }

            if (currentPlayerScore > nextPlayerScore)
            {
                return 1;
            }

            if (currentPlayerScore < nextPlayerScore)
            {
                return -1;
            }

            return 0;
        }            

        private bool GameDraw()
        {
            return engine.IsThreeFoldRepetition() || !engine.IsProgressive() || engine.InsufficientMaterial();
        }

        public void BackPropagateResult(Node n, int result)
        {
            n.Attempts++;

            if (result == 2)
            {
                n.Wins++;
            }

            else if (result == -2)
            {
                n.Losses++;
            }

            else if (result == 1)
            {
                n.Advantages++;
            }
            else
            {
                n.Disadvantages++;
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
                BackPropagateResult(n.Parent, result * -1);
            }
        }
    }

    internal class Node
    {        
        public int Attempts { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Advantages { get; set; }
        public int Disadvantages { get; set; }
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
            Advantages = 0;
            Disadvantages = 0;
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
            double totalFinalDecisions = Wins + Losses;
            double totalEarlyDecisions = Advantages + Disadvantages;

            if (totalFinalDecisions > 0 && totalEarlyDecisions > 0)
            {   
                return ((Wins - Losses) / totalFinalDecisions)
                    + ((Advantages - Disadvantages) / totalEarlyDecisions) * 0.5;
            }

            if (totalFinalDecisions > 0)
            {
                return (Wins - Losses) / totalFinalDecisions;
            }

            if (totalEarlyDecisions > 0)
            {
                return ((Advantages - Disadvantages) / totalEarlyDecisions) * 0.5;
            }

            return 0;                                   
        }
    }
}
