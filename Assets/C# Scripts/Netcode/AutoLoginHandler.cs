using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace FirePixel.Networking
{
    public class AutoLoginHandler : MonoBehaviour
    {
        [SerializeField] private string mainSceneName = "Main Menu";
        private string loginSceneName;

        private AsyncOperation mainSceneLoadOperation;


        private async void Awake()
        {
            loginSceneName = SceneManager.CurrentSceneName;
            mainSceneLoadOperation = SceneManager.LoadSceneAsync(mainSceneName, LoadSceneMode.Additive, false);

            mainSceneLoadOperation.completed += (_) =>
            {
                SceneManager.UnLoadSceneAsync(loginSceneName);
            };

            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignOut();

            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            mainSceneLoadOperation.allowSceneActivation = true;
        }
    }
}