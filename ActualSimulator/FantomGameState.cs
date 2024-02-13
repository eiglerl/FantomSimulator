using FantomMapLibrary;
namespace ActualSimulator;

public struct FantomGameState //: IDeterminizableState<FantomGameState>
{
    public int? FantomPosition;
    public HashSet<int> FantomPossiblePositions;
    public Dictionary<Transport, int> FantomTokens;

    public List<int?> DetectivesPositions;
    public List<Dictionary<Transport, int>> DetectivesTokens;

    public int Turn;
    public bool FantomsTurn;

    public static FantomGameState Start(Dictionary<Transport, int> fantomTokens, Dictionary<Transport, int> detectivesTokens, int detectivesCount=2)
    {
        FantomGameState state = new()
        {
            FantomPosition = null,
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

    public FantomGameState Deepcopy()
    {
        FantomGameState copy = new()
        {
            FantomPosition = this.FantomPosition,
            FantomPossiblePositions = this.FantomPossiblePositions,
            FantomTokens = new(this.FantomTokens),

            DetectivesPositions = new(this.DetectivesPositions),
            DetectivesTokens = new(this.DetectivesTokens),

            Turn = this.Turn,
            FantomsTurn = this.FantomsTurn
        };
        return copy;
    }

    public FantomGameState DeterminizeUniform(Random rnd)
    {
        var determinizedState = this.Deepcopy();
        int possibleStatesCount = determinizedState.FantomPossiblePositions.Count;
        if (possibleStatesCount <= 1)
            return determinizedState;
        var possibleStates = determinizedState.FantomPossiblePositions.ToArray();
        var chosenState = possibleStates[rnd.Next(possibleStatesCount)];
        determinizedState.FantomPosition = chosenState;
        determinizedState.FantomPossiblePositions = [];
        return determinizedState;
    }
}
