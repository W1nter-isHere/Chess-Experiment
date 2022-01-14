using IO;
using UnityEngine;
using Utilities;

public class ChessPieceScript : MonoBehaviour, ISerializable<DeadChessPieceData>
{
    public int Row { get; set; }
    public int Column { get; set; }

    public string Type { get; set; }
    public bool White { get; set; }
    public uint MovesAmount { get; set; } = 0;

    public Texture2D texture2D;
    
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

    public void LoadData(DeadChessPieceData data)
    {
        if (data is ChessPieceData aliveData)
        {
            Row = aliveData.Row;
            Column = aliveData.Column;
            MovesAmount = aliveData.MovesAmount;
        }

        White = data.White;
        Type = data.Type;

        transform.position = GameManagerScript.Utilities.GetPosition(Row, Column);
        var pieceSpriteRenderer = GetComponent<SpriteRenderer>();
        
        if (Type.Equals(PieceTypes.Pawn.Name))
        {
            pieceSpriteRenderer.sprite = Sprite.Create(texture2D, PieceTypes.SerializeType(Type).GetValueOrDefault(PieceTypes.Pawn).TextureRect,
                new Vector2(0, 0), 200f);
        }
        else
        {
            pieceSpriteRenderer.sprite = Sprite.Create(texture2D, PieceTypes.SerializeType(Type).GetValueOrDefault(PieceTypes.Pawn).TextureRect,
                new Vector2(0, 0), 200f);
        }

        pieceSpriteRenderer.color = White ? Color.white : Color.black;
    }

    public DeadChessPieceData SaveData()
    {
        return ChessPieceData.FromReal(this);
    }
}
