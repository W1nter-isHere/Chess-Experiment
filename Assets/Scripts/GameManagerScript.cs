using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Pieces;

public class GameManagerScript : MonoBehaviour
{
    public static readonly Dictionary<(int, int), ChessPieceScript> PiecesOnBoard =
        new Dictionary<(int, int), ChessPieceScript>();

    public static readonly Dictionary<(int, int), CellScript> CellsOnBoard = new Dictionary<(int, int), CellScript>();

    private static Dictionary<ChessPieceScript, (float, float)> _moveQueue =
        new Dictionary<ChessPieceScript, (float, float)>();

    private static Dictionary<ChessPieceScript, (float, float)> _moveQueueCached;

    public float time;
    [FormerlySerializedAs("text")] public Text roundText;
    public Text timeText;
    public GameObject piecePrefab;
    [FormerlySerializedAs("cellObject")] public GameObject cellPrefab;
    public GameObject deadPileManager;
    public Texture2D chessTexture;
    public Texture2D overlayTexture;
    [FormerlySerializedAs("_round")] public uint round = 1;

    private CellScript _selectedCell;

    void Start()
    {
        InitializeBoard();
        roundText.text = "Round: " + round;
    }

    private void Update()
    {
        time += Time.deltaTime;

        float minutes = Mathf.FloorToInt(time / 60);
        float seconds = Mathf.FloorToInt(time % 60);

        timeText.text = "Time: " + string.Format("{0:00}:{1:00}", minutes, seconds);

        _moveQueueCached = new Dictionary<ChessPieceScript, (float, float)>(_moveQueue);
        foreach (var toMove in _moveQueue)
        {
            var obj = toMove.Key.gameObject;
            var vec3 = obj.transform.position;
            vec3 = new Vector3(math.lerp(vec3.x, toMove.Value.Item1, 0.05f),
                math.lerp(vec3.y, toMove.Value.Item2, 0.05f), -0.1f);
            obj.transform.position = vec3;

            if (Math.Round(vec3.x, 5) == Math.Round(toMove.Value.Item1, 5) &&
                Math.Round(vec3.y, 5) == Math.Round(toMove.Value.Item2, 5))
            {
                _moveQueueCached.Remove(toMove.Key);
            }
        }

        _moveQueue = new Dictionary<ChessPieceScript, (float, float)>(_moveQueueCached);
    }

    public void SelectCell(CellScript cell)
    {
        // if there is already a selected cell, plan to move selected piece on selected cell to newly clicked cell
        if (_selectedCell != null)
        {
            var movement = new PieceMovement(_selectedCell.ChessOnTop.Type, (cell.Row, cell.Column),
                (_selectedCell.Row, _selectedCell.Column));

            var selectedCellSpriteRenderer = _selectedCell.gameObject.GetComponent<SpriteRenderer>();
            selectedCellSpriteRenderer.sprite = null;

            foreach (var move in movement.GetPossibleMovements(_selectedCell.ChessOnTop))
            {
                var spriteRenderer = CellsOnBoard[move.GetDestCell()].gameObject.GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = null;
            }

            if (cell.ChessOnTop == null)
            {
                if (Move(movement, false))
                {
                    cell.ChessOnTop = _selectedCell.ChessOnTop;
                    _selectedCell.ChessOnTop = null;
                    round++;
                    roundText.text = "Round: " + round;
                }
            }
            else if (_selectedCell.ChessOnTop.CanEat(cell.ChessOnTop))
            {
                if (Move(movement, true))
                {
                    cell.ChessOnTop = _selectedCell.ChessOnTop;
                    _selectedCell.ChessOnTop = null;
                    round++;
                    roundText.text = "Round: " + round;
                }
            }

            _selectedCell = null;
        }
        else
        {
            if (cell.ChessOnTop == null) return;
            
            // check if it is your turn to move the chess piece
            var pieceOnCell = cell.ChessOnTop;
            if (round % 2 != 0)
                if (!pieceOnCell.White)
                    return;

            if (round % 2 == 0)
                if (pieceOnCell.White)
                    return;
            _selectedCell = cell;

            // highlight selected cell
            var selectedCellSpriteRenderer = _selectedCell.gameObject.GetComponent<SpriteRenderer>();
            selectedCellSpriteRenderer.sprite =
                Sprite.Create(overlayTexture, new Rect(0, 0, 100, 100), new Vector2(0.1f, 0.055f));
            selectedCellSpriteRenderer.color = new Color(Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.2f);

            var movement = new PieceMovement(_selectedCell.ChessOnTop.Type, (cell.Row, cell.Column),
                (_selectedCell.Row, _selectedCell.Column));

            // highlight possible positions to move to
            foreach (var move in movement.GetPossibleMovements(cell.ChessOnTop))
            {
                var spriteRenderer = CellsOnBoard[move.GetDestCell()].gameObject.GetComponent<SpriteRenderer>();
                spriteRenderer.sprite =
                    Sprite.Create(overlayTexture, new Rect(0, 0, 100, 100), new Vector2(0.1f, 0.055f));
                spriteRenderer.color = new Color(Color.green.r, Color.green.g, Color.green.b, 0.3f);
            }
        }
    }

