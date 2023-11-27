using FantomMapLibrary;

namespace ActualSimulator;

public class FantomAI : IPlayerBase<Map, Node>
{
    Map Map { get; set; }
    Dictionary<Transport, int> Transports { get; set; }
    List<Dictionary<Transport, int>> OpponentTransports { get; set; }

    int? CurrentPosition = null;
    List<int?> OpponentPositions;
    int OpponentCounter = 0;

    private FantomAI(Map map, int numberOfDetectives)
    {
        Map = map;
        Transports = [];
        OpponentTransports = new();
        OpponentPositions = [];
        for (int i = 0; i < numberOfDetectives; i++)
        {
            OpponentTransports.Add(new());
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
            HashSet<Transport> possibleTransports = new(Transports.Where(x => x.Value > 0).Select(x => x.Key));
            List<Move> possibleMoves = [];
            foreach (var tr in possibleTransports)
            {
                HashSet<INode> possibleNodes = node.ConnectedNodes[tr];
                foreach (var n in possibleNodes)
                    possibleMoves.Add(new(n.ID, tr));
            }
            return possibleMoves[rnd.Next(possibleMoves.Count)];
        }
    }

    public Task<Move> GetMoveAsync()
    {
        return Task.Run(GetMove);
    }

    public void OpponentMove(Move move)
    {
        int index = OpponentCounter;
        OpponentPositions[index] = move.pos;
        if (move.tr != Transport.Nothing)
            OpponentTransports[index][move.tr]--;
        OpponentCounter = (OpponentCounter + 1) % OpponentTransports.Count;
    }

    public void PlayIsNotOK(Move lastMove)
    {
        throw new NotImplementedException();
    }

    public void PlayIsOK(Move lastMove)
    {
        CurrentPosition = lastMove.pos;
        if (lastMove.tr != Transport.Nothing)
            Transports[lastMove.tr]--;
    }

    public void SetOpponentTransports(Dictionary<Transport, int> transports)
    {
        for (int i = 0; i < OpponentTransports.Count; i++)
            OpponentTransports[i] = new(transports);
    }

    public void SetTransports(Dictionary<Transport, int> transports)
        => Transports = new(transports);
    

    // Factory method to create an instance of the player
    public static FantomAI CreateInstance(Map ggs, int numberOfDetectives) => new(ggs, numberOfDetectives);

}
