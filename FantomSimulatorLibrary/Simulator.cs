using FantomMapLibrary;
using System.Security;

namespace FantomSimulatorLibrary;

public interface ISimulator
{
    public void SimulateOneStep();
    public void SimulateWholeGame();
}

public class Simulator : ISimulator
{
    private ILogger _logger;
    private IPlayerBase _fantomPlayer;
    private IPlayerBase _detectivesPlayer;
    private GameInfo _gameInfo;

 
    public Simulator(GameInfo gameInfo, IPlayerBase fantom, IPlayerBase detectives, ILogger logger)
    {
        _logger = logger;
        _fantomPlayer = fantom;
        _detectivesPlayer = detectives;
        _gameInfo = gameInfo;
    }
    public void SimulateOneStep()
    {
        var whoPlaysNow = _gameInfo.WhoPlaysNow;
        IPlayerBase currentPlayer;
        if (whoPlaysNow.FantomPlays)
            currentPlayer = _fantomPlayer;
        else
            currentPlayer = _detectivesPlayer;

        var move = currentPlayer.GetMove();

        if (!_gameInfo.IsMovePossible(move))
            move = _gameInfo.RandomMoveForPlayer();


    }

    public void SimulateWholeGame()
    {
        throw new NotImplementedException();
    }
}
