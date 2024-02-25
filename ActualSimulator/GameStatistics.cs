using FantomMapLibrary;
using FantomSimulatorLibrary;
using System.Diagnostics;
using System.IO;
namespace ActualSimulator;

public class GameStatistics
{
    int id;
    public struct GameInformation
    {
        public List<Move> Moves;
        public GameOutcome Outcome;

        public void SetOutcome(GameOutcome outcome)
            => Outcome = outcome;
    }

    public Dictionary<int, GameInformation> Games;

    public GameStatistics()
    {
        id = 1;
        Games = new() { { id, new() { Moves = [] } } };
    }

    public void GetNextMove(Move move)
    {
        if (!Games.ContainsKey(id))
            Games[id] = new() { Moves = [] };
        Games[id].Moves.Add(move);
    }

    public void GetGameOutcome(GameOutcome outcome)
    {
        Games[id] = new() { Moves = Games[id].Moves, Outcome = outcome };
        id++;
    }

    public void SaveToFileReadable(string fileName)
    {
        int playerCounter = 1;
        using StreamWriter writer = new StreamWriter(fileName);
        foreach ((var key, var game) in Games)
        {
            writer.WriteLine($"Game {key}");

            foreach (var move in game.Moves)
            {
                string whoPlays = playerCounter % 5 == 0 ? "Fantom" : $"Detective {playerCounter % 5}";
                writer.WriteLine($"{whoPlays} moved to {move.NewPosition} using {move.Tr}");
                playerCounter++;
            }

            string who = game.Outcome == GameOutcome.FantomWon ? "Fantom" : "Detectives";
            writer.WriteLine($"{who} won!");
            writer.WriteLine();
        }
        writer.Flush();
        //string fullPath = Path.GetFullPath(fileName);
        //Console.WriteLine("File will be created at: " + fullPath);

    }

    public void SaveToFile(string fileName)
    {
        using StreamWriter writer = new StreamWriter(fileName);
        foreach ((var key, var game) in Games)
        {
            writer.WriteLine($"game{key}");
            for (int i = 0; i < game.Moves.Count; i++)
            {
                var move = game.Moves[i];
                writer.Write($"{move.NewPosition},{(int)move.Tr}");
                if (i != game.Moves.Count - 1)
                    writer.Write($"-");
            }
            writer.WriteLine($"{(int)game.Outcome}");
        }
        writer.Flush();
    }

}


//public interface IStatisticsKeeping<TState, TAction>
//{
//    public void NewGameStarts(int id);
//    public void UpdateGame(int id, TAction action);
//    public void UpdateGame(TAction action);
//    public void RecapGame(int id);
//    public void RecapAllGames();
//}

//public class GameStatistics : IStatisticsKeeping<FantomGameState, FantomGameAction>
//{
//    public IGameDescription<FantomGameState, FantomGameAction> Description { get; set; }
//    public Dictionary<int, List<FantomGameAction>> Games { get; set; }
//    private int? lastAddedID;

//    public GameStatistics(IGameDescription<FantomGameState, FantomGameAction> description)
//    {
//        Description = description;
//        Games = [];
//    }

//    public void NewGameStarts(int id)
//    {
//        Games.Add(id, []);
//        lastAddedID = id;
//    }

//    public void UpdateGame(int id, FantomGameAction action)
//        => Games[id].Add(action);

//    // updates last game
//    public void UpdateGame(FantomGameAction action)
//    {
//        if (lastAddedID.HasValue)
//            Games[lastAddedID.Value].Add(action);
//        else
//            throw new ArgumentNullException("No game added.");
//    }

//    public void RecapAllGames()
//    {
//        foreach (var key in Games.Keys)
//            RecapGame(key);
//    }

//    public void RecapGame(int id)
//    {
//        if (Games.ContainsKey(id))
//            RecapActions(Games[id]);
        
//    }

//    /// <summary>
//    /// Assumes .ToString is implemented properly on TAction.
//    /// </summary>
//    /// <param name="actions"></param>
//    public static void RecapActions(List<FantomGameAction> actions)
//    {
//        foreach (var action in actions)
//            Console.WriteLine(action);
//    }


//}
