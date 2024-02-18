using ActualSimulator;
using FantomMapLibrary;
using FantomSimulatorLibrary;

namespace FantomSimulatorConsoleUI;

public class DetectivesAIMCTS : IPlayerBase<Map, Node>
{
    Map Map { get; set; }
    PlayerInfo Opponent { get; set; }
    List<PlayerInfo> Players { get; set; }

    int numberOfDetectives;
    int PlayerCounter = 0;
    FantomGameAction? CurrentAction = null;
    List<Move> Moves;

    MapDescription MapDescription { get; set; }
    FantomGameState? currentState = null;
    FantomGameState CurrentState
    {
        get
        {
            currentState ??= FantomGameState.Start(Opponent.Tokens, Players[0].Tokens, detectivesCount: Players.Count);
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
        Opponent = new() { Position = null, Tokens = [] };
        Players = [];
        Moves = [];
        this.numberOfDetectives = numberOfDetectives;
        for (int i = 0; i < numberOfDetectives; i++)
        {
            Players.Add(new() { Position = null, Tokens = [] });
        }
        MapDescription = new MapDescription(Map, 15, fantomPlayer: false);

    }

    public Move GetMove()
    {
        if (CurrentAction is null)
        {
            var tree = new MonteCarloTreeSearch<FantomGameState, FantomGameAction>.Tree(MapDescription, CurrentState);
            MonteCarloTreeSearch<FantomGameState, FantomGameAction> mcts = new();
            double time = 2; //+ ((CurrentState.Turn == 1) ? 1500 : 0);
            CurrentAction = mcts.Simulate(tree, time);
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
        Opponent.MoveTo(move);

        //Console.Write("Possible fantom positions: ");
        //foreach (var pos in CurrentState.FantomPossiblePositions)
        //    Console.Write($"{pos}, ");
        //Console.WriteLine();
    }

    public void PlayIsNotOK(Move lastMove)
    {
        throw new NotImplementedException();
    }

    public void PlayIsOK(Move lastMove)
    {
        int index = PlayerCounter;
        Moves.Add(lastMove);

        if (index == numberOfDetectives - 1)
        {
            CurrentState = MapDescription.NextState(CurrentState, new FantomGameAction() { Moves = new(Moves) });
            Moves = [];
            CurrentAction = null;
        }

        Players[index].MoveTo(lastMove);
        PlayerCounter = (PlayerCounter + 1) % numberOfDetectives;
    }

    public void SetOpponentTransports(Dictionary<Transport, int> transports)
    {
        Opponent = new() { Position = Opponent.Position, Tokens = new(transports) };
    }

    public void SetTransports(Dictionary<Transport, int> transports)
    {
        for (int i = 0; i < numberOfDetectives; i++)
            Players[i] = new() { Position = Players[i].Position, Tokens = new(transports) };
    }

    public static DetectivesAIMCTS CreateInstance(Map ggs, int numberOfDetectives) => new(ggs, numberOfDetectives);

}
