using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Basic 3-reel slot machine. Starts with 100 coins.
/// Symbols: OpenGameArt CC-BY (Cougarmint). UI chrome: Kenney CC0.
/// </summary>
public class SlotMachineGame : MonoBehaviour
{
    const int StartingCoins = 100;

    struct SymbolDef
    {
        public string Id;
        public Sprite Sprite;
        public int Weight;
        public int TriplePayout; // multiplier of bet for 3-of-a-kind
    }

    int _coins = StartingCoins;
    int _bet = 5;
    bool _spinning;

    SymbolDef[] _symbols;
    Image[] _reelImages = new Image[3];
    int[] _reelResults = new int[3];

    Text _coinsText;
    Text _betText;
    Text _resultText;
    Button _spinButton;
    Sprite _white;
    Font _font;
    System.Action _onExit;

    public static SlotMachineGame StartNew(Transform host, Font font, Sprite whiteUi, System.Action onExit)
    {
        var go = new GameObject("SlotMachineGame");
        if (host != null) go.transform.SetParent(host, false);
        var game = go.AddComponent<SlotMachineGame>();
        game._font = font;
        game._white = whiteUi;
        game._onExit = onExit;
        game.Build();
        return game;
    }

    void Build()
    {
        LoadSymbols();
        BuildUi();
        RefreshHud("Good luck!");
        Debug.Log("[Slots] Ready with " + _coins + " coins. Symbols loaded: " + (_symbols != null ? _symbols.Length : 0));
    }

