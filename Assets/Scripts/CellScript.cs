using UnityEngine;

public class CellScript : MonoBehaviour
{
    public ChessPieceScript ChessOnTop { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
    
    void OnMouseDown()
    {
        var obj = GameObject.Find("GameManager");
        var script = obj.GetComponent<GameManagerScript>();

        if (script.escapeUI.activeSelf) return;
        
        Debug.Log(Row.ToString() + ", " + Column.ToString());
        if (ChessOnTop != null)
        {
            Debug.Log(ChessOnTop.Type);
            Debug.Log(ChessOnTop.Row.ToString() + ", " + ChessOnTop.Column.ToString());
        }
        
        script.SelectCell(this);
    }
}
