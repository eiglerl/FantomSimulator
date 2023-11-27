using FantomMapLibrary;

namespace ActualSimulator;

public class DetectivesAI : IPlayerBase<Map, Node>
{
    Map Map { get; set; }
    Dictionary<Transport, int> OpponentTransports { get; set; }
    List<Dictionary<Transport, int>> Transports { get; set; }

    int? OpponentPosition = null;
    List<int?> CurrentPositions;
    int PlayerCounter = 0;

    private DetectivesAI(Map map, int numberOfDetectives)
    {
        Map = map;
        Transports = [];
        CurrentPositions = [];
        for (int i = 0; i < numberOfDetectives; i++)
        {
            Transports.Add([]);
            CurrentPositions.Add(null);
        } 
        OpponentTransports = [];
    }

    public Move GetMove()
    {
        return GetRandomMove();
    }

    private Move GetRandomMove()
    {
        Random rnd = new();
        HashSet<int> detectivesPositionsSet = new(CurrentPositions.Where(x => x.HasValue).Select(x => x.Value));

        if (CurrentPositions[PlayerCounter] == null)
        {
            int? newPos = null;
            while (newPos == null || (newPos != null && detectivesPositionsSet.Contains(newPos.Value)))
            {
                newPos = rnd.Next(Map.Nodes.Count);
            }
            int id = Map.Nodes[newPos.Value].ID;
            return new(id);
        }
        else
        {
            var node = Map.GetNodeByID(CurrentPositions[PlayerCounter].Value);
            HashSet<Transport> possibleTransports = new(Transports[PlayerCounter].Where(x => x.Value > 0).Select(x => x.Key));
            List<Move> possibleMoves = [];
            foreach (var tr in possibleTransports)
            {
                var possibleNodes = node.ConnectedNodes[tr].Where(x => !detectivesPositionsSet.Contains(x.ID)).Select(x => x);
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
        OpponentPosition = move.pos;
        if (move.tr != Transport.Nothing)
            OpponentTransports[move.tr]--;
    }

    public void PlayIsNotOK(Move lastMove)
    {
        throw new NotImplementedException();
    }

    public void PlayIsOK(Move lastMove)
    {
        int index = PlayerCounter;
        CurrentPositions[index] = lastMove.pos;
        if (lastMove.tr != Transport.Nothing)
            Transports[index][lastMove.tr]--;
        PlayerCounter = (index + 1) % Transports.Count; 
    }

    public void SetOpponentTransports(Dictionary<Transport, int> transports)
        => OpponentTransports = new(transports);
    

    public void SetTransports(Dictionary<Transport, int> transports)
    {
        for (int i = 0; i < Transports.Count; i++)
            Transports[i] = new(transports);
        
    }

    // Factory method to create an instance of the player
    public static DetectivesAI CreateInstance(Map ggs, int numberOfDetectives) => new(ggs, numberOfDetectives);
}
