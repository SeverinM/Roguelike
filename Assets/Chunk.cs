using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Chunk
{
    Vector2Int bottomLeft;
    public Vector2Int BottomLeft => bottomLeft;

    Vector2Int topRight;
    public Vector2Int TopRight => topRight;

    DatabaseSprite db;
    TilesManager tileMan;
    Color col = Color.white;

    public string Name { get; set; }

    bool buildLeft;
    public bool BuildLeft
    {
        get
        {
            return buildLeft;
        }
        set
        {
            buildLeft = value;
        }
    }

    bool buildRight;
    public bool BuildRight
    {
        get
        {
            return buildRight;
        }

        set
        {
            buildRight = value;
        }
    }

    bool buildUp;
    public bool BuildUp
    {
        get
        {
            return buildUp;
        }
        set
        {
            buildUp = value;
        }
    }

    bool buildDown;
    public bool BuildDown
    {
        get
        {
            return buildDown;
        }
        set
        {
            buildDown = value;
        }
    }

    public int Height => TopRight.y - BottomLeft.y;
    public int Width => TopRight.x - BottomLeft.x;

    Dictionary<Vector2Int, Tuple<GameObject, DatabaseSprite.TypeTile>> relativePosition = new Dictionary<Vector2Int, Tuple<GameObject, DatabaseSprite.TypeTile>>();

    public Chunk(Vector2Int btnLeft, Vector2Int tpRight, DatabaseSprite database, TilesManager tilesMan, bool bL = true, bool bR = true, bool bU = true, bool bD = true)
    {
        bottomLeft = btnLeft;
        topRight = tpRight;
        db = database;
        tileMan = tilesMan;

        buildLeft = bL;
        buildRight = bR;
        buildUp = bU;
        buildDown = bD;
    }

    public void SetColor(Color col)
    {
        foreach(Tuple<GameObject, DatabaseSprite.TypeTile> gob in relativePosition.Values)
        {
            gob.Item1.GetComponent<SpriteRenderer>().color = col;
        }
    }

    public DatabaseSprite.TypeTile GetType(Vector2Int coord)
    {
        if (!relativePosition.ContainsKey(coord - bottomLeft))
            return DatabaseSprite.TypeTile.NONE;
        else
            return relativePosition[coord - bottomLeft].Item2;
    }

    public static implicit operator bool(Chunk ch)
    {
        return (ch != null);
    }

    public void BuildFloor()
    {
        int width = (int)Mathf.Abs(bottomLeft.x - topRight.x);
        int height = (int)Mathf.Abs(bottomLeft.y - topRight.y);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //en haut
                Sprite sprt = db.floor.allConnected;

                if (!tileMan.GetOwnership(bottomLeft + new Vector2Int(x, y)))
                {
                    GameObject gob = new GameObject();
                    gob.AddComponent<SpriteRenderer>();
                    gob.transform.position = db.getTruePosition(bottomLeft + new Vector2Int(x, y));
                    relativePosition[new Vector2Int(x, y)] = new Tuple<GameObject, DatabaseSprite.TypeTile>(gob, DatabaseSprite.TypeTile.FLOOR);
                }               
            }
        }
    }

    public void Remove(Vector2Int position)
    {
        if (relativePosition.ContainsKey(position - bottomLeft))
        {
            GameObject.Destroy(relativePosition[position - bottomLeft].Item1);
            relativePosition.Remove(position - bottomLeft);
        }
    }

    public List<Vector2Int> GetCollisions(Chunk otherChunk)
    {
        List<Vector2Int> output = new List<Vector2Int>();
        int width = topRight.x - bottomLeft.x;
        int height = topRight.y - bottomLeft.y;
        for (int x = 0; x < width; x ++)
        {
            for (int y = 0; y < height; y++)
            {
                if (otherChunk.isInside(bottomLeft + new Vector2Int(x, y)))
                    output.Add(new Vector2Int(x, y));
            }
        }
        return output;
    }

    public bool isInside(Vector2Int pos)
    {
        return (pos.x >= bottomLeft.x && pos.x <= topRight.x && pos.y >= bottomLeft.y && pos.y <= topRight.y);
    }

    public void BuildWalls()
    {
        for (int x = 0; x < Width; x++)
        {
            Vector2Int above = new Vector2Int(x, Height);
            Vector2Int below = new Vector2Int(x, -1);

            if (tileMan.GetType(bottomLeft + above) != DatabaseSprite.TypeTile.FLOOR)
            {
                TrySetType(new Vector2Int(x, Height - 1), DatabaseSprite.TypeTile.WALL);
            }

            if (tileMan.GetType(bottomLeft + below) != DatabaseSprite.TypeTile.FLOOR)
            {
                TrySetType(new Vector2Int(x, 0), DatabaseSprite.TypeTile.WALL);
            }
        }

        for (int y = 0; y < Height; y++)
        {
            Vector2Int left = new Vector2Int(-1, y);
            Vector2Int right = new Vector2Int(Width, y);

            if (tileMan.GetType(bottomLeft + left) != DatabaseSprite.TypeTile.FLOOR)
            {
                TrySetType(new Vector2Int(0, y), DatabaseSprite.TypeTile.WALL);
            }

            if (tileMan.GetType(bottomLeft + right) != DatabaseSprite.TypeTile.FLOOR)
            {
                TrySetType(new Vector2Int(Width - 1, y), DatabaseSprite.TypeTile.WALL);
            }
        }

        TrySetType(new Vector2Int(-1, Height), DatabaseSprite.TypeTile.WALL);
    }

    public void UpdateVisual()
    {
        foreach(Vector2Int pos in relativePosition.Keys)
        {
            relativePosition[pos].Item1.GetComponent<SpriteRenderer>().sprite = getSprite(relativePosition[pos].Item2, pos);
        }
    }

    public void Excavate(Vector2Int btnLeft, Vector2Int tpRight)
    {
        int width = Mathf.Abs(tpRight.x - btnLeft.x);
        int height = Mathf.Abs(tpRight.y - btnLeft.y);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int coords = btnLeft + new Vector2Int(x, y);
                TrySetType(coords, DatabaseSprite.TypeTile.NONE);

                //All floors around the hole become walls
                if (y == 0)
                {
                    TrySetType(coords - new Vector2Int(0, 1), DatabaseSprite.TypeTile.WALL);
                }

                if (y == height - 1)
                {
                    TrySetType(coords + new Vector2Int(0, 1), DatabaseSprite.TypeTile.WALL);
                }

                if (x == 0)
                {
                    TrySetType(coords - new Vector2Int(1, 0), DatabaseSprite.TypeTile.WALL);
                }

                if (x == width - 1)
                {
                    TrySetType(coords + new Vector2Int(1, 0), DatabaseSprite.TypeTile.WALL);
                }
            }
        }

        TrySetType(btnLeft - new Vector2Int(1, 1), DatabaseSprite.TypeTile.WALL);
        TrySetType(btnLeft + new Vector2Int(-1, height), DatabaseSprite.TypeTile.WALL);
        TrySetType(btnLeft + new Vector2Int(width, -1), DatabaseSprite.TypeTile.WALL);
        TrySetType(btnLeft + new Vector2Int(width, height), DatabaseSprite.TypeTile.WALL);
    }

    public void TrySetType(Vector2Int pos , DatabaseSprite.TypeTile type)
    {
        if (relativePosition.ContainsKey(pos))
        {
            relativePosition[pos] = new Tuple<GameObject, DatabaseSprite.TypeTile>(relativePosition[pos].Item1, type);
        }
    }

    Sprite getSprite(DatabaseSprite.TypeTile type , Vector2Int pos)
    {
        pos += bottomLeft;
        DatabaseSprite.TypeTile xP = tileMan.GetType(pos + new Vector2Int(1, 0));
        DatabaseSprite.TypeTile xM = tileMan.GetType(pos + new Vector2Int(-1, 0));
        DatabaseSprite.TypeTile yP = tileMan.GetType(pos + new Vector2Int(0, 1));
        DatabaseSprite.TypeTile yM = tileMan.GetType(pos + new Vector2Int(0, -1));

        int number = 0;
        if (type == DatabaseSprite.TypeTile.FLOOR)
        {
            if (xP == DatabaseSprite.TypeTile.FLOOR) number += 0b0001;
            if (yM == DatabaseSprite.TypeTile.FLOOR) number += 0b0010;
            if (xM == DatabaseSprite.TypeTile.FLOOR) number += 0b0100;
            if (yP == DatabaseSprite.TypeTile.FLOOR) number += 0b1000;
            return db.floor.GetSprite(number);
        }
        if (type == DatabaseSprite.TypeTile.WALL)
        {
            if (xP == DatabaseSprite.TypeTile.WALL) number += 0b0001;
            if (yM == DatabaseSprite.TypeTile.WALL) number += 0b0010;
            if (xM == DatabaseSprite.TypeTile.WALL) number += 0b0100;
            if (yP == DatabaseSprite.TypeTile.WALL) number += 0b1000;
            return db.wall.GetSprite(number);
        }
        return null;
    }

    public bool GetIsBuild(int orientation)
    {
        switch (orientation)
        {
            case 0:
                return buildUp;
            case 1:
                return buildRight;
            case 2:
                return buildDown;
            case 3:
                return buildLeft;
            default:
                return buildUp;
        }
    }
}
