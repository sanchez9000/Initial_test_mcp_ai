using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Mini-game: click appearing circles before they vanish. Basic UI shapes only.
/// </summary>
public class CatchCircleGame : MonoBehaviour
{
    [SerializeField] float spawnInterval = 0.9f;
    [SerializeField] float circleLifetime = 1.4f;

    Text _scoreText;
    int _score;
    float _spawnTimer;
    Transform _canvasRoot;
    Sprite _circleSprite;

    void Update()
    {
        if (_canvasRoot == null) return;
        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= spawnInterval)
        {
            _spawnTimer = 0f;
            SpawnCircle();
        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Landing");
    }

    void SpawnCircle()
    {
        var go = new GameObject("Circle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(_canvasRoot, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        float size = Random.Range(70f, 120f);
        rt.sizeDelta = new Vector2(size, size);
        rt.anchoredPosition = new Vector2(Random.Range(-300f, 300f), Random.Range(-150f, 120f));

        var image = go.GetComponent<Image>();
        image.sprite = _circleSprite;
        image.color = new Color(Random.Range(0.3f, 1f), Random.Range(0.3f, 1f), Random.Range(0.3f, 1f), 1f);

        var button = go.GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            _score++;
            if (_scoreText != null)
                _scoreText.text = "Caught: " + _score;
            Destroy(go);
        });

        Destroy(go, circleLifetime);
    }

    public static void BuildUi(Transform canvasRoot, CatchCircleGame game)
    {
        game._canvasRoot = canvasRoot;
        game._circleSprite = MakeCircleSprite(64);

        var bg = CreateImage(canvasRoot, "Background", new Color(0.1f, 0.13f, 0.17f, 1f));
        StretchFull(bg.rectTransform);

        var title = CreateText(canvasRoot, "Title", "Catch the Circle", 32, Color.white);
        title.rectTransform.anchoredPosition = new Vector2(0, 240);
        title.rectTransform.sizeDelta = new Vector2(500, 50);

        var score = CreateText(canvasRoot, "Score", "Caught: 0", 28, new Color(0.5f, 1f, 0.7f));
        score.rectTransform.anchoredPosition = new Vector2(0, 190);
        score.rectTransform.sizeDelta = new Vector2(300, 40);
        game._scoreText = score;

        CreateBackButton(canvasRoot, game.BackToMenu);
    }

    static Sprite MakeCircleSprite(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float r = size * 0.5f;
        float r2 = r * r;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x + 0.5f - r;
                float dy = y + 0.5f - r;
                bool inside = dx * dx + dy * dy <= r2;
                tex.SetPixel(x, y, inside ? Color.white : Color.clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
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
