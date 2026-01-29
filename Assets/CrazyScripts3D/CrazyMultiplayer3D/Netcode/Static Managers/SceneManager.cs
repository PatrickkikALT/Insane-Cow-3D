using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;



/// <summary>
/// Custom SceneManager
/// </summary>
public static class SceneManager
{
    public static string CurrentSceneName => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;


    public static void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, mode);
    }

    public static AsyncOperation LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, bool autoLoadSceneWhenFinished = true)
    {
        AsyncOperation loadSceneOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, mode);

        loadSceneOperation.allowSceneActivation = autoLoadSceneWhenFinished;

        return loadSceneOperation;
    }
    public static AsyncOperation UnLoadSceneAsync(string sceneName)
    {
        return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
    }


    /// <summary>
    /// MUST be called on server. Load a scene on the network.
    /// </summary>
    public static SceneEventProgressStatus LoadSceneOnNetwork_OnServer(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        return NetworkManager.Singleton.SceneManager.LoadScene(sceneName, mode);
    }
    /// <summary>
    /// MUST be called on server. Unload a scene on the network.
    /// </summary>
    public static SceneEventProgressStatus UnLoadSceneOnNetwork_OnServer(Scene scene)
    {
        return NetworkManager.Singleton.SceneManager.UnloadScene(scene);
    }


    public static void SetActiveScene(string sceneName)
    {
        Scene sceneToSetActive = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);

        UnityEngine.SceneManagement.SceneManager.SetActiveScene(sceneToSetActive);
    }
    public static Scene GetSceneByName(string name)
    {
        return UnityEngine.SceneManagement.SceneManager.GetSceneByName(name);
    }
}
