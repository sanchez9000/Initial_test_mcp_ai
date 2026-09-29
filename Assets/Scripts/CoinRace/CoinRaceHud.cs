using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Simple overlay HUD for Coin Race scores and end-of-round buttons.
/// </summary>
public class CoinRaceHud : MonoBehaviour
{
    Text _playerScore;
    Text _aiScore;
    Text _status;
    GameObject _resultPanel;
    Font _font;
    Sprite _white;

    public void Build(Transform parent, Font font, Sprite whiteSprite, UnityAction onMenu, UnityAction onRematch)
    {
        _font = font;
        _white = whiteSprite;

        var canvasGo = new GameObject("CoinRaceHUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(parent, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);

        Transform root = canvasGo.transform;
        _playerScore = MakeLabel(root, "PlayerScore", "You: 0", new Vector2(-500, 320), new Color(0.4f, 0.7f, 1f));
        _aiScore = MakeLabel(root, "AiScore", "CPU: 0", new Vector2(500, 320), new Color(1f, 0.4f, 0.4f));
        _status = MakeLabel(root, "Status", "First to 10", new Vector2(0, 320), Color.white);

        MakeButton(root, "BtnMenu", "Menu", new Vector2(-560, -300), new Color(0.35f, 0.35f, 0.4f), onMenu);

        _resultPanel = new GameObject("ResultPanel", typeof(RectTransform), typeof(Image));
        _resultPanel.transform.SetParent(root, false);
        var panelRt = _resultPanel.GetComponent<RectTransform>();
        panelRt.anchorMin = panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(420, 220);
        var panelImg = _resultPanel.GetComponent<Image>();
        panelImg.sprite = _white;
        panelImg.color = new Color(0.1f, 0.1f, 0.14f, 0.92f);
        _resultPanel.SetActive(false);

        var resultTitle = MakeLabel(_resultPanel.transform, "ResultTitle", "You Win!", Vector2.zero, Color.white);
        resultTitle.rectTransform.anchoredPosition = new Vector2(0, 50);
        resultTitle.fontSize = 36;

        MakeButton(_resultPanel.transform, "BtnRematch", "Rematch", new Vector2(0, -20), new Color(0.25f, 0.55f, 0.95f), onRematch);
        MakeButton(_resultPanel.transform, "BtnMenu2", "Menu", new Vector2(0, -90), new Color(0.75f, 0.25f, 0.25f), onMenu);
    }

    public void SetScores(int player, int ai, int winScore)
    {
        if (_playerScore != null) _playerScore.text = "You: " + player;
        if (_aiScore != null) _aiScore.text = "CPU: " + ai;
        if (_status != null && (_resultPanel == null || !_resultPanel.activeSelf))
            _status.text = "First to " + winScore;
    }

    public void ShowResult(bool playerWon)
    {
        if (_resultPanel == null) return;
        _resultPanel.SetActive(true);
        var title = _resultPanel.transform.Find("ResultTitle");
        if (title != null)
        {
            var t = title.GetComponent<Text>();
            if (t != null) t.text = playerWon ? "You Win!" : "CPU Wins!";
        }
        if (_status != null)
            _status.text = playerWon ? "Victory" : "Defeat";
    }

    public void HideResult()
    {
        if (_resultPanel != null)
            _resultPanel.SetActive(false);
    }

    Text MakeLabel(Transform parent, string name, string text, Vector2 pos, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<Text>();
        t.text = text;
        t.font = _font;
        t.fontSize = 28;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
        var rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(280, 40);
        rt.anchoredPosition = pos;
        return t;
    }

    void MakeButton(Transform parent, string name, string label, Vector2 pos, Color color, UnityAction onClick)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(160, 48);
        rt.anchoredPosition = pos;
        var img = go.GetComponent<Image>();
        img.sprite = _white;
        img.color = color;
        go.GetComponent<Button>().onClick.AddListener(onClick);

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
    }
}
