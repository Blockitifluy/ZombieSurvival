namespace ZombieSurvival.Engine.NodeSystem;

[Serializable]
public class TreeException : Exception
{
    public TreeException() { }
    public TreeException(string message) : base(message) { }
    public TreeException(string message, Exception inner) : base(message, inner) { }
}

public sealed class Tree
{
    private static Tree? CurrentTree;

    public static Tree GetTree()
    {
        if (CurrentTree != null)
        {
            return CurrentTree;
        }

        throw new TreeException("Tree doesn't exist");
    }

    public static Tree InitaliseTree()
    {
        if (CurrentTree != null)
        {
            throw new TreeException("Tree already exists");
        }

        CurrentTree = new();
        return CurrentTree;
    }

    private readonly List<Node> Nodes = [];

    public List<Node> GetAllNodes()
    {
        return Nodes;
    }

    public bool IsNodeRegistered(Node node)
    {
        return Nodes.Contains(node);
    }

    public Guid RegisterNode(Node node)
    {
        if (IsNodeRegistered(node))
        {
            throw new TreeException("This node is already registered");
        }
        Nodes.Add(node);

        Guid id = Guid.NewGuid();
        return id;
    }

    public void UnregisterNode(Node node)
    {
        if (!IsNodeRegistered(node))
        {
            throw new TreeException("This node is not registered");
        }
        Nodes.Remove(node);
    }

    internal Tree() { }
}