using System;
using System.Collections;
using System.IO;
using System.Linq;
using IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonManagerScript : MonoBehaviour
{
    public GameObject loadingScreen;
    public Slider slider;
    public GameObject mainMenu;
    public Dropdown loadSaveDropDown;

    private bool _dropDownOpened = false;
    private bool _initialized = false;
    
    private void Awake()
    {
        loadingScreen.SetActive(false);
        loadSaveDropDown.gameObject.SetActive(false);
    }

    public void OpenChess(String scene)
    {
        StartCoroutine(LoadSceneAsync(scene));
        PlayerPrefs.SetInt("LastSavedChess", PlayerPrefs.GetInt("LastSavedChess") + 1);
    }

    public void UseAI()
    {
        GameManagerScript.UseAI = true;
    }

    public void NoUseAI()
    {
        GameManagerScript.UseAI = false;
    }
    
    IEnumerator LoadSceneAsync(String scene)
    {
        var operation = SceneManager.LoadSceneAsync(scene);
        mainMenu.SetActive(false);
        loadingScreen.SetActive(true);
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);
            slider.value = progress;
            Debug.Log(progress);
            yield return null;
        }
    }

    public void LoadSave()
    {
        if (!_initialized)
        {
            var filesNames = Directory.GetFiles(IOUtilities.SavesPaths);
            foreach (var fileName in filesNames)
            {
                var s = fileName.Substring(fileName.LastIndexOf('/') + 1);
                loadSaveDropDown.options.Add(new Dropdown.OptionData(s));
            }

            _initialized = true;

            if (!loadSaveDropDown.options.Any())
            {
                loadSaveDropDown.value = 0;
                OnDropDownChange();
            }
        }
        loadSaveDropDown.gameObject.SetActive(!_dropDownOpened);
        loadSaveDropDown.onValueChanged.AddListener(delegate {
            OnDropDownChange();
        });

        if (_dropDownOpened) 
            _dropDownOpened = false;
        else if (!_dropDownOpened) 
            _dropDownOpened = true;
    }

    private void OnDropDownChange()
    {
        Debug.Log("changed to" + loadSaveDropDown.captionText.text);
        IOUtilities.SaveToOpen = loadSaveDropDown.captionText.text;
    }
    
    public void Settings()
    {
        
    }

    public void QuitGame()
    {
        Application.Quit();
    }


}

