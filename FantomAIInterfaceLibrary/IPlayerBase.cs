namespace FantomMapLibrary;

public interface IPlayerBase<MapType, NodeType>
    where MapType : IMap<NodeType>
    where NodeType : INode
    //public interface IPlayerBase<MapType>
    //    where MapType : IMap<INode>
{
    // Called when the player's move is valid
    public void PlayIsOK(Move lastMove);
    // Called when the player's move is not valid
    public void PlayIsNotOK(Move lastMove);

    // Synchronously gets the player's move
    public Move GetMove();
    // Asynchronously gets the player's move
    public Task<Move> GetMoveAsync();

    // Sets the available transport tokens for the player
    public void SetTransports(Dictionary<Transport, int> transports);
    // Sets the available transport tokens for the opponent
    public void SetOpponentTransports(Dictionary<Transport, int> transports);

    // Factory method to create an instance of the player
    public static IPlayerBase<MapType, NodeType> CreateInstance(MapType ggs, int numberOfDetectives) => throw new NotImplementedException();

    // Called when the opponent makes a move
    public void OpponentMove(Move move);
}
