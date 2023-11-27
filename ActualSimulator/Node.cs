namespace ActualSimulator;
using FantomMapLibrary;

public record Node(int ID, HashSet<Transport> Transports, Dictionary<Transport, HashSet<INode>> ConnectedNodes) : INode
{
    // Explicit interface implementation
    //Dictionary<Transport, HashSet<INode>> INode.ConnectedNodes { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }
    // Public property with the desired return type
    //public Dictionary<Transport, HashSet<Node>> ConnectedNodesWithType => ConnectedNodes;
}


//public class Node : INode
//{
//    public int ID { get; init; }
//    public HashSet<Transport> Transports { get; init; }
//    public Dictionary<Transport, HashSet<Node>> ConnectedNodes { get; init; }
//    Dictionary<Transport, HashSet<INode>> INode.ConnectedNodes { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }
//}