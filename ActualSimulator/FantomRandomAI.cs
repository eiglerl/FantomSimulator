using FantomMapLibrary;

namespace ActualSimulator;

public class FantomRandomAI : IPlayerBase<Map, Node>
{
    Map Map { get; set; }
    Dictionary<Transport, int> Transports { get; set; }
    List<Dictionary<Transport, int>> OpponentTransports { get; set; }

    int? CurrentPosition = null;
    List<int?> OpponentPositions;
    int OpponentCounter = 0;

    private FantomRandomAI(Map map, int numberOfDetectives)
    {
        Map = map;
        Transports = [];
        OpponentTransports = [];
        OpponentPositions = [];
        for (int i = 0; i < numberOfDetectives; i++)
        {
            OpponentTransports.Add([]);
            OpponentPositions.Add(null);
        }
    }
    public Move GetMove()
    {
        return GetRandomMove();
    }

    private Move GetRandomMove()
    {
        Random rnd = new();

        if (CurrentPosition == null)
        {
            int? newPos = null;
            HashSet<int> opponentPositionsSet = new(OpponentPositions.Where(x => x.HasValue).Select(x => x.Value));
            while (newPos == null || (newPos != null && opponentPositionsSet.Contains(newPos.Value)))
            {
                newPos = rnd.Next(Map.Nodes.Count);
            }
            int id = Map.Nodes[newPos.Value].ID;
            return new(id);
        }
        else
        {
            var node = Map.GetNodeByID(CurrentPosition.Value);
            HashSet<Transport> possibleTransports = new(Transports.Where(x => x.Value > 0 && node.Transports.Contains(x.Key)).Select(x => x.Key));
            List<Move> possibleMoves = [];
            foreach (var tr in possibleTransports)
            {
                HashSet<INode> possibleNodes = node.ConnectedNodes[tr];
                foreach (var n in possibleNodes)
                    possibleMoves.Add(new(n.ID, tr));
            }
            if (possibleMoves.Count == 0)
            {
                return new(CurrentPosition.Value);
            }
            return possibleMoves[rnd.Next(possibleMoves.Count)];
        }
    }

    public Task<Move> GetMoveAsync()
    {
        return Task.Run(GetMove);
    }

    private void UpdateTokens(Move move)
    {
        if (move.ContainsTransport())
            Transports[move.Tr]++;
    }


    public void OpponentMove(Move move)
    {
        int index = OpponentCounter;
        OpponentPositions[index] = move.NewPosition;
        if (move.ContainsTransport())
            OpponentTransports[index][move.Tr]--;
        UpdateTokens(move);
        OpponentCounter = (OpponentCounter + 1) % OpponentTransports.Count;
    }

    public void PlayIsNotOK(Move lastMove)
    {
        throw new NotImplementedException();
    }

    public void PlayIsOK(Move lastMove)
    {
        CurrentPosition = lastMove.NewPosition;
        if (lastMove.Tr != Transport.Nothing)
            Transports[lastMove.Tr]--;
    }

    public void SetOpponentTransports(Dictionary<Transport, int> transports)
    {
        for (int i = 0; i < OpponentTransports.Count; i++)
            OpponentTransports[i] = new(transports);
    }

    public void SetTransports(Dictionary<Transport, int> transports)
        => Transports = new(transports);
    

    // Factory method to create an instance of the player
    public static FantomRandomAI CreateInstance(Map ggs, int numberOfDetectives) => new(ggs, numberOfDetectives);

}
