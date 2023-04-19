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

    [SerializeField] private List<VisualTreeAsset>  templates; //TODO: Move this to UIManager?

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
    }

    private void Start()
    {
        OpenPageOnAnEmptyStack<MainMenu>();
    }

    #endregion

    #region Public Functions

    public void CloseTopPage()
    {
        (stack.Cast<DictionaryEntry>().ElementAt(stack.Count - 1).Key as Page).HidePage();
        GOPages.Push(stack[stack.Count - 1] as GameObject);
        stack.RemoveAt(stack.Count - 1);
    }

    public void OpenPageOnAnEmptyStack<T>(object[] arfs = null, bool executeHideCalls = true) where T : Page, new()
    {
        for (int i = stack.Count - 1; i >= 0; i--)
        {
            if (executeHideCalls)
                (stack.Cast<DictionaryEntry>().ElementAt(i).Key as Page).HidePage();

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
        pageToAdd.ShowPage(arfs);
        pageToAdd.SetSortOrder(stack.Count);
    }

    public void AddPageToStack<T>(object[] args = null) where T : Page, new()
    {
        GameObject page             = GOPages.Pop();
        page.transform.localScale   = Vector3.one;

        T pageToAdd                 = new T();
        UIDocument uIDocument       = page.GetComponent<UIDocument>();

        string templateName = typeof(T).ToString().Split("`")[0].Trim();
        uIDocument.visualTreeAsset = templates.Find(x => x.name == templateName);
        pageToAdd.SetUIDoc(uIDocument);

        stack.Add(pageToAdd, page);

        page.SetActive(true);
        pageToAdd.ShowPage(args);
        pageToAdd.SetSortOrder(stack.Count);
    }

    #endregion

    #region Private Functions

    private void MaxPageLimitReached()
    {
        //TODO: Add a cap to the number of pagees that can be added to the stack
    }

    #endregion
}
