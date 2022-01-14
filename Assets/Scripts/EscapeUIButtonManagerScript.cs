using System.Collections;
using IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeUIButtonManagerScript : MonoBehaviour
{
    public void Save()
    {
        IOUtilities.Save();
    }

    public void Settings()
    {
        
    }

    public void MainMenu()
    {
        StartCoroutine(Load());
        
        IEnumerator Load()
        {
            var operation = SceneManager.LoadSceneAsync("Menu");
            while (Mathf.Clamp01(operation.progress / .9f) != 1)
            {
                float progress = Mathf.Clamp01(operation.progress / .9f);
                Debug.Log(progress);
                yield return null;
            }
        }
    }
}
