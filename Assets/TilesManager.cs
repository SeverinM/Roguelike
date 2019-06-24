using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;

public class TilesManager : MonoBehaviour
{
    public static int MaxDepth = 5;

    [SerializeField]
    DatabaseSprite db;

    [SerializeField]
    Vector2Int bottomLeft;

    List<Chunk> allChunks = new List<Chunk>();

    delegate Tuple<Vector2Int, Vector2Int> TupleParam();

    public Text txt;

    public void Build()
    {
        StartBuild();
    }

    public void SetValue(Slider slider)
    {
        MaxDepth = (int)slider.value;
        txt.text = MaxDepth.ToString();
    }

    public void StartBuild()
    {
        Composite firstParent = new BaseTree(this);
        Node nd = firstParent;
        while (nd != null)
        {
            nd = nd.Interpret();
        }

        foreach (Chunk ch in allChunks)
        {
            ch.BuildWalls();
        }

        foreach (Chunk ch in allChunks)
        {
            ch.BuildWalls();
        }

        foreach (Chunk ch in allChunks)
        {
            ch.UpdateVisual();
        }
    }

    public Chunk BuildRoom(Vector2Int bottomLeft , Vector2Int size, bool bL = true, bool bR = true, bool bU = true , bool bD = true)
    {
        Chunk ch = new Chunk(bottomLeft, bottomLeft + size, db, this,bL,bR,bU,bD);
        ch.BuildFloor();
        allChunks.Add(ch);
        return ch;
    }

    public void Remove(Vector2Int position)
    {
        foreach(Chunk ch in allChunks)
        {
            if (ch.GetType(position) != DatabaseSprite.TypeTile.NONE)
            {
                ch.Remove(position);
                break;
            }
        }
    }

    public DatabaseSprite.TypeTile GetType(Vector2Int position)
    {
        Chunk owner = GetOwnership(position);
        if (owner)
            return owner.GetType(position);
        else
            return DatabaseSprite.TypeTile.NONE;
    }

    public Chunk GetOwnership(Vector2Int position)
    {
        Chunk ch = null;
        foreach(Chunk chunk in allChunks)
        {
            if (chunk.GetType(position) != DatabaseSprite.TypeTile.NONE)
            {
                ch = chunk;
                break;
            }
        }
        return ch;
    }

    //Allow to claim a specific square on the map to build a room
    public Tuple<Vector2Int,Vector2Int> GetRoom(Chunk ch, int off, int widthBuild , int max, ref int orient)
    {
        int height = ch.TopRight.y - ch.BottomLeft.y;
        int width = ch.TopRight.x - ch.BottomLeft.x;

        List<TupleParam> allTests = new List<TupleParam>();

        //Up
        allTests.Add(() =>
        {
            Tuple<Vector2Int, Vector2Int> output = GetCorners(ch.BottomLeft + new Vector2Int(off, height), false, false, widthBuild, max);
            if (output.Item2.y - output.Item1.y > 4)
            {
                Debug.Log("up");
                return output;
            }
            return DatabaseSprite.noCorner;
        });

        //Right
        allTests.Add(() =>
        {
            Tuple<Vector2Int, Vector2Int> output = GetCorners(ch.BottomLeft + new Vector2Int(width, off), false, true, widthBuild, max);
            if (output.Item2.x - output.Item1.x > 4)
            {
                Debug.Log("droite");
                return output;
            }
            return DatabaseSprite.noCorner;
        });

        //Down
        allTests.Add(() =>
        {
            Tuple<Vector2Int, Vector2Int> output = GetCorners(ch.BottomLeft + new Vector2Int(off, -1), true, false, widthBuild, max);
            if (output.Item2.y - output.Item1.y > 4)
            {
                output = new Tuple<Vector2Int, Vector2Int>(output.Item1 + Vector2Int.up, output.Item2 + Vector2Int.up);
                Debug.Log("bas");
                return output;
            }
            return DatabaseSprite.noCorner;
        });

        //left
        allTests.Add(() =>
        {
            Tuple<Vector2Int, Vector2Int> output = GetCorners(ch.BottomLeft + new Vector2Int(-1, off), true, true, widthBuild, max);
            if (output.Item2.x - output.Item1.x > 4)
            {
                output = new Tuple<Vector2Int, Vector2Int>(output.Item1 + Vector2Int.right, output.Item2 + Vector2Int.right);
                Debug.Log("left");
                return output;
            }
            return DatabaseSprite.noCorner;
        });

        //All test is done randomly
        List<int> allFunctions = new List<int> { 0, 1, 2, 3 };
        allFunctions = allFunctions.OrderBy(x => UnityEngine.Random.Range(0, 100)).ToList();

        //At the second time , try to bypass
        for (int i = 0; i < 2; i++)
        {
            for (int index = 0; index < 4; index++)
            {
                int orientation = allFunctions[index];
                if (!ch.GetIsBuild(orientation) || i > 0)
                {
                    Tuple<Vector2Int, Vector2Int> output = allTests[orientation]();
                    if (output != DatabaseSprite.noCorner)
                    {
                        orient = orientation;
                        return output;
                    }
                }
            }
        }
       
        return DatabaseSprite.noCorner;
    }

