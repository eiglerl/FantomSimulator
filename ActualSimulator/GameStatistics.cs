namespace ActualSimulator;

public interface IStatisticsKeeping<TState, TAction>
{
    public void NewGameStarts(int id);
    public void UpdateGame(int id, TAction action);
    public void UpdateGame(TAction action);
    public void RecapGame(int id);
    public void RecapAllGames();
}

public class GameStatistics : IStatisticsKeeping<FantomGameState, FantomGameAction>
{
    public IGameDescription<FantomGameState, FantomGameAction> Description { get; set; }
    public Dictionary<int, List<FantomGameAction>> Games { get; set; }
    private int? lastAddedID;

    public GameStatistics(IGameDescription<FantomGameState, FantomGameAction> description)
    {
        Description = description;
        Games = [];
    }

    public void NewGameStarts(int id)
    {
        Games.Add(id, []);
        lastAddedID = id;
    }

    public void UpdateGame(int id, FantomGameAction action)
        => Games[id].Add(action);

    // updates last game
    public void UpdateGame(FantomGameAction action)
    {
        if (lastAddedID.HasValue)
            Games[lastAddedID.Value].Add(action);
        else
            throw new ArgumentNullException("No game added.");
    }

    public void RecapAllGames()
    {
        foreach (var key in Games.Keys)
            RecapGame(key);
    }

    public void RecapGame(int id)
    {
        if (Games.ContainsKey(id))
            RecapActions(Games[id]);
        
    }

    /// <summary>
    /// Assumes .ToString is implemented properly on TAction.
    /// </summary>
    /// <param name="actions"></param>
    public static void RecapActions(List<FantomGameAction> actions)
    {
        foreach (var action in actions)
            Console.WriteLine(action);
    }


}
