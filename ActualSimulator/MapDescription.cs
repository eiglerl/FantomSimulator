using FantomMapLibrary;
using System;
using System.Linq;
namespace ActualSimulator;

public class MapDescription : IGameDescription<FantomGameState, FantomGameAction>
{
    public FantomGameState Root => FantomGameState.Start(new Dictionary<Transport, int>() { { Transport.Cab, 10 } }, new Dictionary<Transport, int>() { { Transport.Cab, 10 } }, detectivesCount: 2);
    public int MaxLen;
    public Dictionary<int, Dictionary<Transport, HashSet<INode>>> Edges;
    private Map Map;
    private double fantomWinValue;
    private double detectivesWinValue;
    Random rnd;

    public MapDescription(Map map, int gameLen, bool fantomPlayer=true, int seed=42)
    {
        Map = map;
        Edges = CreateEdges(map);
        MaxLen = gameLen;
        if (fantomPlayer)
        {
            fantomWinValue = 1;
            detectivesWinValue = -1;
        }
        else
        {
            fantomWinValue = -1;
            detectivesWinValue = 1;
        }
        rnd = new Random(seed);
    }

    //private bool IsFantomVisible(FantomGameState state) 
    //    => FantomVisibleTurns.Contains(state.Turn);

    private Dictionary<int, Dictionary<Transport, HashSet<INode>>> CreateEdges(Map map)
    {
        var nodes = map.Nodes;
        Dictionary<int, Dictionary<Transport, HashSet<INode>>> edges = [];
        foreach (var node in nodes)
            edges[node.ID] = node.ConnectedNodes;
        return edges;
    }

    public List<FantomGameAction> Actions(FantomGameState state)
    {
        if (state.Turn > MaxLen)
            return [];
        List<FantomGameAction> actions = [];

        if (state.FantomsTurn)
        {
            // First turn
            if (state.FantomPosition is null || state.FantomPosition == 0)
            {
                foreach (var node in Map.Nodes)
                {
                    if (!state.DetectivesPositions.Contains(node.ID))
                        actions.Add(new() { Moves = [new Move(node.ID)] });
                    //actions.Add(new() { Moves = [(null, node)], UsedTransports = [null] });

                }
            }

            // Other turns
            else
            {
                var possibleEdges = Edges[state.FantomPosition.Value];
                //var possibleEdges = Edges[state.FantomPosition];
                HashSet<int> taken = state.DetectivesPositions.Select(n => n.Value).ToHashSet();

                foreach (var tr in possibleEdges.Keys)
                {
                    foreach (var node in possibleEdges[tr])
                    {
                        if (!taken.Contains(node.ID))
                            actions.Add(new() { Moves = [new Move(node.ID, tr)] });
                        //actions.Add(new() { Moves = [(state.FantomPosition, node)], UsedTransports = [Tr] });
                    }
                }
                if (actions.Count == 0)
                {
                    var tr = possibleEdges.Keys.First();
                    var node = possibleEdges[tr].First();
                    actions.Add(new() { Moves = [new Move(node.ID, tr)] });
                }
            }
        }
        else
        {
            // First turn
            if (state.DetectivesPositions.Any(x => x is null))
            {
                actions = GenerateCombinationsOfNodes(Map.Nodes, state.DetectivesPositions.Count);
            }

            // Other turns
            else
            {
                actions = GenerateCombinations(state.DetectivesPositions, Edges);
            }
        }
        if (actions.Any(x => x.Moves.Count == 0))
        {
            Console.WriteLine("wtf");
        }
        return actions;
    }

    static List<FantomGameAction> GenerateCombinationsOfNodes(List<Node> nodes, int length)
    {
        List<FantomGameAction> allCombinations = [];
        GenerateCombinationsOfNodesHelper(nodes, length, 0, new() { Moves = [] }, allCombinations);
        return allCombinations;
    }

