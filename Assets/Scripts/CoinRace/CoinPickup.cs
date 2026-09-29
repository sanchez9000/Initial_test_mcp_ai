using UnityEngine;

/// <summary>
/// Coin collectible. Notifies CoinRaceGame on contact with player or AI.
/// </summary>
public class CoinPickup : MonoBehaviour
{
    public CoinRaceGame game;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (game == null) return;
        bool isPlayer = other.GetComponent<ClickToMoveAgent>() != null;
        bool isAi = other.GetComponent<CoinRaceAI>() != null;
        if (isPlayer || isAi)
            game.OnCoinCollected(this, other.gameObject);
    }
}
