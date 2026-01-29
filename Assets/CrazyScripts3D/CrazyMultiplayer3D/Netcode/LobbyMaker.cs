using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;



namespace FirePixel.Networking
{
    /// <summary>
    /// MB class responsible for creating and joining lobbies
    /// </summary>
    public class LobbyMaker : MonoBehaviour
    {
        public static LobbyMaker Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
        }


        [Header("Scene to load when joining or creating a lobby\nLeave empty for no scene load")]
        [SerializeField] private string nextSceneName = "Pre-MainGame";
        [SerializeField] private GameObject invisibleScreenCover;

        [SerializeField] private float lobbySearchInterval = 3;
        [SerializeField] private bool instaLoadSceneOnHost = true;

        private CancellationTokenSource lobbySearchCts;
        private bool inSearchLobbyScreen = false;


        private void Start()
        {
            _ = SearchLobbiesTimerAsync();
        }

        public void SetMenuState(bool _inSearchLobbyScreen)
        {
            inSearchLobbyScreen = _inSearchLobbyScreen;
        }

        private async Task SearchLobbiesTimerAsync()
        {
            lobbySearchCts?.Cancel(); // Cancel any previous loop
            lobbySearchCts = new CancellationTokenSource();
            CancellationToken token = lobbySearchCts.Token;

            while (!token.IsCancellationRequested)
            {
                // Wait until inSearchLobbyScreen is true
                while (!inSearchLobbyScreen && !token.IsCancellationRequested)
                {
                    await Task.Delay(100, token);
                }

                if (token.IsCancellationRequested) break;

                // Perform the lobby search
                (bool lobbyFound, List<Lobby> lobbies) = await FindLobbiesAsync();

                if (lobbyFound)
                {
                    LobbyUIMananager.Instance.CreateLobbyUI(lobbies);
                }

                // Wait for the configured interval, or stop early if needed
                try
                {
                    await Task.Delay((int)(lobbySearchInterval * 1000), token);
                }
                catch (TaskCanceledException) { break; }
            }
        }


        public async void CreateLobbyAsync_Button()
        {
            await CreateLobbyAsync();
        }
        public async Task<bool> CreateLobbyAsync()
        {
            invisibleScreenCover?.SetActive(true);
            int maxPlayers = 16;

            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1, "europe-west4");
                RelayHostData _hostData = new RelayHostData
                {
                    Key = allocation.Key,
                    Port = (ushort)allocation.RelayServer.Port,
                    AllocationID = allocation.AllocationId,
                    AllocationIDBytes = allocation.AllocationIdBytes,
                    ConnectionData = allocation.ConnectionData,
                    IPv4Address = allocation.RelayServer.IpV4
                };

