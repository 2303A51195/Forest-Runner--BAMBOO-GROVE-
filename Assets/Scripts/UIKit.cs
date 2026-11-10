using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

/// <summary>Small helpers that build uGUI elements (panels, labels, buttons) from code.</summary>
public static class UIKit
{
    static Font face;

    public static Font Face
    {
        get
        {
            if (face == null)
            {
                Font f = null;
#if UNITY_6000_0_OR_NEWER
                try { f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch (System.Exception) { }
#else
                try { f = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch (System.Exception) { }
#endif
                if (f == null) f = Font.CreateDynamicFontFromOSFont(new string[] { "Arial", "Segoe UI", "Helvetica" }, 40);
                face = f;
            }
            return face;
        }
    }

    public static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        var m = go.AddComponent<InputSystemUIInputModule>();
        m.AssignDefaultActions();
#else
        go.AddComponent<StandaloneInputModule>();
#endif
    }

    public static RectTransform NewRect(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    public static void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }

    public static Image Img(Transform parent, string name, Sprite s, Color c, Vector2 anchor, Vector2 pos, Vector2 size, bool sliced = false)
    {
        var rt = NewRect(parent, name);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var im = rt.gameObject.AddComponent<Image>();
        im.sprite = s;
        im.color = c;
        im.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        im.raycastTarget = false;
        return im;
    }

    public static Text Label(Transform parent, string text, int size, Color color, Vector2 anchor, Vector2 pos, Vector2 box, TextAnchor align = TextAnchor.MiddleCenter, bool outline = true)
    {
        var rt = NewRect(parent, "Text_" + text);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = box;
        var t = rt.gameObject.AddComponent<Text>();
        t.font = Face;
        t.fontSize = size;
        t.fontStyle = FontStyle.Bold;
        t.alignment = align;
        t.color = color;
        t.text = text;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        if (outline)
        {
            var o = rt.gameObject.AddComponent<Outline>();
            o.effectColor = new Color(0f, 0f, 0f, 0.55f);
            o.effectDistance = new Vector2(3f, -3f);
        }
        return t;
    }

    public static Button MakeButton(Transform parent, string label, Vector2 anchor, Vector2 pos, Vector2 size, Color color, UnityAction onClick, int fontSize = 56)
    {
        // darker "edge" under the button for a chunky look
        Img(parent, "BtnEdge", SpriteFactory.Rounded, SpriteFactory.Darken(color, 0.4f), anchor, pos + new Vector2(0f, -10f), size, true);
        var im = Img(parent, "Btn_" + label, SpriteFactory.Rounded, color, anchor, pos, size, true);
        im.raycastTarget = true;
        var b = im.gameObject.AddComponent<Button>();
        b.targetGraphic = im;
        var cb = ColorBlock.defaultColorBlock;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f);
        cb.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
        cb.selectedColor = Color.white;
        cb.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.8f);
        cb.fadeDuration = 0.08f;
        b.colors = cb;
        b.onClick.AddListener(() => AudioManager.Play(Sfx.Click));
        if (onClick != null) b.onClick.AddListener(onClick);
        if (!string.IsNullOrEmpty(label))
            Label(im.transform, label, fontSize, Color.white, new Vector2(0.5f, 0.5f), Vector2.zero, size);
        return b;
    }

    public static void SetButtonText(Button b, string text)
    {
        var t = b.GetComponentInChildren<Text>();
        if (t != null) t.text = text;
    }
}
