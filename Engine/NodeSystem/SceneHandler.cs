using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ZombieSurvival.Engine.NodeSystem;

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
        if (obj is null)
        {
            throw new LoadingNodeException(msg);
        }
    }
    public LoadingNodeException() { }
    public LoadingNodeException(string message) : base(message) { }
    public LoadingNodeException(string message, System.Exception inner) : base(message, inner) { }
}

public static partial class SceneHandler
{
    #region Saving Scene
    private struct ExportProp
    {
        public object? PropValue;
        public bool IsResource = false;

        public string Name;
        public string ValueString;

        public Type ValueType;
        public Type NodeType;

        public string Save()
        {
            string exportValue = ValueString;

            StringBuilder builder = new();
            builder.Append($"\t{ValueType} ");

            if (IsResource) // Adds resource flag
            {
                builder.Append($"{Resource.ResourceFlag} ");
            }
            builder.Append($"{Name}={exportValue}");

            return builder.ToString();
        }

        public override readonly string ToString()
        {
            return $"ExportNode {Name}, {ValueType}";
        }

        public ExportProp(PropertyInfo info, Node node)
        {
            PropValue = info.GetValue(node);
            Type valueType = PropValue == null ? info.PropertyType : PropValue.GetType();

            if (Resource.IsSavedResource(PropValue, out string? path))
            {
                IsResource = true;
                ValueString = path!;
            }
            else
            {
                string json = JsonSerializer.Serialize(PropValue, valueType, JSONOptions);
                ValueString = json;
            }

            Name = info.Name;
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
            if (exportAttribute is null)
            {
                continue;
            }

            ExportProp exportNode = new(info, node);
            exportNodes.Add(exportNode);
        }

        return [.. exportNodes];
    }

    private static string ExportNodeToString(SaveNodeAttribute saveNode, ExportProp[] exports, uint localID, uint parentID)
    {
        StringBuilder builder = new();

        builder.AppendLine($"[{saveNode.SavedName} local-id='{localID}' parent='{parentID}']");

        foreach (ExportProp export in exports)
        {
            builder.AppendLine(export.Save());
        }
        return builder.ToString();
    }

