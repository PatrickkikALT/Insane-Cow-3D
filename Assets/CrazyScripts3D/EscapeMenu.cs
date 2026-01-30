using FirePixel.Networking;
using Unity.Netcode;
using Unity.Services.Lobbies;
using UnityEngine;

public class EscapeMenu : MonoBehaviour {
  [SerializeField] private GameObject panel;
  
  public void PressEscape() {
    panel.SetActive(!panel.activeSelf);
  }

  public void Resume() {
    panel.SetActive(false);
  }

  public void Quit() {
    NetworkManager.Singleton.DisconnectClient(NetworkManager.Singleton.LocalClientId);
    LobbyService.Instance.RemovePlayerAsync(LobbyManager.LobbyId, NetworkManager.Singleton.LocalClientId.ToString());
    SceneManager.LoadScene("CrazyMainMenu3D");
  }
}