    static void GenerateCombinationsOfNodesHelper(List<Node> nodes, int length, int index, FantomGameAction currentCombination, List<FantomGameAction> allCombinations)
    {
        if (currentCombination.Moves.Count == length)
        {
            allCombinations.Add(new() { Moves = new(currentCombination.Moves) });
            //allCombinations.Add(new() { Moves = new(currentCombination.Moves), UsedTransports = new(currentCombination.UsedTransports) });
            return;
        }

        for (int i = index; i < nodes.Count; i++)
        {
            currentCombination.Moves.Add(new(nodes[i].ID));
            //currentCombination.Moves.Add((null, nodes[i]));
            //currentCombination.UsedTransports.Add(null);
            GenerateCombinationsOfNodesHelper(nodes, length, i + 1, currentCombination, allCombinations);
            currentCombination.Moves.RemoveAt(currentCombination.Moves.Count - 1);
            //currentCombination.UsedTransports.RemoveAt(currentCombination.UsedTransports.Count - 1);
        }
    }

    static List<FantomGameAction> GenerateCombinations(List<int?> positions, Dictionary<int, Dictionary<Transport, HashSet<INode>>> edges)
    {
        List<int> reversed = new();
        foreach (var p in positions)
            reversed.Add(p.Value);
        reversed.Reverse();
        List<FantomGameAction> allCombinations = [];
        //GenerateCombinationsHelper(reversed, edges, 0, new() { Moves = [], UsedTransports = [] }, allCombinations);
        GenerateCombinationsHelper(reversed, edges, 0, new() { Moves = [] }, allCombinations);
        return allCombinations;
    }

    static void GenerateCombinationsHelper(List<int> positions, Dictionary<int, Dictionary<Transport, HashSet<INode>>> edges, int index, FantomGameAction currentCombination, List<FantomGameAction> allCombinations)
    {
        if (index == positions.Count)
        {
            allCombinations.Add(new FantomGameAction() { Moves = new(currentCombination.Moves) });
            //allCombinations.Add(new FantomGameAction() { Moves = currentCombination.Moves, UsedTransports = currentCombination.UsedTransports });
            return;
        }

        int currentPosition = positions[index];

        foreach (var tr in edges[currentPosition].Keys)
        {
            foreach (var possibleMove in edges[currentPosition][tr])
            {
                if (!currentCombination.Moves.Any(pair => pair.NewPosition == possibleMove.ID) && IsNewPositionFree(positions, index, possibleMove.ID))
                //if (!currentCombination.Moves.Any(pair => pair.To.ID == possibleMove.ID || pair.From == possibleMove))
                {
                    // update current
                    currentCombination.Moves.Add(new Move(possibleMove.ID, tr));
                    //currentCombination.Moves.Add((currentPosition, possibleMove));
                    //currentCombination.UsedTransports.Add(tr);

                    GenerateCombinationsHelper(positions, edges, index + 1, currentCombination, allCombinations);

                    // remove
                    currentCombination.Moves.RemoveAt(currentCombination.Moves.Count - 1);
                    //currentCombination.UsedTransports.RemoveAt(currentCombination.UsedTransports.Count - 1);
                }
            }
        }

    }

    private static bool IsNewPositionFree(List<int> positions, int index, int newPosition)
    {
        for (int i = 0; i < Math.Min(positions.Count, index); i++)
        {
            if (positions[i] == newPosition)
                return false;
        }
        return true;
    }


