using FantomMapLibrary;

namespace FantomSimulatorLibrary;

public struct PlayerInfo
{ 
    public int? Position;
    public Dictionary<Transport, int> Tokens;

    private void UpdateTokens(Transport tr, int amount = -1)
        => Tokens[tr] -= amount;

    public void MoveTo(Move move)
    {
        Position = move.pos;
        if (move.tr != Transport.Nothing)
            UpdateTokens(move.tr);
    }
}
