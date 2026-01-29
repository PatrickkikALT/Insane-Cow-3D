using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;


namespace FirePixel.Networking
{
    public static class LobbyManager
    {
        public static Lobby CurrentLobby { get; private set; }
        public static string LobbyId => CurrentLobby.Id;
        public static string LobbyCode => CurrentLobby.Data["joinCode"].Value;



        /// <summary>
        /// Set the lobby reference for host and clients here and start heartbeat coroutine if called from the server (or host)
        /// </summary>
        public static async Task SetLobbyData(Lobby lobby, bool calledFromHost = false)
        {
            CurrentLobby = lobby;

            if (calledFromHost)
            {
#pragma warning disable CS4014
                HeartbeatLobbyTask(CurrentLobby.Id, 25000);
#pragma warning restore CS4014

                // Host doesnt have to save rejoin data, if host disconnects, the lobby will be deleted
                return;
            }
        }


        /// <summary>
        /// MUST be called on server. Deletes Lobby Async
        /// </summary>
        public async static Task DeleteLobbyAsync_OnServer()
        {
            await LobbyService.Instance.DeleteLobbyAsync(LobbyId);
        }

        /// <summary>
        /// MUST be called on server. Deletes Lobby instantly
        /// </summary>
        public static void DeleteLobbyInstant_OnServer()
        {
            //_ = UpdateLobbyDataAsync(LobbyId, "LobbyTerminated", "true");

            LobbyService.Instance.DeleteLobbyAsync(LobbyId);
        }

        /// <summary>
        /// MUST be called on server. Sets Lobby.IsLocked state
        /// </summary>
        public static async Task SetLobbyLockStateAsync_OnServer(bool isLocked)
        {
            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions()
            {
                IsLocked = isLocked,
            };

            try
            {
                // Update the lobby with the new field value
                CurrentLobby = await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, updateLobbyOptions);
            }
            catch (System.Exception e)
            {
                DebugLogger.LogError($"Error updating lobby: {e.Message}");
            }
        }

        public static async Task UpdateLobbyDataAsync(string lobbyId, string key, string value)
        {
            try
            {
                UpdateLobbyOptions updateOptions = new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        [key] = new DataObject(
                            visibility: DataObject.VisibilityOptions.Member, // who can see this
                            value: value
                        )
                    }
                };

                CurrentLobby = await LobbyService.Instance.UpdateLobbyAsync(lobbyId, updateOptions);

                DebugLogger.Log($"Lobby updated: {key} = {value}");
            }
            catch (LobbyServiceException e)
            {
                DebugLogger.LogWarning($"Failed to update lobby data: {e}");
            }
        }


        /// <summary>
        /// Send ping to server every "pingDelayTicks" so it doesnt auto delete itself.
        /// </summary>
        private static async Task HeartbeatLobbyTask(string lobbyId, int pingDelayTicks)
        {
            while (true)
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);

                await Task.Delay(pingDelayTicks);
            }
        }
    }
}