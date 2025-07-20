namespace ZombieSurvival.Engine.NodeSystem;

[Serializable]
public class TreeException : Exception
{
    public TreeException() { }
    public TreeException(string message) : base(message) { }
    public TreeException(string message, Exception inner) : base(message, inner) { }
}

public sealed class Tree : IDisposable
{
    private static Tree? CurrentTree;

    /// <summary>
    /// Gets the current tree.
    /// </summary>
    /// <returns>The current tree</returns>
    /// <exception cref="TreeException">Fired when the Tree has not been initised.</exception>
    public static Tree GetTree()
    {
        if (CurrentTree is not null)
        {
            return CurrentTree;
        }

        throw new TreeException("Tree doesn't exist");
    }

    /// <summary>
    /// Creates a new Tree.
    /// </summary>
    /// <returns>The tree just created.</returns>
    /// <exception cref="TreeException">Fired when Tree already exists.</exception>
    public static Tree InitaliseTree()
    {
        if (CurrentTree is not null)
        {
            throw new TreeException("Tree already exists");
        }

        CurrentTree = new();
        return CurrentTree;
    }

    private readonly List<Node> Nodes = [];

    /// <summary>
    /// Gets all node registered in the Tree.
    /// </summary>
    /// <returns></returns>
    public List<Node> GetAllNodes()
    {
        return [.. Nodes];
    }

    /// <summary>
    /// Is this node registered.
    /// </summary>
    /// <param name="node">The node being checked.</param>
    /// <returns>True, if registered.</returns>
    public bool IsNodeRegistered(Node node)
    {
        return Nodes.Contains(node);
    }

    /// <summary>
    /// Registers a node.
    /// </summary>
    /// <remarks>
    /// Registering means that a node can be updated.
    /// </remarks>
    /// <param name="node">The node being registed.</param>
    /// <returns>The ID to be assigned to the node. Not set by function.</returns>
    /// <exception cref="TreeException">This node is already registered.</exception>
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

    /// <summary>
    /// Unregisters a node.
    /// </summary>
    /// <param name="node">The node being unregisted.</param>
    /// <exception cref="TreeException">This node is already unregistered.</exception>
    public void UnregisterNode(Node node)
    {
        if (!IsNodeRegistered(node))
        {
            throw new TreeException("This node is not registered");
        }
        Nodes.Remove(node);
    }

    public void UpdateAllNodes(double delta)
    {
        var nodes = GetAllNodes();

        foreach (Node node in nodes)
        {
            try
            {
                node.Update(delta);
            }
            catch (Exception err)
            {
                Console.WriteLine($"Uncaught error in {node}\n{err}");
            }
        }
    }

    // In milliseconds
    public const int FixedUpdateTime = 50;

    public static double FixedUpdateSeconds => (double)50 / 1000;

    public void UpdateAllNodesFixed(object? state)
    {
        var nodes = GetAllNodes();


        foreach (Node node in nodes)
        {
            try
            {
                node.UpdateFixed();
            }
            catch (Exception err)
            {
                Console.WriteLine($"Uncaught error in {node}\n{err}");
            }
        }
    }

    private readonly Timer FixedUpdateTimer;

    void IDisposable.Dispose()
    {
        FixedUpdateTimer.Dispose();
    }

    internal Tree()
    {
        FixedUpdateTimer = new(UpdateAllNodesFixed, null, 0, FixedUpdateTime);
    }
}