using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImageProcessing
{
    public static class ComplexSimulations
    {
        //The flow weight determines to how many neighbours a cell sprads its flow
        static float _flowDensity = 1.0f;
        public static Vector3[][] FlowHexagonally(Dictionary<Vector2, Vector2[]> coordToNeighbours, float[][] heightMap, Vector3[][] previousFlow)
        {
            //This function will return a flow map
            //That is the next step of a given previous flow array
            //Following a heightmap

            //Initialize new flow
            int xSize, ySize;
            xSize = previousFlow.Length; ySize = previousFlow[0].Length;
            Vector3[][] newFlow = new Vector3[xSize][];
            for (int x = 0; x < xSize; x++)
                newFlow[x] = new Vector3[ySize];

            //Go through each cell

            //Step 1
            //Find a new direction according to the current direction
            //And the heightmap, and mix it with the old direction

            //Step 2
            //Mixs the wind with the neighbours in its direction

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    Vector3 newDirection = previousFlow[x][y];
                    //Find out to how many neighbours it will spread based on its density
                    //1.0 represents all neighbours
                    int spread = (int)Mathf.InverseLerp(3, 0, _flowDensity);
                    //Bump the chosen neighbours as heightmap
                    List<Vector2> affectedNeighbours = new List<Vector2>();


                    newFlow[x][y] = Vector3.zero;
                }
            }

            return newFlow;
        }

        static Vector2[] GetNeighboursInCommon(Vector2[] neighboursA, Vector2[] neighboursB)
        {

            List<Vector2> overlap = new List<Vector2>();
            for (int i = 0; i < neighboursA.Length; i++)
            {
                for (int k = 0; k < neighboursB.Length; k++)
                {
                    if (neighboursA[i] == neighboursB[k])
                        overlap.Add(neighboursA[i]);

                }
            }
            return overlap.ToArray();
        }
    }
    public static class GenerationMaps
    {
        public static float[][] GenerateTemperatureMap(float[][] heightMap)
        {
            float[][] result = ImageProcessingHelper.InitializeArray(heightMap.Length, heightMap[0].Length, 0);
            return result;
        }
    }
    public static class ImageProcessingHelper
    {
        public static float[][] InitializeArray(int sizeX, int sizeY, float defaultValue)
        {
            float[][] newArray = new float[sizeX][];
            for (int x = 0; x < sizeX; x++)
            {
                newArray[x] = new float[sizeY];
                for (int y = 0; y < sizeX; y++)
                {
                    newArray[x][y] = defaultValue;
                }
            }
            return newArray;
        }

    }
    public static class Pass
    {
        //Passes apply filters and extract information on it
        public static Vector2[] PassHeightFillPercentage(float[][] heightMap, float fillPercentage, float fillStep)
        {
            //This method is used to "raise water" in a map array 
            // NOT! ---> Leaving a percentage of the land available
            //It just makes anything lower than the percentage water
            int sizeX = heightMap.Length;
            int sizeY = heightMap[0].Length;
            //Get apex
            float highestHeight = 0;
            for (int x = 0; x < sizeX; x++)
                for (int y = 0; y < sizeY; y++)
                    if (heightMap[x][y] > highestHeight)
                        highestHeight = heightMap[x][y];


            List<Vector2> filledTiles = new List<Vector2>();
            float waterLevel = Mathf.Lerp(0, highestHeight, fillPercentage);
            for (int x = 0; x < sizeX; x++)
                for (int y = 0; y < sizeY; y++)
                    if (Mathf.InverseLerp(0, highestHeight, heightMap[x][y]) < fillPercentage)
                    {
                        heightMap[x][y] = waterLevel - fillStep;
                        filledTiles.Add(new Vector2(x, y));
                    }

            return filledTiles.ToArray();
        }
}
    public static class GenerationAlgorithms
    {
      
        public static float[][] GenerateSquarePadding(int sizeX, int sizeY, int paddingSize = 1)
        {
            Debug.LogWarning("GenerateSquarePadding bug: Overlapping effect on corners due to duplicate calculations.");
            float[][] result = ImageProcessingHelper.InitializeArray(sizeX, sizeY, 1);

            for (int x = 0; x < sizeX; x++)
                for (int y = 0; y < paddingSize; y++)
                    if (y < sizeY)
                        result[x][y] *= Mathf.Pow(Mathf.InverseLerp(0, paddingSize, y), 3);

            for (int x = 0; x < sizeX; x++)
                for (int y = sizeY - 1; y >= sizeY - paddingSize; y--)
                    if (y >= 0)
                        result[x][y] *= Mathf.Pow(Mathf.InverseLerp(sizeY, sizeY - paddingSize, y), 3);

            for (int y = 0; y < sizeY; y++)
                for (int x = 0; x < paddingSize; x++)
                    if (x < sizeX)
                        result[x][y] *= Mathf.Pow(Mathf.InverseLerp(0, paddingSize, x), 3);

            for (int y = 0; y < sizeY; y++)
                for (int x = sizeX - 1; x >= sizeX - paddingSize; x--)
            if (x >= 0)
                        result[x][y] *= Mathf.Pow(Mathf.InverseLerp(sizeX, sizeX - paddingSize, x),3);

            //Set edges to zero
            for (int x = 0; x < sizeX; x++)
                for (int y = 0; y < sizeY; y++)
                    if (x == 0 || y == 0 || x >= sizeX - 1 || y >= sizeY - 1)
                        result[x][y] = 0;

            //float _debugBorderCount = 0; //This number needs to be 0, otherwise the borders are not flattened
            //for (int x = 0; x < sizeX; x++)
            //    for (int y = 0; y < sizeY; y++)
            //        if (x == 0 || y == 0 || x >= sizeX - 1 || y >= sizeY - 1)
            //            _debugBorderCount += result[x][y];
            //Debug.Log(_debugBorderCount);

            return result;
        }
        public static float[][] GeneratePerlinNoise(int sizeX, int sizeY, float scale, float strength)
        {
            //Create the base ondulations on the terrain
            //Variables
            float _impact = strength;
            float _scale = scale * 10;
            float _seed = Random.Range(0.00f, 99999f);

            //Create array in memory
            float[][] heightMap = new float[sizeX][];
            for (int i = 0; i < sizeX; i++)
                heightMap[i] = new float[sizeY];

            //Run algorithm
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    //Multiplying x by 2 counters the stretchiness cause by an hexagon board
                    heightMap[x][y] = _impact * Mathf.PerlinNoise(_seed + (x / _scale) , _seed + (y / _scale));
                }
            }

            //Return generated array
            return heightMap;
        }

    }

}
