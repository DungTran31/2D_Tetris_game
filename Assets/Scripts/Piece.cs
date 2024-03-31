using System;
using System.Data.Common;
using UnityEngine;
using UnityEngine.UIElements;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }
    public int rotationIndex { get; private set; }

    public float stepDelay = 1f;
    public float moveDelay = 0.1f;
    public float lockDelay = 0.5f;

    private float stepTime;
    private float moveTime;
    private float lockTime;

    private float individualScoreTime;
    public int individualScore = 100;
    public int individualScoreOrigin = 100;

    AudioManager audioManager;

    public static bool canInput = true;
    private bool countdownFinished = false;

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        countdownFinished = false;
        CountdownController.CountdownFinished += OnCountdownFinished;
    }

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.board = board;
        this.position = position;
        this.data = data;
        this.rotationIndex = 0;
        this.stepTime = Time.time + this.stepDelay;
        this.moveTime = Time.time + this.moveDelay;
        this.lockTime = 0f;
        canInput = true;

        if(this.cells == null)
        {
            this.cells = new Vector3Int[data.cells.Length];
        }

        for (int i = 0; i < data.cells.Length; i++)
        {
            this.cells[i] = (Vector3Int)data.cells[i];
        }
    }
    private void OnDestroy()
    {
        CountdownController.CountdownFinished -= OnCountdownFinished; 
    }

    private void OnCountdownFinished()
    {
        countdownFinished = true;
    }
    private void Update()
    {
        if (!countdownFinished) return;
        this.board.Clear(this);

        // We use a timer to allow the player to make adjustments to the piece
        // before it locks in place
        this.lockTime += Time.deltaTime;
        if (canInput)
        {
            if (Input.GetKeyDown(KeyCode.Q)) 
            {
                audioManager.PlaySFX(audioManager.rotate);
                Rotate(-1);
            } 
            else if (Input.GetKeyDown(KeyCode.E))
            {
                audioManager.PlaySFX(audioManager.rotate);
                Rotate(1);
            }

            // Handle hard drop
            if (Input.GetKeyDown(KeyCode.Space))
            {
                HardDrop();
            }

            // Allow the player to hold movement keys but only after a move delay
            // so it does not move too fast
            if (Time.time > moveTime)
            {
                HandleMoveInputs();
            }

            // Advance the piece to the next row every x seconds
            if (Time.time > stepTime)
            {
                Step();
            }
        }

        UpdateIndividualScore();
        this.board.Set(this);
    }

    private void HandleMoveInputs()
    {
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) ||
            Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            audioManager.PlaySFX(audioManager.move);
        }

        // Soft drop movement
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            if (Move(Vector2Int.down))
            {
                // Update the step time to prevent double movement
                stepTime = Time.time + stepDelay;
            }
        }

        // Left/right movement
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            Move(Vector2Int.left);
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            Move(Vector2Int.right);
        }
    }

    private void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
            continue;
        }

        Lock();
    }

    private void Step()
    {
        stepTime = Time.time + stepDelay;

        // Step down to the next row
        Move(Vector2Int.down);

        // Once the piece has been inactive for too long it becomes locked
        if (lockTime >= lockDelay)
        {
            Lock();
        }
    }

    private void Lock()
    {
        this.board.Set(this);
        audioManager.PlaySFX(audioManager.land);
        bool isCleared = this.board.ClearLines();
        if (isCleared)
        {
            audioManager.PlaySFX(audioManager.lineCleared);
        }
        Board.currentScore += individualScore;
        individualScore = individualScoreOrigin;
        FindObjectOfType<Board>().UpdateHighScore();
        this.board.SpawnPiece();
    }

    

    public bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = this.position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = this.board.IsValidPosition(this, newPosition);

        if(valid)
        {
            this.position = newPosition;
            moveTime = Time.time + moveDelay;
            this.lockTime = 0;
        }
        return valid;
    }

    private void Rotate(int direction)
    {

        int originalRotation = this.rotationIndex;
        this.rotationIndex = Wrap(this.rotationIndex +direction, 0, 4);

        ApplyRotationMatrix(direction);

        if (!TestWallKicks(this.rotationIndex, direction))
        {
            this.rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3 cell = this.cells[i];

            int x, y;
            switch (this.data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;
                default:
                    x = Mathf.RoundToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;
            }
            this.cells[i] = new Vector3Int(x, y, 0);
        }
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for(int i = 0; i < this.data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = this.data.wallKicks[wallKickIndex, i];

            if (Move(translation))
            {
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if(rotationIndex < 0)
        {
            wallKickIndex--;
        }
        return Wrap(wallKickIndex, 0, this.data.wallKicks.GetLength(0));
    }
    private int Wrap(int input, int min, int max)
    {
        if (input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }

    private void UpdateIndividualScore()
    {
        if (individualScoreTime < 1)
        {
            individualScoreTime += Time.deltaTime;
        }
        else
        {
            individualScoreTime = 0;
            individualScore = Mathf.Max(individualScore - 10, 0);
        }
    }
}
