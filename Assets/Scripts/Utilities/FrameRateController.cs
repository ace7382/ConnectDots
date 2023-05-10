using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FrameRateController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private bool showFPS;
    [SerializeField] private float fontSize;
    [SerializeField] private UIDocument uiDoc;

    #endregion

    #region

    private Label counter;

    #endregion

    #region Unity Functions

    void Awake()
    {
        Application.targetFrameRate = 60;

        StartCoroutine(ShowFPS());
    }

    #endregion

    #region Private Functions

    private IEnumerator ShowFPS()
    {
        counter = uiDoc.rootVisualElement.Q<Label>();
        WaitForSeconds w = new WaitForSeconds(.2f);
        while (true)
        {
            if (showFPS)
            {
                float fps = 1f / Time.deltaTime;

                counter.text = fps.ToString();
            }

            yield return w;
        }
    }

    #endregion
}
