using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Landing menu: load simple games or quit.
/// </summary>
public class LandingMenuController : MonoBehaviour
{
    public void LoadClickSquare()
    {
        SceneManager.LoadScene("GameClickSquare");
    }

    public void LoadCatchCircle()
    {
        SceneManager.LoadScene("GameCatchCircle");
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public static void BuildUi(Transform canvasRoot, LandingMenuController controller)
    {
        var bg = CreateImage(canvasRoot, "Background", new Color(0.1f, 0.12f, 0.16f, 1f));
        StretchFull(bg.rectTransform);

        var banner = CreateImage(canvasRoot, "Banner", new Color(0.18f, 0.22f, 0.32f, 1f));
        banner.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        banner.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        banner.rectTransform.pivot = new Vector2(0.5f, 1f);
        banner.rectTransform.sizeDelta = new Vector2(900, 120);
        banner.rectTransform.anchoredPosition = new Vector2(0, -40);

        var title = CreateText(canvasRoot, "Title", "Choose a Game", 42, Color.white);
        title.rectTransform.anchoredPosition = new Vector2(0, 220);
        title.rectTransform.sizeDelta = new Vector2(600, 60);

        var subtitle = CreateText(canvasRoot, "Subtitle", "Arcade Sample", 22, new Color(1f, 1f, 1f, 0.6f));
        subtitle.rectTransform.anchoredPosition = new Vector2(0, 170);
        subtitle.rectTransform.sizeDelta = new Vector2(600, 40);

        CreateMenuButton(canvasRoot, "BtnClickSquare", "Click the Square", new Vector2(0, 40), new Color(0.25f, 0.55f, 0.95f), controller.LoadClickSquare);
        CreateMenuButton(canvasRoot, "BtnCatchCircle", "Catch the Circle", new Vector2(0, -50), new Color(0.2f, 0.7f, 0.45f), controller.LoadCatchCircle);
        CreateMenuButton(canvasRoot, "BtnExit", "Exit", new Vector2(0, -160), new Color(0.75f, 0.25f, 0.25f), controller.ExitGame);
    }

    static void CreateMenuButton(Transform parent, string name, string label, Vector2 pos, Color color, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(360, 70);
        rt.anchoredPosition = pos;

        var image = go.GetComponent<Image>();
        image.color = color;

        var button = go.GetComponent<Button>();
        var colors = button.colors;
        colors.highlightedColor = color * 1.15f;
        colors.pressedColor = color * 0.85f;
        button.colors = colors;
        button.onClick.AddListener(onClick);

        var text = CreateText(go.transform, "Label", label, 26, Color.white);
        StretchFull(text.rectTransform);
        text.alignment = TextAnchor.MiddleCenter;
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
