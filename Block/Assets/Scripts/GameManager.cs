using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    public static int GRID_WIDTH = 10;
    public static int GRID_HEIGHT = 10;
    public static int NUM_SPAWN = 4;
    public static int POINT_MULTIPLIER = 10;

    private int adsCounter = 4;

    public ScoreManager scoreManager;
    public event System.EventHandler GameOverEvent; 

    int dropCount = 0;

    Block[] blocks;
    Transform pieces;

    Transform[,] grid = new Transform[GRID_WIDTH, GRID_HEIGHT];
    HashSet<int> rowsToCheck = new HashSet<int>();
    HashSet<int> colsToCheck = new HashSet<int>();

    Block[] spawnedBlocks = new Block[NUM_SPAWN];
    Vector2[] spawnPositions = new Vector2[NUM_SPAWN];

    void Awake()
    {
        if (PlayerPrefs.HasKey("adsCounter"))
        {
            adsCounter = PlayerPrefs.GetInt("adsCounter");
        }

        if (instance == null) {
            instance = this;
        }

        else if (instance != this) {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        CalculateSpawnPositions();

        blocks = Resources.LoadAll<Block>("Prefabs/Blocks/");

        pieces = new GameObject("Pieces").transform;

        SpawnBlocks();
    }

    public void RestartGame() {

        PlayerPrefs.SetInt("adsCounter", adsCounter);
        PlayerPrefs.Save();

        Debug.Log("adsCounter"+ adsCounter);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    

    public void GameOver()
    {
        DisableBlocks();

        if (ScoreControl.topScore < ScoreManager.score)
        {
            ScoreControl.topScore = ScoreManager.score;
            PlayerPrefs.SetInt("TopScore", ScoreControl.topScore);
            PlayerPrefs.Save();
        }

        if (GameOverEvent != null)
        {
            GameOverEvent(this, System.EventArgs.Empty);
        }


    }

    public void DropPiece(Block block)
    {
        RoundPosition(block.transform);

        if (IsValidPosition(block))
        {
            block.GetComponent<BoxCollider2D>().enabled = false;

            dropCount++;

            foreach (Transform blockPiece in block.transform)
            {
                int roundedX = Mathf.RoundToInt(blockPiece.position.x);
                int roundedY = Mathf.RoundToInt(blockPiece.position.y);

                grid[roundedX, roundedY] = blockPiece;

                rowsToCheck.Add(roundedY);
                colsToCheck.Add(roundedX);
            }

            ClearLines();

            if (dropCount == NUM_SPAWN)
            {
                SpawnBlocks();
            }

            if (!HasValidDrop())
            {
                GameOver();
            }
        }
        else
        {
            block.ResetPosition(translate: true);
        }

    }

    private bool HasValidDrop()
    {
        foreach (Block block in spawnedBlocks)
        {
            if (block != null && block.GetComponent<BoxCollider2D>().enabled)
            {
                block.ResetScale();

                for (int x = 0; x < GRID_WIDTH; x++)
                {
                    
                    for (int y = 0; y < GRID_HEIGHT; y++)
                    {
                        block.transform.position = new Vector2(x, y);
                        RoundPosition(block.transform);

                        if (IsValidPosition(block))
                        {
                            block.ResetPosition(translate: false);

                            return true;
                        }
                    }
                }
                block.ResetPosition(translate: false);
            }
        }

        return false;
    }

    private void ClearLines()
    {
        HashSet<int> rowsToClear = new HashSet<int>();
        HashSet<int> colsToClear = new HashSet<int>();

        foreach (int row in rowsToCheck)
        {
            if (IsLineFull(row, true))
            {
                rowsToClear.Add(row);
            }
        }

        foreach(int col in colsToCheck)
        {
            if (IsLineFull(col, false))
            {
                colsToClear.Add(col);
            }
        }

        foreach(int row in rowsToClear)
        {
            ClearLine(row, true);
        }

        foreach(int col in colsToClear)
        {
            ClearLine(col, false);
        }

        UpdateScore(rowsToClear.Count + colsToClear.Count);

        rowsToCheck.Clear();
        colsToCheck.Clear();
    }

    private void ClearLine(int index, bool rowFlag)
    {
        if (rowFlag)
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                if (grid[x, index] != null)
                {
                    GameObject blockPiece = grid[x, index].gameObject;
                    Animator blockPieceAnim = blockPiece.GetComponent<Animator>();

                    blockPieceAnim.SetTrigger("DestroyAnimation");

                    Destroy(blockPiece, blockPieceAnim.GetCurrentAnimatorClipInfo(0).Length);
                    grid[x, index] = null;
                }
            }
        }
        else
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                if (grid[index, y] != null)
                {
                    GameObject blockPiece = grid[index, y].gameObject;
                    Animator blockPieceAnim = blockPiece.GetComponent<Animator>();

                    blockPieceAnim.SetTrigger("DestroyAnimation");

                    Destroy(blockPiece, blockPieceAnim.GetCurrentAnimatorClipInfo(0).Length);
                    grid[index, y] = null;
                }
            }
        }
    }

    private bool IsLineFull(int index, bool rowFlag)
    {
        if (rowFlag)
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                if (grid[x, index] == null)
                {
                    return false;
                }
            }
        }

        else
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                if (grid[index, y] == null)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void SpawnBlocks()
    {
        for (int i = 0; i < NUM_SPAWN; i++)
        {
            Block block = blocks[Random.Range(0, blocks.Length)];

            Block toSpawn = Instantiate(block, spawnPositions[i], Quaternion.identity);
            spawnedBlocks[i] = toSpawn;
            toSpawn.transform.SetParent(pieces);

            RotateBlock(toSpawn);

            foreach(Transform blockPiece in toSpawn.transform)
            {
                blockPiece.gameObject.GetComponent<Animator>().SetTrigger("SpawnAnimation");
            }
        }

        dropCount = 0;
    }

    private bool IsValidPosition(Block block)
    {
        foreach (Transform blockPiece in block.transform)
        {
            if (!IsInsideGrid(blockPiece.position) || grid[Mathf.RoundToInt(blockPiece.position.x), Mathf.RoundToInt(blockPiece.position.y)] != null)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsInsideGrid(Vector2 pos)
    {
        int roundedX = Mathf.RoundToInt(pos.x);
        int roundedY = Mathf.RoundToInt(pos.y);

        if (roundedX >= 0 && roundedX <= GRID_WIDTH - 1 && roundedY >= 0 && roundedY <= GRID_HEIGHT - 1)
        {
            
            return true;
        }

        return false;
    }

    private void RotateBlock(Block block)
    {
        int[] rotations = new int[] {0, 90, 180, 270};
        block.transform.Rotate(0,0, rotations[Random.Range(0, block.numRotations + 1)]);
    }

    private void RoundPosition(Transform parent)
    {
        if (parent.childCount > 0)
        {
            Transform child = parent.GetChild(0);

            Vector2 roundedPos = new Vector2(Mathf.Round(child.position.x), Mathf.Round(child.position.y));
            Vector2 offset = (Vector2)child.transform.position - roundedPos;
            parent.position = (Vector2)parent.position - offset;
        }
        else
        {
            parent.position = new Vector2(Mathf.Round(parent.position.x), Mathf.Round(parent.position.y));
        }
    }

    private void CalculateSpawnPositions()
    {
        int partitions = NUM_SPAWN + 2;
        float leftOffset = (GRID_WIDTH - 10) / partitions;

        for(int i = 0; i < NUM_SPAWN; i++)
        {
            if (i == 0)
            {
                spawnPositions[i] = new Vector2(14, 7);
            }
            else if(i == 1)
            {
                spawnPositions[i] = new Vector2(-5, 7);
            }
            else if (i == 2)
            {
                spawnPositions[i] = new Vector2(14, 0);
            }
            else if (i == 3)
            {
                spawnPositions[i] = new Vector2(-5, 0);
            }
        }
    }
    
    private void DisableBlocks()
    {
        foreach (Block block in spawnedBlocks)
        {
            block.GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    private void UpdateScore(int amount)
    {
        scoreManager.Score += amount * POINT_MULTIPLIER;
    }
}
