using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;


namespace FirePixel.Networking
{
    public class LobbyUIMananager : MonoBehaviour
    {
        public static LobbyUIMananager Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
        }


        private LobbyUIPanel[] lobbyUISlots;
        [SerializeField] private int activeLobbyUISlots;


        private void Start()
        {
            lobbyUISlots = GetComponentsInChildren<LobbyUIPanel>(true);
        }

        public void CreateLobbyUI(List<Lobby> lobbies)
        {
            for (int i = 0; i < activeLobbyUISlots; i++)
            {
                lobbyUISlots[i].mainUI.SetActive(false);
            }

            activeLobbyUISlots = lobbies.Count;
            for (int i = 0; i < lobbies.Count; i++)
            {
                LobbyUIPanel targetUISlot = lobbyUISlots[i];

                targetUISlot.mainUI.SetActive(true);
                targetUISlot.lobbyName.text = lobbies[i].Name;
                targetUISlot.lobbyId = lobbies[i].Id;

                int maxPlayers = lobbies[i].MaxPlayers;

                DateTime creationTimeUtc = lobbies[i].Created; // likely in UTC
                DateTime creationTimeLocal = creationTimeUtc.ToLocalTime();
                string creationDate = creationTimeLocal.ToString("HH.mm"); // e.g. "12.00"

                bool full = lobbies[i].AvailableSlots == 0;

                targetUISlot.amountOfPlayersInLobby.text = (maxPlayers - lobbies[i].AvailableSlots).ToString() + "/" + maxPlayers.ToString() + (full ? "Full!" : "");
                targetUISlot.creationDate.text = "created: " + creationDate;
                targetUISlot.Full = full;
            }
        }
    }
}


