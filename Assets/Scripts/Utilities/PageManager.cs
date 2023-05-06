using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PageManager : MonoBehaviour
{
    #region Singleton

    public static PageManager                       instance;

    #endregion

    #region Inspector Variables

    [SerializeField] private GameObject             blankPagePrefab;

    [Space]

    [SerializeField] private List<VisualTreeAsset>  templates;

    #endregion

    #region Private Variables

    private SimplePool<GameObject>                  GOPages;
    private OrderedDictionary                       stack; //Key: Page, Value: GameObject

    #endregion

    #region Unity Functions

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        stack           = new OrderedDictionary();

        GOPages         = new SimplePool<GameObject>(blankPagePrefab);
        GOPages.OnPush  = (item) => { item.SetActive(false); };
        GOPages.Populate(3);
    }

    private void Start()
    {
        StartCoroutine(OpenPageOnAnEmptyStack<MainMenu>());
    }

    #endregion

    #region Public Functions

    public IEnumerator CloseTopPage(bool animateOut = true, bool executeHideCall = true)
    {
        UIManager.instance.TopBar.CanClick = false;

        if (animateOut)
            yield return (stack.Cast<DictionaryEntry>().ElementAt(stack.Count - 1).Key as Page).AnimateOut();

        if (executeHideCall)
            (stack.Cast<DictionaryEntry>().ElementAt(stack.Count - 1).Key as Page).HidePage();

        GOPages.Push(stack[stack.Count - 1] as GameObject);
        stack.RemoveAt(stack.Count - 1);

        UIManager.instance.TopBar.CanClick = true;
    }

    public IEnumerator OpenPageOnAnEmptyStack<T>(object[] arfs = null, bool animateOut = true
        , bool executeHideCalls = true, bool animateIn = true, bool executeShowCall = true) where T : Page, new()
    {
        UIManager.instance.TopBar.CanClick = false;

        for (int i = stack.Count - 1; i >= 0; i--)
        {
            Page p = stack.Cast<DictionaryEntry>().ElementAt(i).Key as Page;

            if (animateOut)
                yield return p.AnimateOut();

            if (executeHideCalls)
                p.HidePage();

            GOPages.Push((GameObject)stack[i]);

            stack.RemoveAt(i);
        }

        GameObject page             = GOPages.Pop();
        page.transform.localScale   = Vector3.one;

        T pageToAdd                 = new T();
        UIDocument uIDocument       = page.GetComponent<UIDocument>();

        string templateName         = typeof(T).ToString().Split("`")[0].Trim(); //TODO: Maybe handle this better~

        uIDocument.visualTreeAsset  = templates.Find(x => x.name == templateName);
        pageToAdd.SetUIDoc(uIDocument);

        stack.Add(pageToAdd, page);

        page.SetActive(true);
        pageToAdd.SetSortOrder(stack.Count);

        if (executeShowCall) pageToAdd.ShowPage(arfs);
        if (animateIn) yield return pageToAdd.AnimateIn();

        UIManager.instance.TopBar.CanClick = true;
    }

    public IEnumerator AddPageToStack<T>(object[] args = null, bool animateIn = true, bool executeShowCall = true) where T : Page, new()
    {
        UIManager.instance.TopBar.CanClick = false;

        GameObject page             = GOPages.Pop();
        page.transform.localScale   = Vector3.one;

        T pageToAdd                 = new T();
        UIDocument uIDocument       = page.GetComponent<UIDocument>();

        string templateName = typeof(T).ToString().Split("`")[0].Trim();
        uIDocument.visualTreeAsset = templates.Find(x => x.name == templateName);
        pageToAdd.SetUIDoc(uIDocument);

        stack.Add(pageToAdd, page);

        page.SetActive(true);
        pageToAdd.SetSortOrder(stack.Count);

        if (executeShowCall) pageToAdd.ShowPage(args);
        if (animateIn) yield return pageToAdd.AnimateIn();

        UIManager.instance.TopBar.CanClick = true;
    }

    #endregion

    #region Private Functions

    private void MaxPageLimitReached()
    {
        //TODO: Add a cap to the number of pages that can be added to the stack
    }

    #endregion
}
