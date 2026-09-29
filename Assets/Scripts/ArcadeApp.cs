using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Single-scene arcade flow: Splash -> Landing -> mini-games.
/// Uses only UI shapes. Compatible with Input System package (activeInputHandler = 1).
/// </summary>
public class ArcadeApp : MonoBehaviour
{
    enum ScreenId { Splash, Landing, ClickSquare, CatchCircle, CoinRace, Slots }

    Transform _root;
    GameObject _arcadeCanvas;
    ScreenId _screen;

    RectTransform _square;
    Text _clickScoreText;
    int _clickScore;
    float _moveTimer;

    Text _catchScoreText;
    int _catchScore;
    float _spawnTimer;
    Sprite _circleSprite;

    Font _font;
    Sprite _whiteSprite;
    CoinRaceGame _coinRace;
    SlotMachineGame _slots;

    void Awake()
    {
        Debug.Log("[ArcadeApp] Awake â€” building UI");
        _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (_font == null)
            _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        _whiteSprite = MakeWhiteSprite();

        EnsureEventSystem();
        BuildCanvas();
        ShowSplash();
    }

    void Update()
    {
        if (_screen == ScreenId.Splash)
        {
            bool pressed = false;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                pressed = true;
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                pressed = true;
            if (pressed)
                ShowLanding();
        }
        else if (_screen == ScreenId.ClickSquare && _square != null)
        {
            _moveTimer += Time.deltaTime;
            if (_moveTimer >= 0.85f)
            {
                _moveTimer = 0f;
                MoveSquare();
            }
        }
        else if (_screen == ScreenId.CatchCircle)
        {
            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= 0.9f)
            {
                _spawnTimer = 0f;
                SpawnCircle();
            }
        }
    }

    void EnsureEventSystem()
    {
        var existing = FindAnyObjectByType<EventSystem>();
        if (existing != null)
        {
            if (existing.GetComponent<InputSystemUIInputModule>() == null)
            {
                var old = existing.GetComponent<BaseInputModule>();
                if (old != null)
                    Destroy(old);
                existing.gameObject.AddComponent<InputSystemUIInputModule>();
            }
            return;
        }

        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<InputSystemUIInputModule>();
    }

    void BuildCanvas()
    {
        var go = new GameObject("ArcadeCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        _arcadeCanvas = go;
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        _root = go.transform;
    }

    void ClearUi()
    {
        if (_root == null) return;
        for (int i = _root.childCount - 1; i >= 0; i--)
            Destroy(_root.GetChild(i).gameObject);
        _square = null;
        _clickScoreText = null;
        _catchScoreText = null;
    }

    void ShowSplash()
    {
        _screen = ScreenId.Splash;
        ClearUi();
        Debug.Log("[ArcadeApp] Showing splash");

        // Full-screen clickable background so splash advances via UI too
        var bg = FullBg(new Color(0.08f, 0.09f, 0.12f));
        var bgBtn = bg.gameObject.AddComponent<Button>();
        bgBtn.transition = Selectable.Transition.None;
        bgBtn.onClick.AddListener(ShowLanding);

        Image("AccentRing", new Color(0.2f, 0.55f, 0.95f), new Vector2(420, 420), Vector2.zero);
        Image("CenterDisc", new Color(0.12f, 0.14f, 0.2f), new Vector2(360, 360), Vector2.zero);
        Image("AccentSquare", new Color(0.95f, 0.55f, 0.15f), new Vector2(90, 90), new Vector2(0, 110));
        Label("Title", "Arcade Sample", 48, Color.white, new Vector2(0, 0), new Vector2(720, 120));
        Label("Hint", "Click or waitâ€¦", 22, new Color(1, 1, 1, 0.55f), new Vector2(0, -160), new Vector2(400, 40));
        StartCoroutine(SplashDelay());
    }

    IEnumerator SplashDelay()
    {
        yield return new WaitForSeconds(2.5f);
        if (_screen == ScreenId.Splash)
            ShowLanding();
    }

    void ShowLanding()
    {
        _screen = ScreenId.Landing;
        if (_arcadeCanvas != null)
            _arcadeCanvas.SetActive(true);
        ClearUi();
        Debug.Log("[ArcadeApp] Showing landing");

        FullBg(new Color(0.1f, 0.12f, 0.16f));
        Label("Title", "Choose a Game", 42, Color.white, new Vector2(0, 220), new Vector2(600, 60));
        Label("Subtitle", "Arcade Sample", 22, new Color(1, 1, 1, 0.6f), new Vector2(0, 170), new Vector2(600, 40));
        Button("BtnClickSquare", "Click the Square", new Vector2(0, 90), new Color(0.25f, 0.55f, 0.95f), ShowClickSquare);
        Button("BtnCatchCircle", "Catch the Circle", new Vector2(0, 20), new Color(0.2f, 0.7f, 0.45f), ShowCatchCircle);
        Button("BtnCoinRace", "Coin Race", new Vector2(0, -50), new Color(0.95f, 0.7f, 0.2f), ShowCoinRace);
        Button("BtnSlots", "Slot Machine", new Vector2(0, -120), new Color(0.7f, 0.35f, 0.85f), ShowSlots);
        Button("BtnExit", "Exit", new Vector2(0, -200), new Color(0.75f, 0.25f, 0.25f), ExitGame);
    }

    void ShowSlots()
    {
        _screen = ScreenId.Slots;
        ClearUi();
        if (_arcadeCanvas != null)
            _arcadeCanvas.SetActive(false);

        if (_slots != null)
        {
            _slots.Teardown();
            _slots = null;
        }

        _slots = SlotMachineGame.StartNew(transform, _font, _whiteSprite, OnSlotsExitToMenu);
    }

    void OnSlotsExitToMenu()
    {
        _slots = null;
        ShowLanding();
    }

    void ShowCoinRace()
    {
        _screen = ScreenId.CoinRace;
        ClearUi();
        if (_arcadeCanvas != null)
            _arcadeCanvas.SetActive(false);

        if (_coinRace != null)
        {
            _coinRace.Teardown();
            _coinRace = null;
        }

        _coinRace = CoinRaceGame.StartNew(transform, _font, _whiteSprite, OnCoinRaceExitToMenu);
    }

    void OnCoinRaceExitToMenu()
    {
        _coinRace = null;
        ShowLanding();
    }

    void ShowClickSquare()
    {
        _screen = ScreenId.ClickSquare;
        _clickScore = 0;
        _moveTimer = 0f;
        ClearUi();

        FullBg(new Color(0.12f, 0.14f, 0.18f));
        Label("Title", "Click the Square", 32, Color.white, new Vector2(0, 240), new Vector2(500, 50));
        _clickScoreText = Label("Score", "Score: 0", 28, new Color(1f, 0.9f, 0.4f), new Vector2(0, 190), new Vector2(300, 40));

        var squareGo = new GameObject("TargetSquare", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        squareGo.transform.SetParent(_root, false);
        _square = squareGo.GetComponent<RectTransform>();
        _square.anchorMin = _square.anchorMax = new Vector2(0.5f, 0.5f);
        _square.pivot = new Vector2(0.5f, 0.5f);
        _square.sizeDelta = new Vector2(100, 100);
        var squareImg = squareGo.GetComponent<Image>();
        squareImg.sprite = _whiteSprite;
        squareImg.color = new Color(0.95f, 0.35f, 0.25f);
        squareGo.GetComponent<Button>().onClick.AddListener(() =>
        {
            _clickScore++;
            if (_clickScoreText != null)
                _clickScoreText.text = "Score: " + _clickScore;
            MoveSquare();
            _moveTimer = 0f;
        });
        MoveSquare();

        CornerButton("BtnBack", "Menu", new Color(0.35f, 0.35f, 0.4f), ShowLanding);
    }

    void ShowCatchCircle()
    {
        _screen = ScreenId.CatchCircle;
        _catchScore = 0;
        _spawnTimer = 0f;
        if (_circleSprite == null)
            _circleSprite = MakeCircleSprite(64);
        ClearUi();

        FullBg(new Color(0.1f, 0.13f, 0.17f));
        Label("Title", "Catch the Circle", 32, Color.white, new Vector2(0, 240), new Vector2(500, 50));
        _catchScoreText = Label("Score", "Caught: 0", 28, new Color(0.5f, 1f, 0.7f), new Vector2(0, 190), new Vector2(300, 40));
        CornerButton("BtnBack", "Menu", new Color(0.35f, 0.35f, 0.4f), ShowLanding);
    }

    void MoveSquare()
    {
        if (_square == null) return;
        _square.anchoredPosition = new Vector2(Random.Range(-320f, 320f), Random.Range(-160f, 140f));
    }

    void SpawnCircle()
    {
        var go = new GameObject("Circle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(_root, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        float size = Random.Range(70f, 120f);
        rt.sizeDelta = new Vector2(size, size);
        rt.anchoredPosition = new Vector2(Random.Range(-300f, 300f), Random.Range(-150f, 120f));
        var image = go.GetComponent<Image>();
        image.sprite = _circleSprite;
        image.color = new Color(Random.Range(0.3f, 1f), Random.Range(0.3f, 1f), Random.Range(0.3f, 1f));
        go.GetComponent<Button>().onClick.AddListener(() =>
        {
            _catchScore++;
            if (_catchScoreText != null)
                _catchScoreText.text = "Caught: " + _catchScore;
            Destroy(go);
        });
        Destroy(go, 1.4f);
    }

    void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    static Sprite MakeWhiteSprite()
    {
        var tex = Texture2D.whiteTexture;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
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
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    Image FullBg(Color color)
    {
        var img = Image("Background", color, Vector2.zero, Vector2.zero);
        var rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return img;
    }

    Image Image(string name, Color color, Vector2 size, Vector2 pos)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(_root, false);
        var img = go.GetComponent<Image>();
        img.sprite = _whiteSprite;
        img.type = UnityEngine.UI.Image.Type.Simple;
        img.color = color;
        img.raycastTarget = true;
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        return img;
    }

    Text Label(string name, string content, int size, Color color, Vector2 pos, Vector2 dims)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(_root, false);
        var text = go.GetComponent<Text>();
        text.text = content;
        text.fontSize = size;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = _font;
        text.raycastTarget = false;
        var rt = text.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = dims;
        rt.anchoredPosition = pos;
        return text;
    }

    void Button(string name, string label, Vector2 pos, Color color, UnityEngine.Events.UnityAction onClick, Vector2? size = null)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(_root, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size ?? new Vector2(360, 70);
        rt.anchoredPosition = pos;
        var img = go.GetComponent<Image>();
        img.sprite = _whiteSprite;
        img.color = color;
        go.GetComponent<Button>().onClick.AddListener(onClick);
        var text = Label(name + "_Label", label, 26, Color.white, Vector2.zero, rt.sizeDelta);
        text.transform.SetParent(go.transform, false);
        var trt = text.rectTransform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
    }

    void CornerButton(string name, string label, Color color, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(_root, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(140, 50);
        rt.anchoredPosition = new Vector2(20, -20);
        var img = go.GetComponent<Image>();
        img.sprite = _whiteSprite;
        img.color = color;
        go.GetComponent<Button>().onClick.AddListener(onClick);
        var text = Label(name + "_Label", label, 22, Color.white, Vector2.zero, rt.sizeDelta);
        text.transform.SetParent(go.transform, false);
        var trt = text.rectTransform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
    }
}
