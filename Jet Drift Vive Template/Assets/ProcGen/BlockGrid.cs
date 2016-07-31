using UnityEngine;
using System.Collections;

/// <summary>
/// Class for storing a large grid of objects centered at (0, 0), and accessable with negatives indexers (from [-width / 2, -height / 2] to [width / 2, height / 2])
/// </summary>
public class BlockGrid {

    //Dimensions should always be odd, so that (0, 0) is centered
    private int _width;
    private int _height;

    private int _maxX;
    private int _maxY;

    private BlockGenerator[,] _blocks;

    public int Width
    {
        get
        {
            return _width;
        }
    }

    public int Height
    {
        get
        {
            return _height;
        }
    }

    public int MaxX
    {
        get
        {
            return _maxX;
        }
    }

    public int MaxY
    {
        get
        {
            return _maxY;
        }
    }

    public BlockGenerator[,] Blocks //Shouldn't be needed
    {
        get
        {
            return _blocks;
        }

        set
        {
            _blocks = value;
        }
    }

    public BlockGrid(int maxX, int maxY)
    {
        _maxX = maxX;
        _maxY = maxY;
        _width = (MaxX * 2) + 1;
        _height = (MaxY * 2) + 1;

        _blocks = new BlockGenerator[Width, Height];
    }

    public BlockGenerator GetBlock(int x, int y)
    {
        if (BlockInRange(x, y))
        {
            if (x < 0)
            {
                x += Width;
            }
            if (y < 0)
            {
                y += Height;
            }
            return Blocks[x, y];
        }
        else
        {
            Debug.Log(string.Format("Block index [{0}, {1}] out of range", x, y));
            return null;
        }
    }

    public void SetBlock(int x, int y, BlockGenerator block)
    {
        if (BlockInRange(x, y))
        {
            if (x < 0)
            {
                x+= Width;
            }
            if (y < 0)
            {
                y += Height;
            }

            Blocks[x, y] = block;
        }
        else
        {
            Debug.Log(string.Format("Block index [{0}, {1}] out of range", x, y));
        }
    }

    /// <summary>
    /// Gets a 2-dimensional array of 9 blocks centered around [x, y]
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public BlockGenerator[,] GetAdjacentBlocks(int x, int y)
    {
        BlockGenerator[,] adjacent = new BlockGenerator[3, 3];

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                adjacent[i, j] = GetBlock(i - 1, j - 1);
            }
        }

        return adjacent;
    }

    public bool BlockInRange(int x, int y)
    {
        if (x > MaxX || x < -MaxX || y > MaxY || y < -MaxY)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
