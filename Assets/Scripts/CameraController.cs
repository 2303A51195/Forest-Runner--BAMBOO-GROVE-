using UnityEngine;

/// <summary>
/// Creates/configures the 2D camera, keeps the playfield in a 9:16 portrait frame
/// (letterboxed on wide screens, extended on tall phones) and provides screen shake.
/// </summary>
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    Camera cam;
    Camera letterboxCam;
    float shakeTime, shakeDur, shakeMag;
    int lastW, lastH;

    public Camera Cam { get { return cam; } }

    public static void ComputeView(out Rect rect, out float orthoSize)
    {
        float target = 9f / 16f;
        float window = (float)Screen.width / Mathf.Max(1, Screen.height);
        if (window > target)
        {
            float w = target / window;
            rect = new Rect((1f - w) * 0.5f, 0f, w, 1f);
            orthoSize = 8f;
        }
        else
        {
            rect = new Rect(0f, 0f, 1f, 1f);
            orthoSize = 4.5f / Mathf.Max(0.2f, window);
        }
    }

    public static CameraController Create()
    {
        var existing = Camera.main;
        if (existing == null)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            existing = go.AddComponent<Camera>();
        }
        if (existing.GetComponent<AudioListener>() == null && Object.FindFirstObjectByType<AudioListener>() == null)
            existing.gameObject.AddComponent<AudioListener>();

        var cc = existing.GetComponent<CameraController>();
        if (cc == null) cc = existing.gameObject.AddComponent<CameraController>();
        cc.Setup(existing);
        return cc;
    }

    void Setup(Camera c)
    {
        Instance = this;
        cam = c;
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.36f, 0.72f, 0.91f);
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.transform.rotation = Quaternion.identity;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 50f;
        cam.depth = 0;

        // black bars camera (draws behind the main camera, clears the whole screen)
        var lgo = new GameObject("LetterboxCamera");
        letterboxCam = lgo.AddComponent<Camera>();
        letterboxCam.clearFlags = CameraClearFlags.SolidColor;
        letterboxCam.backgroundColor = Color.black;
        letterboxCam.cullingMask = 0;
        letterboxCam.depth = -10;
        var al = lgo.GetComponent<AudioListener>();
        if (al != null) Destroy(al);

        ApplyLayout();
    }

    void ApplyLayout()
    {
        Rect r; float o;
        ComputeView(out r, out o);
        cam.rect = r;
        cam.orthographicSize = o;
        lastW = Screen.width;
        lastH = Screen.height;
    }

    public static void Shake(float magnitude, float duration)
    {
        if (Instance == null) return;
        Instance.shakeMag = magnitude;
        Instance.shakeDur = Mathf.Max(0.01f, duration);
        Instance.shakeTime = duration;
    }

    public void SetBackground(Color c)
    {
        if (cam != null) cam.backgroundColor = c;
    }

    void LateUpdate()
    {
        if (cam == null) return;
        if (Screen.width != lastW || Screen.height != lastH) ApplyLayout();

        Vector3 pos = new Vector3(0f, 0f, -10f);
        if (shakeTime > 0f)
        {
            shakeTime -= Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(shakeTime / shakeDur);
            pos.x += Random.Range(-1f, 1f) * shakeMag * k;
            pos.y += Random.Range(-1f, 1f) * shakeMag * k;
        }
        cam.transform.position = pos;
    }
}
