using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;

public abstract class Node
{
    protected TilesManager manager;
    protected int depth = 0;
    public int Depth => depth;
    protected Composite parent;
    public Composite Parent => parent;
    public abstract Node GetNextNode();

    /// <summary>
    /// Create chunk (or anything else) and return the next node to interpret
    /// </summary>
    /// <returns></returns>
    public abstract Node Interpret();
}

public abstract class Leaf : Node
{
    public Leaf(Composite parent, TilesManager manager)
    {
        this.parent = parent;
        this.manager = manager;
        if (this.parent != null)
        {
            depth = parent.Depth + 1;
            parent.Childs.Add(this);
        }
        else
            depth = 0;
    }

    public override Node GetNextNode()
    {
        return parent.GetNextNode();
    }
}

public abstract class Composite : Node
{
    protected List<Node> childs = new List<Node>();
    public List<Node> Childs => childs;
    protected int cursorIndex;

    public Composite(Composite parent, TilesManager manager)
    {
        this.parent = parent;
        this.manager = manager;
        
        if (parent != null)
        {
            parent.Childs.Add(this);
            depth = parent.Depth + 1;
        }
        else
            depth = 0;
    }

    public override Node GetNextNode()
    {
        if (cursorIndex < childs.Count)
        {
            Node nd = childs[cursorIndex];
            cursorIndex++;
            return nd;
        }
        else
        {
            if (parent != null)
                return parent.GetNextNode();
            else
                return null;
        }
    }
}
