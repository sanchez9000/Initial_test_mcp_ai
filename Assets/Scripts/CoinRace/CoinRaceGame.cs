using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Top-down coin race bootstrap: arena, player, AI, coins, HUD, win/rematch.
/// </summary>
public class CoinRaceGame : MonoBehaviour
{
    public int winScore = 10;
    public int activeCoinCount = 8;
    public float arenaHalfExtent = 9f;

    readonly List<CoinPickup> _coins = new List<CoinPickup>();
    public List<CoinPickup> ActiveCoins => _coins;

    Transform _worldRoot;
    Camera _raceCam;
    Camera _previousMain;
    GameObject _player;
    GameObject _ai;
    CoinRaceHud _hud;
    Sprite _circleSprite;
    Sprite _squareSprite;
    Font _font;
    Sprite _whiteUi;

    int _playerScore;
    int _aiScore;
    bool _roundOver;

    System.Action _onExitToMenu;

    public static CoinRaceGame StartNew(Transform host, Font font, Sprite whiteUi, System.Action onExitToMenu)
    {
        var go = new GameObject("CoinRaceGame");
        if (host != null)
            go.transform.SetParent(host, false);
        var game = go.AddComponent<CoinRaceGame>();
        game._font = font;
        game._whiteUi = whiteUi;
        game._onExitToMenu = onExitToMenu;
        game.Build();
        return game;
    }

    void Build()
    {
        _circleSprite = MakeCircleSprite(64);
        _squareSprite = MakeSquareSprite(8);
        _previousMain = Camera.main;

        _worldRoot = new GameObject("CoinRaceWorld").transform;
        _worldRoot.SetParent(transform, false);

        BuildCamera();
        BuildGround();
        _player = BuildAgent("Player", new Color(0.25f, 0.55f, 1f), new Vector2(-4f, -3f), true);
        _ai = BuildAgent("CPU", new Color(1f, 0.3f, 0.3f), new Vector2(4f, 3f), false);

        var hudHost = new GameObject("HudHost");
        hudHost.transform.SetParent(transform, false);
        _hud = hudHost.AddComponent<CoinRaceHud>();
        _hud.Build(hudHost.transform, _font, _whiteUi, ExitToMenu, Rematch);

        SpawnInitialCoins();
        _hud.SetScores(0, 0, winScore);
        Debug.Log("[CoinRace] Started — click ground to move. First to " + winScore);
    }

    void BuildCamera()
    {
        var camGo = new GameObject("CoinRaceCamera");
        camGo.transform.SetParent(_worldRoot, false);
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        _raceCam = camGo.AddComponent<Camera>();
        _raceCam.orthographic = true;
        _raceCam.orthographicSize = 10f;
        _raceCam.clearFlags = CameraClearFlags.SolidColor;
        _raceCam.backgroundColor = new Color(0.12f, 0.14f, 0.16f);
        _raceCam.depth = 50;
        _raceCam.tag = "MainCamera";

        if (_previousMain != null && _previousMain != _raceCam)
            _previousMain.enabled = false;
    }

    void BuildGround()
    {
        var ground = new GameObject("Ground");
        ground.transform.SetParent(_worldRoot, false);
        ground.transform.localScale = new Vector3(arenaHalfExtent * 2f, arenaHalfExtent * 2f, 1f);
        var sr = ground.AddComponent<SpriteRenderer>();
        sr.sprite = _squareSprite;
        sr.color = new Color(0.18f, 0.22f, 0.2f);
        sr.sortingOrder = -10;
        // Collider used only for click ray intersection (not for movement blocking)
        var col = ground.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
        col.isTrigger = true;
        ground.layer = 0;

        // Soft border markers (non-blocking visuals)
        CreateBorderMark(new Vector2(0f, arenaHalfExtent), new Vector2(arenaHalfExtent * 2f, 0.15f));
        CreateBorderMark(new Vector2(0f, -arenaHalfExtent), new Vector2(arenaHalfExtent * 2f, 0.15f));
        CreateBorderMark(new Vector2(arenaHalfExtent, 0f), new Vector2(0.15f, arenaHalfExtent * 2f));
        CreateBorderMark(new Vector2(-arenaHalfExtent, 0f), new Vector2(0.15f, arenaHalfExtent * 2f));
    }

