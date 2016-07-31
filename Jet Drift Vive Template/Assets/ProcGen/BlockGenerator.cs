using UnityEngine;
using System.Collections;

/// <summary>
/// Created and destroyed by a CityGenerator, handles creation of building objects, and passes them a base mesh to generate from
/// </summary>
public class BlockGenerator : MonoBehaviour {

    public int BuildingCount;
    public BuildingGenerator[] buildings;

    public void Generate(int count) //Should this take in a reference to the surrounding blocks?
    {
        BuildingCount = count;
        //Create a random array of points, and form buildings from this array and the arrays of bordering blocks
    }
}
