using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Mini-game: click the moving colored square to score. Basic UI shapes only.
/// </summary>
public class ClickSquareGame : MonoBehaviour
{
    [SerializeField] float moveInterval = 0.85f;

    RectTransform _square;
    Text _scoreText;
    int _score;
    float _timer;

    void Start()
    {
        // Built at runtime if scene was empty-setup via builder.
    }

    void Update()
    {
        if (_square == null) return;
        _timer += Time.deltaTime;
        if (_timer >= moveInterval)
        {
            _timer = 0f;
            MoveSquare();
        }
    }

    public void OnSquareClicked()
    {
        _score++;
        if (_scoreText != null)
            _scoreText.text = "Score: " + _score;
        MoveSquare();
        _timer = 0f;
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Landing");
    }

    void MoveSquare()
    {
        if (_square == null) return;
        float x = Random.Range(-320f, 320f);
        float y = Random.Range(-160f, 140f);
        _square.anchoredPosition = new Vector2(x, y);
    }

    public static void BuildUi(Transform canvasRoot, ClickSquareGame game)
    {
        var bg = CreateImage(canvasRoot, "Background", new Color(0.12f, 0.14f, 0.18f, 1f));
        StretchFull(bg.rectTransform);

        var title = CreateText(canvasRoot, "Title", "Click the Square", 32, Color.white);
        title.rectTransform.anchoredPosition = new Vector2(0, 240);
        title.rectTransform.sizeDelta = new Vector2(500, 50);

        var score = CreateText(canvasRoot, "Score", "Score: 0", 28, new Color(1f, 0.9f, 0.4f));
        score.rectTransform.anchoredPosition = new Vector2(0, 190);
        score.rectTransform.sizeDelta = new Vector2(300, 40);
        game._scoreText = score;

        var squareGo = new GameObject("TargetSquare", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        squareGo.transform.SetParent(canvasRoot, false);
        var squareRt = squareGo.GetComponent<RectTransform>();
        squareRt.anchorMin = squareRt.anchorMax = new Vector2(0.5f, 0.5f);
        squareRt.pivot = new Vector2(0.5f, 0.5f);
        squareRt.sizeDelta = new Vector2(100, 100);
        squareRt.anchoredPosition = Vector2.zero;
        var squareImg = squareGo.GetComponent<Image>();
        squareImg.color = new Color(0.95f, 0.35f, 0.25f, 1f);
        var squareBtn = squareGo.GetComponent<Button>();
        squareBtn.onClick.AddListener(game.OnSquareClicked);
        game._square = squareRt;

        CreateBackButton(canvasRoot, game.BackToMenu);
    }

    static void CreateBackButton(Transform parent, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject("BtnBack", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(140, 50);
        rt.anchoredPosition = new Vector2(20, -20);
        go.GetComponent<Image>().color = new Color(0.35f, 0.35f, 0.4f, 1f);
        go.GetComponent<Button>().onClick.AddListener(onClick);
        var label = CreateText(go.transform, "Label", "Menu", 22, Color.white);
        StretchFull(label.rectTransform);
        label.alignment = TextAnchor.MiddleCenter;
    }

    static Image CreateImage(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        return image;
    }

    static Text CreateText(Transform parent, string name, string content, int size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        text.text = content;
        text.fontSize = size;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        var rt = text.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        return text;
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