    void LoadSymbols()
    {
        var loaded = Resources.LoadAll<Sprite>("Slots/Symbols");
        var list = new List<SymbolDef>();

        void Add(string id, string fileHint, int weight, int payout)
        {
            Sprite s = null;
            foreach (var sp in loaded)
            {
                if (sp.name.Equals(fileHint, System.StringComparison.OrdinalIgnoreCase)
                    || sp.name.IndexOf(fileHint, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    s = sp;
                    break;
                }
            }
            if (s == null && loaded.Length > 0)
                s = loaded[list.Count % loaded.Length];
            if (s == null) return;
            list.Add(new SymbolDef { Id = id, Sprite = s, Weight = weight, TriplePayout = payout });
        }

        // Weights favor commons; 7 / bars pay more
        Add("cherry", "cherries", 18, 5);
        Add("lemon", "lemon", 16, 6);
        Add("melon", "melon", 14, 8);
        Add("bell", "bell", 12, 10);
        Add("clover", "clover", 10, 12);
        Add("heart", "heart", 10, 12);
        Add("horseshoe", "horseshoe", 8, 15);
        Add("bar1", "Bar1", 7, 20);
        Add("bar2", "Bar2", 5, 30);
        Add("bar3", "Bar3", 4, 40);
        Add("seven", "Lucky7", 3, 80);

        if (list.Count == 0)
        {
            // Fallback colored squares if sprites not imported yet
            Debug.LogWarning("[Slots] No symbol sprites found in Resources/Slots/Symbols — using fallback colors.");
            var fallback = MakeColorSprite(Color.white);
            list.Add(new SymbolDef { Id = "A", Sprite = fallback, Weight = 10, TriplePayout = 5 });
            list.Add(new SymbolDef { Id = "B", Sprite = fallback, Weight = 8, TriplePayout = 10 });
            list.Add(new SymbolDef { Id = "C", Sprite = fallback, Weight = 5, TriplePayout = 25 });
        }

        _symbols = list.ToArray();
    }

    void BuildUi()
    {
        var canvasGo = new GameObject("SlotsCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        Transform root = canvasGo.transform;

        Sprite panelSprite = LoadUiSprite("button_rectangle_depth_flat") ?? _white;
        Sprite buttonSprite = LoadUiSprite("button_rectangle_depth_gloss") ?? _white;

        // Background
        MakeImage(root, "Bg", _white, new Color(0.08f, 0.1f, 0.14f, 1f), stretch: true);

        // Title
        MakeLabel(root, "Title", "SLOT MACHINE", 40, Color.white, new Vector2(0, 300), new Vector2(600, 50));

        // Machine panel
        var panel = MakeImage(root, "MachinePanel", panelSprite, new Color(0.25f, 0.28f, 0.35f, 1f), new Vector2(0, 40), new Vector2(720, 340));

        // Three reel windows
        float[] xs = { -200f, 0f, 200f };
        for (int i = 0; i < 3; i++)
        {
            var frame = MakeImage(panel.transform, "ReelFrame" + i, panelSprite, new Color(0.12f, 0.12f, 0.16f, 1f), new Vector2(xs[i], 0), new Vector2(170, 170));
            var icon = MakeImage(frame.transform, "Symbol", _symbols[0].Sprite, Color.white, Vector2.zero, new Vector2(140, 140));
            _reelImages[i] = icon;
            _reelResults[i] = 0;
        }

        // HUD
        _coinsText = MakeLabel(root, "Coins", "Coins: 100", 28, new Color(1f, 0.9f, 0.3f), new Vector2(-420, 300), new Vector2(280, 40));
        _betText = MakeLabel(root, "Bet", "Bet: 5", 28, Color.white, new Vector2(420, 300), new Vector2(240, 40));
        _resultText = MakeLabel(root, "Result", "Good luck!", 26, new Color(0.8f, 1f, 0.8f), new Vector2(0, -160), new Vector2(800, 40));

        // Bet controls
        MakeButton(root, "Bet1", "Bet 1", new Vector2(-260, -230), new Color(0.35f, 0.45f, 0.7f), buttonSprite, () => SetBet(1));
        MakeButton(root, "Bet5", "Bet 5", new Vector2(-90, -230), new Color(0.35f, 0.55f, 0.75f), buttonSprite, () => SetBet(5));
        MakeButton(root, "Bet10", "Bet 10", new Vector2(80, -230), new Color(0.4f, 0.5f, 0.8f), buttonSprite, () => SetBet(10));

        _spinButton = MakeButton(root, "Spin", "SPIN", new Vector2(280, -230), new Color(0.2f, 0.75f, 0.35f), buttonSprite, OnSpinClicked);
        var spinRt = _spinButton.GetComponent<RectTransform>();
        spinRt.sizeDelta = new Vector2(180, 64);

        MakeButton(root, "Menu", "Menu", new Vector2(-520, -300), new Color(0.45f, 0.3f, 0.3f), buttonSprite, ExitToMenu);

        // Paytable hint
        MakeLabel(root, "PayHint", "3 matching symbols pays (x bet). Sevens pay big!", 18, new Color(1, 1, 1, 0.55f), new Vector2(0, -300), new Vector2(700, 30));
    }

    Sprite LoadUiSprite(string nameHint)
    {
        var all = Resources.LoadAll<Sprite>("Slots/UI");
        foreach (var s in all)
            if (s.name.IndexOf(nameHint, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return s;
        return null;
    }

    void SetBet(int bet)
    {
        if (_spinning) return;
        _bet = bet;
        RefreshHud(null);
    }

    void OnSpinClicked()
    {
        if (_spinning) return;
        if (_coins < _bet)
        {
            RefreshHud("Not enough coins!");
            return;
        }
        StartCoroutine(SpinRoutine());
    }

    IEnumerator SpinRoutine()
    {
        _spinning = true;
        _spinButton.interactable = false;
        _coins -= _bet;
        RefreshHud("Spinning…");

        // Pick final results with weights
        for (int i = 0; i < 3; i++)
            _reelResults[i] = WeightedPick();

        // Animate reels staggered
        float[] durations = { 0.9f, 1.25f, 1.6f };
        Coroutine[] anims = new Coroutine[3];
        for (int i = 0; i < 3; i++)
            anims[i] = StartCoroutine(AnimateReel(i, durations[i], _reelResults[i]));
        for (int i = 0; i < 3; i++)
            yield return anims[i];

        int payout = EvaluatePayout();
        if (payout > 0)
        {
            _coins += payout;
            RefreshHud("WIN +" + payout + "!");
        }
        else
        {
            RefreshHud("No win — try again");
        }

        _spinning = false;
        _spinButton.interactable = true;

        if (_coins <= 0)
            RefreshHud("Out of coins! Come back from the menu.");
    }

    IEnumerator AnimateReel(int reel, float duration, int finalIndex)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            int idx = Random.Range(0, _symbols.Length);
            _reelImages[reel].sprite = _symbols[idx].Sprite;
            yield return null;
        }
        _reelImages[reel].sprite = _symbols[finalIndex].Sprite;
    }

    int WeightedPick()
    {
        int total = 0;
        foreach (var s in _symbols) total += s.Weight;
        int r = Random.Range(0, total);
        int acc = 0;
        for (int i = 0; i < _symbols.Length; i++)
        {
            acc += _symbols[i].Weight;
            if (r < acc) return i;
        }
        return 0;
    }

    int EvaluatePayout()
    {
        int a = _reelResults[0], b = _reelResults[1], c = _reelResults[2];
        if (a == b && b == c)
            return _bet * _symbols[a].TriplePayout;

        // Two matching cherries / sevens consolations
        if (a == b || b == c || a == c)
        {
            int match = a == b || a == c ? a : b;
            if (_symbols[match].Id == "cherry")
                return _bet * 2;
            if (_symbols[match].Id == "seven")
                return _bet * 3;
        }
        return 0;
    }

    void RefreshHud(string message)
    {
        if (_coinsText != null) _coinsText.text = "Coins: " + _coins;
        if (_betText != null) _betText.text = "Bet: " + _bet;
        if (message != null && _resultText != null) _resultText.text = message;
    }

    void ExitToMenu()
    {
        if (_spinning) return;
        Destroy(gameObject);
        _onExit?.Invoke();
    }

    public void Teardown()
    {
        Destroy(gameObject);
    }

    // --- UI helpers ---

    Image MakeImage(Transform parent, string name, Sprite sprite, Color color, Vector2 pos = default, Vector2 size = default, bool stretch = false)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = sprite != null ? sprite : _white;
        img.color = color;
        img.type = Image.Type.Simple;
        img.preserveAspect = true;
        var rt = img.rectTransform;
        if (stretch)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
        else
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size == default ? new Vector2(100, 100) : size;
            rt.anchoredPosition = pos;
        }
        return img;
    }

    Text MakeLabel(Transform parent, string name, string text, int size, Color color, Vector2 pos, Vector2 dims)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<Text>();
        t.text = text;
        t.font = _font;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
        var rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = dims;
        rt.anchoredPosition = pos;
        return t;
    }

    Button MakeButton(Transform parent, string name, string label, Vector2 pos, Color color, Sprite sprite, UnityAction onClick)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(140, 52);
        rt.anchoredPosition = pos;
        var img = go.GetComponent<Image>();
        img.sprite = sprite != null ? sprite : _white;
        img.color = color;
        img.type = Image.Type.Sliced;
        var btn = go.GetComponent<Button>();
        btn.onClick.AddListener(onClick);

        var textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(go.transform, false);
        var text = textGo.GetComponent<Text>();
        text.text = label;
        text.font = _font;
        text.fontSize = 22;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false;
        var trt = text.rectTransform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        return btn;
    }

    static Sprite MakeColorSprite(Color c)
    {
        var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
            tex.SetPixel(x, y, c);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8);
    }
}
