namespace FantomSimulatorLibrary;
using FantomMapLibrary;


public class GameMap<NodeType> : IMap<NodeType>
    where NodeType : INode
{
    public List<NodeType> Nodes { get; init; }

    public GameMap() 
    {
        Nodes = new();
    }
}


