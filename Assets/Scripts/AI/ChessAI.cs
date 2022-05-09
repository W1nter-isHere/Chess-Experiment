using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class ChessAI : MonoBehaviour
{
    private readonly int[] _pieceValues = {80, 100, 60, 60, 50, 20};
    
    private void Start()
    {
        GameManagerScript.ONMoveEvent += OnMoved;
    }

    public List<IMovement> GetAllPossibleMovesOfSide()
    {
        return GetAllPossibleMovesOfSide(
            GameObject.Find("GameManager").GetComponent<GameManagerScript>().round % 2 != 0);
    }

    public List<IMovement> GetAllPossibleMovesOfSide(bool white)
    {
        
        return null;
    }

    private void OnMoved(ChessPieceScript piece)
    {
        // CheckCheckmate(piece);
    }

    private void CheckCheckmate(ChessPieceScript piece)
    {
        // var possibleMovesOfSide = GetAllPossibleMovesOfSide(piece.White);
        // foreach (var move in possibleMovesOfSide)
        // {
        //     // move.GetDestCell()
        // }
    }
}
