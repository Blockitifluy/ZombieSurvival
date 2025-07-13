using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ZombieSurvival.Engine.NodeSystem.Scene;

[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public sealed class ExportAttribute : Attribute
{
    public ExportAttribute() { }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class SaveNodeAttribute(string savedName) : Attribute
{
    public readonly string SavedName = savedName;
}

public static partial class SceneHandler
{
    #region Saving Scene
    private struct ExportProp
    {
        public string Name;
        public string Json;
        public Type ValueType;
        public Type NodeType;

        public override readonly string ToString()
        {
            return $"ExportNode {Name}, {ValueType}";
        }

        public ExportProp(PropertyInfo info, Node node)
        {
            object? value = info.GetValue(node);
            Type valueType = info.PropertyType;

            string json = JsonSerializer.Serialize(value, JSONOptions);

            Name = info.Name;
            Json = json;
            ValueType = valueType;
            NodeType = node.GetType();
        }
    }

    private static ExportProp[] GetExportPropetries(Node node)
    {
        Type type = node.GetType();

        PropertyInfo[] properties = type.GetProperties();
        List<ExportProp> exportNodes = [];

        foreach (var info in properties)
        {
            ExportAttribute? exportAttribute = info.GetCustomAttribute<ExportAttribute>(true);
            if (exportAttribute == null)
            {
                continue;
            }

            ExportProp exportNode = new(info, node);
            exportNodes.Add(exportNode);
        }

        return [.. exportNodes];
    }

    private static string ExportNodeToString(SaveNodeAttribute saveNode, ExportProp[] exports, Guid localID, Guid parentID)
    {
        StringBuilder builder = new();

        builder.AppendLine($"[{saveNode.SavedName} local-id='{localID.ToString("N")}' parent='{parentID.ToString("N")}']");

        foreach (ExportProp export in exports)
        {
            builder.AppendLine($"\t{export.Name}={export.Json}");
        }
        return builder.ToString();
    }

    private static bool TryToGetParentLocalID(Node child, Dictionary<Node, Guid> nodeToID, out Guid id)
    {
        if (child.Parent == null)
        {
            id = Guid.Empty;
            return false;
        }

        if (!nodeToID.TryGetValue(child.Parent, out var _id))
        {
            id = Guid.Empty;
            Console.WriteLine("Child couldn't parent's ID (are you sure you sorting correctly?)");
            return false;
        }

        id = _id;
        return true;
    }

    /// <summary>
    /// Saves a scene from <paramref name="tree"/> into the <paramref name="path"/>.
    /// </summary>
    /// <param name="tree">Where the nodes is saved.</param>
    /// <param name="path">Where the scene data is saved.</param>
    public static void SaveScene(Tree tree, string path)
    {
        Stopwatch timer = new();
        timer.Start();

        using MemoryStream stream = new();

        var nodes = tree.GetAllNodes();

        nodes.Sort((x, y) => x.GetAncestors().Count - y.GetAncestors().Count);

        Dictionary<Node, Guid> nodeToLocalID = [];
        foreach (Node node in nodes)
        {
            var saveNode = node.GetType().GetCustomAttribute<SaveNodeAttribute>(false);

            if (saveNode == null)
            {
                continue;
            }

            if (!node.CanBeArchived())
            {
                continue;
            }

            bool hasParent = TryToGetParentLocalID(node, nodeToLocalID, out var parentID);

            Guid localID = Guid.NewGuid();
            ExportProp[] exports = GetExportPropetries(node);

            nodeToLocalID.Add(node, localID);
            string exportText = ExportNodeToString(saveNode, exports, localID, parentID);

            byte[] bytes = Encoding.UTF8.GetBytes(exportText);

            stream.Write(bytes);
        }

        stream.Position = 0L;
        using FileStream fileStream = new(path, FileMode.Create);
        stream.CopyTo(fileStream);

        timer.Stop();

        long msTaken = timer.ElapsedMilliseconds;
        Console.WriteLine($"Saving Scene has taken {(double)msTaken / 1000} seconds");
    }
    #endregion

    #region Loading Scene

    private struct ImportProp
    {
        public required string Key;
        public required string Value;

        public override readonly string ToString()
        {
            return $"Import Property {Key}={Value}";
        }
    }

    private struct ImportNode
    {
        public required string NodeType;
        public required Guid ParentID;
        public required Guid LocalID;
        public List<ImportProp> Propetries = [];

        public override readonly string ToString()
        {
            return $"Import Node of {NodeType} with {Propetries.Count} propetry(s).";
        }

        public ImportNode() { }
    }

    [Serializable]
    public class MalformedSceneException : Exception
    {
        public MalformedSceneException() { }
        public MalformedSceneException(string message) : base(message) { }
        public MalformedSceneException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class LoadingNodeException : Exception
    {
        public static void ThrowIfNull([NotNull] object? obj, string msg)
        {
            if (obj == null)
            {
                throw new LoadingNodeException(msg);
            }
        }
        public LoadingNodeException() { }
        public LoadingNodeException(string message) : base(message) { }
        public LoadingNodeException(string message, System.Exception inner) : base(message, inner) { }
    }

    private static ImportNode ParseImportNode(Match nodeMatch)
    {
        var groups = nodeMatch.Groups;

        string nodeType = groups[1].Value,
        local = groups[2].Value,
        parent = groups[3].Value;

        bool canParseLocal = Guid.TryParse(local, out var localID),
        canParseParent = Guid.TryParse(parent, out var parentID);
        if (!canParseLocal || !canParseParent)
        {
            throw new MalformedSceneException("Couldn't parse either (or both) local or parent!");
        }

        ImportNode importNode = new()
        {
            NodeType = nodeType,
            LocalID = localID,
            ParentID = parentID
        };

        return importNode;
    }

    private static ImportProp ParseImportProp(Match propMatch)
    {
        var groups = propMatch.Groups;

        string name = groups[1].Value,
        value = groups[2].Value;

        ImportProp importProp = new()
        {
            Key = name,
            Value = value
        };

        return importProp;
    }

    private static List<ImportNode> ParseSceneFile(StreamReader sceneStream)
    {
        List<ImportNode> importNodes = [];

        while (sceneStream.Peek() >= 0)
        {
            string? line = sceneStream.ReadLine();
            if (line == null)
            {
                continue;
            }
            line = line.Trim();

            Match nodeMatch = RegexNode().Match(line),
            propMatch = RegexProp().Match(line);

            if (nodeMatch.Success)
            {
                ImportNode importNode = ParseImportNode(nodeMatch);

                importNodes.Add(importNode);
            }
            else if (propMatch.Success)
            {
                if (importNodes.Count <= 0)
                {
                    throw new MalformedSceneException("Property appeared before first node");
                }
                ImportProp importProp = ParseImportProp(propMatch);

                importNodes[^1].Propetries.Add(importProp);
            }
            else
            {
                throw new MalformedSceneException("Line is not a property or node!");
            }
        }

        return importNodes;
    }

    private static Dictionary<Guid, Node> LoadNodesFromImport(List<ImportNode> importNodes)
    {
        Dictionary<Guid, Node> IDToNode = [];

        foreach (ImportNode import in importNodes)
        {
            Type nodeType = NodeNameToType[import.NodeType];
            Node? node = (Node?)Activator.CreateInstance(nodeType);
            LoadingNodeException.ThrowIfNull(node, "Node couldn't be constructed (has no parameterless contructor)");

            if (import.ParentID != Guid.Empty) // Node has a parent
            {
                Node parentNode = IDToNode[import.ParentID];
                node.Parent = parentNode;
            }

            foreach (ImportProp prop in import.Propetries)
            {
                PropertyInfo? info = nodeType.GetProperty(prop.Key);
                LoadingNodeException.ThrowIfNull(info, $"Propetry {prop.Key} doesn't exist on {node}.");

                if (!info.CanWrite)
                {
                    throw new LoadingNodeException($"Propetry {prop.Key} does exist but can't be set.");
                }

                byte[] jsonBytes = Encoding.UTF8.GetBytes(prop.Value);

                object? value = JsonSerializer.Deserialize(jsonBytes, info.PropertyType, JSONOptions);

                info.SetValue(node, value);
            }

            IDToNode.Add(import.LocalID, node);
        }

        return IDToNode;
    }

    /// <summary>
    /// Loads a scene from <paramref name="path"/> into the selected <paramref name="tree"/>.
    /// </summary>
    /// <param name="tree">Where the nodes are loaded.</param>
    /// <param name="path">Where the scene data is loaded.</param>
    public static void LoadScene(Tree tree, string path)
    {
        Stopwatch timer = new();
        timer.Start();

        using StreamReader sceneStream = new(path);

        List<ImportNode> importNodes = ParseSceneFile(sceneStream);

        Dictionary<Guid, Node> IDToNode = LoadNodesFromImport(importNodes);

        foreach (Node node in IDToNode.Values)
        {
            node.Awake();
            node._ID = tree.RegisterNode(node);
            node.Start();
        }

        timer.Stop();

        long msTaken = timer.ElapsedMilliseconds;
        Console.WriteLine($"Loading Scene taken {(double)msTaken / 1000} seconds");
    }

    const string NodeRegex = @"^\[(.+?) local-id='([\w\d]{32})' parent='([\w\d]{32})'\]$";

    [GeneratedRegex(NodeRegex)]
    private static partial Regex RegexNode();

    const string PropRegex = @"^\s*([\w\d]+?)=(.+?)$";

    [GeneratedRegex(PropRegex)]
    private static partial Regex RegexProp();
    #endregion

    private static readonly Dictionary<string, Type> NodeNameToType = [];

    private static readonly JsonSerializerOptions JSONOptions = new()
    {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    static SceneHandler()
    {
        Assembly assem = typeof(SceneHandler).Assembly;

        Type baseNodeType = typeof(Node);

        foreach (Type type in assem.GetTypes())
        {
            var saveNode = type.GetCustomAttribute<SaveNodeAttribute>();

            if (saveNode == null)
            {
                continue;
            }
            if (!type.IsSubclassOf(baseNodeType) && type != baseNodeType)
            {
                Console.WriteLine($"{type.FullName} doesn't inherit from Node but has the SaveNode Attribute");
                continue;
            }

            NodeNameToType.Add(saveNode.SavedName, type);
        }
    }
}