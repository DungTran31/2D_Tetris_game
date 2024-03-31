using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Tilemap nextTetrominoTilemap;
    public Tilemap holdTetrominoTilemap;
    public Piece activePiece { get; private set; }
    public TetrominoData[] tetrominoes;
    private TetrominoData nextTetromino;
    private TetrominoData holdTetromino;
    public Vector3Int spawnPosition;
    public Vector3Int nextPosition;
    public Vector3Int holdPosition;
    public Vector2Int boardSize = new Vector2Int(10, 20);

    // Scoring parameters
    public static int currentScore;
    public int scoreOneLine = 400;
    public int scoreTwoLine = 1000;
    public int scoreThreeLine = 3000;
    public int scoreFourLine = 12000;
    private int numberOfRowsThisTurn = 0;
    private UIManager uiManager;
    public Text hud_score;

    private int startingHighScore;
    // Audio paramters
    AudioManager audioManager;

    public static bool isPaused = false;
    TetrominoData currentData;

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-this.boardSize.x / 2, -this.boardSize.y / 2);
            return new RectInt(position, this.boardSize);
        }
    }

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        uiManager = FindObjectOfType<UIManager>();
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.activePiece = GetComponentInChildren<Piece>();
        for (int i = 0; i < tetrominoes.Length; i++)
        {
            this.tetrominoes[i].Initialize();
        }
        SetNextTetromino();
        SetHoldTetromino();
        StartGame();
    }

    private void StartGame()
    {
        this.tilemap.ClearAllTiles();
        Time.timeScale = 1f;
        currentScore = 0;
        hud_score.text = "0";
        SpawnPiece();
        startingHighScore = PlayerPrefs.GetInt("highscore");
    }

    private void Update()
    {
        if (!isPaused) // Kiểm tra xem trò chơi có đang tạm dừng không
        {
            UpdateScore();
            UpdateUI();
        }
        CheckUserInput();
    }
    private void CheckUserInput()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (Time.timeScale == 1)
            {
                PauseGame();
            }
            else
            {
                if (uiManager.pauseScreen.activeSelf) // Kiểm tra xem màn hình pause có đang hiển thị không
                {
                    ResumeGame();
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
        {
            // Kiểm tra khi người dùng ấn phím Enter hoặc Enter trên bàn phím số
            SwapHoldTetromino(); ;
        }
    }
    public void Option()
    {
        audioManager.PlaySFX(audioManager.clicked);
        uiManager.optionScreen.SetActive(true);

        if (uiManager.pauseScreen.activeSelf)
        {
            uiManager.pauseScreen.SetActive(false);
        }
    }

    public void CloseOption()
    {
        audioManager.PlaySFX(audioManager.clicked);
        uiManager.optionScreen.SetActive(false);
        uiManager.pauseScreen.SetActive(true);
    }
    private void PauseGame()
    {
        Time.timeScale = 0;
        uiManager.Pause();
        audioManager.MusicSource.Pause();
        isPaused = true;
    }

    private void ResumeGame()
    {
        audioManager.MusicSource.Play();
        Time.timeScale = 1;
        uiManager.pauseScreen.SetActive(false);
        isPaused = false;
    }

    public void SetNextTetromino()
    {
        int random = Random.Range(0, tetrominoes.Length);
        nextTetromino = tetrominoes[random];
        // Debug.Log(nextTetromino.tetromino.ToString());
        DrawNextTetromino();
    }

    private void DrawNextTetromino()
    {
        nextTetrominoTilemap.ClearAllTiles();

        foreach (var cell in nextTetromino.cells)
        {
            nextTetrominoTilemap.SetTile((Vector3Int)cell + nextPosition, nextTetromino.tile);
        }
    }

    public void SetHoldTetromino()
    {
        int random = Random.Range(0, tetrominoes.Length);
        holdTetromino = tetrominoes[random];
        // Debug.Log(holdTetromino.tetromino.ToString());
        DrawHoldTetromino();
    }

    private void DrawHoldTetromino()
    {
        holdTetrominoTilemap.ClearAllTiles();

        foreach (var cell in holdTetromino.cells)
        {
            holdTetrominoTilemap.SetTile((Vector3Int)cell + holdPosition, holdTetromino.tile);
        }
    }

    private void SwapHoldTetromino()
    {
        Vector3Int currentPosition = this.activePiece.position;
        for (int i = 0; i < this.activePiece.cells.Length; i++)
        {
            Vector3Int cell = this.activePiece.cells[i] + this.activePiece.position;
            this.tilemap.SetTile(cell, null);
        }

        TetrominoData temp = currentData;
        currentData = holdTetromino;
        holdTetromino = temp;
        DrawHoldTetromino();
        Vector3Int newPosition = currentPosition;
        Debug.Log(newPosition);

        if (currentData.GetTetromino().Equals(Tetromino.I) && currentPosition.x >= 2)
        {
            newPosition += Vector3Int.left;
        } 
        else if (currentData.GetTetromino().Equals(Tetromino.I) && currentPosition.x <= -4)
        {
            newPosition += Vector3Int.right;
        }
        this.activePiece.Initialize(this, newPosition, currentData);

        if (IsValidPosition(this.activePiece, newPosition))
        {
            Set(activePiece);
        }
    }

    
    public void SpawnPiece()
    {
        currentData = nextTetromino;

        this.activePiece = GetComponentInChildren<Piece>();
        this.activePiece.Initialize(this, spawnPosition, currentData);

        if (IsValidPosition(this.activePiece, this.spawnPosition))
        {
            Set(activePiece);
        }
        else
        {
            GameOver();
        }

        SetNextTetromino();
    }


    private void GameOver()
    {
        Piece.canInput = false;
        Time.timeScale = 0f;
        // this.tilemap.ClearAllTiles();
        audioManager.PlaySFX(audioManager.gameOver);
        audioManager.StopMusic();
        uiManager.GameOver();
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = this.Bounds;
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            if (this.tilemap.HasTile(tilePosition))
                return false;
        }
        return true;
    }

    public bool ClearLines()
    {
        bool isCleared = false;
        RectInt bounds = this.Bounds;
        int row = bounds.yMin;

        while (row < bounds.yMax)
        {
            if (IsLineFull(row))
            {
                isCleared = true;
                LineClear(row);
            }
            else
            {
                row++;
            }
        }

        return isCleared;
    }

    private void LineClear(int row)
    {
        RectInt bounds = this.Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            this.tilemap.SetTile(position, null);
        }

        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = this.tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                this.tilemap.SetTile(position, above);
            }

            row++;
        }
    }

    private bool IsLineFull(int row)
    {
        RectInt bounds = this.Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            if (!this.tilemap.HasTile(position))
            {
                return false;
            }
        }

        // Since we found a full row, we increment the full row variable
        numberOfRowsThisTurn++;

        return true;
    }
    
    public void UpdateUI()
    {
        hud_score.text = currentScore.ToString();
    }

    public void UpdateScore()
    { 
        if(numberOfRowsThisTurn > 0)
        {
            if(numberOfRowsThisTurn == 1)
            {
                ClearedOneLine();
            }
            else if(numberOfRowsThisTurn == 2)
            {
                ClearedTwoLine();
            }
            else if (numberOfRowsThisTurn == 3)
            {
                ClearedThreeLine();
            }
            else if (numberOfRowsThisTurn == 4)
            {
                ClearedFourLine();
            }
            numberOfRowsThisTurn = 0;
            UpdateHighScore();
        }
    } 
    public void ClearedOneLine()
    {
        currentScore += scoreOneLine;
    }

    public void ClearedTwoLine()
    {
        currentScore += scoreTwoLine;
    }
    public void ClearedThreeLine()
    {
        currentScore += scoreThreeLine;
    }
    public void ClearedFourLine()
    {
        currentScore += scoreFourLine;
    }

    public void UpdateHighScore()
    {
        if(currentScore > startingHighScore)
        {
            PlayerPrefs.SetInt("highscore", currentScore);
        }
    }
}
