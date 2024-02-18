using FantomMapLibrary;

namespace FantomSimulatorLibrary;

public interface ISimulator
{
    public void SimulateOneStep();
    public GameOutcome SimulateWholeGame();
}

public enum GameOutcome { FantomWon, DetectivesWon, NotYet }


public class Simulator<MapType, NodeType> : ISimulator
    where MapType : IMap<NodeType>
    where NodeType : INode
{
    private ILogger _logger;
    private GameInfo<MapType, NodeType> _gameInfo;
    private IPlayerBase<MapType, NodeType> _fantomPlayer;
    private IPlayerBase<MapType, NodeType> _detectivesPlayer;
    private HashSet<int> _fantomVisibleTurns = [3, 6, 11, 16, 21];
    // OLD
    // [3,5] - 33/50 fantom win
    // [5] - 45/50
    // [2,5] 34/50

    // NEW 
    // [3,5] - 25/50
    // [5] - 36/50
    // [2,5] 27/50


    public delegate void GetNextMove(Move move);
    private GetNextMove RecieveNextMoveCall;
    public void UpdateCallOnMoveDelegate(GetNextMove function)
    {
        RecieveNextMoveCall += function;
    }

    public delegate void GetGameOutcome(GameOutcome outcome);
    private GetGameOutcome RecieveGameOutcomeCall;
    public void UpdateCallOnGameOutcomeDelegate(GetGameOutcome function)
    {
        RecieveGameOutcomeCall += function;
    }


    public Simulator(
        GameInfo<MapType, NodeType> gameInfo,
        IPlayerBase<MapType, NodeType> fantom, IPlayerBase<MapType, NodeType> detectives,
        ILogger logger)
    {
        _logger = logger;
        _fantomPlayer = fantom;
        _detectivesPlayer = detectives;
        _gameInfo = gameInfo;
    }
    public void SimulateOneStep()
    {
        var whoPlaysNow = _gameInfo.WhoPlaysNow;
        IPlayerBase<MapType, NodeType> currentPlayer, opponentPlayer;

        if (whoPlaysNow.FantomPlays)
        {
            currentPlayer = _fantomPlayer;
            opponentPlayer = _detectivesPlayer;
            _gameInfo.TurnCounter++;
            _logger.Var = "Fantom";
        }
        else
        {
            currentPlayer = _detectivesPlayer;
            opponentPlayer = _fantomPlayer;
            _logger.Var = $"Detective {whoPlaysNow.DetectiveIndex + 1}";
        }
        var move = currentPlayer.GetMove();

        if (!_gameInfo.IsMovePossible(move))
        {
            Console.Write($"Tried to move to {move.NewPosition} from");
            move = _gameInfo.RandomMoveForPlayer();
        }

        if (whoPlaysNow.FantomPlays)
            _logger.LogMessage(LogType.Info, $"Turn {_gameInfo.TurnCounter}");
        _logger.LogMessage(LogType.Move, $"{_logger.Var} moves to {move.NewPosition} using {move.Tr}");
        
        currentPlayer.PlayIsOK(move);
        if (whoPlaysNow.FantomPlays && !_fantomVisibleTurns.Contains(_gameInfo.TurnCounter))
        {
            opponentPlayer.OpponentMove(new Move(move.Tr));
            //if (whoPlaysNow.FantomPlays)
            //    Console.WriteLine($"Detectives recieve {new Move(move.Tr).NewPosition}");
        }
        else
        {
            opponentPlayer.OpponentMove(move);
            if (whoPlaysNow.FantomPlays)
                Console.WriteLine($"Detectives recieve {move.NewPosition}");
        }

        _gameInfo.AcceptMove(move);

        if (_gameInfo.IsGameOver() != GameOutcome.NotYet)
        {
            if (_gameInfo.IsGameOver() == GameOutcome.FantomWon)
                _logger.Var = "Fantom";
            else
                _logger.Var = "Detectives";
            _logger.LogMessage(LogType.Move, $"{_logger.Var} won!");
        }
            
    }

    public GameOutcome SimulateWholeGame()
    {
        Console.WriteLine("Game starts");
        Console.Write($"Visible turns: ");
        foreach (var t in _fantomVisibleTurns)
            Console.Write($"{t}, ");
        Console.WriteLine();
        while (_gameInfo.IsGameOver() == GameOutcome.NotYet)
        {
            SimulateOneStep();
        }

        return _gameInfo.IsGameOver();
        
    }
}
