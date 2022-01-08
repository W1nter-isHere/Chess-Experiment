using UnityEngine;

public class CellScript : MonoBehaviour
{
    public ChessPieceScript ChessOnTop { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
    
    void OnMouseDown()
    {
        Debug.Log(Row.ToString() + ", " + Column.ToString());
        if (ChessOnTop != null)
        {
            Debug.Log(ChessOnTop.Type.Name);
            Debug.Log(ChessOnTop.Row.ToString() + ", " + ChessOnTop.Column.ToString());
        }
        
        GameObject obj = GameObject.Find("GameManager");
        GameManagerScript script = obj.GetComponent<GameManagerScript>();
        script.SelectCell(this);
    }
}
