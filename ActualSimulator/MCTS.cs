using FantomSimulatorLibrary;
namespace ActualSimulator;


public interface IGameDescription<TStateName, TAction>
{
    public TStateName NextState(TStateName stateName, TAction action);
    public List<TAction> Actions(TStateName stateName);
    public double? Reward(TStateName stateName);
    public TStateName Root { get; }
}


public class MonteCarloTreeSearch<TStateName, TAction>
    where TAction : notnull
{
    public double ExplorationParameter = Math.Sqrt(2);

    public class Node
    {
        public int Visits { get; set; }
        public int Wins { get; set; }
        public double Value { get; set; }
        public Dictionary<TAction, Node> Children { get; set; }
        public Node? Parent { get; set; }
        public TStateName StateName { get; init; }

        public IGameDescription<TStateName, TAction> GameDescription { get; set; }

        public Node(IGameDescription<TStateName, TAction> gs, TStateName stateName, Node? parent=null)
        {
            Visits = 0; Value = 0;
            Children = [];
            Parent = parent;
            GameDescription = gs;
            StateName = stateName;
        }

        public Node(Node oldNode)
        {
            Visits = oldNode.Visits;
            Value = oldNode.Value;
            Children = new(oldNode.Children);
            Parent = oldNode.Parent;
            GameDescription = oldNode.GameDescription;
            StateName = oldNode.StateName;
        }

        public bool IsTerminal()
            => GameDescription.Reward(StateName) is not null;
            //=> GameDescription.Actions(StateName).Count == 0;
        public bool IsRoot()
            => Parent is null;

        public void UpdateStats(double result)
        {
            Visits++;
            if (result > 0)
                Wins++;
        }

    }

    public class Tree
    {
        public Node Root { get; set; }
        private double ExplorationParameter = Math.Sqrt(2);
        public Random rnd = new();


        public Tree(IGameDescription<TStateName, TAction> gameInfo, TStateName stateName)
        {
            Root = new(gameInfo, stateName);
        }

        public Node Traverse(Node node)
        {
            while (IsFullyExpanded(node))
            {
                if (node.IsTerminal())
                    return node;
                node = GetBestChild(node);
                
            }

            return PickUnvisited(node);
        }

        public bool IsFullyExpanded(Node node)
        {
            if (node.Children.Count > node.GameDescription.Actions(node.StateName).Count)
            {
                Console.WriteLine("wtf");
            }
            if (node.Children.Count == node.GameDescription.Actions(node.StateName).Count)
                return true;
            return false;
        }

        public Node? GetBestChild(Node node)
        {
            double max = 0;
            Node? bestNode = null;
            foreach (var child in node.Children.Values)
            {
                double value = UpperConfidenceBound1(child, node);
                if (bestNode is null ||  value > max)
                {
                    max = value;
                    bestNode = child;
                }
            }
            return bestNode;
        }

        private double UpperConfidenceBound1(Node currentNode, Node parent)
        {
            // Unexplored node
            if (currentNode.Visits == 0)
                return double.MaxValue;
 
            return currentNode.Wins / currentNode.Visits + ExplorationParameter * Math.Sqrt(Math.Log(parent.Visits) / currentNode.Visits);
        }

        public Node PickUnvisited(Node node)
        {
            var keys = node.Children.Keys;
            var actions = node.GameDescription.Actions(node.StateName);
            var possibleActions = actions.Where(a => !keys.Contains(a)).ToList();
            TAction chosenAction = possibleActions[rnd.Next(0, possibleActions.Count)];
            Node newNode = new(node.GameDescription, node.GameDescription.NextState(node.StateName, chosenAction), node);
            node.Children[chosenAction] = newNode;
            return newNode;
        }

        public double? Rollout(Node node)
        {
            Node variable = new(node);
            while (!variable.IsTerminal())
            {
                var action = variable.GameDescription.Actions(variable.StateName)[rnd.Next(0, variable.GameDescription.Actions(variable.StateName).Count - 1)];
                variable = new(variable.GameDescription, variable.GameDescription.NextState(variable.StateName, action));
            }
            return variable.GameDescription.Reward(variable.StateName);
        }

        public void BackPropagate(Node? node, double result)
        {
            if (node is null)
                return;
            node.UpdateStats(result);
            BackPropagate(node.Parent, result);
        }

    }

    public TAction Simulate(Tree tree, double time)
    {
        DateTime end = DateTime.Now.AddSeconds(time);

        while (DateTime.Now < end)
        {
            var leaf = tree.Traverse(tree.Root);
            var simulationResult = tree.Rollout(leaf);
            if (simulationResult is not null)
                tree.BackPropagate(leaf, simulationResult.Value);
        }

        var result = tree.Root.Children.MaxBy(item => item.Value.Wins).Key;
        return result;
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
            foreach (var move in Moves)
            {
                hash = hash * 23 + move.From.GetHashCode();
                hash = hash * 23 + move.To.GetHashCode();
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

public class GlassesFantomGame : IGameDescription<GlassesFantomState, GlassesFantomAction>
{
    public int GameLen = 8;
    public GlassesFantomState Root 
    { get
        {
            return new() { FantomPosition = 4, DetectivesPositions = [1, 2], FantomsTurn = true, Turn = 0 };
        }
    }
    public List<GlassesFantomAction> Actions(GlassesFantomState state)
    {
        if (state.Turn > GameLen)
            return [];
        List<GlassesFantomAction> actions = [];
        if (state.FantomsTurn)
        {
            var possibleMoves = Edges[state.FantomPosition];
            HashSet<int> taken = state.DetectivesPositions.ToHashSet();
            foreach (var move in possibleMoves)
            {
                if (!taken.Contains(move))
                    actions.Add(new([(state.FantomPosition, move)]));
            }
        }
        else
            actions = GenerateCombinations(state.DetectivesPositions, Edges);
        return actions;
    }

    public GlassesFantomState NextState(GlassesFantomState state, GlassesFantomAction action)
    {
        GlassesFantomState newState = new(state);
        if (state.FantomsTurn)
        {
            newState.FantomPosition = action.Moves[0].To;   
        }
        else
        {
            for (int i = 0; i < action.Moves.Count; i++)
                newState.DetectivesPositions[i] = action.Moves[i].To;

            newState.Turn++;
        }
        newState.FantomsTurn = !state.FantomsTurn;
        return newState;
    }

    public double? Reward(GlassesFantomState state)
    {
        if (state.Turn > GameLen)
            return 1;

        foreach (var detPos in state.DetectivesPositions)
        {
            if (detPos == state.FantomPosition)
                return -1;
        }

        return null;
    }


    static List<GlassesFantomAction> GenerateCombinations(List<int> positions, Dictionary<int, HashSet<int>> edges)
    {
        List<int> reversed = new List<int>(positions);
        reversed.Reverse();
        List<GlassesFantomAction> allCombinations = new List<GlassesFantomAction>();
        GenerateCombinationsHelper(reversed, edges, 0, new List<(int From, int To)>(), allCombinations);
        return allCombinations;
    }

    static void GenerateCombinationsHelper(List<int> positions, Dictionary<int, HashSet<int>> edges, int index, List<(int From, int To)> currentCombination, List<GlassesFantomAction> allCombinations)
    {
        if (index == positions.Count)
        {
            allCombinations.Add(new GlassesFantomAction(new List<(int From, int To)>(currentCombination)));
            return;
        }

        int currentPosition = positions[index];

        foreach (var possibleMove in edges[currentPosition])
        {
            if (!currentCombination.Any(pair => pair.To == possibleMove || pair.From == possibleMove))
            {
                currentCombination.Add((currentPosition, possibleMove));
                GenerateCombinationsHelper(positions, edges, index + 1, currentCombination, allCombinations);
                currentCombination.RemoveAt(currentCombination.Count - 1);
            }
        }
    }
    //     2           6
    //   /   \       /   \
    // 1       4 - 5       8
    //   \   /       \   /
    //     3           7
    public Dictionary<int, HashSet<int>> Edges = new()
    {
        { 1, new() { 2, 3 } },
        { 2, new() { 1, 4 } },
        { 3, new() { 1, 4 } },
        { 4, new() { 2, 3, 5 } },
        { 5, new() { 4, 6, 7 } },
        { 6, new() { 5, 8 } },
        { 7, new() { 5, 8 } },
        { 8, new() { 6, 7 } },
    };
}

public class RockPaperScissors : IGameDescription<string, char>
{
    private Dictionary<string, int> Rewards = new()
    {
        { "RR", 1 },
        { "PP", 0 },
        { "SS", 0 },

        { "RS", 1 },
        { "PR", 1 },
        { "SP", 1 },

        { "RP", 1 },
        { "PS", -1 },
        { "SR", -1 },
    };

    public string Root { get; } = "";

    public List<char> Actions(string stateName)
    {
        if (stateName.Length >= 2)
            return [];
        return ['R', 'P', 'S'];
    }

    public string NextState(string stateName, char action)
    {
        return stateName + action;
    }

    public double? Reward(string stateName)
    {
        bool correctKey = Rewards.TryGetValue(stateName, out var result);
        if (correctKey)
            return result;

        return null;
    }
}


