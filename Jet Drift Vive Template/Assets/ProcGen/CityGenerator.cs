using UnityEngine;
using System.Collections;

/// <summary>
/// Adds and removes blocks of buildings from the world
/// Handles interactions between blocks as they are added and removed
/// </summary>
public class CityGenerator : MonoBehaviour {

    public Vector3 playerPosition;

    public float blockSize;
    public int minBuildingsPerBlock;
    public int maxBuildingsPerBlock;

    public int loadedAreaSize = 8; //Edge length of the square of loaded blocks, roughly centered around the player

    public BlockGenerator[,] cityBlocks;
}
