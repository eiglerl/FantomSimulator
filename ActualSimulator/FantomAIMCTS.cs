using FantomMapLibrary;
using FantomSimulatorLibrary;

namespace ActualSimulator;

public class FantomAIMCTS : IPlayerBase<Map, Node>
{
    Map Map { get; set; }
    Dictionary<Transport, int> Transports { get; set; }
    List<Dictionary<Transport, int>> OpponentTransports { get; set; }

    int? CurrentPosition = null;
    List<int?> OpponentPositions;
    int OpponentCounter = 0;
    List<Move> OpponentMoves;

    //MonteCarloTreeSearch<FantomGameState, FantomGameAction> MonteCarloTreeSearch { get; set; }
    MapDescription MapDescription { get; set; }

    FantomGameState? currentState = null;
    FantomGameState CurrentState
    {
        get
        {
            currentState  ??= FantomGameState.Start(Transports, OpponentTransports[0]);
            return currentState.Value;
        }
        set
        {
            currentState = value;
        }
    }


    private FantomAIMCTS(Map map, int numberOfDetectives)
    {
        Map = map;
        Transports = [];
        OpponentTransports = new();
        OpponentPositions = [];
        OpponentMoves = [];
        for (int i = 0; i < numberOfDetectives; i++)
        {
            OpponentTransports.Add(new());
            OpponentPositions.Add(null);
        }

        MapDescription = new MapDescription(Map, 8);
        //MonteCarloTreeSearch = new MonteCarloTreeSearch<FantomGameState, FantomGameAction>();
    }

    public Move GetMove()
    {
        var tree = new MonteCarloTreeSearch<FantomGameState, FantomGameAction>.Tree(MapDescription, CurrentState);
        MonteCarloTreeSearch<FantomGameState, FantomGameAction> mcts = new();
        FantomGameAction move = mcts.Simulate(tree, 1);
        return move.Moves[0];
    }

    public Task<Move> GetMoveAsync()
    {
        return Task.Run(GetMove);
    }

    public void OpponentMove(Move move)
    {
        int index = OpponentCounter;
        OpponentMoves.Add(move);
        if (index == OpponentPositions.Count - 1)
        {
            CurrentState = MapDescription.NextState(CurrentState, new FantomGameAction() { Moves = OpponentMoves });
            OpponentMoves = [];
        }

        OpponentPositions[index] = move.NewPosition;
        if (move.Tr != Transport.Nothing)
            OpponentTransports[index][move.Tr]--;
        OpponentCounter = (OpponentCounter + 1) % OpponentPositions.Count;

    }

    public void PlayIsNotOK(Move lastMove)
    {
        throw new NotImplementedException();
    }

    public void PlayIsOK(Move lastMove)
    {
        CurrentPosition = lastMove.NewPosition;
        if (lastMove.Tr != Transport.Nothing)
            Transports[lastMove.Tr]--;

        CurrentState = MapDescription.NextState(CurrentState, new FantomGameAction() { Moves = [lastMove] });
    }

    public void SetOpponentTransports(Dictionary<Transport, int> transports)
    {
        for (int i = 0; i < OpponentTransports.Count; i++)
            OpponentTransports[i] = new(transports);
    }

    public void SetTransports(Dictionary<Transport, int> transports)
        => Transports = new(transports);

    public static FantomAIMCTS CreateInstance(Map ggs, int numberOfDetectives) => new(ggs, numberOfDetectives);

}
