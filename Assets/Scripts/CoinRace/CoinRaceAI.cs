using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Old-school bot: periodically picks a coin (nearest, sometimes randomized among top-K).
/// </summary>
[RequireComponent(typeof(RaycastPathMover))]
public class CoinRaceAI : MonoBehaviour
{
    public CoinRaceGame game;
    public float rethinkInterval = 0.35f;
    public float randomizeChance = 0.35f;
    public int topK = 3;

    RaycastPathMover _mover;
    float _timer;
    bool _enabled = true;

    void Awake()
    {
        _mover = GetComponent<RaycastPathMover>();
    }

    public void SetBrainEnabled(bool enabled)
    {
        _enabled = enabled;
        if (!enabled)
            _mover.ClearDestination();
    }

    void Update()
    {
        if (!_enabled || game == null) return;
        _timer += Time.deltaTime;
        if (_timer < rethinkInterval) return;
        _timer = 0f;
        ChooseTarget();
    }

    public void ForceRethink()
    {
        _timer = rethinkInterval;
    }

    void ChooseTarget()
    {
        if (game.ActiveCoins == null || game.ActiveCoins.Count == 0)
        {
            Vector2 rnd = new Vector2(Random.Range(-8f, 8f), Random.Range(-8f, 8f));
            _mover.SetDestination(rnd);
            return;
        }

        // Copy then sort by distance (don't mutate the live list)
        var coins = new List<CoinPickup>();
        foreach (var c in game.ActiveCoins)
            if (c != null) coins.Add(c);
        if (coins.Count == 0)
        {
            Vector2 rnd = new Vector2(Random.Range(-8f, 8f), Random.Range(-8f, 8f));
            _mover.SetDestination(rnd);
            return;
        }

        Vector2 pos = transform.position;
        coins.Sort((a, b) =>
        {
            float da = ((Vector2)a.transform.position - pos).sqrMagnitude;
            float db = ((Vector2)b.transform.position - pos).sqrMagnitude;
            return da.CompareTo(db);
        });

        int pickIndex = 0;
        int candidates = Mathf.Min(topK, coins.Count);
        if (candidates > 1 && Random.value < randomizeChance)
            pickIndex = Random.Range(0, candidates);

        // Optional short raycast "look" toward chosen coin for flavor / blocked check
        Vector2 target = coins[pickIndex].transform.position;
        Vector2 dir = (target - pos).normalized;
        RaycastHit2D look = Physics2D.Raycast(pos, dir, 12f);
        if (look.collider != null && look.collider.GetComponent<CoinPickup>() == null && look.collider.gameObject != gameObject)
        {
            // Try next candidate if first is blocked
            for (int i = 0; i < candidates; i++)
            {
                Vector2 t = coins[i].transform.position;
                Vector2 d = (t - pos).normalized;
                RaycastHit2D h = Physics2D.Raycast(pos, d, 12f);
                if (h.collider == null || h.collider.GetComponent<CoinPickup>() != null)
                {
                    target = t;
                    break;
                }
            }
        }

        _mover.SetDestination(target);
    }
}
