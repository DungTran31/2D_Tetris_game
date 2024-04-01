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

    private float lastTapTime;
    private float doubleTapTimeThreshold = 0.5f; // Ngưỡng thời gian giữa hai lần chạm

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
    }

    public void StartGame()
    {
        lastTapTime = -doubleTapTimeThreshold;
        audioManager.MusicSource.Play();
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
#if UNITY_IOS
        CheckTouchInput();
#endif
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
                if (uiManager.optionScreen.activeSelf) // Kiểm tra xem màn hình pause có đang hiển thị không
                {
                    audioManager.PlaySFX(audioManager.clicked);
                    uiManager.optionScreen.SetActive(false);
                    uiManager.pauseScreen.SetActive(true);
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
        {
            // Kiểm tra khi người dùng ấn phím Enter hoặc Enter trên bàn phím số
            SwapHoldTetromino(); ;
        }
    }

    private void CheckTouchInput()
    {
        // Kiểm tra các touch được thực hiện trên màn hình
        for (int i = 0; i < Input.touchCount; ++i)
        {
            Touch touch = Input.GetTouch(i);

            // Kiểm tra nếu touch được kích hoạt
            if (touch.phase == TouchPhase.Ended)
            {
                // Lấy thời gian hiện tại
                float currentTime = Time.time;

                // Tính thời gian kể từ lần chạm trước đó
                float timeSinceLastTap = currentTime - lastTapTime;

                // Nếu thời gian kể từ lần chạm trước đó nhỏ hơn ngưỡng thời gian giữa double tap
                if (timeSinceLastTap < doubleTapTimeThreshold)
                {
                    // Thực hiện hành động khi double tap
                    SwapHoldTetromino();
                }

                // Lưu thời gian của lần chạm hiện tại để kiểm tra double tap cho lần chạm tiếp theo
                lastTapTime = currentTime;
            }
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
    public void PauseGame()
    {
        Time.timeScale = 0;
        uiManager.Pause();
        audioManager.MusicSource.Pause();
        isPaused = true;
    }

    public void ResumeGame()
    {
        audioManager.MusicSource.UnPause();
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
            Vector3Int adjustedPosition = nextPosition;

            if (nextTetromino.GetTetromino().Equals(Tetromino.I))
            {
                adjustedPosition += new Vector3Int(0, -1, 0);
            }
            else if (nextTetromino.GetTetromino().Equals(Tetromino.O))
            {
                adjustedPosition += new Vector3Int(-1, 0, 0);
            }

            nextTetrominoTilemap.SetTile((Vector3Int)cell + adjustedPosition, nextTetromino.tile);
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
        Vector2 holdPositionOffset = new Vector2(-0.5f, -0.5f);
        foreach (var cell in holdTetromino.cells)
        {
            Vector3Int adjustedPosition = holdPosition;

            if (holdTetromino.GetTetromino().Equals(Tetromino.I))
            {
                adjustedPosition += new Vector3Int(0, -1, 0);
            }
            else if (holdTetromino.GetTetromino().Equals(Tetromino.O))
            {
                adjustedPosition += new Vector3Int(-1, 0, 0);
            }

            holdTetrominoTilemap.SetTile((Vector3Int)cell + adjustedPosition, holdTetromino.tile);
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

    public void ClearTiles()
    {
        this.tilemap.ClearAllTiles();
        this.holdTetrominoTilemap.ClearAllTiles();
        this.nextTetrominoTilemap.ClearAllTiles();
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
