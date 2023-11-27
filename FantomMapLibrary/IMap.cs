namespace FantomMapLibrary;


public interface IMap<NodeType> where NodeType : INode
{
    public List<NodeType> Nodes { get; init; }
    public NodeType GetNodeByID(int id) { return Nodes[id - 1]; }
}