    public FantomGameState NextState(FantomGameState state, FantomGameAction action)
    {
        var copyState = state.Deepcopy();
        if (state.FantomsTurn)
        {
            // Fantom is visible
            if (action.Moves[0].ContainsPosition())
            {
                copyState.FantomPosition = action.Moves[0].NewPosition;
                copyState.FantomPossiblePositions = [action.Moves[0].NewPosition];
            }

            // First turn (Fantom can be anywhere)
            else if (!action.Moves[0].ContainsTransport())
            {
                copyState.FantomPosition = null;
                copyState.FantomPossiblePositions = [];
                foreach (var node in Map.Nodes)
                {
                    if (!state.DetectivesPositions.Contains(node.ID))
                        copyState.FantomPossiblePositions.Add(node.ID);
                }
            }

            // Other turns (we know used transport and previous possible positions => set of new possible positions)
            else
            {
                HashSet<int> temp = [];
                HashSet<int> taken = state.DetectivesPositions.Select(n => n.Value).ToHashSet();

                foreach (var pos in copyState.FantomPossiblePositions)
                {
                    //HashSet<INode>? possibleNodes;
                    if (!Edges[pos].TryGetValue(action.Moves[0].Tr, out HashSet<INode>? possibleNodes))
                        continue;
                    //var possibleNodes = Edges[pos][action.Moves[0].Tr];
                    foreach (var node in possibleNodes)
                    {
                        if (!taken.Contains(node.ID))
                            temp.Add(node.ID);
                    }
                }
                copyState.FantomPosition = null;
                copyState.FantomPossiblePositions = temp;
            }

            // Update transports
            if (action.Moves[0].Tr != Transport.Nothing)
                copyState.FantomTokens[action.Moves[0].Tr]--;
            //copyState.FantomPosition = action.Moves[0].To;
            //if (action.UsedTransports[0] is not null)
            //    copyState.FantomTokens[action.UsedTransports[0].Value]--;
        }
        else
        {
            for (var i = 0; i < action.Moves.Count; i++)
            {
                copyState.DetectivesPositions[i] = action.Moves[i].NewPosition;
                if (action.Moves[i].Tr != Transport.Nothing)
                    copyState.DetectivesTokens[i][action.Moves[i].Tr]--;
                //copyState.DetectivesPositions[i] = action.Moves[i].To;
                //if (action.UsedTransports[i] is not null)
                //    copyState.DetectivesTokens[i][action.UsedTransports[i].Value]--;
            }
            copyState.Turn++;
        }
        copyState.FantomsTurn = !copyState.FantomsTurn;
        return copyState;
    }

    public Dictionary<FantomGameAction, FantomGameState>? GetAllDeterminizationsIfInfoSet(FantomGameState state)
    {
        if (state.Turn == 0)
        {
            foreach (var node in Map.Nodes)
            {
                if (!state.DetectivesPositions.Contains(node.ID))
                    state.FantomPossiblePositions.Add(node.ID);
            }
                
        }
        if (state.FantomPossiblePositions.Count <= 1) 
            return null;
        
        var possibleStates = state.FantomPossiblePositions.ToArray();
        var dict = new Dictionary<FantomGameAction, FantomGameState>();

        foreach (var ps in possibleStates)
        {
            var determinizedSTate = state.Deepcopy();
            determinizedSTate.FantomPosition = ps;
            determinizedSTate.FantomPossiblePositions = [ps];
            dict.Add(new() { Moves = [new Move() { NewPosition = ps }] }, determinizedSTate);
        }
        return dict;
    }

    //public FantomGameState DeterminizeUniform(FantomGameState state)
    //{
    //    var determinizedState = state.Deepcopy();
    //    int possibleStatesCount = determinizedState.FantomPossiblePositions.Count;
    //    if (possibleStatesCount == 1)
    //        return determinizedState;
    //    var possibleStates = determinizedState.FantomPossiblePositions.ToArray();
    //    var chosenState = possibleStates[rnd.Next(possibleStatesCount)];
    //    determinizedState.FantomPosition = chosenState;
    //    determinizedState.FantomPossiblePositions = [];
    //    return determinizedState;
    //}

    public double? Reward(FantomGameState state)
    {
        if (state.Turn > MaxLen)
            return fantomWinValue;

        foreach (var detPos in state.DetectivesPositions)
        {
            if (detPos == state.FantomPosition)
                return detectivesWinValue;
        }

        return null;
    }
}
