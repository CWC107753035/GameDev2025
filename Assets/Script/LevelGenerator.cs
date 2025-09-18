using UnityEngine;

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

    private int[,] fullLevelMap;
    private int[,] horizontalVertical;

    void Start()
    {
        int quadHeight = levelMap.GetLength(0);
        int quadWidth = levelMap.GetLength(1);
        horizontalVertical = new int[quadHeight, quadWidth];
        for (int y = 0; y < quadHeight; y++)
        {
            for (int x = 0; x < quadWidth; x++)
            {
                horizontalVertical[y, x] = -1;
            }
        }
        for (int y = 0; y < quadHeight; y++)
        {
            for (int x = 0; x < quadWidth; x++)
            {
                int tile = levelMap[y, x];
                if (tile == 5 || tile == 6 || tile == 8 || tile == 0) continue;
                else if (tile == 1 || tile == 3 || tile == 7) horizontalVertical[y, x] = 0; //corner
                else
                {
                    int up = GetTile(x, y - 1, horizontalVertical);
                    int down = GetTile(x, y + 1, horizontalVertical);
                    int left = GetTile(x - 1, y, horizontalVertical);
                    int right = GetTile(x + 1, y, horizontalVertical);
                    if (up == 0 || up == 1 || down == 1 || down == 0) horizontalVertical[y, x] = 1; //vertical
                    else horizontalVertical[y, x] = 2; //horizontal
                }
            }
        }
        PrintHorizontalVerticalMap();
        GenerateFullMapData();
        InstantiateLevel();
        AdjustCamera();
    }

    void GenerateFullMapData()
    {
        int quadHeight = levelMap.GetLength(0);
        int quadWidth = levelMap.GetLength(1);

        int fullHeight = quadHeight;
        int fullWidth = quadWidth;

        fullLevelMap = new int[fullHeight, fullWidth];

        for (int y = 0; y < quadHeight; y++)
        {
            for (int x = 0; x < quadWidth; x++)
            {
                int tile = levelMap[y, x];
                fullLevelMap[y, x] = tile;
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

                // Get prefab 
                GameObject prefab = GetPrefabForTile(tileType);
                if (prefab == null) continue;

                // Determine position and rotation
                Vector3 position = new Vector3(x + 0.5f, -y - 0.5f, 0);
                Quaternion rotation = CalculateRotation(x, y, tileType, horizontalVertical);

                // Create the tile in the scene
                Instantiate(prefab, position, rotation, this.transform);
            }
        }
    }

    void AdjustCamera()
    {
        int mapHeight = fullLevelMap.GetLength(0);
        int mapWidth = fullLevelMap.GetLength(1);

        // Center the camera
        float cameraX = (mapWidth - 1) / 2.0f;
        float cameraY = -(mapHeight - 1) / 2.0f;
        Camera.main.transform.position = new Vector3(cameraX, cameraY, -10f);

        // Adjust the orthographic size
        float screenRatio = (float)Screen.width / (float)Screen.height;
        float mapRatio = (float)mapWidth / (float)mapHeight;

        float orthoSize;
        float padding = 1.5f; // Add some padding around the edges

        if (mapRatio > screenRatio)
        {
            // Level is wider than the screen, so width is the limiting factor
            orthoSize = (mapWidth / 2.0f) / screenRatio;
        }
        else
        {
            // Level is taller than the screen, so height is the limiting factor
            orthoSize = mapHeight / 2.0f;
        }

        Camera.main.orthographicSize = orthoSize + padding;
    }

    /// Determines the correct rotation for a tile by checking its neighbors.
    private Quaternion CalculateRotation(int x, int y, int tileType, int[,] map)
    {
        // Pellets don't need rotation
        if (tileType == 5 || tileType == 6)
        {
            return Quaternion.identity;
        }

        // Check neighbors //vertical:1 horizontal:2
        int up = GetTile(x, y - 1, map);
        int down = GetTile(x, y + 1, map);
        int left = GetTile(x - 1, y, map);
        int right = GetTile(x + 1, y, map);

        float angle = 0;

        switch (tileType)
        {
            case 1: // Outside Corner
                if ( (down == 0 && right == 2) || (down == 1 && right == 0) || (down == 0 && right == 0)) angle = 0;   // Top-left
                else if ((down == 0 && left == 2) || (down == 1 && left == 0) || (down == 0 && left == 0)) angle = 270;  // Top-right
                else if ((up == 0 && left == 2) || (up == 1 && left == 0) || (up == 0 && left == 0)) angle = 180;  // Bottom-right
                else angle = 90; // Bottom-left
                if (right == 2 && down == 1) angle = 0;   // Top-left
                else if (left == 2 && down == 1) angle = 270;   // Top-right
                else if (up == 1 && left == 2) angle = 180;  // Bottom-right
                else if (up == 1 && right == 2) angle = 90; // Bottom-left
                break;

            case 2: // Outside Wall 
                if (left == 0 || right == 0) angle = 90;   // Horizontal wall
                else if (up == 0 || down == 0) angle = 0;
                if (left == 2 || right == 2) angle = 90;
                else if (up == 1 || down == 1) angle = 0;
                break;

            case 3: // Inside Corner 
                if ( (down == 0 && right == 2) || (down == 1 && right == 0) || (down == 0 && right == 0)) angle = 0;   // Top-left
                else if ((down == 0 && left == 2) || (down == 1 && left == 0) || (down == 0 && left == 0)) angle = 270;  // Top-right
                else if ((up == 0 && left == 2) || (up == 1 && left == 0) || (up == 0 && left == 0)) angle = 180;  // Bottom-right
                else angle = 90; // Bottom-left
                if (right == 2 && down == 1) angle = 0;   // Top-left
                else if (left == 2 && down == 1) angle = 270;   // Top-right
                else if (up == 1 && left == 2) angle = 180;  // Bottom-right
                else if (up == 1 && right == 2) angle = 90; // Bottom-left
                break;
            case 4: // Outside Wall 
                if (left == 0 || right == 0) angle = 90;   // Horizontal wall
                else if (up == 0 || down == 0) angle = 0;
                if (left == 2 || right == 2) angle = 90;
                else if (up == 1 || down == 1) angle = 0;
                break;
        }

        return Quaternion.Euler(0, 0, angle);
    }

    /// Returns the appropriate prefab for a given tile type ID.
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
            default: return null;
        }
    }

    /// Safely gets a tile from the map, handling out-of-bounds requests.
    private int GetTile(int x, int y, int[,] map)
    {
        int height = map.GetLength(0);
        int width = map.GetLength(1);

        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return map[y, x];
        }
        return -1; // out if bound
    }
    void PrintHorizontalVerticalMap()
    {
        // StringBuilder is more efficient for creating long strings than regular concatenation.
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("--- Horizontal/Vertical Map --- \n");

        int height = horizontalVertical.GetLength(0);
        int width = horizontalVertical.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                switch (horizontalVertical[y, x])
                {
                    case -1:
                        sb.Append(" . "); // Empty or Pellet
                        break;
                    case 0:
                        sb.Append("C"); // Corner
                        break;
                    case 1:
                        sb.Append("V"); // Vertical
                        break;
                    case 2:
                        sb.Append("H"); // Horizontal
                        break;
                    default:
                        sb.Append("?");
                        break;
                }
            }
            sb.Append("\n"); // New line for the next row
        }

        Debug.Log(sb.ToString());
    }
}