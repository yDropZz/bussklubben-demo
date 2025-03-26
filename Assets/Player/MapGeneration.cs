using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGeneration : MonoBehaviour
{

    [Header("Prefabs")]
    GameObject grassPrefab;
    [SerializeField] GameObject[] grassPrefabs;
    [SerializeField] GameObject leftRoadPrefab;
    [SerializeField] GameObject rightRoadPrefab;
    [SerializeField] GameObject leftRailPrefab;
    [SerializeField] GameObject rightRailPrefab;

    [Header("Settings")]
    [SerializeField] int initialTiles = 10;
    [SerializeField] int tileLength = 10;
    [SerializeField] Transform player;
    [SerializeField] int spawnAheadDistance = 100;
    [SerializeField] int spawnBehindDistance = 100;
    [SerializeField] float roadSpawnProbability = 0.1f; // Probability of spawning a road tile (10% by default).
    [SerializeField] float railSpawnProbability = .95f; // Probability of spawning a rail tile, but it's BACKWARDS. (5% by default).


    // private variables
    bool isGeneratingInitialTiles = false;
    int lastGeneratedZ = 0;
    private List<string> tileHistory = new List<string>(); // list to keep track of last generated tiles.
    public List<GameObject> spawnedTiles = new List<GameObject>(); // list to keep track of all spawned tiles.

//-------------------------------------------------------------

    void Start()
    {
        ClearTiles();
    }

    void Update()
    {
        //Generate more ties ahead of the player
        int playerZ = Mathf.FloorToInt(player.position.z);
        while(playerZ + spawnAheadDistance > lastGeneratedZ)
        {
            GenerateTile(lastGeneratedZ + tileLength);
        }

        //Despawn tiles behind the player
        DespawnTilesBehind();

    }

    void GenerateInitialTiles()
    {
        isGeneratingInitialTiles = true;

        for(int i = 1; i < initialTiles; i++)
            {
                GenerateTile(i * tileLength);
            }

        isGeneratingInitialTiles = false;
    }

    void DespawnTilesBehind()
    {
        for(int i = spawnedTiles.Count - 1; i >= 0; i--)
        {
            if(spawnedTiles[i].transform.position.z < player.position.z - spawnBehindDistance)
            {
                Destroy(spawnedTiles[i]);
                spawnedTiles.RemoveAt(i);
            }
        }
    }

    private void GenerateTile(int zPosition)
    {
        GameObject prefabToSpawn = GetPrefabWithRules();

        GameObject spawnedTile = Instantiate(prefabToSpawn, new Vector3(0, 0, zPosition), Quaternion.identity);
        spawnedTiles.Add(spawnedTile);

        lastGeneratedZ = zPosition;

        //Increase difficulty over time
        if(isGeneratingInitialTiles == false)
        {
            IncreaseDifficulty();
        }
    }

    void IncreaseDifficulty()
    {
        roadSpawnProbability = Mathf.Clamp(roadSpawnProbability + 0.005f, 0f, 0.9f);
    }

    private GameObject GetPrefabWithRules()
    {
        // Randomly select between one of the available tiles
        GameObject prefabToSpawn = null;
        string selectedTile = "";


        for(int attempts = 0; attempts < 20; attempts++)
        {
            int randomIndex = GetWeightedRandomIndex();
            switch(randomIndex)
            {
                case 0:
                    selectedTile = "grass";
                    SelectGrassPrefab();
                    prefabToSpawn = grassPrefab;
                    break;
                case 1:
                    selectedTile = "left_road";
                    prefabToSpawn = leftRoadPrefab;
                    break;
                case 2:
                    selectedTile = "right_road";
                    prefabToSpawn = rightRoadPrefab;
                    break;
                case 3: selectedTile = "left_rail";
                    prefabToSpawn = leftRailPrefab;
                    break;
                case 4: selectedTile = "right_rail";
                    prefabToSpawn = rightRailPrefab;
                    break;
            }
            // Check if the selected tile is valid
            if(IsValidTile(selectedTile))
            {
                break;
            }

        }

        // Add the selected tile to history
        tileHistory.Add(selectedTile);

        // Remove the first element if the list is bigger than 4, in order to kee pthe history size manageable.
        if(tileHistory.Count > 4)
        {
            tileHistory.RemoveAt(0);
        }

        return prefabToSpawn;

    }

    void SelectGrassPrefab()
    {
        grassPrefab = grassPrefabs[Random.Range(0, grassPrefabs.Length)];
    }

    private int GetWeightedRandomIndex()
    {
        // This will generate a weighted random index based on the roadSpawnProbability (that scales over time).
        float randomValue = Random.value;
        int i = 0;
        if(randomValue < roadSpawnProbability)
        {
            i = Random.Range(1, 3); //1 & 2 == roads. 3 & 4 == rails.
        }
        else if(randomValue > railSpawnProbability)
        {
            i = Random.Range(3, 5); // 3 & 4 == rails.
        }
        else
        {
            i = 0; // grass/safespot = 0
        }

        return i;
    }

    private bool IsValidTile(string selectedTile)
    {
        // Rule 1: No more than 3 tiles in a row, no matter the tile type.
        if(tileHistory.Count >= 3)
        {
            if(tileHistory[tileHistory.Count - 1] == selectedTile && 
               tileHistory[tileHistory.Count - 2] == selectedTile && 
               tileHistory[tileHistory.Count - 3] == selectedTile)
            {
                return false;
            }
        }

        // Rule 2: No more than 2 road tiles in a row.
        if(tileHistory.Count >= 2)
        {
            if(tileHistory[tileHistory.Count - 1] == "left_road" &&
             tileHistory[tileHistory.Count - 2] == "right_road")
            {
                return false;
            }
            if(tileHistory[tileHistory.Count - 1] == "right_road" &&
             tileHistory[tileHistory.Count - 2] == "left_road")
            {
                return false;
            }
        }

        // Rule 3: No more than 1 Grass Tile in a row.
        if(tileHistory.Count >= 1)
        {
            if(tileHistory[tileHistory.Count - 1] == "grass" && selectedTile == "grass")
            {
                return false;
            }
        }

        // Rule 4: Only 1 rail tile in a row.
        if(tileHistory.Count >= 1)
        {
            if(tileHistory[tileHistory.Count - 1] == "left_rail" && selectedTile == "left_rail")
            {
                return false;
            }
            if(tileHistory[tileHistory.Count - 1] == "right_rail" && selectedTile == "right_rail")
            {
                return false;
            }
            if(tileHistory[tileHistory.Count - 1] == "left_rail" && selectedTile == "right_rail")
            {
                return false;
            }
            if(tileHistory[tileHistory.Count - 1] == "right_rail" && selectedTile == "left_rail")
            {
                return false;
            }
        }

        // Rule 5:
        if(selectedTile == "left_rail" || selectedTile == "right_rail")
        {
            if(tileHistory[tileHistory.Count - 1] != "grass")
            {
                return false;
            }
        }


        return true;
    }

    public void ClearTiles()
    {
        foreach(GameObject tile in spawnedTiles)
        {
            Destroy(tile);
        }

        spawnedTiles.Clear();
        tileHistory.Clear();
        lastGeneratedZ = 0;
        roadSpawnProbability = .3f;

        GenerateInitialTiles();
    }

}
