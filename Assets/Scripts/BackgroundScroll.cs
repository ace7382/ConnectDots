using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//https://stackoverflow.com/a/59747931

[RequireComponent(typeof(RawImage))]
public class BackgroundScroll : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private RectTransform _parentRectTransform;
    [SerializeField] private RawImage _image;
    [Header("Settings")]
    [SerializeField] private Vector2 repeatCount;
    [SerializeField] private Vector2 scroll;
    [SerializeField] private Vector2 offset;

    private void Awake()
    {
        if (!_image) _image = GetComponent<RawImage>();

        _image.uvRect = new Rect(offset, repeatCount);
    }

    // Start is called before the first frame update
    private void Start()
    {
        if (!_rectTransform) _rectTransform = GetComponent<RectTransform>();
        if (!_parentRectTransform) _parentRectTransform = GetComponentInParent<RectTransform>();

        SetScale();
    }

    // Update is called once per frame
    private void Update()
    {
        SetScale();

        offset += scroll * Time.deltaTime;
        _image.uvRect = new Rect(offset, repeatCount);
    }

    private void SetScale()
    {
        // get the diagonal size of the screen since the parent is the Canvas with
        // ScreenSpace overlay it is always fiting the screensize
        var parentCorners = new Vector3[4];
        _parentRectTransform.GetLocalCorners(parentCorners);
        var diagonal = Vector3.Distance(parentCorners[0], parentCorners[2]);

        // set width and height to at least the diagonal
        _rectTransform.sizeDelta = new Vector2(diagonal, diagonal);
    }

    public void Set(Texture2D texture, Color color)
    {
        _image.color = color;
        _image.texture = texture;
    }
}
