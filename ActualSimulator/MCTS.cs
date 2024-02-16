using FantomSimulatorLibrary;
using System.Xml.XPath;
namespace ActualSimulator;


public interface IGameDescription<TStateName, TAction>
    where TAction : notnull
{
    public TStateName NextState(TStateName stateName, TAction action);
    public List<TAction> Actions(TStateName stateName);
    public TAction RandomAction(TStateName stateName, HashSet<TAction> usedActions);
    public TAction RandomAction(TStateName stateName);
    public long NumberOfActions(TStateName stateName);
    public Dictionary<TAction, TStateName>? GetAllDeterminizationsInInfoSet(TStateName stateName);
    public double? Reward(TStateName stateName);
    public TStateName Root { get; }
}

public class MonteCarloTreeSearch<TStateName, TAction>
    where TAction : notnull
{
    public class Node
    {
        public int Visits { get; set; }
        public int Wins { get; set; }
        //public double Value { get; set; }
        public Dictionary<TAction, Node> Children { get; set; }
        public Node? Parent { get; set; }
        public TStateName StateName { get; set; }
        public bool DeterminizationNode { get; set; } = false;

        public bool Player1Plays;

        public IGameDescription<TStateName, TAction> GameDescription { get; set; }

        public Node(IGameDescription<TStateName, TAction> gs, TStateName stateName, bool player1Plays, Node? parent = null)
        {
            Visits = 0;
            Children = [];
            Parent = parent;
            GameDescription = gs;
            StateName = stateName;
            Player1Plays = player1Plays;
        }

        public Node(Node oldNode)
        {
            Visits = oldNode.Visits;
            Children = new(oldNode.Children);
            Parent = oldNode.Parent;
            GameDescription = oldNode.GameDescription;
            StateName = oldNode.StateName;
        }

        public bool IsTerminal()
            => GameDescription.Reward(StateName) is not null;
        //public double GetReward()
        //{
        //    var reward = GameDescription.Reward(StateName);

        //}

        public bool IsRoot()
            => Parent is null;

        public void UpdateStats(double result)
        {
            Visits++;
            if ((Player1Plays && result > 0) || (!Player1Plays && result < 0))
                Wins++;
        }

    }

    public class Tree
    {
        public Node Root { get; set; }
        private double ExplorationParameter = Math.Sqrt(2);
        public Random rnd;


        public Tree(IGameDescription<TStateName, TAction> gameInfo, TStateName stateName, Random? rnd = null)
        {
            Root = new(gameInfo, stateName, player1Plays: false);
            if (rnd is null)
                this.rnd = new();
            else
                this.rnd = rnd;
        }

        public Node Traverse(Node node)
        {
            while (IsFullyExpanded(node))
            {
                node = GetBestChild(node);
            }
            if (node.IsTerminal())
                return node;
            return PickUnvisited(node);
        }

        public bool IsFullyExpanded(Node node)
        {
            if (node.DeterminizationNode || (node.Children.Count == node.GameDescription.NumberOfActions(node.StateName) && !node.IsTerminal()))//node.GameDescription.NumberOfActions(node.StateName))
                return true;
            return false;
        }

        private Node GetRandomDeterminization(Node node)
        {
            var possibleActions = node.Children.Keys.ToList();
            return node.Children[possibleActions[rnd.Next(possibleActions.Count)]];
        }

        public Node GetBestChild(Node node)
        {
            // If node is information set with >1 histories
            if (node.DeterminizationNode)
                // return a random determinization
                return GetRandomDeterminization(node);

            // else return the child with highest criterion
            double max = 0;
            Node bestNode = node.Children.Values.First();
            foreach (var child in node.Children.Values)
            {
                // the criterion is UPC1
                double value = UpperConfidenceBound1(child, node);
                if (value > max)
                {
                    max = value;
                    bestNode = child;
                }
            }
            return bestNode;
        }

        private double UpperConfidenceBound1(Node currentNode, Node parent)
        {
            // Unexplored node => has high value
            if (currentNode.Visits == 0)
                return double.MaxValue;

            return currentNode.Wins / currentNode.Visits + ExplorationParameter * Math.Sqrt(Math.Log(parent.Visits) / currentNode.Visits);
        }

        public Node PickUnvisited(Node node)
        {
            // pick random possible action, create a child and update the parent
            var randomAction = node.GameDescription.RandomAction(node.StateName, node.Children.Keys.ToHashSet());
            Node newNode = new(node.GameDescription, node.GameDescription.NextState(node.StateName, randomAction), !node.Player1Plays, node);
            node.Children[randomAction] = newNode;
            return newNode;
        }

        public double? Rollout(Node node)
        {
            Node tempNode = new(node);
            while (!tempNode.IsTerminal())
            {
                var action = tempNode.GameDescription.RandomAction(tempNode.StateName);
                var newState = tempNode.GameDescription.NextState(tempNode.StateName, action);
                tempNode = new(tempNode.GameDescription, newState, !tempNode.Player1Plays);
            }
            return tempNode.GameDescription.Reward(tempNode.StateName);
        }

        public void BackPropagate(Node? node, double result)
        {
            if (node is null)
                return;
            node.UpdateStats(result);
            BackPropagate(node.Parent, result);
        }

    }

    public Node CreateNodeFromDeterminizations(Node parent, Dictionary<TAction, TStateName> determinizations)
    {
        if (determinizations.Count == 1)
            return parent;

        var newNode = new Node(parent);
        newNode.DeterminizationNode = true;
        foreach (TAction key in determinizations.Keys)
        {
            var child = new Node(parent.GameDescription, determinizations[key], parent.Player1Plays, parent);
            newNode.Children.Add(key, child);
        }
        return newNode;
    }

    private Node PrepareDeterminizationsIfNeccessary(Node root)
    {
        Node simulationRoot = root;

        // Check if the root is an infoset with >1 histories
        var det = root.GameDescription.GetAllDeterminizationsInInfoSet(root.StateName);
        // Create a temp root with a child for each determinization if it is
        if (det is not null)
            simulationRoot = CreateNodeFromDeterminizations(root, det);

        return simulationRoot;
    }

    public TAction Simulate(Tree tree, double time)
    {
        var simulationRoot = PrepareDeterminizationsIfNeccessary(tree.Root);

        DateTime end = DateTime.Now.AddSeconds(time);

        while (DateTime.Now < end)
        {
            var leaf = tree.Traverse(simulationRoot);
            var simulationResult = tree.Rollout(leaf);
            if (simulationResult.HasValue)
                tree.BackPropagate(leaf, simulationResult.Value);
        }

        return BestActionWithDeterminization(simulationRoot);
    }

    public TAction Simulate(Tree tree, int iterations)
    {
        var simulationRoot = PrepareDeterminizationsIfNeccessary(tree.Root);

        for (int i = 0; i < iterations; i++)
        {
            var leaf = tree.Traverse(simulationRoot);
            var simulationResult = tree.Rollout(leaf);
            if (simulationResult.HasValue)
                tree.BackPropagate(leaf, simulationResult.Value);
        }

        return BestActionWithDeterminization(simulationRoot);
    }

    private TAction BestActionWithDeterminization(Node root)
    {
        if (root.DeterminizationNode)
        {
            // If the root is a node with children as determinizations
            // sum up the number of win for each possible action
            Dictionary<TAction, int> wins = [];
            foreach ((var key, var child) in root.Children)
            {
                foreach ((var key2, var grandchild) in child.Children)
                {
                    wins.TryAdd(key2, 0);
                    wins[key2] += grandchild.Wins;
                }
            }
            // Return the action with max number of wins
            return wins.MaxBy(item => item.Value).Key;
        }
        // Otherwise return the key to the child with most wins
        return root.Children.MaxBy(item => item.Value.Wins).Key;
    }

}


