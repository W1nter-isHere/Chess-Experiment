using Pieces;
using UnityEngine;

public class ChessPieceScript : MonoBehaviour
{
    public int Row { get; set; }
    public int Column { get; set; }
    public PieceType Type { get; set; }
    public bool White { get; set; }
    public uint MovesAmount { get; set; } = 0;

    public bool CanEat(ChessPieceScript chessPiece)
    {
        return chessPiece != null && Movement.IsInRange(Type, (Row, Column), (chessPiece.Row, chessPiece.Column))&& White ? !chessPiece.White : chessPiece.White;
    }
    
    public bool CanEat((int, int) cord)
    {
        try
        {
            var chessPiece = GameManagerScript.PiecesOnBoard[cord];
            return Movement.IsInRange(Type, (Row, Column), (chessPiece.Row, chessPiece.Column)) && White
                ? !chessPiece.White
                : chessPiece.White;
        }
        catch
        {
            return false;
        }
    }
}
