using ActualSimulator;
using FantomMapLibrary;

namespace FantomSimulatorConsoleUI;

public class DetectivesAIMCTS : IPlayerBase<Map, Node>
{
    Map Map { get; set; }
    Dictionary<Transport, int> OpponentTransports { get; set; }
    List<Dictionary<Transport, int>> Transports { get; set; }

    int? OpponentPosition = null;
    List<int?> CurrentPositions;
    int PlayerCounter = 0;
    FantomGameAction? CurrentAction = null;
    List<Move> Moves;


    MapDescription MapDescription { get; set; }
    FantomGameState? currentState = null;
    FantomGameState CurrentState
    {
        get
        {
            currentState ??= FantomGameState.Start(OpponentTransports, Transports[0]);
            return currentState.Value;
        }
        set
        {
            currentState = value;
        }

    }

    private DetectivesAIMCTS(Map map, int numberOfDetectives)
    {
        Map = map;
        Transports = [];
        OpponentTransports = [];
        CurrentPositions = [];
        Moves = [];
        for (int i = 0; i < numberOfDetectives; i++)
        {
            Transports.Add([]);
            CurrentPositions.Add(null);
        }
        MapDescription = new MapDescription(Map, 8, fantomPlayer: false);

    }

    public Move GetMove()
    {
        if (CurrentAction is null)
        {
            var tree = new MonteCarloTreeSearch<FantomGameState, FantomGameAction>.Tree(MapDescription, CurrentState);
            MonteCarloTreeSearch<FantomGameState, FantomGameAction> mcts = new();
            CurrentAction = mcts.Simulate(tree, 0.1);
            //var correctMoves = CurrentAction.Value.Moves;
            //correctMoves.Reverse
            CurrentAction.Value.Moves.Reverse();
        }
        return CurrentAction.Value.Moves[PlayerCounter];
    }

    public Task<Move> GetMoveAsync()
    {
        return Task.Run(GetMove);
    }

    public void OpponentMove(Move move)
    {
        CurrentState = MapDescription.NextState(CurrentState, new FantomGameAction() { Moves = [move] });
        if (move.Tr != Transport.Nothing)
            OpponentTransports[move.Tr]--;
        if (move.ContainsPosition())
            OpponentPosition = move.NewPosition;
    }

    public void PlayIsNotOK(Move lastMove)
    {
        throw new NotImplementedException();
    }

    public void PlayIsOK(Move lastMove)
    {
        int index = PlayerCounter;
        Moves.Add(lastMove);
        if (index == CurrentPositions.Count - 1)
        {
            CurrentState = MapDescription.NextState(CurrentState, new FantomGameAction() { Moves = new(Moves) });
            Moves = [];
            CurrentAction = null;
        }
        CurrentPositions[index] = lastMove.NewPosition;
        if (lastMove.Tr != Transport.Nothing)
            Transports[index][lastMove.Tr]--;
        PlayerCounter = (PlayerCounter + 1) % CurrentPositions.Count;
    }

    public void SetOpponentTransports(Dictionary<Transport, int> transports)
    {
        OpponentTransports = new(transports);
    }

    public void SetTransports(Dictionary<Transport, int> transports)
    {
        for (int i = 0; i < Transports.Count; i++)
            Transports[i] = new(transports);
    }

    public static DetectivesAIMCTS CreateInstance(Map ggs, int numberOfDetectives) => new(ggs, numberOfDetectives);

}
