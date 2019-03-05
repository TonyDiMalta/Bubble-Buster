using UnityEngine;
using System.Collections;
using Assets.Scripts;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Text DisplayedScore;
    public GameObject BubblePrefab;
    
    private GameObject bubbleToLaunch;
    public MyMaterial bubbleMaterial { get; private set; }
    private Vector2 bubbleDirection;
    
    private bool IsGameOver = false;
    private bool IsGamePaused = false;

    [SerializeField, Range(1, 1000)]
    private int ScoreMultiplier = 100;
    private int CurrentScore = 0;

    [Header("Bubbles Attributes")]
    [Tooltip("The minimum number of bubbles the player need to match to remove them")]
    [SerializeField, Range(1, 10)]
    private int MinBubblesToRemove = 2;
    [Tooltip("The speed of the bubble when it is fired")]
    [SerializeField, Range(1f, 100f)]
    private float BubbleSpeed = 10f;

    private HexGrid GameBoard;
    
    void Start()
    {
        bubbleMaterial = MyMaterial.GetRandomMaterial();
        GameBoard = GameObject.FindGameObjectWithTag("GameBoard").GetComponent<HexGrid>();
    }
    
    void Update()
    {
        if (IsGamePaused == true)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("MainScene");

        if (bubbleToLaunch != null)
        {
            Rigidbody rb = bubbleToLaunch.GetComponent<Rigidbody>();
            rb.velocity = bubbleDirection * BubbleSpeed;
        }
    }

    public void FireBubbleTowardsPosition(Vector3 startPosition, Vector3 destPosition)
    {
        if (bubbleToLaunch != null)
        {
            return;
        }

        bubbleToLaunch = GameBoard.CreateBubble(transform, bubbleMaterial);

        Vector3 startWorldPos = Camera.main.ScreenToWorldPoint(startPosition);
        startWorldPos.y += bubbleToLaunch.GetComponent<Renderer>().bounds.size.y * bubbleToLaunch.transform.lossyScale.y;
        startWorldPos.z = transform.lossyScale.z;
        bubbleToLaunch.transform.position = startWorldPos;

        Vector3 destWorldPos = Camera.main.ScreenToWorldPoint(destPosition) - startWorldPos;
        destWorldPos.z = startWorldPos.z;
        bubbleDirection = destWorldPos.normalized;
        Rigidbody rb = bubbleToLaunch.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionZ;
        rb.velocity = bubbleDirection * BubbleSpeed;
    }

    public void AddBubbleToGameBoard(Bubble bubble)
    {
        Vector3 position = GameBoard.transform.InverseTransformPoint(bubble.transform.position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        if (GameBoard.HasCellBubble(coordinates.X, coordinates.Y) == false)
        {
            GameBoard.AddBubbleToCoordinates(coordinates.X, coordinates.Y, bubble);
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

        IsGameOver = CheckIsGameOver();
        if (IsGameOver)
            StartCoroutine(GotoGameOver());

        bubbleMaterial = MyMaterial.GetRandomMaterial();
        bubbleToLaunch = null;
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
