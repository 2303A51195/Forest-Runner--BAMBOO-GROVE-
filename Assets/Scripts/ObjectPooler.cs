using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Generic object pool keyed by string. Objects are built on demand by a builder function.</summary>
public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance { get; private set; }

    readonly Dictionary<string, Stack<GameObject>> pools = new Dictionary<string, Stack<GameObject>>();

    public static ObjectPooler Ensure()
    {
        if (Instance == null)
        {
            var go = new GameObject("ObjectPooler");
            Instance = go.AddComponent<ObjectPooler>();
        }
        return Instance;
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public GameObject Get(string key, Func<GameObject> builder)
    {
        GameObject go = null;
        Stack<GameObject> st;
        if (pools.TryGetValue(key, out st))
        {
            while (st.Count > 0 && go == null) go = st.Pop();
        }
        if (go == null)
        {
            go = builder();
            go.name = key;
            go.transform.SetParent(transform, false);
            var tag = go.GetComponent<PooledTag>();
            if (tag == null) tag = go.AddComponent<PooledTag>();
            tag.key = key;
        }
        go.SetActive(true);
        return go;
    }

    public void Release(GameObject go)
    {
        if (go == null) return;
        var tag = go.GetComponent<PooledTag>();
        if (tag == null) { Destroy(go); return; }
        go.SetActive(false);
        Stack<GameObject> st;
        if (!pools.TryGetValue(tag.key, out st))
        {
            st = new Stack<GameObject>();
            pools[tag.key] = st;
        }
        st.Push(go);
    }
}
