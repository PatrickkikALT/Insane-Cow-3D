using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace FirePixel.Networking
{
    public class LobbyUIPanel : MonoBehaviour
    {
        public GameObject mainUI;

        public Button button;

        public TextMeshProUGUI lobbyName;
        public TextMeshProUGUI creationDate;
        public TextMeshProUGUI amountOfPlayersInLobby;
        public string lobbyId;


        public bool full;
        public bool Full
        {
            get
            {
                return full;
            }
            set
            {
                button.enabled = !value;
                full = value;
            }
        }


        private void Start()
        {
            mainUI = transform.GetChild(0).gameObject;

            button = GetComponentInChildren<Button>(true);

            TextMeshProUGUI[] textFields = GetComponentsInChildren<TextMeshProUGUI>(true);
            lobbyName = textFields[0];
            creationDate = textFields[1];
            amountOfPlayersInLobby = textFields[2];
        }

        public async void JoinLobby()
        {
            await LobbyMaker.Instance.JoinLobbyByIdAsync(lobbyId);
        }
    }
}