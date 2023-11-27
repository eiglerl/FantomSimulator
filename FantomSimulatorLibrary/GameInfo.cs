using FantomMapLibrary;
namespace FantomSimulatorLibrary;

public class GameInfo<MapType, NodeType>
    where MapType : IMap<NodeType>
    where NodeType : INode
{
    public record struct WhoPlays(bool FantomPlays, int DetectiveIndex);

    public int TurnCounter;
    private int _exactPlayerNow = 1;
    public WhoPlays WhoPlaysNow
    {
        get
        {
            // 0 if Fantom plays
            if (_exactPlayerNow == 0)
                return new(true, 0);
            // >0 else, '_exactPlayerNow'-1 is the index of the detective
            return new(false, _exactPlayerNow-1);
        }
    }
    private PlayerInfo GetCurrentPlayer
    {
        get
        {
            var whoPlays = WhoPlaysNow;
            PlayerInfo playerInfo;
            if (whoPlays.FantomPlays)
                playerInfo = FantomInfo;
            else
                playerInfo = DetectivesInfo[whoPlays.DetectiveIndex];
            return playerInfo;
        }
    }
    public PlayerInfo FantomInfo;
    public List<PlayerInfo> DetectivesInfo;
    public MapType Map;
    public GameRules GameRules;

    // Starting a new game
    public GameInfo(MapType map, GameRules gameRules)
    {
        TurnCounter = 0;
        Map = map;
        GameRules = gameRules;
        FantomInfo = new() { Position = null, Tokens = new(GameRules.FantomStartTokens) };
        DetectivesInfo = [];
        for (int i = 0; i < GameRules.NumberOfDetectives; i++)
            DetectivesInfo.Add(new() { Position = null, Tokens = new(GameRules.DetectivesStartTokens)});
    }

    // Loading an already existing game
    public GameInfo(int turnCounter, PlayerInfo fantomInfo, List<PlayerInfo> detectivesInfo, MapType map, GameRules gameRules)
    {
        TurnCounter = turnCounter;
        FantomInfo = fantomInfo;
        DetectivesInfo = detectivesInfo;
        Map = map;
        GameRules = gameRules;
    }

    public enum GameOutcome { FantomWon, DetectivesWon, NotYet }
    public GameOutcome IsGameOver()
    {
        // Game has already reached the max turns
        if (TurnCounter > GameRules.GameLen)
            return GameOutcome.FantomWon;

        // Check if detectives are on the same position as the Fantom
        int? fantomPosition = FantomInfo.Position;
        if (fantomPosition != null && IsSpaceOccupiedByDetectives(fantomPosition.Value))
            return GameOutcome.DetectivesWon;

        return GameOutcome.NotYet;
    }

    private bool IsSpaceOccupiedByDetectives(int pos)
    {
        foreach (var detectiveInfo in DetectivesInfo)
        {
            if (pos == detectiveInfo.Position)
                return true;
        }
        return false;
    }

    private bool IsSpaceOccupiedByAnotherDetective(int pos, int detectiveIndexToNotInclude)
    {
        for (int i = 0; i < DetectivesInfo.Count; i++)
        {
            if (i != detectiveIndexToNotInclude && DetectivesInfo[i].Position == pos)
                return true;
        }
        return false;
    }
    public bool IsMovePossible(Move move)
    {
        var playerInfo = GetCurrentPlayer;

        if (!WhoPlaysNow.FantomPlays && IsSpaceOccupiedByAnotherDetective(move.pos, WhoPlaysNow.DetectiveIndex))
        {
            // check if the space is already occupied
                return false;
        }
        if (playerInfo.Position == null)
            return true;
            

        var currentNode = Map.GetNodeByID(playerInfo.Position.Value);
        var newNode = Map.GetNodeByID(move.pos);

        // Can move to every non occupied space
        if (TurnCounter == 0)   
            return true;            

        // The nodes are not connected using the selected transport
        if (!currentNode.ConnectedNodes[move.tr].Contains(newNode))
            return false;

        // Has enough tokens
        if (playerInfo.Tokens[move.tr] > 0)
            return true;

        return false;
    }

    private HashSet<INode> GetAllConnectedNodes(INode node)
    {
        HashSet<INode> nodes = [];
        foreach (var tr in node.ConnectedNodes.Keys)
            nodes.UnionWith(node.ConnectedNodes[tr]);
        return nodes;
    }


    public Move RandomMoveForPlayer()
    {
        var playerInfo = GetCurrentPlayer;
        Random rnd = new();

        // If its the players first move
        if (playerInfo.Position == null)
            return new(rnd.Next(Map.Nodes.Count) + 1);

        var node = Map.GetNodeByID(playerInfo.Position.Value);

        // Select random transport
        var transportsList = node.Transports.ToList();
        var randomTransport = transportsList[rnd.Next(transportsList.Count)];

        // Selected random possible node
        var connectedNodesByRandomTransport = node.ConnectedNodes[randomTransport].Where(n => !IsSpaceOccupiedByDetectives(n.ID)).ToList();
        var randomNode = connectedNodesByRandomTransport[rnd.Next(connectedNodesByRandomTransport.Count)];

        // TODO: Can be empty
        return new(randomNode.ID, randomTransport);
    }

    public void AcceptMove(Move move)
    {
        var playerInfo = GetCurrentPlayer;
        playerInfo.MoveTo(move);
        _exactPlayerNow = (_exactPlayerNow + 1)%(1 + GameRules.NumberOfDetectives);
    }
}



