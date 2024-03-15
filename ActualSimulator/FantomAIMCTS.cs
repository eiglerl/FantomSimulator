using FantomMapLibrary;
using FantomSimulatorLibrary;

namespace ActualSimulator;

public class FantomAIMCTS : IPlayerBase<Map, Node>
{
    Map Map { get; set; }
    PlayerInfo Player;
    List<PlayerInfo> Opponents;

    int numberOfDetectives;
    int OpponentCounter = 0;
    List<Move> OpponentMoves;
    Random rnd;

    MapDescription MapDescription { get; set; }

    FantomGameState? currentState = null;
    FantomGameState CurrentState
    {
        get
        {
            currentState ??= FantomGameState.Start(Player.Tokens, Opponents[0].Tokens, detectivesCount: numberOfDetectives);
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
        Player = new() { Position = null, Tokens = [] };
        Opponents = [];
        OpponentMoves = [];
        this.numberOfDetectives = numberOfDetectives;
        for (int i = 0; i < numberOfDetectives; i++)
        {
            Opponents.Add(new() { Position = null, Tokens = [] });
        }
        rnd = new();
        MapDescription = new MapDescription(Map, gameLen: 24);
    }

    public Move GetMove()
    {
        var tree = new MonteCarloTreeSearch<FantomGameState, FantomGameAction>.Tree(MapDescription, CurrentState, rnd);
        MonteCarloTreeSearch<FantomGameState, FantomGameAction> mcts = new();
        FantomGameAction move = mcts.Simulate(tree, 0.2);
        //if (move.Moves[0].Tr == Transport.Taxi)
            //Console.WriteLine("not cab");
        return move.Moves[0];
    }

    public Task<Move> GetMoveAsync()
    {
        return Task.Run(GetMove);
    }

    private void UpdateTokens(Move move)
    {
        if (move.ContainsTransport())
            CurrentState.FantomTokens[move.Tr]++;
    }

    public void OpponentMove(Move move)
    {
        int index = OpponentCounter;
        OpponentMoves.Add(move);
        UpdateTokens(move);

        if (index == numberOfDetectives - 1)
        {
            CurrentState = MapDescription.NextState(CurrentState, new FantomGameAction() { Moves = new(OpponentMoves) });
            OpponentMoves = [];
        }

        Opponents[index].MoveTo(move);
        OpponentCounter = (OpponentCounter + 1) % numberOfDetectives;
    }

    public void PlayIsNotOK(Move lastMove)
    {
        throw new NotImplementedException();
    }

    public void PlayIsOK(Move lastMove)
    {
        Player.MoveTo(lastMove);
        CurrentState = MapDescription.NextState(CurrentState, new FantomGameAction() { Moves = [lastMove] });
    }

    public void SetOpponentTransports(Dictionary<Transport, int> transports)
    {
        for (int i = 0; i < numberOfDetectives; i++)
            Opponents[i] = new() { Position = Opponents[i].Position, Tokens = new(transports) };
    }

    public void SetTransports(Dictionary<Transport, int> transports)
        => Player = new() { Position = Player.Position, Tokens= new(transports) };

    public static FantomAIMCTS CreateInstance(Map ggs, int numberOfDetectives) => new(ggs, numberOfDetectives);

}
