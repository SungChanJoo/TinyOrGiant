using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Scene ¿Ã∏ß")]
    [SerializeField] private string[] SceneArray;

    private void Start()
    {
        LoadScene(SceneArray);
    }

    public void LoadScene(string[] SceneArray)
    {
        foreach (string name in SceneArray)
        {
            SceneManager.LoadScene(name, LoadSceneMode.Additive);

        }
    }
}
