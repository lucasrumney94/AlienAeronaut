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

    public override string ToString()
    {
        return string.Format("Int2 ({0}, {1})", x, y);
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

    public int InclusiveWidth
    {
        get { return Width + 1; }
    }

    public int InclusiveHeight
    {
        get { return Height + 1; }
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

    public bool Contains(Int2 index)
    {
        if (Left <= index.x && index.x <= Right && Bottom <= index.y && index.y <= Top)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Returns a new IntRect with both corners shrunk by [layers]
    /// </summary>
    /// <param name="original"></param>
    /// <param name="Layers"></param>
    /// <returns></returns>
    public IntRect Shrink(Int2 Layers)
    {
        Int2 newBottomLeft = new Int2(Left + Layers.x, Bottom + Layers.y);
        Int2 newTopRight = new Int2(Right - Layers.x, Top - Layers.y);
        return new IntRect(newBottomLeft, newTopRight);
    }

    public override string ToString()
    {
        return string.Format("Bottom Left at {0}, Top Right at {1}", BottomLeft.ToString(), TopRight.ToString());
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
    [Range(0f, 1f)]
    public float minRoadWidth;
    [Range(0f, 1f)]
    public float maxRoadWidth;
    public int minBuildingsPerBlock;
    public int maxBuildingsPerBlock;

    public int loadedAreaSize = 8; //Edge length of the square of loaded blocks, roughly centered around the player

    public BlockGrid cityBlocks;
    public IntRect loadedArea;
    private List<BlockGenerator> loadedBlocks;

    void Start()
    {
        //Generate start area
        loadedBlocks = new List<BlockGenerator>();
        IntRect startingRange = GetLoadedBlocksRange(playerPosition, loadedAreaSize);
        loadedArea = startingRange;
        cityBlocks = new BlockGrid(50, 50);
        LoadBlocks(startingRange);
    }

    void Update()
    {
        playerGridPosition = GetBlockIndexAtPosition(playerPosition);
        if (playerGridPosition != lastPlayerGridPosition)
        {
            loadedArea = GetLoadedBlocksRange(playerPosition, loadedAreaSize);
            LoadBlocks(loadedArea);
            UnloadBlocks(loadedArea);
        }

        lastPlayerGridPosition = playerGridPosition;
    }

    private Int2 GetBlockIndexAtPosition(Vector3 position)
    {
        //Assuming that block [0, 0] will be placed at (0, 0, 0)
        return new Int2(Mathf.FloorToInt(position.x / blockSize), Mathf.FloorToInt(position.z / blockSize)); //Will this be incorrect for negative values of position.{x, y}?
    }

    private Vector3 GetPositionAtBlockIndex(Int2 index)
    {
        return new Vector3(index.x * blockSize, 0f, index.y * blockSize);
    }

    private IntRect GetLoadedBlocksRange(Vector3 center, int radius)
    {
        Vector3 bottomLeft = center;
        bottomLeft.x -= (float)radius * blockSize;
        bottomLeft.z -= (float)radius * blockSize;

        Vector3 topRight = center;
        topRight.x += (float)radius * blockSize;
        topRight.z += (float)radius * blockSize;

        return new IntRect(GetBlockIndexAtPosition(bottomLeft), GetBlockIndexAtPosition(topRight));
    }

    /// <summary>
    /// Adds blocks in range to loadedBlocks, creating new blocks if they don't exist yet, and calling Generate() on blocks that should be visable
    /// </summary>
    /// <param name="range"></param>
    private void LoadBlocks(IntRect range)
    {
        IntRect generatedArea = range.Shrink(new Int2(1, 1));
        //Initialize all blocks, with a one-wide buffer edge between null blocks and generated geometry
        for (int x = range.Left; x <= range.Right; x++)
        {
            for (int y = range.Bottom; y <= range.Top; y++)
            {
                if (cityBlocks.BlockInRange(x, y))
                {
                    BlockGenerator currentBlock = cityBlocks.GetBlock(x, y);

                    if (currentBlock == null)
                    {
                        currentBlock = CreateBlock(new Int2(x, y));
                    }
                    else
                    {
                        currentBlock.WakeUp();
                    }

                    loadedBlocks.Add(currentBlock);
                }
            }
        }

        //Generate geometry inside buffer edge
        for (int x = generatedArea.Left; x <= generatedArea.Right; x++)
        {
            for (int y = generatedArea.Bottom; y <= generatedArea.Top; y++)
            {
                if (cityBlocks.BlockInRange(x, y) && cityBlocks.GetBlock(x, y) != null)
                {
                    BlockGenerator[,] surroundingBlocks = cityBlocks.GetAdjacentBlocks(new Int2(x, y));
                    List<Vector3> surroundingPoints = new List<Vector3>();
                    for (int i = 0; i < 3; i++) //Loop through surroundingBlocks
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (!(i == 1 && j == 1)) //Skip middle block because it is the active one
                            {
                                if (surroundingBlocks[i, j] != null && surroundingBlocks[i, j].pointsSet == true) //If the block exists and has been initialized
                                {
                                    //Debug.Log("Block at " + cityBlocks.GetBlock(x, y).indexPosition + " is surrounded by " + surroundingBlocks[i, j].indexPosition);

                                    //Offset each point in pointlist by the position of the surrounding block relative to the center
                                    Vector3[] pointList = surroundingBlocks[i, j].pointList;
                                    List<Vector3> offsetPointlist = new List<Vector3>();
                                    for (int point = 0; point < pointList.Length; point++)
                                    {
                                        Vector3 offset = new Vector3((i - 1) * blockSize, 0f, (j - 1) * blockSize);
                                        offsetPointlist.Add(pointList[point] + offset);
                                    }
                                    surroundingPoints.AddRange(offsetPointlist);
                                }
                            }
                        }
                    }
                    cityBlocks.GetBlock(x, y).Generate(surroundingPoints.ToArray());
                }
            }
        }
    }

    /// <summary>
    /// Removes from loadedBlocks and disables all blocks outside of range
    /// </summary>
    /// <param name="range"></param>
    private void UnloadBlocks(IntRect range) //Totally fucked
    {
        List<BlockGenerator> newLoadedBlocks = new List<BlockGenerator>();
        foreach (BlockGenerator block in loadedBlocks)
        {
            if (range.Contains(block.indexPosition))
            {
                newLoadedBlocks.Add(block);
            }
            else
            {
                block.Disable();
            }
        }

        loadedBlocks = newLoadedBlocks;
    }

    private BlockGenerator CreateBlock(Int2 index)
    {
        Vector3 position = GetPositionAtBlockIndex(index);

        BlockGenerator newGenerator = Instantiate(BlockGeneratorPrefab).GetComponent<BlockGenerator>();
        cityBlocks.SetBlock(index.x, index.y, newGenerator);
        newGenerator.transform.SetParent(this.transform);
        newGenerator.transform.position = position;
        newGenerator.Initialize(index, Random.Range(minBuildingsPerBlock, maxBuildingsPerBlock), blockSize, Random.Range(minRoadWidth, maxRoadWidth));

        return newGenerator;
    }
}
