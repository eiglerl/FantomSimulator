namespace FantomSimulatorConsoleUI;
using FantomSimulatorLibrary;
using FantomMapLibrary;
using ActualSimulator;
using System.Xml.Linq;
using System.Diagnostics;

internal class Program
{
    static void Main(string[] args)
    {
        int numberOfDetectives = 2;
        Dictionary<Transport, int> initialTokens = new() { { Transport.Cab, 12 } };
        Map map = MapCreator.EasyMap();

        int numberOfRepeats = 100;
        int fantomWins = 0;

        for (int i = 0; i < numberOfRepeats; i++)
        {
            Console.WriteLine($"Game {i+1}");
            GameRules gameRules = new(gameLen: 8, numberOfDetectives: numberOfDetectives, fantomStartTokens: initialTokens, detectivesStartTokens: initialTokens);
            GameInfo<Map, Node> gameInfo = new(map: map, gameRules: gameRules);


            //var fantom = FantomAI.CreateInstance(map, numberOfDetectives); // 7/100
            var fantom = FantomAIMCTS.CreateInstance(map, numberOfDetectives); // 74/100
            var detectives = DetectivesAI.CreateInstance(map, numberOfDetectives);
            fantom.SetTransports(initialTokens);
            fantom.SetOpponentTransports(initialTokens);
            detectives.SetTransports(initialTokens);
            detectives.SetOpponentTransports(initialTokens);

            Simulator<Map, Node> simulator = new(gameInfo: gameInfo,
                fantom: fantom,
                detectives: detectives,
                logger: new ConsoleLogger(verbosity: 0));

            var outcome = simulator.SimulateWholeGame();
            if (outcome == GameOutcome.FantomWon)
                fantomWins++;
        }

        Console.WriteLine("------");
        Console.WriteLine($"Fantom winrate {fantomWins}/{numberOfRepeats}");


        // Testing MCTS

        //RockPaperScissors game = new();
        //var tree = new MonteCarloTreeSearch<string, char>.Tree(game, game.Root);
        //MonteCarloTreeSearch<string, char> mcts = new();
        //var action = mcts.Simulate(tree, 1);

        //GlassesFantomGame game = new();
        //var tree = new MonteCarloTreeSearch<GlassesFantomState, GlassesFantomAction>.Tree(game, game.Root);
        //MonteCarloTreeSearch<GlassesFantomState, GlassesFantomAction> mcts = new();
        //var action = mcts.Simulate(tree, 1);
        //PrintActions(action);

        //var newState = game.NextState(game.Root, action);
        //tree = new MonteCarloTreeSearch<GlassesFantomState, GlassesFantomAction>.Tree(game, newState);
        //mcts = new();
        //action = mcts.Simulate(tree, 1);
        //PrintActions(action);
    }

    public static void PrintActions(GlassesFantomAction actions)
    {
        foreach (var a in actions.Moves)
            Console.Write(a);
        Console.WriteLine();
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
            nodes.Add(new Node(ID: i + 1, [tr], new() { { tr, new() } }));
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

