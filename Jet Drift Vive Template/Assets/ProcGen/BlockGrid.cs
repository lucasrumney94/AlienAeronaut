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

    /// <summary>
    /// Gets the block at [x, y], returning null if outside of range
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public BlockGenerator GetBlock(int x, int y, bool wrap = false)
    {
        if (wrap)
        {
            Int2 wrappedIndex = WrapIndex(x, y);
            x = wrappedIndex.x;
            y = wrappedIndex.y;
        }
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

    public void SetBlock(int x, int y, BlockGenerator block, bool wrap = false)
    {
        if (wrap)
        {
            Int2 wrappedIndex = WrapIndex(x, y);
            x = wrappedIndex.x;
            y = wrappedIndex.y;
        }
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
    /// <param name="wrap">Should blocks on the opposite edge of the grid be wrapped to?</param>
    /// <returns></returns>
    public BlockGenerator[,] GetAdjacentBlocks(int x, int y, bool wrap = false)
    {
        BlockGenerator[,] adjacent = new BlockGenerator[3, 3];

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int adjX = x + i - 1;
                int adjY = y + j - 1;
                adjacent[i, j] = GetBlock(adjX, adjY, wrap);
            }
        }

        return adjacent;
    }

    /// <summary>
    /// Returns if the block at given position exists in the grid
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="wrap">Always returns true when wrap is true</param>
    /// <returns></returns>
    public bool BlockInRange(int x, int y, bool wrap = false)
    {
        if (wrap)
        {
            return true;
        }

        if (x > MaxX || x < -MaxX || y > MaxY || y < -MaxY)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public Int2 WrapIndex(int x, int y)
    {
        while (x < -MaxX)
        {
            x += Width;
        }
        while (x > MaxX)
        {
            x -= Width;
        }

        while (y < -MaxY)
        {
            y += Height;
        }
        while (y > MaxY)
        {
            y -= Height;
        }

        return new Int2(x, y);
    }
}