                _hostData.JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);


                CreateLobbyOptions options = new CreateLobbyOptions
                {
                    IsPrivate = false,
                    IsLocked = false,

                    Data = new Dictionary<string, DataObject>()
                    {
                        {
                            "joinCode", new DataObject(
                            visibility: DataObject.VisibilityOptions.Public,
                            value: _hostData.JoinCode)
                        },
                        {
                            "LobbyTerminated", new DataObject(
                            visibility: DataObject.VisibilityOptions.Public,
                            value: "false")
                        },
                    }
                };

                Lobby lobby = await LobbyService.Instance.CreateLobbyAsync("New Lobby", maxPlayers, options);

                await LobbyManager.SetLobbyData(lobby, true);

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                    _hostData.IPv4Address,
                    _hostData.Port,
                    _hostData.AllocationIDBytes,
                    _hostData.Key,
                    _hostData.ConnectionData);

                NetworkManager.Singleton.StartHost();

                if (instaLoadSceneOnHost)
                {
                    LoadNextScene();
                }

                return true;
            }
            catch (LobbyServiceException e)
            {
                invisibleScreenCover?.SetActive(false);

                DebugLogger.Log(e.ToString());

                return false;
            }
        }

        public void LoadNextScene()
        {
            if (string.IsNullOrEmpty(nextSceneName) == false)
            {
                // Load next scene through network, so all joining clients will also load it automatically
                SceneManager.LoadSceneOnNetwork_OnServer(nextSceneName);
            }

            DebugLogger.LogError("Error scene name is invalid", string.IsNullOrEmpty(nextSceneName));
        }


        public async void AutoJoinLobbyAsync_Button()
        {
            await AutoJoinLobbyAsync();
        }
        public async Task<bool> AutoJoinLobbyAsync()
        {
            invisibleScreenCover?.SetActive(true);

            try
            {
                (bool lobbyFound, List<Lobby> lobbies) = await FindLobbiesAsync();

                if (lobbyFound == false)
                {
                    return await CreateLobbyAsync();
                }

                // Join oldest joinable lobby
                Lobby lobby = lobbies[0];

                await LobbyManager.SetLobbyData(lobby, false);

                string joinCode = lobby.Data["joinCode"].Value;
                JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);


                RelayJoinData _joinData = new RelayJoinData
                {
                    Key = allocation.Key,
                    Port = (ushort)allocation.RelayServer.Port,
                    AllocationID = allocation.AllocationId,
                    AllocationIDBytes = allocation.AllocationIdBytes,
                    ConnectionData = allocation.ConnectionData,
                    HostConnectionData = allocation.HostConnectionData,
                    IPv4Address = allocation.RelayServer.IpV4
                };

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                    _joinData.IPv4Address,
                    _joinData.Port,
                    _joinData.AllocationIDBytes,
                    _joinData.Key,
                    _joinData.ConnectionData,
                    _joinData.HostConnectionData);

                NetworkManager.Singleton.StartClient();

                return true;
            }
            catch (LobbyServiceException e)
            {
                invisibleScreenCover?.SetActive(false);
                DebugLogger.Log(e.ToString());

                return false;
            }
        }

        private async Task<(bool, List<Lobby>)> FindLobbiesAsync()
        {
            try
            {
                QueryLobbiesOptions queryOptions = new QueryLobbiesOptions
                {
                    Filters = new List<QueryFilter>
                {
                    //Only get open lobbies (non private)
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.AvailableSlots,
                        op: QueryFilter.OpOptions.GT,
                        value: "-1"),

                    //Only show non locked lobbies (lobbies that are not yet in a started match)
                     new QueryFilter(
                         field: QueryFilter.FieldOptions.IsLocked,
                         op: QueryFilter.OpOptions.EQ,
                         value: "false"),
                },

                    Order = new List<QueryOrder>
                {
                    //Show the oldest lobbies first
                    new QueryOrder(true, QueryOrder.FieldOptions.Created),
                    //
                    new QueryOrder(false, QueryOrder.FieldOptions.AvailableSlots),
                }
                };

                QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(queryOptions);

                return (response.Results.Count > 0, response.Results);
            }
            catch (LobbyServiceException e)
            {
                DebugLogger.Log(e.ToString());

                return (false, null);
            }
        }


        public async void JoinLobbyByIdAsync_Button(string lobbyId)
        {
            await JoinLobbyByIdAsync(lobbyId);
        }
        public async Task<bool> JoinLobbyByIdAsync(string lobbyId)
        {
            if (lobbyId.Length != 22) return false;

            invisibleScreenCover?.SetActive(true);

            try
            {
                Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);//, new JoinLobbyByIdOptions
                //{
                //    Player = new Player(ClientManager.LocalPlayerGUID)
                //});

                await LobbyManager.SetLobbyData(lobby);

                string joinCode = lobby.Data["joinCode"].Value;
                JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                RelayJoinData _joinData = new RelayJoinData
                {
                    Key = allocation.Key,
                    Port = (ushort)allocation.RelayServer.Port,
                    AllocationID = allocation.AllocationId,
                    AllocationIDBytes = allocation.AllocationIdBytes,
                    ConnectionData = allocation.ConnectionData,
                    HostConnectionData = allocation.HostConnectionData,
                    IPv4Address = allocation.RelayServer.IpV4
                };

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                    _joinData.IPv4Address,
                    _joinData.Port,
                    _joinData.AllocationIDBytes,
                    _joinData.Key,
                    _joinData.ConnectionData,
                    _joinData.HostConnectionData);

                NetworkManager.Singleton.StartClient();

                return true;
            }
            catch (LobbyServiceException e)
            {
                invisibleScreenCover?.SetActive(false);

                DebugLogger.Log(e.ToString());

                return false;
            }
        }

        public async Task<bool> RejoinLobbyAsync(string lobbyId)
        {
            try
            {
                Lobby lobby = await LobbyService.Instance.ReconnectToLobbyAsync(lobbyId);

                string joinCode = lobby.Data["joinCode"].Value;
                JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                RelayJoinData _joinData = new RelayJoinData
                {
                    Key = allocation.Key,
                    Port = (ushort)allocation.RelayServer.Port,
                    AllocationID = allocation.AllocationId,
                    AllocationIDBytes = allocation.AllocationIdBytes,
                    ConnectionData = allocation.ConnectionData,
                    HostConnectionData = allocation.HostConnectionData,
                    IPv4Address = allocation.RelayServer.IpV4
                };

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                    _joinData.IPv4Address,
                    _joinData.Port,
                    _joinData.AllocationIDBytes,
                    _joinData.Key,
                    _joinData.ConnectionData,
                    _joinData.HostConnectionData);

                NetworkManager.Singleton.StartClient();

                return true;
            }
            catch (Exception e)
            {
                invisibleScreenCover?.SetActive(false);

                DebugLogger.Log(e.ToString());

                return false;
            }
        }


        private void OnDestroy()
        {
            lobbySearchCts?.Cancel();
        }
    }
}