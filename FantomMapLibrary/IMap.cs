namespace FantomMapLibrary;

public interface IMap
{
    public List<INode> Nodes { get; init; }
    public INode GetNodeByID(int id) {  return Nodes[id-1]; }
}
