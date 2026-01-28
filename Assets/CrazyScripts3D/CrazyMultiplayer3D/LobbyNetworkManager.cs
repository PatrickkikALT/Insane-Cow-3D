using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyNetworkManager : MonoBehaviour {
  public static LobbyNetworkManager Instance { get; private set; }

  [SerializeField] private string lobbySceneName = "LobbyScene";
  [SerializeField] private string gameSceneName = "GameScene";

  private void Awake() {
    if (Instance && Instance != this) {
      Destroy(gameObject);
      return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);
  }

  public void StartHost() {
    NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    NetworkManager.Singleton.StartHost();
    LoadLobbyScene();
  }

  public void StartClient() {
    NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    NetworkManager.Singleton.StartClient();
  }

  public void Disconnect() {
    if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient) {
      NetworkManager.Singleton.Shutdown();
    }

    NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;

    SceneManager.LoadScene("CrazyMainMenu3D");
  }

  private void OnClientConnected(ulong clientId) {
    Debug.Log($"Client {clientId} connected");

    if (NetworkManager.Singleton.IsHost && SceneManager.GetActiveScene().name != lobbySceneName) {
      LoadLobbyScene();
    }
  }

  private void OnClientDisconnected(ulong clientId) {
    Debug.Log($"Client {clientId} disconnected");

    if (NetworkManager.Singleton.IsHost) {
      LobbyManager.Instance?.RemovePlayer(clientId);
    }
    else {
      SceneManager.LoadScene("CrazyMainMenu3D");
    }
  }

  public void LoadLobbyScene() {
    if (NetworkManager.Singleton.IsHost) {
      NetworkManager.Singleton.SceneManager.LoadScene(lobbySceneName, LoadSceneMode.Single);
    }
  }

  public void LoadGameScene() {
    if (NetworkManager.Singleton.IsHost) {
      NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }
  }
}