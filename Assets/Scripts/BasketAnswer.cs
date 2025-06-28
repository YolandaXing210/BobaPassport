using UnityEngine;

public class BasketAnswer : MonoBehaviour
{
    public int basketIndex; // 0 to 3
    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball")) // Tag your basketballs!
        {
            gameManager.BasketScored(this);
        }
    }
}
