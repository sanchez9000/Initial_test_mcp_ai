using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Splash screen: centered "Arcade Sample" title, auto-advances to Landing.
/// </summary>
public class SplashController : MonoBehaviour
{
    [SerializeField] float displaySeconds = 2.5f;
    [SerializeField] string nextScene = "Landing";

    bool _advancing;

    void Start()
    {
        StartCoroutine(AdvanceAfterDelay());
    }

    void Update()
    {
        if (_advancing) return;
        if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
            GoNext();
    }

    IEnumerator AdvanceAfterDelay()
    {
        yield return new WaitForSeconds(displaySeconds);
        GoNext();
    }

    void GoNext()
    {
        if (_advancing) return;
        _advancing = true;
        SceneManager.LoadScene(nextScene);
    }

    public static void BuildUi(Transform canvasRoot)
    {
        var bg = CreateUiImage(canvasRoot, "Background", new Color(0.08f, 0.09f, 0.12f, 1f));
        StretchFull(bg.rectTransform);

        var ring = CreateUiImage(canvasRoot, "AccentRing", new Color(0.2f, 0.55f, 0.95f, 1f));
        ring.rectTransform.sizeDelta = new Vector2(420, 420);
        Center(ring.rectTransform);

        var disc = CreateUiImage(canvasRoot, "CenterDisc", new Color(0.12f, 0.14f, 0.2f, 1f));
        disc.rectTransform.sizeDelta = new Vector2(360, 360);
        Center(disc.rectTransform);

        var square = CreateUiImage(canvasRoot, "AccentSquare", new Color(0.95f, 0.55f, 0.15f, 1f));
        square.rectTransform.sizeDelta = new Vector2(90, 90);
        square.rectTransform.anchoredPosition = new Vector2(0, 110);
        CenterPivot(square.rectTransform);

        var titleGo = new GameObject("Title", typeof(RectTransform), typeof(Text));
        titleGo.transform.SetParent(canvasRoot, false);
        var title = titleGo.GetComponent<Text>();
        title.text = "Arcade Sample";
        title.alignment = TextAnchor.MiddleCenter;
        title.color = Color.white;
        title.fontSize = 48;
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (title.font == null)
            title.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        var titleRt = title.rectTransform;
        titleRt.sizeDelta = new Vector2(700, 120);
        Center(titleRt);

        var hintGo = new GameObject("Hint", typeof(RectTransform), typeof(Text));
        hintGo.transform.SetParent(canvasRoot, false);
        var hint = hintGo.GetComponent<Text>();
        hint.text = "Click or waitâ€¦";
        hint.alignment = TextAnchor.MiddleCenter;
        hint.color = new Color(1f, 1f, 1f, 0.55f);
        hint.fontSize = 22;
        hint.font = title.font;
        var hintRt = hint.rectTransform;
        hintRt.sizeDelta = new Vector2(400, 40);
        hintRt.anchoredPosition = new Vector2(0, -160);
        CenterPivot(hintRt);
    }

    static Image CreateUiImage(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        return image;
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void Center(RectTransform rt)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
    }

    static void CenterPivot(RectTransform rt)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
    }
}
