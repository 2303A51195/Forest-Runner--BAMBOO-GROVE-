using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Overlay canvas whose "Root" rectangle always matches the 9:16 game area, so menus line up with the
/// game on any window shape (PC letterbox, tall phones, ...).
/// </summary>
public class UIRoot : MonoBehaviour
{
    public RectTransform root;
    CanvasScaler scaler;

    public static UIRoot Create()
    {
        UIKit.EnsureEventSystem();
        var go = new GameObject("UI", typeof(RectTransform));
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var sc = go.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1080f, 1920f);
        sc.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        go.AddComponent<GraphicRaycaster>();
        var ui = go.AddComponent<UIRoot>();
        ui.scaler = sc;
        ui.root = UIKit.NewRect(go.transform, "Root");
        ui.Fit();
        return ui;
    }

    void Update() { Fit(); }

    void Fit()
    {
        Rect r; float o;
        CameraController.ComputeView(out r, out o);
        root.anchorMin = new Vector2(r.xMin, r.yMin);
        root.anchorMax = new Vector2(r.xMax, r.yMax);
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;
        float window = (float)Screen.width / Mathf.Max(1, Screen.height);
        scaler.matchWidthOrHeight = window > 9f / 16f ? 1f : 0f;
    }
}
