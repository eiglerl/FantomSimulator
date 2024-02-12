﻿using FantomMapLibrary;
using System.Security;

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
    private HashSet<int> _fantomVisibleTurns = [];


    public Simulator(GameInfo<MapType, NodeType> gameInfo, IPlayerBase<MapType, NodeType> fantom, IPlayerBase<MapType, NodeType> detectives, ILogger logger)
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
            move = _gameInfo.RandomMoveForPlayer();

        if (whoPlaysNow.FantomPlays)
            _logger.LogMessage(LogType.Info, $"Turn {_gameInfo.TurnCounter}");
        _logger.LogMessage(LogType.Move, $"{_logger.Var} moves to {move.NewPosition} using {move.Tr}");
        
        currentPlayer.PlayIsOK(move);
        if (whoPlaysNow.FantomPlays && !_fantomVisibleTurns.Contains(_gameInfo.TurnCounter))
            opponentPlayer.OpponentMove(new Move(move.Tr));
        else
            opponentPlayer.OpponentMove(move);

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
        while (_gameInfo.IsGameOver() == GameOutcome.NotYet)
        {
            SimulateOneStep();
        }

        return _gameInfo.IsGameOver();
        
    }
}
