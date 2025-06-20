using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneManager : MonoBehaviour
{
    private static LoadSceneManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // თავიდან აირიდე დუბლიკატები
        }
    }
    public void ChangeScene(int index)
    {
        SceneManager.LoadScene(index);
    }
}
