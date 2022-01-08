using UnityEngine;


public class DeadPileManagerScript : MonoBehaviour
{
    private int _whiteX = -8;
    private int _whiteY = 2;

    private int _blackX = 8;
    private int _blackY = 2;

    public ParticleSystem deathParticleSystem;

    public void AddToDeadPile(ref ChessPieceScript toBeEatenPiece)
    {
        GameManagerScript.PiecesOnBoard.Remove((toBeEatenPiece.Row, toBeEatenPiece.Column));
        deathParticleSystem.transform.position = GameManagerScript.GetPosition(toBeEatenPiece.Row, toBeEatenPiece.Column);
        deathParticleSystem.Play(false);
        toBeEatenPiece.Column = -1;
        toBeEatenPiece.Row = -1;
        GameManagerScript.QueueMove(toBeEatenPiece, toBeEatenPiece.White ? getWhiteDeadPosition() : getBlackDeadPosition());
        toBeEatenPiece.transform.rotation = new Quaternion(0, 0, Random.Range(-0.6f, 0.6f), 1);
    }

    public Vector3 getWhiteDeadPosition()
    {
        Vector3 vec3 = new Vector3(_whiteX, _whiteY, -0.1f);
        _whiteX += 1;
        if (_whiteX > -5)
        {
            _whiteY += 1;
            _whiteX = -8;
        }

        return vec3;
    }
    
    public Vector3 getBlackDeadPosition()
    {
        Vector3 vec3 = new Vector3(_blackX, _blackY, -0.1f);
        _blackX -= 1;
        if (_blackX < 5)
        {
            _blackY -= 1;
            _blackX = -8;
        }

        return vec3;
    }
}