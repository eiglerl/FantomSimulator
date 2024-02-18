using FantomMapLibrary;
using System;
using System.ComponentModel.Design;
using System.Linq;
namespace ActualSimulator;

public class MapDescription : IGameDescription<FantomGameState, FantomGameAction>
{
    public FantomGameState Root => FantomGameState.Start(new Dictionary<Transport, int>() { { Transport.Cab, 10 } }, new Dictionary<Transport, int>() { { Transport.Cab, 10 } }, detectivesCount: 2);
    public int MaxLen;
    public Dictionary<int, Dictionary<Transport, HashSet<INode>>> Edges;
    private readonly Map Map;
    private readonly double fantomWinValue;
    private readonly double detectivesWinValue;
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

    private Dictionary<int, Dictionary<Transport, HashSet<INode>>> CreateEdges(Map map)
    {
        var nodes = map.Nodes;
        Dictionary<int, Dictionary<Transport, HashSet<INode>>> edges = [];
        foreach (var node in nodes)
            edges[node.ID] = node.ConnectedNodes;
        return edges;
    }
    private long NumberOfActionsFantom(FantomGameState state)
    {
        // First turn
        if (state.Turn == 1)
            return Map.Nodes.Count;

        // Other turns
        var possibleEdges = Edges[state.GetFantomExactPositionIfKnown()];
        HashSet<int> taken = state.DetectivesPositions.Select(n => n.Value).ToHashSet();
        long count = 0;

        foreach (var tr in possibleEdges.Keys)
        {
            foreach (var node in possibleEdges[tr])
            {
                if (!taken.Contains(node.ID))
                    count++;
            }
        }
        // At least 1 even if blocked - has to lose
        return Math.Max(count, 1);
    }
    private long NumberOfActionsDetectives(FantomGameState state)
    {
        // First turn
        if (state.Turn == 0)
        {
            long count = 1;
            for (int i = 0; i < state.DetectivesPositions.Count; i++)
                count *= (Map.Nodes.Count - i);
            return count;
        }

        // Other turns
        return GenerateCombinations(state.DetectivesPositions, Edges).Count;
    }

    public long NumberOfActions(FantomGameState state)
    {
        if (state.Turn > MaxLen)
            return 0;

        if (state.FantomsTurn)
        {
            return NumberOfActionsFantom(state);
            //// First turn>	ActualSimulator.dll!ActualSimulator.MapDescription.NumberOfActions(ActualSimulator.FantomGameState state) Line 87	C#

            //if (state.FantomPossiblePositions is null || state.FantomPossiblePositions.Count == 0)
            //    return Map.Nodes.Count;
            //// Other turns
            //else
            //{
            //    var possibleEdges = Edges[state.GetFantomExactPositionIfKnown()];
            //    //var possibleEdges = Edges[state.FantomPosition];
            //    HashSet<int> taken = state.DetectivesPositions.Select(n => n.Value).ToHashSet();
            //    long count = 0;

            //    foreach (var tr in possibleEdges.Keys)
            //    {
            //        foreach (var node in possibleEdges[tr])
            //        {
            //            if (!taken.Contains(node.ID))
            //                count++;
            //            //actions.Add(new() { Moves = [(state.FantomPosition, node)], UsedTransports = [Tr] });
            //        }
            //    }
            //    return Math.Max(count,1);
            //}
        }
        else
        {
            return NumberOfActionsDetectives(state);
            //// First turn
            //if (state.DetectivesPositions.Any(x => x is null))
            //{
            //    long count = 1;
            //    for (int i = 0; i < state.DetectivesPositions.Count; i++)
            //        count *= (Map.Nodes.Count - i);
            //    return count;
            //}

            //// Other turns
            //else
            //{
            //    return GenerateCombinations(state.DetectivesPositions, Edges).Count;
            //}
        }

    }

