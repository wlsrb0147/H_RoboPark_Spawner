using UnityEngine;

public class PageController : MonoBehaviour
{
    public static PageController Instance;

    [SerializeField] public GameObject[] pages;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private VideoManager videoManager;
    
    private int _currentPage;

    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            if (value >= pages.Length)
            {
                _currentPage = value % pages.Length;
            }
            else if (value < 0)
            {
                _currentPage = value + pages.Length;
            }
            else
            {
                _currentPage = value;
            }
        }
    }

    void Awake()
    {
        GoTitle();
        Instance = this;
        DebugEx.Log("Start");
    }

    
    public void CloseSinglePage(int x)
    {
        DebugEx.Log("Close page index : " + x);
        pages[x].SetActive(false);
    }

    public void LoadNextPage()
    {
        int nextPage = (CurrentPage + 1) % pages.Length;
        OpenPage(nextPage);             
        CloseSinglePage(CurrentPage);  
        CurrentPage = nextPage;         
    }
    
    public void GoTitle()
    {
        CurrentPage = 0;
        pages[0].SetActive(true);

        for (int i = 1; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
        }
    }
    
    public void OpenPage(int pageNum)
    {
        inputManager.SetCurrentIndex(pageNum,0);
        DebugEx.Log("OpenPage : " + pageNum);

        if (pageNum < pages.Length)
        {
            pages[pageNum].SetActive(true);
            CurrentPage = pageNum;
        }
    }
}
