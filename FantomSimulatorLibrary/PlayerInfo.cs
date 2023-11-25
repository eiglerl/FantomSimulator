using FantomMapLibrary;

namespace FantomSimulatorLibrary;

public struct PlayerInfo
{ 
    public int? Position;
    public Dictionary<Transport, int> Tokens;

    void updateTokens(Transport tr, int amount = -1)
        => Tokens[tr] -= amount;

    public void MoveTo(Move move)
    {
        Position = move.pos;
        updateTokens(move.tr);
    }
}
