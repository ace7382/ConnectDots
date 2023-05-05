using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Test : MonoBehaviour
{
    public bool go;
    private VisualElement v;
    [SerializeField] private Texture2D texture;
    [SerializeField] private Color color1, color2;

    // Start is called before the first frame update
    void Start()
    {
        UIDocument u = GetComponent<UIDocument>();
        v = new VisualElement();
        v.style.SetWidth(150f);
        v.style.SetHeight(150f);

        u.rootVisualElement.Add(v);
    }

    private void Update()
    {
        if (go)
        {
            v.style.backgroundImage = MakeStripes(color1, color2);

            go = false;
        }
    }

    private void OnBecameInvisible()
    {
        
    }

    private Texture2D MakeStripes(Color col1, Color col2)
    {
        Color[] pixels = texture.GetPixels();

        Debug.Log(pixels.Length);

        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].a == 1f)
            {
                Debug.Log("Color 1");
                pixels[i] = col1;
            }
            else
            {
                Debug.Log("Color 2");
                pixels[i] = col2;
            }
        }


        Texture2D ret = new Texture2D(texture.width, texture.height);
        ret.SetPixels(pixels);
        ret.Apply();

        return ret;
    }
}
