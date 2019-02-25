using UnityEngine;
using System.Collections;
using System.Linq;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShowHighScores : MonoBehaviour
{

    
    // Use this for initialization
    void Start()
    {
        DisplayScores();
    }

    private void DisplayScores()
    {
        Text txt = GameObject.Find("HighScores").GetComponent<Text>();


        if (Globals.GameScore != 0)
            txt.text = "Your score: " + Globals.GameScore + "\n";
        else
            txt.text = string.Empty;

        foreach (var score in manager.GetScores())
        {
            txt.text += string.Format("{0} - {1}", score.Date.ToShortDateString(), score.ScoreInt) + "\n";
        }
    }

    public void GoToMainScene()
    {
        SceneManager.LoadScene("MainScene");
    }

    public ScoreManager manager;

    void Awake()
    {
        manager = new ScoreManager();
    }

   
}
