using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class BaseTree : Composite
{
    public BaseTree(TilesManager manager) : base(null, manager) { }

    public override Node Interpret()
    {
        Room rm = new WholeLevel(4, 20, 20, this, manager, null, 0);
        Room start = new Room(1, 5, 5, this, manager, rm, UnityEngine.Random.Range(0,3));
        start.SetColor(Color.blue);
        Room end = new Room(1, 5, 5, this, manager, rm, UnityEngine.Random.Range(0,3));
        end.SetColor(Color.red);
        return GetNextNode();
    }
}

#region Room

public class WholeLevel : Room
{
    public override Node Interpret()
    {
        Build();
        for (int i = 0; i < 3; i++)
        {
            new Branch(this, manager, this);
        }
        return GetNextNode();
    }

    public WholeLevel(int plug, int width, int height, Composite parent, TilesManager manager, Room rm, int offset) : base(plug, width, height,parent, manager, rm, offset) { }
}

public class Branch : Composite
{
    Room plugRoom;
    public override Node Interpret()
    {
        Room corr = new Corridor(Mathf.Max(4, 7 - depth), UnityEngine.Random.Range(5,25), this, manager, plugRoom, UnityEngine.Random.Range(-2, 2));
        Room end = new Room(3, 9, 9,this, manager, corr, UnityEngine.Random.Range(-2,2));
        Color tempCol = new Color(1, 1.0f - (depth / (float)TilesManager.MaxDepth), 1.0f - (depth / (float)TilesManager.MaxDepth), 1);
        Debug.Log(tempCol);
        corr.SetColor(tempCol);
        end.SetColor(tempCol);

        int number = UnityEngine.Random.value > 0.5f ? 2 : 3;
        for (int i = 0; i < number; i++)
        {
            if (ThrowProba())
            {
                new Branch(this, manager, end);
            }
        }

        return GetNextNode();
    }

    bool ThrowProba()
    {
        return (UnityEngine.Random.value > (depth / TilesManager.MaxDepth));
    }

    public Branch(Composite parent , TilesManager manager , Room rm) : base (parent , manager)
    {
        plugRoom = rm;
    }
}

public class Room : Composite
{
    protected int nbPlug;
    protected int width;
    protected int height;
    protected Room plugRoom;
    protected int offset;
    protected Color col = Color.white;

    protected Chunk linkedChunk;
    public Chunk LinkedChunk
    {
        get
        {
            return linkedChunk;
        }
        set
        {
            linkedChunk = value;
        }
    }

    protected int linkedOrientation;
    public int LinkedOrientation
    {
        get
        {
            return linkedOrientation;
        }
        set
        {
            linkedOrientation = value;
        }
    }

    public Room(int plug, int width, int height, Composite parent , TilesManager manager, Room rm, int offset) : base(parent , manager)
    {
        nbPlug = plug;
        this.width = width;
        this.height = height;
        this.offset = offset;
        plugRoom = rm;
    }

    public void SetColor(Color col)
    {
        this.col = col;
    }

    public override Node Interpret()
    {
        Build();
        return GetNextNode();
    }

    protected void Build()
    {
        if (plugRoom != null)
        {
            bool[] allConn = manager.GetOrientation(plugRoom.LinkedOrientation, nbPlug);
            Tuple<Vector2Int, Vector2Int> output = manager.GetRoom(plugRoom.LinkedChunk, offset, width, height, ref linkedOrientation);
            linkedChunk = manager.BuildRoom(output.Item1, (output.Item2 - output.Item1) + new Vector2Int(0, 1), allConn[3], allConn[1], allConn[0], allConn[2]);
        }
        else
        {
            bool[] allConn = manager.GetOrientation(UnityEngine.Random.Range(0, 3), nbPlug);
            for (int i = 0; i < 4; i++)
            {
                if (!allConn[i])
                {
                    linkedOrientation = i;
                    break;
                }
            }
            linkedChunk = manager.BuildRoom(new Vector2Int(), new Vector2Int(width, height), allConn[3], allConn[1], allConn[0], allConn[2]);
        }
        linkedChunk.SetColor(col);
    }
}

//Special room where plug are always on opposites
public class Corridor : Room
{
    public Corridor(int width, int height, Composite parent, TilesManager manager, Room plugRoom, int offset): base(2, width, height , parent , manager , plugRoom, offset)
    { 
    }

    public override Node Interpret()
    {
        Tuple<Vector2Int, Vector2Int> output = manager.GetRoom(plugRoom.LinkedChunk, 0, width, height, ref linkedOrientation);
        bool[] allConn = manager.GetOrientation(linkedOrientation, 1);
        allConn[(linkedOrientation + 2) % 4] = false;
        linkedChunk = manager.BuildRoom(output.Item1, (output.Item2 - output.Item1) + new Vector2Int(0, 1), allConn[3], allConn[1], allConn[0], allConn[2]);
        linkedChunk.SetColor(col);
        return GetNextNode();
    }
}

#endregion

#region RoomModifier

public class RoomModifier : Leaf
{
    protected Room target;

    public RoomModifier(Composite parent, TilesManager manager, Room target) : base(parent, manager) { }

    public override Node Interpret()
    {
        return GetNextNode();
    }
}

public class ExcavationModifier : RoomModifier
{
    Vector2Int offset;
    Vector2Int size;

    public ExcavationModifier(Composite parent, TilesManager manager, Room target, Vector2Int offset , Vector2Int size) : base(parent, manager, target)
    {
        this.offset = offset;
        this.size = size;
    }

    public override Node GetNextNode()
    {
        target.LinkedChunk.Excavate(offset, offset + size);
        return base.Interpret();
    }
}

#endregion