    private FantomGameAction RandomActionFantom(FantomGameState state, HashSet<FantomGameAction> usedActions)
    {
        if (state.FantomPossiblePositions is null || state.FantomPossiblePositions.Count == 0)
            return RandomActionFantomFirstTurn(state, usedActions);
        else
            return RandomActionFantomOtherTurns(state, usedActions);
    }
    private FantomGameAction RandomActionFantomFirstTurn(FantomGameState state, HashSet<FantomGameAction> usedActions)
    {
        var usedNodes = usedActions.Select(a => a.Moves.First().NewPosition).ToHashSet();
        var possibleNodes = Map.Nodes.Where(n => !usedNodes.Contains(n.ID)).ToList();
        Node newNode = possibleNodes[rnd.Next(possibleNodes.Count)];
        return new() { Moves = [new(newNode.ID)] };
    }
    private FantomGameAction RandomActionFantomOtherTurns(FantomGameState state, HashSet<FantomGameAction> usedActions)
    {
        // Assuming this is not the first turn => detectives have their positions
        HashSet<int> taken = state.DetectivesPositions.Select(n => n.Value).ToHashSet();

        // Get connected nodes and transports in the node
        var possibleMoves = Edges[state.GetFantomExactPositionIfKnown()];
        var possibleTransports = state.FantomTokens.Where(x => x.Value > 0 && possibleMoves.ContainsKey(x.Key)).Select(x => x.Key);

        // Go through every possible Transport
        Dictionary<Transport, List<INode>> possibleMovesThatAreEmpty = [];
        foreach (var tr in possibleTransports)
        {
            // Get empty nodes connected by the transport
            var list = possibleMoves[tr].Where(x => !taken.Contains(x.ID)).ToList();
            if (list.Count > 0)
                // If non-empty save the list
                possibleMovesThatAreEmpty[tr] = list;
        }

        // If there are not any possible moves
        if (possibleMovesThatAreEmpty.Count == 0)
        {
            // Stay in place
            return new() { Moves = [new Move(state.GetFantomExactPositionIfKnown())] };

            var mustChooseTr = possibleTransports.First();
            var id = Edges[state.GetFantomExactPositionIfKnown()][mustChooseTr].First().ID;
            return new() { Moves = [new(id, mustChooseTr)] };
        }

        // Otherwise choose transport and node randomly
        var chosenTr = possibleMovesThatAreEmpty.Keys.ToList()[rnd.Next(possibleMovesThatAreEmpty.Count)];
        var chosenID = possibleMovesThatAreEmpty[chosenTr][rnd.Next(possibleMovesThatAreEmpty[chosenTr].Count)].ID;
        return new() { Moves = [new Move(chosenID, chosenTr)] };
    }

    private FantomGameAction RandomActionDetectives(FantomGameState state, HashSet<FantomGameAction> usedActions)
    {
        if (state.DetectivesPositions.Any(x => x is null))
            return RandomActionDetectivesFirstTurn(state, usedActions);
        else
            return RandomActionDetectivesOtherTurns(state, usedActions);
    }
    private FantomGameAction RandomActionDetectivesFirstTurn(FantomGameState state, HashSet<FantomGameAction> usedActions)
    {
        FantomGameAction action = new() { Moves = [] };
        HashSet<int> occupiedPositions = [];
        for (int i = 0; i < state.DetectivesPositions.Count; i++)
        {
            var possiblePositions = Map.Nodes.Where(x => !occupiedPositions.Contains(x.ID)).ToList();
            var newPos = possiblePositions[rnd.Next(possiblePositions.Count)];
            occupiedPositions.Add(newPos.ID);
            action.Moves.Add(new(newPos.ID));
        }
        return action;
    }
    private FantomGameAction RandomActionDetectivesOtherTurns(FantomGameState state, HashSet<FantomGameAction> usedActions)
    {
        FantomGameAction action = new() { Moves = [] };
        HashSet<int> occupiedPositions = state.DetectivesPositions.Select(x => x.Value).ToHashSet();
        for (int i = 0; i < state.DetectivesPositions.Count; i++)
        {
            occupiedPositions.Remove(state.DetectivesPositions[i].Value);
            var possiblePositions = Edges[state.DetectivesPositions[i].Value];
            var possibleTr = state.DetectivesTokens[i].Where(x => x.Value > 0 && possiblePositions.ContainsKey(x.Key)).Select(x => x.Key);

            Dictionary<Transport, List<INode>> emptyMoves = [];
            foreach (var tr in possibleTr)
            {
                var list = possiblePositions[tr].Where(x => !occupiedPositions.Contains(x.ID)).ToList();
                if (list.Count > 0)
                    emptyMoves[tr] = list;
            }

            if (emptyMoves.Count == 0)
            {
                action.Moves.Add(new(state.DetectivesPositions[i].Value));
                continue;
            }

            var chosenTr = emptyMoves.Keys.ToList()[rnd.Next(emptyMoves.Count)];
            var newPos = emptyMoves[chosenTr][rnd.Next(emptyMoves[chosenTr].Count)];
            occupiedPositions.Add(newPos.ID);
            action.Moves.Add(new(newPos.ID, chosenTr));
        }
        return action;

    }

