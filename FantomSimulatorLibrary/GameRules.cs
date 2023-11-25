using FantomMapLibrary;

namespace FantomSimulatorLibrary;

public class GameRules
{
    public int GameLen;
    public Dictionary<Transport, int> FantomStartTokens;
    public Dictionary<Transport, int> DetectivesStartTokens;

    public GameRules(int gameLen, Dictionary<Transport, int> fantomStartTokens, Dictionary<Transport, int> detectivesStartTokens)
    {
        GameLen = gameLen;
        FantomStartTokens = fantomStartTokens;
        DetectivesStartTokens = detectivesStartTokens;
    }
}
