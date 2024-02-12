namespace FantomSimulatorConsoleUI;
using FantomSimulatorLibrary;
using FantomMapLibrary;
using ActualSimulator;

internal class Program
{
    static void Main(string[] args)
    {
        int numberOfDetectives = 2;
        Dictionary<Transport, int> initialTokens = new() { { Transport.Cab, 12 } };
        Map map = MapCreator.EasyMap();
        //Map map = MapCreator.CircleWithX();
        //Map map = MapCreator.CheckBoard(3, 3);

        int numberOfRepeats = 20;
        int fantomWins = 0;

        for (int i = 0; i < numberOfRepeats; i++)
        {
            Console.WriteLine($"Game {i+1}");
            GameRules gameRules = new(gameLen: 8, numberOfDetectives: numberOfDetectives, fantomStartTokens: initialTokens, detectivesStartTokens: initialTokens);
            GameInfo<Map, Node> gameInfo = new(map: map, gameRules: gameRules);


            //var fantom = FantomAI.CreateInstance(map, numberOfDetectives); // 7/100
            var fantom = FantomAIMCTS.CreateInstance(map, numberOfDetectives); // 74/100 with 1 sec time, 
            var detectives = DetectivesAI.CreateInstance(map, numberOfDetectives);
            //var detectives = DetectivesAIMCTS.CreateInstance(map, numberOfDetectives);
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
        //      3           6
        //    /   \       /   \
        //  2       4 - 5       7
        //    \   /       \   /
        //      1           8

        List<(int, int)> connections = new() { (1, 2), (2, 3), (3, 4), (4, 1), (4, 5), (5, 6), (6, 7), (7, 8), (8, 5) };

        // TEST
        //connections.Add((3, 6));
        //connections.Add((1, 8));
        return CreateMapFromConnections(8, connections);
        
    }




    public static Map CircleWithX()
    {

        List<(int, int)> connections = new() { (1, 2), (2, 3), (3, 4), (4, 5), (5, 6), (6, 7), (7, 8), (8, 1), (1, 9), (3, 9), (5, 9), (7, 9) };
        return CreateMapFromConnections(9, connections);
    }

    public static Map CheckBoard(int rowLen, int colLen)
    {
        List<(int, int)> connections = [];
        for (int i = 0; i < rowLen; i++)
        {
            for (int j = 0; j < colLen; j++)
            {
                int current = i + j * rowLen + 1;
                if (i < rowLen - 1)
                {
                    connections.Add((current, current + 1));
                }
                if (j < colLen - 1)
                {
                    connections.Add((current, current + rowLen));
                }
            }
        }

        return CreateMapFromConnections(rowLen*colLen, connections);
    }

    private static Map CreateMapFromConnections(int numberOfNodes, List<(int, int)> connections)
    {
        var tr = Transport.Cab;

        List<Node> nodes = [];
        for (int i = 0; i < numberOfNodes; i++)
        {
            nodes.Add(new Node(ID: i + 1, [tr], new() { { tr, new() } }));
        }

        foreach ((int i, int j) in connections)
        {
            nodes[i - 1].ConnectedNodes[tr].Add(nodes[j - 1]);
            nodes[j - 1].ConnectedNodes[tr].Add(nodes[i - 1]);
        }
        return new(nodes);

    }
}

