namespace FantomMapLibrary;

public interface INode
{
    public int ID { get; init; }
    public HashSet<Transport> Transports { get; init; }
    public Dictionary<Transport, HashSet<INode>> ConnectedNodes { get; init; }
    //public bool ContainsTransport(Transport transport);
    //public HashSet<INode> AllConnectedToTransport(Transport tr);
    //public HashSet<INode> AllConnectedToUsingTransports(HashSet<Transport> transports);
    //public HashSet<INode> AllConnected();
}