    public FantomGameAction RandomAction(FantomGameState state, HashSet<FantomGameAction> usedActions)
    {
        if (state.FantomsTurn)
        {
            return RandomActionFantom(state, usedActions);
            //if (state.FantomPossiblePositions is null || state.FantomPossiblePositions.Count == 0)
            //{
            //    return RandomActionFantomFirstTurn(state, usedActions);
            //    //var usedNodes = usedActions.Select(a => a.Moves.First().NewPosition).ToHashSet();
            //    //var possibleNodes = Map.Nodes.Where(n => !usedNodes.Contains(n.ID)).ToList();
            //    //Node newNode = possibleNodes[rnd.Next(possibleNodes.Count)];
            //    //return new() { Moves = [new(newNode.ID)] };
            //}
            //else
            //{
            //    return RandomActionFantomOtherTurns(state, usedActions);
            //    //HashSet<int> taken = state.DetectivesPositions.Select(n => n.Value).ToHashSet();

            //    //var possibleMoves = Edges[state.GetFantomExactPositionIfKnown()];
            //    //var possibleTr = state.FantomTokens.Where(x => x.Value > 0 && possibleMoves.ContainsKey(x.Key)).Select(x => x.Key);

            //    //Dictionary<Transport, List<INode>> emptyMoves = []; 
            //    //foreach (var tr in possibleTr)
            //    //{
            //    //    var list = possibleMoves[tr].Where(x => !taken.Contains(x.ID)).ToList();
            //    //    if (list.Count > 0)
            //    //        emptyMoves[tr] = list;
            //    //}
            //    //if (emptyMoves.Count == 0)
            //    //{
            //    //    var mustChooseTr = possibleTr.First();
            //    //    var id = Edges[state.GetFantomExactPositionIfKnown()][mustChooseTr].First().ID;
            //    //    return new() { Moves = [new(id, mustChooseTr)] };

            //    //}
            //    //var chosenTr = emptyMoves.Keys.ToList()[rnd.Next(emptyMoves.Count)];
            //    //var chosenID = emptyMoves[chosenTr][rnd.Next(emptyMoves[chosenTr].Count)].ID;
            //    //return new() { Moves = [new Move(chosenID, chosenTr)] };
            //}
        }
        else
        {
            return RandomActionDetectives(state, usedActions);
            //if (state.DetectivesPositions.Any(x => x is null))
            //{
                //FantomGameAction action = new() { Moves = [] };
                //HashSet<int> occupiedPositions = [];
                //for (int i = 0; i < state.DetectivesPositions.Count; i++)
                //{
                //    var possiblePositions = Map.Nodes.Where(x => !occupiedPositions.Contains(x.ID)).ToList();
                //    var newPos = possiblePositions[rnd.Next(possiblePositions.Count)];
                //    occupiedPositions.Add(newPos.ID);
                //    action.Moves.Add(new(newPos.ID));
                //}
                //return action;
            //}
            //else
            //{
            //    return RandomActionDetectivesOtherTurns(state, usedActions);
                //FantomGameAction action = new() { Moves = [] };
                //HashSet<int> occupiedPositions = state.DetectivesPositions.Select(x => x.Value).ToHashSet();
                //for (int i = 0; i < state.DetectivesPositions.Count; i++)
                //{
                //    occupiedPositions.Remove(state.DetectivesPositions[i].Value);
                //    var possiblePositions = Edges[state.DetectivesPositions[i].Value];
                //    var possibleTr = state.DetectivesTokens[i].Where(x => x.Value > 0 && possiblePositions.ContainsKey(x.Key)).Select(x => x.Key);

                //    Dictionary<Transport, List<INode>> emptyMoves = [];
                //    foreach (var tr in possibleTr)
                //    {
                //        var list = possiblePositions[tr].Where(x => !occupiedPositions.Contains(x.ID)).ToList();
                //        if (list.Count > 0)
                //            emptyMoves[tr] = list;
                //    }

                //    if (emptyMoves.Count == 0)
                //    {
                //        action.Moves.Add(new(state.DetectivesPositions[i].Value));
                //        continue;
                //    }

                //    var chosenTr = emptyMoves.Keys.ToList()[rnd.Next(emptyMoves.Count)];
                //    var newPos = emptyMoves[chosenTr][rnd.Next(emptyMoves[chosenTr].Count)];
                //    occupiedPositions.Add(newPos.ID);
                //    action.Moves.Add(new(newPos.ID, chosenTr));
                //}
                //return action;

            //}
        }
    }
    public FantomGameAction RandomAction(FantomGameState state)
        => RandomAction(state, []);

