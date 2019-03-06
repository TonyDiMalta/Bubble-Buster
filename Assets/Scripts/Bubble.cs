using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// A class that each bubble in the game uses
    /// </summary>
    public class Bubble : MonoBehaviour
    {
        public string colorName;
        public bool isOnBoard = false;

        void OnCollisionEnter(Collision collision)
        {
            if (isOnBoard == false &&
                collision.gameObject.tag == "Bubble")
            {
                var gameManager = GameObject.FindGameObjectWithTag("GameController");
                gameManager.GetComponent<GameManager>().AddBubbleToGameBoard(this);
            }
        }
    }
}
