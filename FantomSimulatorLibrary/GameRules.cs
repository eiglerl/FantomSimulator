using FantomMapLibrary;

namespace FantomSimulatorLibrary;

public class GameRules
{
    public int GameLen;
    public int NumberOfDetectives;
    public Dictionary<Transport, int> FantomStartTokens;
    public Dictionary<Transport, int> DetectivesStartTokens;

    public GameRules(int gameLen, int numberOfDetectives, Dictionary<Transport, int> fantomStartTokens, Dictionary<Transport, int> detectivesStartTokens)
    {
        GameLen = gameLen;
        NumberOfDetectives = numberOfDetectives;
        FantomStartTokens = fantomStartTokens;
        DetectivesStartTokens = detectivesStartTokens;
    }
}
