using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Used for indexing 2-dimensional lists and arrays
/// Implements +, -, ==, and !=
/// </summary>
public struct Int2
{
    private int _x;
    private int _y;

    public int x
    {
        get
        {
            return _x;
        }

        set
        {
            _x = value;
        }
    }

    public int y
    {
        get
        {
            return _y;
        }

        set
        {
            _y = value;
        }
    }

    public Int2(int x, int y)
    {
        _x = x;
        _y = y;
    }

    public static Int2 operator +(Int2 a, Int2 b)
    {
        return new Int2(a.x + b.x, a.y + b.y);
    }

    public static Int2 operator -(Int2 a, Int2 b)
    {
        return new Int2(a.x - b.x, a.y - b.y);
    }

    public static bool operator ==(Int2 a, Int2 b)
    {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Int2 a, Int2 b)
    {
        return a.x != b.x || a.y != b.y;
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

/// <summary>
/// Used for indexing a range of values in 2-dimensional lists and arrays
/// </summary>
public struct IntRect
{
    private Int2 _BottomLeft;
    private Int2 _TopRight;

    public Int2 BottomLeft
    {
        get
        {
            return _BottomLeft;
        }

        set
        {
            _BottomLeft = value;
        }
    }

    public Int2 TopRight
    {
        get
        {
            return _TopRight;
        }

        set
        {
            _TopRight = value;
        }
    }

    public int Width
    {
        get
        {
            return Mathf.Abs(TopRight.x - BottomLeft.x);
        }
    }

    public int Height
    {
        get
        {
            return Mathf.Abs(TopRight.y - BottomLeft.y);
        }
    }

    public int Top
    {
        get { return TopRight.y; }
    }

    public int Bottom
    {
        get { return BottomLeft.y; }
    }

    public int Right
    {
        get { return TopRight.x; }
    }

    public int Left
    {
        get { return BottomLeft.x; }
    }

    public IntRect(Int2 bottomLeft, Int2 topRight)
    {
        _BottomLeft = bottomLeft;
        _TopRight = topRight;
    }
}

/// <summary>
/// Adds and removes blocks of buildings from the world
/// Handles interactions between blocks as they are added and removed
/// </summary>
public class CityGenerator : MonoBehaviour {

    public Vector3 playerPosition;
    public Int2 playerGridPosition;
    public Int2 lastPlayerGridPosition;

    public GameObject BlockGeneratorPrefab;

    public float blockSize;
    public int minBuildingsPerBlock;
    public int maxBuildingsPerBlock;

    public int loadedAreaSize = 8; //Edge length of the square of loaded blocks, roughly centered around the player

    private List<List<BlockGenerator>> cityBlocks; //Should look at other options for storing large grid of data
    //public List<List<BlockGenerator>> loadedBlocks; Needed? Should just disable the gameObject of blocks out of range

    void Start()
    {
        //Generate start area
        IntRect startingRange = GetLoadedBlocksRange(playerPosition);
        cityBlocks = new List<List<BlockGenerator>> (startingRange.Width);
        LoadBlocks(startingRange);
    }

    void Update()
    {
        playerGridPosition = GetBlockIndexAtPosition(playerPosition);
        if (playerGridPosition != lastPlayerGridPosition)
        {
            //Change what blocks are loaded
        }

        lastPlayerGridPosition = playerGridPosition;
    }

    private Int2 GetBlockIndexAtPosition(Vector3 position)
    {
        //Assuming that block [0, 0] will be placed at (0, 0, 0)
        return new Int2(Mathf.FloorToInt(position.x / blockSize), Mathf.FloorToInt(position.y / blockSize)); //Will this be incorrect for negative values of position.{x, y}?
    }

    private Vector3 GetPositionAtBlockIndex(Int2 index)
    {
        return new Vector3(index.x * blockSize, index.y * blockSize, 0f);
    }

    private IntRect GetLoadedBlocksRange(Vector3 center)
    {
        Vector3 bottomLeft = center;
        bottomLeft.x -= (float)loadedAreaSize * blockSize / 2f;
        bottomLeft.y -= (float)loadedAreaSize * blockSize / 2f;

        Vector3 topRight = center;
        topRight.x += (float)loadedAreaSize * blockSize / 2f;
        topRight.y += (float)loadedAreaSize * blockSize / 2f;

        return new IntRect(GetBlockIndexAtPosition(bottomLeft), GetBlockIndexAtPosition(topRight));
    }

    private void LoadBlocks(IntRect range)
    {
        for (int x = range.Left; x < range.Right; x++)
        {
            for (int y = range.Bottom; y < range.Top; y++)
            {
                if (cityBlocks[x][y] == null)
                {
                    GenerateBlock(new Int2(x, y));
                }
            }
        }
    }

    private void GenerateBlock(Int2 index)
    {
        Vector3 position = GetPositionAtBlockIndex(index);

        BlockGenerator newGenerator = Instantiate(BlockGeneratorPrefab).GetComponent<BlockGenerator>();
        newGenerator.transform.SetParent(this.transform);
        newGenerator.transform.position = position;
        newGenerator.Generate(Random.Range(minBuildingsPerBlock, maxBuildingsPerBlock));
    }
}