public struct GlassesFantomState
{
    public int FantomPosition;
    //public int FantomTokens;

    public List<int> DetectivesPositions;
    //public List<int> DetectivesTokens;

    public int Turn;
    public bool FantomsTurn;

    public GlassesFantomState(GlassesFantomState state)
    {
        FantomPosition = state.FantomPosition;
        DetectivesPositions = new List<int>(state.DetectivesPositions);
        Turn = state.Turn;
        FantomsTurn = state.FantomsTurn;
    }
}

public struct GlassesFantomAction
{
    public GlassesFantomAction(List<(int from, int to)> moves)
    {
        Moves = moves;
    }
    public List<(int From, int To)> Moves;

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            foreach (var (From, To) in Moves)
            {
                hash = hash * 23 + From.GetHashCode();
                hash = hash * 23 + To.GetHashCode();
            }
            return hash;
        }
    }

    public override bool Equals(object obj)
    {
        if (obj is GlassesFantomAction otherAction)
        {
            return Moves.SequenceEqual(otherAction.Moves);
        }
        return false;
    }
}

//public class GlassesFantomGame : IGameDescription<GlassesFantomState, GlassesFantomAction>
//{
//    public int GameLen = 8;
//    public GlassesFantomState Root
//    {
//        get
//        {
//            return new() { FantomPosition = 4, DetectivesPositions = [1, 2], FantomsTurn = true, Turn = 0 };
//        }
//    }
//    public List<GlassesFantomAction> Actions(GlassesFantomState state)
//    {
//        if (state.Turn > GameLen)
//            return [];
//        List<GlassesFantomAction> actions = [];
//        if (state.FantomsTurn)
//        {
//            var possibleMoves = Edges[state.FantomPosition];
//            HashSet<int> taken = state.DetectivesPositions.ToHashSet();
//            foreach (var move in possibleMoves)
//            {
//                if (!taken.Contains(move))
//                    actions.Add(new([(state.FantomPosition, move)]));
//            }
//        }
//        else
//            actions = GenerateCombinations(state.DetectivesPositions, Edges);
//        return actions;
//    }

