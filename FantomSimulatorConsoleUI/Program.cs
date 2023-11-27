namespace FantomSimulatorConsoleUI;
using FantomSimulatorLibrary;
using FantomMapLibrary;
using ActualSimulator;
using System.Xml.Linq;

internal class Program
{
    static void Main(string[] args)
    {
        int numberOfDetectives = 2;
        Dictionary<Transport, int> initialTokens = new() { { Transport.Cab, 5 } };
        GameRules gameRules = new(gameLen: 5, numberOfDetectives: numberOfDetectives, fantomStartTokens: initialTokens, detectivesStartTokens: initialTokens);
        Map map = MapCreator.EasyMap();
        GameInfo<Map, Node> gameInfo = new(map: map, gameRules: gameRules);

        var fantom = FantomAI.CreateInstance(map, numberOfDetectives);
        var detectives = DetectivesAI.CreateInstance(map, numberOfDetectives);
        fantom.SetTransports(initialTokens);
        fantom.SetOpponentTransports(initialTokens);
        detectives.SetTransports(initialTokens);
        detectives.SetOpponentTransports(initialTokens);

        Simulator<Map, Node> simulator = new(gameInfo: gameInfo,
            fantom: fantom,
            detectives: detectives,
            logger: new ConsoleLogger(verbosity: 5));

        simulator.SimulateWholeGame();
    }
}

class MapCreator
{
    public static Map EasyMap()
    {
        var tr = Transport.Cab;
        List<Node> nodes = [];
        for(int i = 0; i < 8; i++) 
        {
            nodes.Add(new Node(ID: i + 1, new() { tr }, new() { { tr, new() } }));
        }

        List<(int, int)> connections = new() { (1, 2), (2, 3), (3, 4), (4, 1), (4, 5), (5, 6), (6, 7), (7, 8), (8, 5) };
        foreach ((int i, int j) in connections)
        { 
            nodes[i-1].ConnectedNodes[tr].Add(nodes[j-1]);
            nodes[j-1].ConnectedNodes[tr].Add(nodes[i-1]);
        }
        return new(nodes);
    }
}