    public bool Move(IMovement movement, bool eat)
    {
        ChessPieceScript piece;
        try
        {
            piece = PiecesOnBoard[movement.GetCurrentCell()];
        }
        catch
        {
            throw new Exception("Can not enqueue movement for piece as piece does not exist in data table");
        }

        var customMovement = movement.GetPossibleMovements(piece);
        if (customMovement == null) return false;
        foreach (var move in customMovement)
        {
            if (movement.GetDestCell() == move.GetDestCell())
            {
                return RawMove(ref piece, move, eat);
            }
        }

        return false;
    }

    private bool RawMove(ref ChessPieceScript piece, IMovement movement, bool eat)
    {
        if (eat)
        {
            ChessPieceScript toBeEatenPiece;

            try
            {
                toBeEatenPiece = PiecesOnBoard[movement.GetDestCell()];
            }
            catch
            {
                Debug.LogError("Scheduled to eat piece at " + movement.GetDestCell().ToString() +
                               "but no piece found");
                throw new Exception();
            }

            deadPileManager.GetComponent<DeadPileManagerScript>().AddToDeadPile(ref toBeEatenPiece);
        }

        QueueMove(piece, movement);
        PiecesOnBoard.Remove((piece.Row, piece.Column));
        piece.Row = movement.GetDestCell().Item1;
        piece.Column = movement.GetDestCell().Item2;
        piece.MovesAmount += 1;
        PiecesOnBoard.Add((piece.Row, piece.Column), piece);
        return true;
    }

    private void InitializeBoard()
    {
        for (var row = 0; row < 8; row++)
        {
            for (var column = 0; column < 4; column++)
            {
                // create and get script
                var createdPiece = Instantiate(piecePrefab);
                var chessPieceScript = createdPiece.GetComponent<ChessPieceScript>();
                chessPieceScript.Row = row;
                chessPieceScript.Column = column > 1 ? column == 2 ? 6 : 7 : column;

                SetupType(ref chessPieceScript, row, column);

                // setup position
                createdPiece.transform.position = GetPosition(row, chessPieceScript.Column);

                // setup sprite
                var pieceSpriteRenderer = createdPiece.GetComponent<SpriteRenderer>();
                if (column == 2 || column == 3)
                {
                    pieceSpriteRenderer.color = Color.white;
                    chessPieceScript.White = true;
                }
                else
                {
                    pieceSpriteRenderer.color = Color.black;
                }

                if (chessPieceScript.Type.Name.Equals(PieceType.Pawn.Name))
                {
                    pieceSpriteRenderer.sprite = Sprite.Create(chessTexture, chessPieceScript.Type.TextureRect,
                        new Vector2(-0.08f, 0), 60f);
                }
                else
                {
                    pieceSpriteRenderer.sprite = Sprite.Create(chessTexture, chessPieceScript.Type.TextureRect,
                        new Vector2(0, 0), 60f);
                }

                // add to board data
                PiecesOnBoard.Add((row, chessPieceScript.Column), chessPieceScript);
            }

            for (var column = 0; column < 8; column++)
            {
                var createdCell = Instantiate(cellPrefab);
                createdCell.transform.position = GetPosition(row, column);
                var script = createdCell.GetComponent<CellScript>();
                script.Row = row;
                script.Column = column;
                try
                {
                    script.ChessOnTop = PiecesOnBoard[(row, column)];
                }
                catch
                {
                    script.ChessOnTop = null;
                }

                CellsOnBoard.Add((row, column), script);
            }
        }
    }

    private static void SetupType(ref ChessPieceScript script, int row, int column)
    {
        if (column == 1 || column == 2)
        {
            script.Type = PieceType.Pawn;
            return;
        }

        if (column == 0 || column == 3)
        {
            if (row == 0 || row == 7)
            {
                script.Type = PieceType.Rook;
            }

            if (row == 1 || row == 6)
            {
                script.Type = PieceType.Knight;
            }

            if (row == 2 || row == 5)
            {
                script.Type = PieceType.Bishop;
            }

            if (row == 3)
            {
                script.Type = PieceType.Queen;
            }

            if (row == 4)
            {
                script.Type = PieceType.King;
            }
        }
    }

    public static Vector3 GetPosition(int row, int column)
    {
        return new Vector3(-3.9f + row, 3.05f - column, -0.1f);
    }

    public static void QueueMove(ChessPieceScript chessPieceScript, IMovement movement)
    {
        var a = GetPosition(movement.GetDestCell().Item1, movement.GetDestCell().Item2);
        _moveQueue.Add(chessPieceScript, (a.x, a.y));
    }

    public static void QueueMove(ChessPieceScript chessPieceScript, Vector3 movement)
    {
        _moveQueue.Add(chessPieceScript, (movement.x, movement.y));
    }
}