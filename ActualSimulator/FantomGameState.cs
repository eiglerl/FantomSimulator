using FantomMapLibrary;
namespace ActualSimulator;

public struct FantomGameState
{
    public HashSet<int>? FantomPossiblePositions;
    public Dictionary<Transport, int> FantomTokens;

    public List<int?> DetectivesPositions;
    public List<Dictionary<Transport, int>> DetectivesTokens;

    public int Turn;
    public bool FantomsTurn;

    public static FantomGameState Start(Dictionary<Transport, int> fantomTokens, Dictionary<Transport, int> detectivesTokens, int detectivesCount=2)
    {
        FantomGameState state = new()
        {
            FantomPossiblePositions = [],
            FantomTokens = new(fantomTokens),

            DetectivesPositions = [],
            DetectivesTokens = [],

            Turn = 0,
            FantomsTurn = false
        };

        for (int i = 0; i < detectivesCount; i++)
        {
            state.DetectivesPositions.Add(null);
            state.DetectivesTokens.Add(new(detectivesTokens));
        }
        return state;
    }

    public bool IsFantomPositionKnown()
        => FantomPossiblePositions is not null && FantomPossiblePositions.Count == 1;

    public int GetFantomExactPositionIfKnown()
    {
        if (!IsFantomPositionKnown())
            throw new ArgumentException("Fantom position is not known");
        return FantomPossiblePositions.First();
    }

    public FantomGameState Deepcopy()
    {
        FantomGameState copy = new()
        {
            FantomPossiblePositions = new(this.FantomPossiblePositions),
            FantomTokens = new(this.FantomTokens),

            DetectivesPositions = new(this.DetectivesPositions),
            DetectivesTokens = new(),

            Turn = this.Turn,
            FantomsTurn = this.FantomsTurn
        };
        foreach (var tokens in this.DetectivesTokens)
        {
            copy.DetectivesTokens.Add(new(tokens));
        }
        return copy;
    }
}
