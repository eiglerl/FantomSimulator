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

public class MonteCarloTreeSearch<TStateType, TAction>
    where TAction : notnull
{
    public class Node
    {
        public int Visits { get; set; }
        public int Wins { get; set; }

        public Dictionary<TAction, Node> Children { get; set; }
        public Node? Parent { get; set; }
        public TStateType State { get; set; }
        public bool DeterminizationNode { get; set; } = false;

        // Player1 recieves the given reward from GameDescription, Player2 recieves -1*reward
        public bool Player1Plays;

        public IGameDescription<TStateType, TAction> GameDescription { get; set; }

        public Node(IGameDescription<TStateType, TAction> gs, TStateType stateName, bool player1Plays, Node? parent = null)
        {
            Visits = 0;
            Children = [];
            Parent = parent;
            GameDescription = gs;
            State = stateName;
            Player1Plays = player1Plays;
        }

        public Node(Node oldNode)
        {
            Visits = oldNode.Visits;
            Children = new(oldNode.Children);
            Parent = oldNode.Parent;
            GameDescription = oldNode.GameDescription;
            State = oldNode.State;
        }

        public bool IsTerminal()
            => GameDescription.Reward(State) is not null;

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


        public Tree(IGameDescription<TStateType, TAction> gameInfo, TStateType stateName, Random? rnd = null)
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
            if (node.DeterminizationNode || (node.Children.Count == node.GameDescription.NumberOfActions(node.State) && !node.IsTerminal()))//node.GameDescription.NumberOfActions(node.StateName))
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
            var randomAction = node.GameDescription.RandomAction(node.State, node.Children.Keys.ToHashSet());
            Node newNode = new(node.GameDescription, node.GameDescription.NextState(node.State, randomAction), !node.Player1Plays, node);
            node.Children[randomAction] = newNode;
            return newNode;
        }

        public double? Rollout(Node node)
        {
            Node tempNode = new(node);
            while (!tempNode.IsTerminal())
            {
                var action = tempNode.GameDescription.RandomAction(tempNode.State);
                var newState = tempNode.GameDescription.NextState(tempNode.State, action);
                tempNode = new(tempNode.GameDescription, newState, !tempNode.Player1Plays);
            }
            return tempNode.GameDescription.Reward(tempNode.State);
        }

        public void BackPropagate(Node? node, double result)
        {
            if (node is null)
                return;
            node.UpdateStats(result);
            BackPropagate(node.Parent, result);
        }

    }

    public Node CreateNodeFromDeterminizations(Node parent, Dictionary<TAction, TStateType> determinizations)
    {
        if (determinizations.Count == 1)
            return parent;

        var newNode = new Node(parent) { DeterminizationNode = true };
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
        var det = root.GameDescription.GetAllDeterminizationsInInfoSet(root.State);
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