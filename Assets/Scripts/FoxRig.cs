using UnityEngine;
using static SpriteFactory;

/// <summary>The player's fox, built from sprites and animated in code (run cycle, jump, slide, lean).</summary>
public class FoxRig
{
    public Transform root, shadow, body, legL, legR, tail, scarfA, scarfB, ear;

    public static FoxRig Build(Transform parent)
    {
        var f = new FoxRig();
        f.root = new GameObject("Fox").transform;
        f.root.SetParent(parent, false);

        Color orange = Hex("#F2812B"), dark = Hex("#B45A14"), cream = Hex("#FFF1DC"), brown = Hex("#4A2C17"), red = Hex("#E53946");

        f.shadow = C(f.root, new Color(0f, 0f, 0f, 0.3f), 0f, 0.02f, 1.5f, 0.42f, -5).transform;

        f.body = new GameObject("Body").transform;
        f.body.SetParent(f.root, false);
        var b = f.body;

        f.legL = R(b, brown, -0.32f, 0.25f, 0.3f, 0.5f, 1).transform;
        f.legR = R(b, brown, 0.32f, 0.25f, 0.3f, 0.5f, 1).transform;
        C(b, Hex("#2E1A0C"), -0.32f, 0.04f, 0.38f, 0.2f, 1);
        C(b, Hex("#2E1A0C"), 0.32f, 0.04f, 0.38f, 0.2f, 1);

        C(b, orange, 0f, 0.95f, 1.25f, 1.2f, 2);
        C(b, Lighten(orange, 0.25f), -0.3f, 1.15f, 0.4f, 0.5f, 3);
        R(b, Hex("#2E9E4A"), 0f, 0.95f, 0.64f, 0.6f, 3);          // little leaf backpack
        C(b, Hex("#4CCB63"), 0f, 1.0f, 0.5f, 0.46f, 4);
        R(b, Hex("#1F7A38"), 0f, 1.0f, 0.05f, 0.44f, 5);

        // head
        C(b, orange, 0f, 1.78f, 1.05f, 0.92f, 4);
        C(b, cream, -0.5f, 1.66f, 0.3f, 0.34f, 3);
        C(b, cream, 0.5f, 1.66f, 0.3f, 0.34f, 3);
        T(b, orange, -0.33f, 2.32f, 0.4f, 0.6f, 4, 8f);
        T(b, orange, 0.33f, 2.32f, 0.4f, 0.6f, 4, -8f);
        T(b, dark, -0.33f, 2.3f, 0.2f, 0.34f, 5, 8f);
        T(b, dark, 0.33f, 2.3f, 0.2f, 0.34f, 5, -8f);
        f.ear = C(b, Hex("#4CCB63"), 0.1f, 2.3f, 0.34f, 0.17f, 6, 25f).transform;  // leaf on head

        // scarf
        R(b, red, 0f, 1.4f, 0.95f, 0.24f, 6);
        f.scarfA = T(b, red, 0.5f, 1.25f, 0.3f, 0.5f, 6, -20f).transform;
        f.scarfB = T(b, Darken(red, 0.2f), 0.62f, 1.18f, 0.24f, 0.4f, 5, -40f).transform;

        // bushy tail (we see the fox from behind)
        f.tail = new GameObject("Tail").transform;
        f.tail.SetParent(b, false);
        f.tail.localPosition = new Vector3(0f, 0.55f, 0f);
        C(f.tail, orange, 0f, 0.1f, 0.62f, 1.1f, 7);
        C(f.tail, dark, 0.15f, 0.0f, 0.2f, 0.7f, 8);
        C(f.tail, Color.white, 0f, 0.5f, 0.6f, 0.45f, 9);

        return f;
    }

    public void SetVisible(bool v) { body.gameObject.SetActive(v); }

    public void Animate(float t, float height, bool grounded, bool sliding, float lean)
    {
        float bob = (grounded && !sliding) ? Mathf.Abs(Mathf.Sin(t)) * 0.07f : 0f;
        body.localPosition = new Vector3(0f, height + bob, 0f);
        body.localRotation = Quaternion.Euler(0f, 0f, lean);
        body.localScale = sliding ? new Vector3(1.2f, 0.5f, 1f) : Vector3.one;

        if (!grounded)
        {
            legL.localPosition = new Vector3(-0.36f, 0.5f, 0f);
            legR.localPosition = new Vector3(0.36f, 0.5f, 0f);
        }
        else
        {
            legL.localPosition = new Vector3(-0.32f, 0.25f + Mathf.Max(0f, Mathf.Sin(t)) * 0.2f, 0f);
            legR.localPosition = new Vector3(0.32f, 0.25f + Mathf.Max(0f, Mathf.Sin(t + Mathf.PI)) * 0.2f, 0f);
        }
        tail.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 0.5f) * 18f);
        scarfA.localRotation = Quaternion.Euler(0f, 0f, -20f + Mathf.Sin(t * 1.3f) * 14f);
        scarfB.localRotation = Quaternion.Euler(0f, 0f, -40f + Mathf.Sin(t * 1.3f + 1f) * 18f);
        ear.localRotation = Quaternion.Euler(0f, 0f, 25f + Mathf.Sin(t) * 10f);

        float s = 1f / (1f + height * 0.45f);
        shadow.localScale = new Vector3(1.5f * s, 0.42f * s, 1f);
    }
}
