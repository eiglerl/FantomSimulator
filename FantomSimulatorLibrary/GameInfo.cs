using FantomMapLibrary;
using static FantomSimulatorLibrary.GameInfo;

namespace FantomSimulatorLibrary;

public class GameInfo
{
    public record struct WhoPlays(bool FantomPlays, int DetectiveIndex);

    public int TurnCounter;
    private int _exactPlayerNow;
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
    public IMap Map;
    public GameRules GameRules;

    public GameInfo(int turnCounter, PlayerInfo fantomTokens, List<PlayerInfo> detectivesTokens, IMap map, GameRules gameRules)
    {
        TurnCounter = turnCounter;
        FantomInfo = fantomTokens;
        DetectivesInfo = detectivesTokens;
        Map = map;
        GameRules = gameRules;
    }

    public bool IsGameOver()
    {
        // Game has already reached the max turns
        if (TurnCounter > GameRules.GameLen)
            return true;

        // Check if detectives are on the same position as the Fantom
        int fantomPosition = FantomInfo.Position;
        return IsSpaceOccupiedByDetectives(fantomPosition);
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
            

        var currentNode = Map.GetNodeByID(playerInfo.Position);
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
        HashSet<INode> nodes = new();
        foreach (var tr in node.ConnectedNodes.Keys)
            nodes.UnionWith(node.ConnectedNodes[tr]);
        return nodes;
    }


    public Move RandomMoveForPlayer()
    {
        var playerInfo = GetCurrentPlayer;
        var node = Map.GetNodeByID(playerInfo.Position);

        // Select random transport
        var transportsList = node.Transports.ToList();
        Random rnd = new();
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
    }
}



