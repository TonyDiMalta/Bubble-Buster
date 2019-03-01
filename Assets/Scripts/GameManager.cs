using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Text DisplayedScore;
    public GameObject Explosion;
    public GameObject BubblePrefab;

    List<Bubble> BubblesToLaunch = new List<Bubble>();
    //List<Bubble> BubblesToRemove = new List<Bubble>();
    
    private bool IsGameOver = false;
    private bool IsGamePaused = false;

    [SerializeField, Range(1, 1000)]
    private int ScoreMultiplier = 100;
    private int CurrentScore = 0;

    [Header("Bubbles Attributes")]
    [Tooltip("The number of bubbles the player can preview before launching them")]
    [SerializeField, Range(1, 10)]
    private int NumberOfBubblesToLaunch = 4;
    [Tooltip("The minimum number of bubbles the player need to match to remove them")]
    [SerializeField, Range(1, 10)]
    private int MinBubblesToRemove = 2;

    private HexGrid GameBoard;

    // Use this for initialization
    void Start()
    {
        IsGameOver = false;
        GameBoard = GameObject.FindGameObjectWithTag("GameBoard").GetComponent<HexGrid>();

        InitializeBubblesToLaunch();
    }

    /// <summary>
    /// initializes the bubbles the player can use
    /// </summary>
    private void InitializeBubblesToLaunch()
    {
        for (int numberOfBubbles = 0; numberOfBubbles < NumberOfBubblesToLaunch; ++numberOfBubbles)
        {
            AddBubbleToLaunch(numberOfBubbles);
        }
    }
    
    void AddBubbleToLaunch(int numberOfPreviousBubbles)
    {
        MyMaterial material = MyMaterial.GetRandomMaterial(); //get a random color
                                                              //create a new bubble
        var go = (GameObject)Instantiate(BubblePrefab,
            new Vector3(this.transform.localPosition.x,
            (float)numberOfPreviousBubbles * BubblePrefab.transform.localScale.y + BubblePrefab.transform.localPosition.y, 0f), Quaternion.identity);
        go.tag = material.ColorName;
        go.transform.SetParent(this.transform, false);
        BubblesToLaunch.Add(new Bubble(go, material));

        var renderer = go.transform.GetComponent<Renderer>();
        renderer.material = material; //set the color
    }

    RaycastHit hit;
    // Update is called once per frame
    void Update()
    {
        if (IsGamePaused == true)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("MainScene");

        if (Input.GetButtonDown("Fire1"))
        {
            Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(inputRay, out hit))
            {
                Vector3 position = GameBoard.transform.InverseTransformPoint(hit.point);
                HexCoordinates coordinates = HexCoordinates.FromPosition(position);
                if (GameBoard.HasCellBubble(coordinates.X, coordinates.Y) == false)
                {
                    GameBoard.AddBubbleToCoordinates(coordinates.X, coordinates.Y);
                    var clusterBubbles = GameBoard.GetClusterFromCoordinates(coordinates.X, coordinates.Y);

                    if (clusterBubbles != null &&
                        clusterBubbles.Count >= MinBubblesToRemove)
                    {
                        foreach (var bubbleCoordinates in clusterBubbles)
                        {
                            GameBoard.RemoveBubbleFromCoordinates(bubbleCoordinates.X, bubbleCoordinates.Y);
                        }

                        var floatingClusters = GameBoard.GetFloatingClusters();

                        foreach (var bubbleCoordinates in floatingClusters)
                        {
                            GameBoard.RemoveBubbleFromCoordinates(bubbleCoordinates.X, bubbleCoordinates.Y);
                        }

                        UpdateScore(clusterBubbles.Count + floatingClusters.Count);
                    }

                    GameBoard.SetTurnOver();
                    if (GameBoard.IsTurnEventCountReached() == true)
                    {
                        GameBoard.AddNewBubblesRow();
                    }
                }
            }

            IsGameOver = CheckIsGameOver();
            if (IsGameOver)
                StartCoroutine(GotoGameOver());
        }
    }

    private IEnumerator GotoGameOver()
    {
        IsGamePaused = true;
        if (this.CurrentScore > 0)
        {
            ScoreManager sm = new ScoreManager();
            sm.AddScore(new ScoreEntry() { ScoreInt = this.CurrentScore, Date = DateTime.Now });
        }
        yield return new WaitForSeconds(2f);
        Globals.GameScore = this.CurrentScore;
        SceneManager.LoadScene("ScoreboardScene");
    }

    private bool CheckIsGameOver()
    {
        int width = GameBoard.Width;

        // Check if there are bubbles in the bottom row
        for (int x = 0; x < width; ++x)
        {
            if (GameBoard.HasCellBubble(x, 0) == true)
            {
                return true;
            }
        }

        return false;
    }
    
    private void UpdateScore(int CountBubblesBursted)
    {
        while (CountBubblesBursted > 0)
        {
            CurrentScore += CountBubblesBursted * ScoreMultiplier;
            --CountBubblesBursted;
        }
        DisplayedScore.text = CurrentScore.ToString();
    }
}
