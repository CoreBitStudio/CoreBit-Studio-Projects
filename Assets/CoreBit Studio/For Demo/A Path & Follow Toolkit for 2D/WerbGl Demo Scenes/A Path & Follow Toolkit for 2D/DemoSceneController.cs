using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class DemoSceneController : MonoBehaviour
{
    [SerializeField] private FollowerPathAgent followerPathAgent;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject startGameButton;
    [SerializeField] private GameObject resetGameButton;
    [SerializeField] private UnityEvent onStartGame;

    public static bool isRestart = false;
    public void StartGame()
    {
        followerPathAgent.CanFollow = true;
        menuPanel.SetActive(false);
        onStartGame?.Invoke();
    }
    public void Restart()
    {
        isRestart = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    private void OnEnable()
    {
        Time.timeScale = 1;

        startGameButton.SetActive(true);
        resetGameButton.SetActive(false);
        menuPanel.SetActive(true);
        if (isRestart)
        {
            StartGame();
            isRestart = false;
        }
    }
    public void OnEnd()
    {
        Time.timeScale = 0;

        startGameButton.SetActive(false);
        resetGameButton.SetActive(true);
        menuPanel.SetActive(true);
    }
}
