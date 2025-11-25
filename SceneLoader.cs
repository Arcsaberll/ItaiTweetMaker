using UnityEngine;
using UnityEngine.SceneManagement; // 忘れない！！
public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}