    private static bool TryToGetParentLocalID(Node child, Dictionary<Node, uint> nodeToID, out uint id)
    {
        if (child.Parent is null)
        {
            id = 0;
            return false;
        }

        if (!nodeToID.TryGetValue(child.Parent, out var _id))
        {
            id = 0;
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

        uint i = 0;

        Dictionary<Node, uint> nodeToLocalID = [];
        foreach (Node node in nodes)
        {
            i++;
            var saveNode = node.GetType().GetCustomAttribute<SaveNodeAttribute>(false);

            if (saveNode is null)
            {
                continue;
            }

            if (!node.CanBeArchived())
            {
                continue;
            }

            bool hasParent = TryToGetParentLocalID(node, nodeToLocalID, out var parentID);

            ExportProp[] exports = GetExportPropetries(node);

            nodeToLocalID.Add(node, i);
            string exportText = ExportNodeToString(saveNode, exports, i, parentID);

            byte[] bytes = Encoding.UTF8.GetBytes(exportText);

            stream.Write(bytes);
        }

        stream.Position = 0L;
        using FileStream fileStream = new(path, FileMode.Create);
        stream.CopyTo(fileStream);

        timer.Stop();

        long msTaken = timer.ElapsedMilliseconds;
        Console.WriteLine($"Saving Scene taken {(double)msTaken / 1000} seconds");
    }
    #endregion

    #region Loading Scene

    private struct ImportProp
    {
        public required string Key;
        public required string Value;
        public required Type ValueType;
        public required bool IsResource;

        public override readonly string ToString()
        {
            return $"Import Property {ValueType} {Key}={Value}";
        }
    }

    private struct ImportNode
    {
        public required string NodeType;
        public required uint ParentID;
        public required uint LocalID;
        public List<ImportProp> Propetries = [];

        public override readonly string ToString()
        {
            return $"Import Node of {NodeType} with {Propetries.Count} propetry(s).";
        }

        public ImportNode() { }
    }

    private static ImportNode ParseImportNode(Match nodeMatch)
    {
        var groups = nodeMatch.Groups;

        string nodeType = groups[1].Value,
        local = groups[2].Value,
        parent = groups[3].Value;

        bool canParseLocal = uint.TryParse(local, out var localID),
        canParseParent = uint.TryParse(parent, out var parentID);
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

        string typeName = groups[1].Value,
        name = groups[3].Value,
        value = groups[4].Value;

        bool isResource = groups[2].Value == "r";

        Type? type = Type.GetType(typeName);
        ArgumentNullException.ThrowIfNull(type, nameof(typeName));

        ImportProp importProp = new()
        {
            Key = name,
            Value = value,
            ValueType = type,
            IsResource = isResource
        };

        return importProp;
    }

    private static List<ImportNode> ParseSceneFile(StreamReader sceneStream)
    {
        List<ImportNode> importNodes = [];

        uint i = 0;

        while (sceneStream.Peek() >= 0)
        {
            i++;
            string? line = sceneStream.ReadLine();
            if (line is null)
            {
                continue;
            }
            line = line.Trim();

            Match nodeMatch = RegexNode().Match(line),
            propMatch = RegexProp().Match(line),
            commentMatch = RegexComment().Match(line);

            try
            {
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
                else if (commentMatch.Success)
                {
                    continue;
                }
                else
                {
                    throw new MalformedSceneException($"Line {i} is not a property or node!");
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"Error thrown on line {i}");
                throw;
            }
        }

        return importNodes;
    }

    private static void SetPropertiesOfNode(List<ImportProp> importProps, Type nodeType, Node node)
    {
        foreach (ImportProp prop in importProps)
        {
            PropertyInfo? info = nodeType.GetProperty(prop.Key);
            LoadingNodeException.ThrowIfNull(info, $"Propetry {prop.Key} doesn't exist on {node}.");

            if (!info.CanWrite)
            {
                throw new LoadingNodeException($"Propetry {prop.Key} does exist but can't be set.");
            }

            object? value;
            if (prop.IsResource)
            {
                value = Resource.LoadResourceFromFile(prop.Value, prop.ValueType);
            }
            else
            {
                byte[] jsonBytes = Encoding.UTF8.GetBytes(prop.Value);
                value = JsonSerializer.Deserialize(jsonBytes, prop.ValueType, JSONOptions);
            }


            info.SetValue(node, value);
        }
    }

    private static Dictionary<uint, Node> LoadNodesFromImport(List<ImportNode> importNodes)
    {
        Dictionary<uint, Node> IDToNode = [];

        foreach (ImportNode import in importNodes)
        {
            Type nodeType = NodeNameToType[import.NodeType];
            Node? node = (Node?)Activator.CreateInstance(nodeType);
            LoadingNodeException.ThrowIfNull(node, "Node couldn't be constructed (has no parameterless contructor)");

            if (import.ParentID != 0) // Node has a parent
            {
                Node parentNode = IDToNode[import.ParentID];
                node.Parent = parentNode;
            }

            SetPropertiesOfNode(import.Propetries, nodeType, node);

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

        Dictionary<uint, Node> IDToNode = LoadNodesFromImport(importNodes);

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

    const string NodeRegex = @"^\[(.+?) local-id='(\d+?)' parent='(\d+?)'\]$";

    [GeneratedRegex(NodeRegex)]
    private static partial Regex RegexNode();

    const string PropRegex = @"^\s*([\w\d.]+?)\s+(?:(r)\s+)?([\w\d]+?)=(.+?)$";

    [GeneratedRegex(PropRegex)]
    private static partial Regex RegexProp();

    const string CommentRegex = @"^\s*#.*$";

    [GeneratedRegex(CommentRegex)]
    private static partial Regex RegexComment();
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

            if (saveNode is null)
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