namespace SimulatorUnitTests;
using Xunit;
using ActualSimulator;
using FantomMapLibrary;
using Moq;

public class GameRulesTest
{
    private Map GetSmallMapWithOneTransport(Transport tr)
    {
        List<Node> nodes = [];
        for (int i = 1; i < 5; i++)
            nodes.Add(new Node(ID: i, Transports: [tr], ConnectedNodes: new() { { tr, new() } }));

        List<(int, int)> Connections = [(1, 2), (2, 3), (3, 4), (4, 1)];

        foreach ((int i, int j) in Connections)
        {
            nodes[i].ConnectedNodes[tr].Add(nodes[j]);
            nodes[j].ConnectedNodes[tr].Add(nodes[i]);
        }
        return new Map(nodes);
    }
    [Theory]
    [InlineData(1, 2, true)]
    [InlineData(2, 3, true)]
    [InlineData(4, 3, true)]
    [InlineData(4, 1, true)]
    [InlineData(1, 4, true)]
    [InlineData(1, 1, false)]
    [InlineData(1, 3, false)]
    [InlineData(3, 1, false)]
    [InlineData(4, 2, false)]

    public void CheckValidityOfMove(int from, int to, bool existsEdge)
    {
        var tr = Transport.Cab;
        var nodes = GetSmallMapWithOneTransport(tr);


    }
}