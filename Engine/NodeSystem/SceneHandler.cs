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
    private const string NoNodeRefFound = "null";

    private interface IFlagHandler
    {
        public char FlagCharacter { get; }
        public PropertyFlag Flag { get; }

        public bool IsFlag(object value, out string valueStr);
        public object? HandleValue(ImportProp import);
    }

    private class ResourceFlagHandler : IFlagHandler
    {
        public char FlagCharacter => 'r';
        public PropertyFlag Flag => PropertyFlag.Resource;

        public bool IsFlag(object value, out string valueStr)
        {
            bool isResource = Resource.IsSavedResource(value, out string? path);
            valueStr = path ?? "";
            return isResource;
        }

        public object? HandleValue(ImportProp import)
        {
            return Resource.LoadResourceFromFile(import.Value, import.ValueType);
        }
    }

    private class NodeRefFlagHandler : IFlagHandler
    {
        public char FlagCharacter => 'n';
        public PropertyFlag Flag => PropertyFlag.NodeRef;

        public bool IsFlag(object export, out string valueStr)
        {
            bool isNode = export.GetType() == typeof(Node) || export.GetType().IsSubclassOf(typeof(Node));

            valueStr = "";
            return isNode;
        }

        public object? HandleValue(ImportProp import)
        {
            return null;
        }
    }

    private static readonly IFlagHandler[] FlagHandlers = [new ResourceFlagHandler(), new NodeRefFlagHandler()];

    [Flags]
    public enum PropertyFlag
    {
        None = 0,
        Resource = 1 << 1,
        NodeRef = 1 << 2
    }

    #region Saving Scene
    private struct ExportNode(Node node, uint id)
    {
        public Type NodeType = node.GetType();
        public ExportProp[] Exports = GetExportPropetries(node);
        public Node Node = node;
        public uint ID = id;

        public string Save(Dictionary<Node, uint> nodeToLocalID, uint localID)
        {
            var saveNode = Node.GetType().GetCustomAttribute<SaveNodeAttribute>(false);
            if (saveNode is null)
            {
                return "";
            }

            TryToGetParentLocalID(Node, nodeToLocalID, out var parentID);

            StringBuilder builder = new();

            builder.AppendLine($"[{saveNode.SavedName} local-id='{localID}' parent='{parentID}']");

            foreach (ExportProp export in Exports)
            {
                builder.AppendLine(export.Save());
            }
            return builder.ToString();
        }
    }

    private struct ExportProp
    {
        public object? Source;
        public PropertyFlag Flags = PropertyFlag.None;

        public string Name;
        public string Value;

        public Type ValueType;

        private readonly string FlagStr = "";

        public string Save()
        {
            string exportValue = Value;

            StringBuilder builder = new();
            builder.Append($"\t{ValueType} ");

            if (Flags != PropertyFlag.None)
            {
                builder.Append(FlagStr + " ");
            }

            builder.Append($"{Name}={exportValue}");

            return builder.ToString();
        }

        public override readonly string ToString()
        {
            return $"ExportNode {Name}, {ValueType}";
        }

#pragma warning disable CS8618
        public ExportProp(PropertyInfo info, Node node)
#pragma warning restore CS8618 
        {
            Source = info.GetValue(node);

            ValueType = Source is null ? info.PropertyType : Source.GetType();
            Name = info.Name;

            foreach (IFlagHandler handler in FlagHandlers)
            {
                if (Source is null)
                {
                    break;
                }

                if (!handler.IsFlag(Source, out string? valueStr))
                {
                    continue;
                }

                Flags |= handler.Flag;
                FlagStr += handler.FlagCharacter;
                Value = valueStr;
            }

            if (Flags == PropertyFlag.None)
            {
                string json = JsonSerializer.Serialize(Source, ValueType, JSONOptions);
                Value = json;
            }

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

    private static void CompleteNodeRefrences(Dictionary<Node, uint> nodeToLocalID, List<ExportNode> exports)
    {
        foreach (ExportNode exp in exports)
        {
            for (int i = 0; i < exp.Exports.Length; i++)
            {
                ExportProp prop = exp.Exports[i];
                if (!prop.Flags.HasFlag(PropertyFlag.NodeRef))
                {
                    continue;
                }

                if (prop.Source is not Node refNode)
                {
                    exp.Exports[i].Value = NoNodeRefFound;
                    continue;
                }

                if (!nodeToLocalID.TryGetValue(refNode, out uint id))
                {
                    Console.WriteLine($"Propetry {prop.Name} depends on Node {refNode}, however it has not been saved.");
                    continue;
                }
                exp.Exports[i].Value = id.ToString();
            }
        }
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
        List<ExportNode> exports = [];
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

            ExportNode exportNode = new(node, i);
            exports.Add(exportNode);
            nodeToLocalID.Add(node, i);
        }

        CompleteNodeRefrences(nodeToLocalID, exports);

        foreach (ExportNode export in exports)
        {
            Node node = export.Node;

            string exportText = export.Save(nodeToLocalID, export.ID);
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
        public required PropertyFlag Flags;

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

    private static PropertyFlag ParseFlags(string flagStr)
    {
        PropertyFlag flags = PropertyFlag.None;
        string[] flagSep = flagStr.Split("");

        foreach (string c in flagSep)
        {
            foreach (IFlagHandler handler in FlagHandlers)
            {
                if (handler.FlagCharacter.ToString() != c)
                {
                    continue;
                }
                flags |= handler.Flag;
            }
        }

        return flags;
    }

    private static ImportProp ParseImportProp(Match propMatch)
    {
        var groups = propMatch.Groups;

        string typeName = groups[1].Value,
        flags = groups[2].Value,
        name = groups[3].Value,
        value = groups[4].Value;

        Type? type = Type.GetType(typeName);
        ArgumentNullException.ThrowIfNull(type, nameof(typeName));

        PropertyFlag propFlags = ParseFlags(flags);

        ImportProp importProp = new()
        {
            Key = name,
            Value = value,
            ValueType = type,
            Flags = propFlags
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

            object? value = null;
            foreach (IFlagHandler handler in FlagHandlers)
            {
                if (!prop.Flags.HasFlag(handler.Flag))
                {
                    continue;
                }
                value = handler.HandleValue(prop);
            }

            if (prop.Flags == PropertyFlag.None)
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

        foreach (ImportNode import in importNodes)
        {
            Node node = IDToNode[import.LocalID];
            Type typeNode = node.GetType();

            foreach (ImportProp prop in import.Propetries)
            {
                if (!prop.Flags.HasFlag(PropertyFlag.NodeRef))
                {
                    continue;
                }

                uint id = uint.Parse(prop.Value);

                if (!IDToNode.TryGetValue(id, out Node? refNode))
                {
                    continue;
                }

                PropertyInfo? info = typeNode.GetProperty(prop.Key);
                ArgumentNullException.ThrowIfNull(info);
                info.SetValue(node, refNode);
            }
        }

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

    const string PropRegex = @"^\s*([\w\d.]+?)\s+(?:(r?n?)\s+)?([\w\d]+?)=(.+?)$";

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