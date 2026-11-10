using UnityEngine;

/// <summary>A single pooled sprite particle (sparkles, dust, rings).</summary>
public class FxParticle : MonoBehaviour
{
    SpriteRenderer sr;
    Vector3 vel;
    float life, maxLife, gravity, size, endScale, spin;
    Color col;

    public void Setup(Sprite s, Vector3 pos, Vector3 v, float lifeTime, Color c, float sz, float grav, float endScl, float spinSpeed)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        sr.sprite = s;
        sr.color = c;
        transform.position = pos;
        transform.rotation = Quaternion.identity;
        transform.localScale = new Vector3(sz, sz, 1f);
        vel = v; life = maxLife = lifeTime; col = c; size = sz; gravity = grav; endScale = endScl; spin = spinSpeed;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        life -= dt;
        if (life <= 0f)
        {
            if (ObjectPooler.Instance != null) ObjectPooler.Instance.Release(gameObject);
            else Destroy(gameObject);
            return;
        }
        transform.position += vel * dt;
        vel.y -= gravity * dt;
        float t = 1f - life / maxLife;
        float s = size * Mathf.Lerp(1f, endScale, t);
        transform.localScale = new Vector3(s, s, 1f);
        Color c = col;
        c.a = col.a * (1f - t * t);
        sr.color = c;
        if (spin != 0f) transform.Rotate(0f, 0f, spin * dt);
    }
}
