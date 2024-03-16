using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using FantomMapLibrary;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Numerics;

namespace ActualSimulator;

public class MapLoaderFromJson
{
    [System.Serializable]
    public struct NodeDataHolder
    {
        //public int ID;
        public List<Transport> Transports; // from hashset
        public StringPosition Position;
        //public Dictionary<Transport, List<int>> ConnectedTo;
        public EnumDictionary ConnectedTo;
    }
    [Serializable]
    public struct StringPosition
    {
        public string x, y, z;
    }
    [System.Serializable]
    public struct EnumIntPair
    {
        public int Key;
        public List<int> Values;
    }
    [System.Serializable]
    public struct EnumDictionary
    {
        public List<EnumIntPair> Dict;
    }

    public static Map LoadFromFile(string path)
    {
        try
        {
            string jsonString = File.ReadAllText(path);
            return LoadFromJson(jsonString);
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("File not found. Please provide the correct file path.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        return null;
    }
        
    public static Map LoadFromJson(string json)
    {
        var nodesData = JsonConvert.DeserializeObject<Dictionary<string, List<NodeDataHolder>>>(json);
        var nodes = NodeDataHolderToNode(nodesData["data"]);
        Map map = new(nodes);
        return map;
    }

    private static List<Node> NodeDataHolderToNode(List<NodeDataHolder> nodeDataHolders)
    {
        List<Node> nodes = [];
        int id = 1;

        foreach (var data in nodeDataHolders)
        {
            HashSet<Transport> transports = new(data.Transports);
            Dictionary<Transport, HashSet<INode>> connected = [];
            foreach (var tr in transports)
                connected[tr] = [];
            Node node = new(id, transports, connected);
            nodes.Add(node);
            id++;
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            var connectedNodes = nodeDataHolders[i].ConnectedTo;

            foreach (var item in connectedNodes.Dict)
            {
                if (item.Values.Count == 0)
                    continue;
                Transport tr = (Transport)item.Key;
                HashSet<INode> idConnectedNodes = new HashSet<INode>(item.Values.Select(x => nodes[x - 1]));
                node.ConnectedNodes[tr] = new(idConnectedNodes);
            }
        }
        return nodes;
    }
}