    Tuple<Vector2Int, Vector2Int> GetCorners(Vector2Int position,bool reversed, bool horizontale , int width, int limit)
    {
        int count = 0;
        //Commence toujours a gauche
        if (!horizontale)
        {
            for (int y = 0; y < limit; y++)
            {
                for (int x = position.x; x < position.x + width; x++, count++)
                {
                    //Obstacle trouvé , on arrete
                    Vector2Int tempPos = new Vector2Int(x, position.y + (reversed ? -y : y));
                    if (GetType(tempPos) != DatabaseSprite.TypeTile.NONE)
                    {
                        if (!reversed)
                            return new Tuple<Vector2Int, Vector2Int>(position, new Vector2Int(position.x + width, tempPos.y + 1));
                        else
                            return new Tuple<Vector2Int, Vector2Int>(new Vector2Int(position.x, tempPos.y - 1), new Vector2Int(position.x + width, position.y));
                    }
                }
            }

            Vector2Int btnLeft = new Vector2Int(position.x, position.y + (reversed ? -limit: 0));
            Vector2Int tpRight = new Vector2Int(position.x + width, position.y + (reversed ? 0 : limit));
            return new Tuple<Vector2Int, Vector2Int>(btnLeft, tpRight);
        }
       
        //Commence toujours en bas
        else
        {
            count = 0;
            for (int x = 0; x < limit; x++)
            {
                for (int y = position.y; y < position.y + width; y++ , count++)
                {
                    Vector2Int tempPos = new Vector2Int(position.x + (reversed ? -x : x), y);
                    //Obstacle trouvé , on arrete
                    if (GetType(tempPos) != DatabaseSprite.TypeTile.NONE)
                    {
                        if (!reversed)
                            return new Tuple<Vector2Int, Vector2Int>(position + new Vector2Int(-2, 0), new Vector2Int(tempPos.x - 1, position.y + width));
                        else
                        {
                            return new Tuple<Vector2Int, Vector2Int>(new Vector2Int(tempPos.x + 1, position.y + width), new Vector2Int(position.x + 2, position.y + width));

                        }
                    }
                }
            }

            return new Tuple<Vector2Int, Vector2Int>(new Vector2Int(position.x + (reversed ? -limit: 0), position.y),
                new Vector2Int(position.x + (reversed ? 0 : limit), position.y + width));
        }
    }

    /// <summary>
    /// 0 = haut
    /// 1 = droite
    /// 2 = bas 
    /// 3 = gauche
    /// </summary>
    /// <param name="orientationCame"></param>
    /// <param name="number"></param>
    /// <returns></returns>
    public bool[] GetOrientation(int orientationCame, int number = 0)
    {
        List<bool> allBools = new List<bool> { true, true, true, true};
        for (int i = 0; i < Mathf.Clamp(number, 1, 4); i++)
        {
            allBools[i] = false;
        }
        allBools = allBools.OrderBy(x => UnityEngine.Random.Range(0, 100)).ToList();
        //On "decale" la liste
        while (allBools[orientationCame])
        {
            allBools.Add(false);
            allBools[allBools.Count - 1] = allBools[0];
            allBools.RemoveAt(0);
        }

        return allBools.ToArray();
    }
}
