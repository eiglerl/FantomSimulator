namespace FantomSimulatorLibrary;
using FantomMapLibrary;


public class GameMap : IMap
{
    public List<INode> Nodes { get; init; }

    public GameMap() 
    {
        Nodes = new();
    }
}


