using Unity.Netcode;
using UnityEngine;
using TMPro; // If using TextMeshPro for UI

public class Nametag : NetworkBehaviour {
  private NetworkVariable<NetworkString> playerName = new NetworkVariable<NetworkString>(
    new NetworkString("Player"),
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Owner
  );

  [SerializeField] private TextMeshProUGUI nametagText;

  private Camera _camera;

  public override void OnNetworkSpawn() {
    base.OnNetworkSpawn();
    
    playerName.OnValueChanged += OnNameChanged;
    
    UpdateNametagDisplay(playerName.Value.ToString());
    
    if (IsOwner) {
      string selectedName = PlayerPrefs.GetString("PlayerName", "");
      if (selectedName == "") {
        selectedName = "Player_" + UnityEngine.Random.Range(1, 100);
      }
      SetPlayerName(selectedName);
      nametagText.transform.parent.gameObject.SetActive(false);
    }

    _camera = Camera.main;
  }

  public override void OnNetworkDespawn() {
    base.OnNetworkDespawn();
    playerName.OnValueChanged -= OnNameChanged;
  }
  public void SetPlayerName(string newName) {
    if (IsOwner) {
      playerName.Value = new NetworkString(newName);
      gameObject.name = newName;
    }
  }

  private void FixedUpdate() {
    if (!IsServer) {
      transform.LookAt(transform.position + _camera.transform.forward);
    }
  }
  
  private void OnNameChanged(NetworkString oldName, NetworkString newName) {
    UpdateNametagDisplay(newName.ToString());
  }

  private void UpdateNametagDisplay(string name) {
    if (nametagText) {
      nametagText.text = name;
    }
    gameObject.name = name;
  }
  
  [Rpc(SendTo.Server)]
  public void RequestNameChangeServerRpc(string newName) {
    if (!string.IsNullOrEmpty(newName) && newName.Length <= 20) {
      playerName.Value = new NetworkString(newName);
    }
  }
}

public struct NetworkString : INetworkSerializable {
  private string value;

  public NetworkString(string value) {
    this.value = value;
  }

  public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
    serializer.SerializeValue(ref value);
  }

  public override string ToString() {
    return value;
  }

  public static implicit operator string(NetworkString ns) => ns.value;
}