//    public GlassesFantomState NextState(GlassesFantomState state, GlassesFantomAction action)
//    {
//        GlassesFantomState newState = new(state);
//        if (state.FantomsTurn)
//        {
//            newState.FantomPosition = action.Moves[0].To;
//        }
//        else
//        {
//            for (int i = 0; i < action.Moves.Count; i++)
//                newState.DetectivesPositions[i] = action.Moves[i].To;

//            newState.Turn++;
//        }
//        newState.FantomsTurn = !state.FantomsTurn;
//        return newState;
//    }

//    public double? Reward(GlassesFantomState state)
//    {
//        if (state.Turn > GameLen)
//            return 1;

//        foreach (var detPos in state.DetectivesPositions)
//        {
//            if (detPos == state.FantomPosition)
//                return -1;
//        }

//        return null;
//    }


//    static List<GlassesFantomAction> GenerateCombinations(List<int> positions, Dictionary<int, HashSet<int>> edges)
//    {
//        List<int> reversed = new(positions);
//        reversed.Reverse();
//        List<GlassesFantomAction> allCombinations = [];
//        GenerateCombinationsHelper(reversed, edges, 0, [], allCombinations);
//        return allCombinations;
//    }

//    static void GenerateCombinationsHelper(List<int> positions, Dictionary<int, HashSet<int>> edges, int index, List<(int From, int To)> currentCombination, List<GlassesFantomAction> allCombinations)
//    {
//        if (index == positions.Count)
//        {
//            allCombinations.Add(new GlassesFantomAction(new List<(int From, int To)>(currentCombination)));
//            return;
//        }

//        int currentPosition = positions[index];

//        foreach (var possibleMove in edges[currentPosition])
//        {
//            if (!currentCombination.Any(pair => pair.To == possibleMove || pair.From == possibleMove))
//            {
//                currentCombination.Add((currentPosition, possibleMove));
//                GenerateCombinationsHelper(positions, edges, index + 1, currentCombination, allCombinations);
//                currentCombination.RemoveAt(currentCombination.Count - 1);
//            }
//        }
//    }
//    //     2           6
//    //   /   \       /   \
//    // 1       4 - 5       8
//    //   \   /       \   /
//    //     3           7
//    public Dictionary<int, HashSet<int>> Edges = new()
//    {
//        { 1, new() { 2, 3 } },
//        { 2, new() { 1, 4 } },
//        { 3, new() { 1, 4 } },
//        { 4, new() { 2, 3, 5 } },
//        { 5, new() { 4, 6, 7 } },
//        { 6, new() { 5, 8 } },
//        { 7, new() { 5, 8 } },
//        { 8, new() { 6, 7 } },
//    };
//}

//public class RockPaperScissors : IGameDescription<string, char>
//{
//    private Dictionary<string, int> Rewards = new()
//    {
//        { "RR", 1 },
//        { "PP", 0 },
//        { "SS", 0 },

//        { "RS", 1 },
//        { "PR", 1 },
//        { "SP", 1 },

//        { "RP", 1 },
//        { "PS", -1 },
//        { "SR", -1 },
//    };

//    public string Root { get; } = "";

//    public List<char> Actions(string stateName)
//    {
//        if (stateName.Length >= 2)
//            return [];
//        return ['R', 'P', 'S'];
//    }

//    public string NextState(string stateName, char action)
//    {
//        return stateName + action;
//    }

//    public double? Reward(string stateName)
//    {
//        bool correctKey = Rewards.TryGetValue(stateName, out var result);
//        if (correctKey)
//            return result;

//        return null;
//    }
//}


