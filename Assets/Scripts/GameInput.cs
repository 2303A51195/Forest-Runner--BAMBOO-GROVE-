using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// One place for all input: keyboard, mouse-drag swipes and touch swipes.
/// Works whether Unity is set to the old Input Manager or the new Input System.
/// </summary>
public static class GameInput
{
    public static bool Left, Right, Jump, Slide, Pause;

    static bool tracking;
    static Vector2 startPos;
    static int lastFrame = -1;

    public static void Poll()
    {
        if (lastFrame == Time.frameCount) return;
        lastFrame = Time.frameCount;
        Left = Right = Jump = Slide = Pause = false;

#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            Left |= kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame;
            Right |= kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame;
            Jump |= kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame;
            Slide |= kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame;
            Pause |= kb.escapeKey.wasPressedThisFrame || kb.pKey.wasPressedThisFrame;
        }
        var ptr = Pointer.current;
        if (ptr != null)
        {
            if (ptr.press.wasPressedThisFrame)
            {
                tracking = true;
                startPos = ptr.position.ReadValue();
            }
            else if (tracking && ptr.press.isPressed)
            {
                DetectSwipe(ptr.position.ReadValue());
            }
            if (ptr.press.wasReleasedThisFrame) tracking = false;
        }
#else
        Left |= Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);
        Right |= Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
        Jump |= Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space);
        Slide |= Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);
        Pause |= Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);

        if (Input.GetMouseButtonDown(0))
        {
            tracking = true;
            startPos = Input.mousePosition;
        }
        else if (tracking && Input.GetMouseButton(0))
        {
            DetectSwipe(Input.mousePosition);
        }
        if (Input.GetMouseButtonUp(0)) tracking = false;
#endif
    }

    static void DetectSwipe(Vector2 pos)
    {
        Vector2 d = pos - startPos;
        float threshold = Mathf.Max(40f, Screen.height * 0.035f);
        if (d.magnitude < threshold) return;
        if (Mathf.Abs(d.x) > Mathf.Abs(d.y))
        {
            if (d.x > 0f) Right = true; else Left = true;
        }
        else
        {
            if (d.y > 0f) Jump = true; else Slide = true;
        }
        tracking = false; // one swipe per touch
    }
}
