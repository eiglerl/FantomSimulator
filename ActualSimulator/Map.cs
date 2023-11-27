namespace ActualSimulator;
using FantomMapLibrary;

public class Map : IMap<Node> 

{
    public Map(List<Node> nodes)
    {
        Nodes = nodes;
    }

    public List<Node> Nodes { get; init; }
    public Node GetNodeByID(int id)
        => Nodes[id - 1];
    
}