    void CreateBorderMark(Vector2 pos, Vector2 scale)
    {
        var go = new GameObject("Border");
        go.transform.SetParent(_worldRoot, false);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(scale.x, scale.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = _squareSprite;
        sr.color = new Color(0.35f, 0.4f, 0.38f);
        sr.sortingOrder = -9;
    }

    GameObject BuildAgent(string name, Color color, Vector2 pos, bool isPlayer)
    {
        var go = new GameObject(name);
        go.transform.SetParent(_worldRoot, false);
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = _circleSprite;
        sr.color = color;
        sr.sortingOrder = 5;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;

        var mover = go.AddComponent<RaycastPathMover>();
        mover.speed = isPlayer ? 6.5f : 5.8f;
        mover.arenaHalfExtent = arenaHalfExtent - 0.5f;

        if (isPlayer)
        {
            var lineGo = new GameObject("PathLine");
            lineGo.transform.SetParent(go.transform, false);
            var lr = lineGo.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.startWidth = 0.06f;
            lr.endWidth = 0.02f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = new Color(0.4f, 0.8f, 1f, 0.8f);
            lr.endColor = new Color(0.4f, 0.8f, 1f, 0.1f);
            lr.sortingOrder = 4;
            mover.pathLine = lr;

            var click = go.AddComponent<ClickToMoveAgent>();
            click.worldCamera = _raceCam;
        }
        else
        {
            var ai = go.AddComponent<CoinRaceAI>();
            ai.game = this;
        }

        return go;
    }

    void SpawnInitialCoins()
    {
        for (int i = 0; i < activeCoinCount; i++)
            SpawnCoin();
    }

    void SpawnCoin()
    {
        Vector2 pos = RandomFreePoint();
        var go = new GameObject("Coin");
        go.transform.SetParent(_worldRoot, false);
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * 0.55f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = _circleSprite;
        sr.color = new Color(1f, 0.85f, 0.2f);
        sr.sortingOrder = 3;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.45f;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;

        var coin = go.AddComponent<CoinPickup>();
        coin.game = this;
        _coins.Add(coin);
    }

    Vector2 RandomFreePoint()
    {
        for (int attempt = 0; attempt < 30; attempt++)
        {
            Vector2 p = new Vector2(
                Random.Range(-arenaHalfExtent + 1f, arenaHalfExtent - 1f),
                Random.Range(-arenaHalfExtent + 1f, arenaHalfExtent - 1f));
            bool farEnough = true;
            if (_player != null && Vector2.Distance(p, _player.transform.position) < 1.2f) farEnough = false;
            if (_ai != null && Vector2.Distance(p, _ai.transform.position) < 1.2f) farEnough = false;
            foreach (var c in _coins)
            {
                if (c != null && Vector2.Distance(p, c.transform.position) < 1f)
                {
                    farEnough = false;
                    break;
                }
            }
            if (farEnough) return p;
        }
        return Vector2.zero;
    }

    public void OnCoinCollected(CoinPickup coin, GameObject collector)
    {
        if (_roundOver || coin == null) return;

        _coins.Remove(coin);
        Destroy(coin.gameObject);

        bool player = collector.GetComponent<ClickToMoveAgent>() != null;
        if (player) _playerScore++;
        else _aiScore++;

        _hud.SetScores(_playerScore, _aiScore, winScore);
        SpawnCoin();

        var ai = _ai != null ? _ai.GetComponent<CoinRaceAI>() : null;
        if (ai != null) ai.ForceRethink();

        if (_playerScore >= winScore)
            EndRound(true);
        else if (_aiScore >= winScore)
            EndRound(false);
    }

    void EndRound(bool playerWon)
    {
        _roundOver = true;
        SetAgentsFrozen(true);
        _hud.ShowResult(playerWon);
        Debug.Log(playerWon ? "[CoinRace] Player wins!" : "[CoinRace] AI wins!");
    }

    void SetAgentsFrozen(bool frozen)
    {
        if (_player != null)
        {
            var m = _player.GetComponent<RaycastPathMover>();
            if (m != null) m.SetFrozen(frozen);
            var c = _player.GetComponent<ClickToMoveAgent>();
            if (c != null) c.SetInputEnabled(!frozen);
        }
        if (_ai != null)
        {
            var m = _ai.GetComponent<RaycastPathMover>();
            if (m != null) m.SetFrozen(frozen);
            var brain = _ai.GetComponent<CoinRaceAI>();
            if (brain != null) brain.SetBrainEnabled(!frozen);
        }
    }

    public void Rematch()
    {
        _playerScore = 0;
        _aiScore = 0;
        _roundOver = false;
        _hud.HideResult();
        _hud.SetScores(0, 0, winScore);

        foreach (var c in _coins)
            if (c != null) Destroy(c.gameObject);
        _coins.Clear();

        if (_player != null) _player.transform.position = new Vector2(-4f, -3f);
        if (_ai != null) _ai.transform.position = new Vector2(4f, 3f);

        SetAgentsFrozen(false);
        SpawnInitialCoins();
        var ai = _ai != null ? _ai.GetComponent<CoinRaceAI>() : null;
        if (ai != null) ai.ForceRethink();
    }

    public void ExitToMenu()
    {
        Teardown();
        _onExitToMenu?.Invoke();
    }

    public void Teardown()
    {
        if (_previousMain != null)
            _previousMain.enabled = true;
        Destroy(gameObject);
    }

    static Sprite MakeCircleSprite(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float r = size * 0.5f;
        float r2 = r * r;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = x + 0.5f - r;
            float dy = y + 0.5f - r;
            tex.SetPixel(x, y, dx * dx + dy * dy <= r2 ? Color.white : Color.clear);
        }
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    static Sprite MakeSquareSprite(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
            tex.SetPixel(x, y, Color.white);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
