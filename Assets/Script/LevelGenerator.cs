using UnityEngine;
using System.Text; // Required for the print function

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private GameObject manualLevelContainer;
    [SerializeField] private GameObject outsideCorner;
    [SerializeField] private GameObject outsideWall;
    [SerializeField] private GameObject insideCorner;
    [SerializeField] private GameObject insideWall;
    [SerializeField] private GameObject standardPellet;
    [SerializeField] private GameObject powerPellet;
    [SerializeField] private GameObject tJunction;
    [SerializeField] private GameObject ghostWall;
    private int[,] levelMap =
    {
        {1,2,2,2,2,2,2,2,2,2,2,2,2,7},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,4},
        {2,6,4,0,0,4,5,4,0,0,0,4,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,3},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,5},
        {2,5,3,4,4,3,5,3,3,5,3,4,4,4},
        {2,5,3,4,4,3,5,4,4,5,3,4,4,3},
        {2,5,5,5,5,5,5,4,4,5,5,5,5,4},
        {1,2,2,2,2,1,5,4,3,4,4,3,0,4},
        {0,0,0,0,0,2,5,4,3,4,4,3,0,3},
        {0,0,0,0,0,2,5,4,4,0,0,0,0,0},
        {0,0,0,0,0,2,5,4,4,0,3,4,4,8},
        {2,2,2,2,2,1,5,3,3,0,4,0,0,0},
        {0,0,0,0,0,0,5,0,0,0,4,0,0,0}
    };

    //full-sized map
    private int[,] fullLevelMap;
    private int[,] fullHorizontalVertical;

    // determine the wall rotate
    private int[,] horizontalVertical;

    void Start()
    {
        // Delete the manual level
        if (manualLevelContainer != null) Destroy(manualLevelContainer);

        int quadHeight = levelMap.GetLength(0);
        int quadWidth = levelMap.GetLength(1);
        horizontalVertical = new int[quadHeight, quadWidth];

        for (int y = 0; y < quadHeight; y++)
        {
            for (int x = 0; x < quadWidth; x++) horizontalVertical[y, x] = -1;
        }

        for (int y = 0; y < quadHeight; y++)
        {
            for (int x = 0; x < quadWidth; x++)
            {
                int tile = levelMap[y, x];
                if (tile == 5 || tile == 6 || tile == 0) continue;
                if (tile == 1 || tile == 3 || tile == 7) horizontalVertical[y, x] = 0;
                else
                {
                    int up = GetTile(x, y - 1, horizontalVertical);
                    int down = GetTile(x, y + 1, horizontalVertical);
                    if (up == 0 || up == 1 || down == 1 || down == 0) horizontalVertical[y, x] = 1;
                    else horizontalVertical[y, x] = 2;
                }
            }
        }
        GenerateFullMapData();
        InstantiateLevel();
        AdjustCamera();
        // PrintHorizontalVerticalMap();
    }

    void GenerateFullMapData()
    {
        int quadHeight = levelMap.GetLength(0);
        int quadWidth = levelMap.GetLength(1);

        // expand the w and height
        int fullHeight = (quadHeight * 2) - 1;
        int fullWidth = quadWidth * 2;

        //full-sized maps.
        fullLevelMap = new int[fullHeight, fullWidth];
        fullHorizontalVertical = new int[fullHeight, fullWidth];

        // Loop through every tile of the original top-left quadrant.
        for (int y = 0; y < quadHeight; y++)
        {
            for (int x = 0; x < quadWidth; x++)
            {
                int tileType = levelMap[y, x];
                int hvData = horizontalVertical[y, x];

                //top-left
                fullLevelMap[y, x] = tileType;
                fullHorizontalVertical[y, x] = hvData;

                //top-right 
                fullLevelMap[y, fullWidth - 1 - x] = tileType;
                fullHorizontalVertical[y, fullWidth - 1 - x] = hvData;

                //bottom mirroring
                if (y < quadHeight - 1)
                {
                    // bottom-left
                    fullLevelMap[fullHeight - 1 - y, x] = tileType;
                    fullHorizontalVertical[fullHeight - 1 - y, x] = hvData;

                    // bottom-right
                    fullLevelMap[fullHeight - 1 - y, fullWidth - 1 - x] = tileType;
                    fullHorizontalVertical[fullHeight - 1 - y, fullWidth - 1 - x] = hvData;
                }
            }
        }
    }

    void InstantiateLevel()
    {
        int height = fullLevelMap.GetLength(0);
        int width = fullLevelMap.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int tileType = fullLevelMap[y, x];
                if (tileType == 0) continue;

                GameObject prefab = GetPrefabForTile(tileType);
                if (prefab == null) continue;

                Vector3 position = new Vector3(x + 0.5f, -y - 0.5f, 0);

                Quaternion rotation = CalculateRotation(x, y, tileType, fullHorizontalVertical);

                GameObject newTile = Instantiate(prefab, position, rotation, this.transform);

                if (tileType == 7)
                {
                    newTile.transform.localScale = ShouldMirrorTile(x, y, fullHorizontalVertical);
                }
            }
        }
    }

    void AdjustCamera()
    {
        int mapHeight = fullLevelMap.GetLength(0);
        int mapWidth = fullLevelMap.GetLength(1);

        // Center the camera
        float cameraX = mapWidth / 2.0f;
        float cameraY = -mapHeight / 2.0f;
        Camera.main.transform.position = new Vector3(cameraX, cameraY, mapHeight * -23f/28f);
    }

    private Quaternion CalculateRotation(int x, int y, int tileType, int[,] map)
    {
        if (tileType == 5 || tileType == 6) return Quaternion.identity;

        // horizontal: 2 vertical: 1
        int up = GetTile(x, y - 1, map);
        int down = GetTile(x, y + 1, map);
        int left = GetTile(x - 1, y, map);
        int right = GetTile(x + 1, y, map);
        float angle = 0;

        switch (tileType)
        {
            case 1: // Outside Corner
            case 3: // Inside Corner
                if ((down == 0 && right == 2) || (down == 1 && right == 0) || (down == 0 && right == 0)) angle = 0; // Top-left
                else if ((down == 0 && left == 2) || (down == 1 && left  == 0) || (down == 0 && left  == 0)) angle = 270; // Top-right
                else if ((up == 0 && left == 2) || (up == 1 && left == 0) || (up == 0 && left == 0)) angle = 180; // Bottom-right
                else angle = 90; // Bottom-left
                if (right == 2 && down == 1) angle = 0; // Top-left
                else if (left == 2 && down == 1) angle = 270; // Top-right
                else if (up == 1 && left == 2) angle = 180; // Bottom-right
                else if (up == 1 && right == 2) angle = 90; // Bottom-left
                break;
            case 8: //ghost wall
                if (left == 0 || right == 0) angle = 0; // Horizontal wall
                else if (up == 0 || down == 0) angle = 90;
                if (left == 2 || right == 2) angle = 0; // Horizontal wall
                else if (up == 1 || down == 1) angle = 90;
                break;
            case 2: // Outside Wall
            case 4: // Inside Wall
                if (left == 0 || right == 0) angle = 90; // Horizontal wall
                else if (up == 0 || down == 0) angle = 0;
                if (left == 2 || right == 2) angle = 90; // Horizontal wall
                else if (up == 1 || down == 1) angle = 0;
                break;
        }
        return Quaternion.Euler(0, 0, angle);
    }

    // Your helper functions remain unchanged.
    private GameObject GetPrefabForTile(int tileType)
    {
        switch (tileType)
        {
            case 1: return outsideCorner;
            case 2: return outsideWall;
            case 3: return insideCorner;
            case 4: return insideWall;
            case 5: return standardPellet;
            case 6: return powerPellet;
            case 7: return tJunction;
            case 8: return ghostWall;
            default: return null;
        }
    }
    private int GetTile(int x, int y, int[,] map)
    {
        if (map == null) return -1;
        int height = map.GetLength(0);
        int width = map.GetLength(1);
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return map[y, x];
        }
        return -1;
    }

    private Vector3 ShouldMirrorTile(int x, int y, int[,] map)
    {
        int up = GetTile(x, y - 1, map);
        int down = GetTile(x, y + 1, map);
        int left = GetTile(x - 1, y, map);
        int right = GetTile(x + 1, y, map);
        if (left == 2 && down == 1) return new Vector3(1, 1, 1);
        else if (right == 2 && down == 1) return new Vector3(-1, 1, 1);
        else if (right == 2 && up == 1) return new Vector3(-1, -1, 1);
        else if (left == 2 && up == 1) return new Vector3(1, -1, 1);
        return new Vector3(1, 1, 1);
    }

    // void PrintHorizontalVerticalMap()
    // {
    //     // StringBuilder is more efficient for creating long strings than regular concatenation.
    //     System.Text.StringBuilder sb = new System.Text.StringBuilder();
    //     sb.Append("--- Horizontal/Vertical Map --- \n");

    //     int height = horizontalVertical.GetLength(0);
    //     int width = horizontalVertical.GetLength(1);

    //     for (int y = 0; y < height; y++)
    //     {
    //         for (int x = 0; x < width; x++)
    //         {
    //             switch (horizontalVertical[y, x])
    //             {
    //                 case -1:
    //                     sb.Append(" . "); // Empty or Pellet
    //                     break;
    //                 case 0:
    //                     sb.Append("C"); // Corner
    //                     break;
    //                 case 1:
    //                     sb.Append("V"); // Vertical
    //                     break;
    //                 case 2:
    //                     sb.Append("H"); // Horizontal
    //                     break;
    //                 default:
    //                     sb.Append("?");
    //                     break;
    //             }
    //         }
    //         sb.Append("\n"); // New line for the next row
    //     }

    //     Debug.Log(sb.ToString());
    // }
}