    private List<FantomGameAction> ActionsFantom(FantomGameState state)
    {
        if (state.FantomPossiblePositions is null || state.FantomPossiblePositions.Count == 0)
            return ActionsFantomFirstTurn(state);
        else
            return ActionsFantomOtherTurns(state);
    }
    private List<FantomGameAction> ActionsFantomFirstTurn(FantomGameState state)
    {
        List<FantomGameAction> actions = [];
        foreach (var node in Map.Nodes)
        {
            if (!state.DetectivesPositions.Contains(node.ID))
                actions.Add(new() { Moves = [new Move(node.ID)] });
        }
        return actions;
    }
    private List<FantomGameAction> ActionsFantomOtherTurns(FantomGameState state)
    {
        List<FantomGameAction> actions = [];
        var possibleEdges = Edges[state.GetFantomExactPositionIfKnown()];
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
        return actions;
    }
    private List<FantomGameAction> ActionsDetectives(FantomGameState state)
    {
        List<FantomGameAction> actions = [];
        // First turn
        if (state.DetectivesPositions.Any(x => x is null))
            actions = GenerateCombinationsOfNodes(Map.Nodes, state.DetectivesPositions.Count);

        // Other turns
        else
            actions = GenerateCombinations(state.DetectivesPositions, Edges);
        return actions;
    }

