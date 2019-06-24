using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

public class DatabaseSprite : MonoBehaviour{
    [System.Serializable]
    public class Tiles
    {
        public Sprite topLeft;
        public Sprite leftSide;
        public Sprite rightSide;
        public Sprite topRight;
        public Sprite upSide;
        public Sprite downSide;
        public Sprite bottomRight;
        public Sprite bottomLeft;
        public Sprite allConnected;
        public Sprite linkDown;
        public Sprite linkUpDown;
        public Sprite linkUp;
        public Sprite linkRight;
        public Sprite linkLeft;
        public Sprite linkRightLeft;
        public Sprite neutral;

        public Sprite GetSprite(int nb)
        {
            switch (nb)
            {
                case 0:
                    return neutral;
                case 1:
                    return linkRight;
                case 2:
                    return linkDown;
                case 3:
                    return topLeft;
                case 4:
                    return leftSide;
                case 5:
                    return linkRightLeft;
                case 6:
                    return topRight;
                case 7:
                    return upSide;
                case 8:
                    return linkUp;
                case 9:
                    return bottomLeft;
                case 10:
                    return linkUpDown;
                case 11:
                    return leftSide;
                case 12:
                    return bottomRight;
                case 13:
                    return downSide;
                case 14:
                    return rightSide;
                case 15:
                    return allConnected;
                default:
                    return allConnected;
            }
        }
    }

    [System.Serializable]
    public class TilesFence
    {
        public Sprite vertical;
        public Sprite horizontal;
    }

    public enum TypeTile
    {
        NONE,
        FLOOR,
        WALL,
        DOOR
    }

    public Tiles floor;
    public Tiles wall;
    public TilesFence fences;

    float widthSprite;
    public float Width => widthSprite;

    float heightSprite;
    public float Height => heightSprite;

    [SerializeField]
    GameObject etalon;

    public static Tuple<Vector2Int, Vector2Int> noCorner;
    public void Awake()
    {
        noCorner = new Tuple<Vector2Int, Vector2Int>(new Vector2Int(), new Vector2Int());
        Bounds bd = etalon.GetComponent<SpriteRenderer>().bounds;
        widthSprite = bd.size.x;
        heightSprite = bd.size.y;
    }

    public Vector3 getTruePosition(Vector2Int position)
    {
        Vector3 output = new Vector3(position.x * Width, position.y * Height, 0);
        return output;
    }
}