    public List<FantomGameAction> Actions(FantomGameState state)
    {
        if (state.Turn > MaxLen)
            return [];

        if (state.FantomsTurn)
        {
            return ActionsFantom(state);
            //// First turn
            ////if (state.FantomPosition is null || state.FantomPosition == 0)
            //if (state.FantomPossiblePositions is null || state.FantomPossiblePositions.Count == 0)
            //{
            //    foreach (var node in Map.Nodes)
            //    {
            //        if (!state.DetectivesPositions.Contains(node.ID))
            //            actions.Add(new() { Moves = [new Move(node.ID)] });
            //        //actions.Add(new() { Moves = [(null, node)], UsedTransports = [null] });

            //    }
            //}

            //// Other turns
            //else
            //{
            //    var possibleEdges = Edges[state.GetFantomExactPositionIfKnown()];
            //    //var possibleEdges = Edges[state.FantomPosition];
            //    HashSet<int> taken = state.DetectivesPositions.Select(n => n.Value).ToHashSet();

            //    foreach (var tr in possibleEdges.Keys)
            //    {
            //        foreach (var node in possibleEdges[tr])
            //        {
            //            if (!taken.Contains(node.ID))
            //                actions.Add(new() { Moves = [new Move(node.ID, tr)] });
            //            //actions.Add(new() { Moves = [(state.FantomPosition, node)], UsedTransports = [Tr] });
            //        }
            //    }
            //    if (actions.Count == 0)
            //    {
            //        var tr = possibleEdges.Keys.First();
            //        var node = possibleEdges[tr].First();
            //        actions.Add(new() { Moves = [new Move(node.ID, tr)] });
            //    }
            //}
        }
        else
        {
            return ActionsDetectives(state);
            //// First turn
            //if (state.DetectivesPositions.Any(x => x is null))
            //{
            //    actions = GenerateCombinationsOfNodes(Map.Nodes, state.DetectivesPositions.Count);
            //}

            //// Other turns
            //else
            //{
            //    actions = GenerateCombinations(state.DetectivesPositions, Edges);
            //}
        }
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
        bool canContinue = false;

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

                    canContinue = true;
                }
            }
        }
        // if the player cannot move at all (is blocked)
        if (!canContinue)
        {
            // update current
            currentCombination.Moves.Add(new Move(positions[index]));

            GenerateCombinationsHelper(positions, edges, index + 1, currentCombination, allCombinations);

            // remove
            currentCombination.Moves.RemoveAt(currentCombination.Moves.Count - 1);

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

    private FantomGameState NextTurnAfterFantomPlayVisible(FantomGameState state, FantomGameAction action)
    {
        state.FantomPossiblePositions = [action.Moves[0].NewPosition];
        return state;
    }
    private FantomGameState NextTurnAfterFantomPlayFirstTurn(FantomGameState state, FantomGameAction action)
    {
        state.FantomPossiblePositions = [];
        foreach (var node in Map.Nodes)
        {
            if (!state.DetectivesPositions.Contains(node.ID))
                state.FantomPossiblePositions.Add(node.ID);
        }
        return state;
    }
    private FantomGameState NextTurnAfterFantomPlayOtherTurns(FantomGameState state, FantomGameAction action)
    {
        HashSet<int> temp = [];
        HashSet<int> taken = state.DetectivesPositions.Select(n => n.Value).ToHashSet();

        foreach (var pos in state.FantomPossiblePositions)
        {
            if (!Edges[pos].TryGetValue(action.Moves[0].Tr, out HashSet<INode>? possibleNodes))
                continue;
            foreach (var node in possibleNodes)
            {
                if (!taken.Contains(node.ID))
                    temp.Add(node.ID);
            }
        }
        state.FantomPossiblePositions = temp;
        return state;
    }

    private FantomGameState NextTurnAfterFantomPlay(FantomGameState state, FantomGameAction action)
    {
        if (action.Moves[0].ContainsTransport())
            state.FantomTokens[action.Moves[0].Tr]--;
        // Fantom is visible
        if (action.Moves[0].ContainsPosition())
            return NextTurnAfterFantomPlayVisible(state, action);

        // First turn
        else if (state.Turn == 1)
            return NextTurnAfterFantomPlayFirstTurn(state, action);

        // Other turns
        else
            return NextTurnAfterFantomPlayOtherTurns(state, action);
    }
    private FantomGameState NextTurnAfterDetectivesPlay(FantomGameState state, FantomGameAction action)
    {
        for (var i = 0; i < action.Moves.Count; i++)
        {
            state.DetectivesPositions[i] = action.Moves[i].NewPosition;
            if (action.Moves[i].Tr != Transport.Nothing)
                state.DetectivesTokens[i][action.Moves[i].Tr]--;
        }
        state.Turn++;
        return state;
    }
    
    public FantomGameState NextState(FantomGameState state, FantomGameAction action)
    {
        var copyState = state.Deepcopy();
        if (state.FantomsTurn)
        {
            copyState = NextTurnAfterFantomPlay(copyState, action);
            // Fantom is visible
            //if (action.Moves[0].ContainsPosition())
            //{
            //    //copyState.FantomPosition = action.Moves[0].NewPosition;
            //    copyState.FantomPossiblePositions = [action.Moves[0].NewPosition];
            //}

            //// First turn (Fantom can be anywhere)
            //else if (!action.Moves[0].ContainsTransport())
            //{
            //    //copyState.FantomPosition = null;
            //    copyState.FantomPossiblePositions = [];
            //    foreach (var node in Map.Nodes)
            //    {
            //        if (!state.DetectivesPositions.Contains(node.ID))
            //            copyState.FantomPossiblePositions.Add(node.ID);
            //    }
            //}

            //// Other turns (we know used transport and previous possible positions => set of new possible positions)
            //else
            //{

            //    HashSet<int> temp = [];
            //    HashSet<int> taken = state.DetectivesPositions.Select(n => n.Value).ToHashSet();

            //    foreach (var pos in copyState.FantomPossiblePositions)
            //    {
            //        if (!Edges[pos].TryGetValue(action.Moves[0].Tr, out HashSet<INode>? possibleNodes))
            //            continue;
            //        foreach (var node in possibleNodes)
            //        {
            //            if (!taken.Contains(node.ID))
            //                temp.Add(node.ID);
            //        }
            //    }
            //    copyState.FantomPossiblePositions = temp;
            //}

            //// Update transports
            //if (action.Moves[0].Tr != Transport.Nothing)
            //    copyState.FantomTokens[action.Moves[0].Tr]--;
        }
        else
        {
            copyState = NextTurnAfterDetectivesPlay(copyState, action);
            //for (var i = 0; i < action.Moves.Count; i++)
            //{
            //    copyState.DetectivesPositions[i] = action.Moves[i].NewPosition;
            //    if (action.Moves[i].Tr != Transport.Nothing)
            //        copyState.DetectivesTokens[i][action.Moves[i].Tr]--;
            //}
            //copyState.Turn++;
        }
        copyState.FantomsTurn = !copyState.FantomsTurn;
        return copyState;
    }

    public Dictionary<FantomGameAction, FantomGameState>? GetAllDeterminizationsInInfoSet(FantomGameState state)
    {
        // Prepare the state for turn 0, Fantom can be anywhere
        if (state.Turn == 0)
        {
            foreach (var node in Map.Nodes)
            {
                if (!state.DetectivesPositions.Contains(node.ID))
                    state.FantomPossiblePositions.Add(node.ID);
            }
                
        }

        // The node already represents a history not an infoset
        if (state.FantomPossiblePositions.Count <= 1) 
            return null;
        
        // Create a dictionary
        var possibleStates = state.FantomPossiblePositions.ToArray();
        var dict = new Dictionary<FantomGameAction, FantomGameState>();

        // And store every possible state where Fantom recieves one of the possible positions
        foreach (var ps in possibleStates)
        {
            var determinizedState = state.Deepcopy();
            determinizedState.FantomPossiblePositions = [ps];
            dict.Add(new() { Moves = [new Move() { NewPosition = ps }] }, determinizedState);
        }
        return dict;
    }

    public double? Reward(FantomGameState state)
    {
        // Fantom survives
        if (state.Turn > MaxLen)
            return fantomWinValue;

        // cannot be terminal as it is an information set
        if (!state.IsFantomPositionKnown())
            return null;

        foreach (var detPos in state.DetectivesPositions)
        {
            // Detectives stand on the same node as the fantom
            if (detPos == state.GetFantomExactPositionIfKnown())
                return detectivesWinValue;
        }

        return null;
    }
}
