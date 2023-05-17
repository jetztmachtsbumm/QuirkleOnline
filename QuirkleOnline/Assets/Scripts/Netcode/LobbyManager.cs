using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{

    public static LobbyManager Instance { get; private set; }

    public event EventHandler<List<Lobby>> OnLobbyListChanged;

    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float listLobbiesTimer;

    public void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("There is more than one LobbyManager object active in the scene!");
            Destroy(gameObject);
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        InitializeAuthentication();
    }

    private void Update()
    {
        LobbyHeartbeat();
        HandelLobbyList();
    }

    private void LobbyHeartbeat()
    {
        if (IsLobbyHost())
        {
            heartbeatTimer -= Time.deltaTime;
            if(heartbeatTimer <= 0)
            {
                float heartbeatTimerMax = 15f;
                heartbeatTimer = heartbeatTimerMax;

                LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    private bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private void HandelLobbyList()
    {
        if (joinedLobby == null && AuthenticationService.Instance.IsSignedIn && !WaitingForPlayersUI.IsWaitingForPlayers)
        {
            listLobbiesTimer -= Time.deltaTime;
            if(listLobbiesTimer <= 0)
            {
                float listLobbiesTimerMax = 1f;
                listLobbiesTimer = listLobbiesTimerMax;
                ListLobbies();
            }
        }
    }

    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                }
            };

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            OnLobbyListChanged?.Invoke(this, queryResponse.Results);
        } catch(LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }

    private async void InitializeAuthentication()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(UnityEngine.Random.Range(1000, 10000).ToString());

            await UnityServices.InitializeAsync();

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    private async Task<Allocation> AllocateRelay(int maxPlayers)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);

            return allocation;
        } catch(RelayServiceException e)
        {
            Debug.LogException(e);

            return default;
        }
    }

    private async Task<string> GetRelayJoinCode(Allocation allocation)
    {
        try
        {
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            return relayJoinCode;
        } catch(RelayServiceException e)
        {
            Debug.LogException(e);

            return default;
        }
    }

    private async Task<JoinAllocation> JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            return joinAllocation;
        } catch(RelayServiceException e)
        {
            Debug.LogException(e);

            return default;
        }
    }

    public async void CreateLobby(string lobbyName, int maxPlayers, bool isPrivate)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, new CreateLobbyOptions
            {
                IsPrivate = isPrivate
            });

            Allocation allocation = await AllocateRelay(maxPlayers);

            string relayJoinCode = await GetRelayJoinCode(allocation);

            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            });

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));

            MultiplayerManager.Instance.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene("WaitingForPlayersScene", LoadSceneMode.Single);
        } catch(LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }

    public async void QuickJoin()
    {
        try
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            string relayJoinCode = joinedLobby.Data["RelayJoinCode"].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            MultiplayerManager.Instance.StartClient();
        } catch(LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }

    public async void JoinWithCode(string lobbyCode)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

            string relayJoinCode = joinedLobby.Data["RelayJoinCode"].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            MultiplayerManager.Instance.StartClient();
        } catch(LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }

    public async void JoinWithLobbyId(string lobbyId)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);

            string relayJoinCode = joinedLobby.Data["RelayJoinCode"].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            MultiplayerManager.Instance.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }

    public async void DeleteLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);

                joinedLobby = null;
            } catch(LobbyServiceException e)
            {
                Debug.LogException(e);
            }
        }
    }

    public async void LeaveLobby()
    {
        if(joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);

                joinedLobby = null;
            } catch(LobbyServiceException e)
            {
                Debug.LogException(e);
            }
        }
    }

    public Lobby GetJoinedLobby()
    {
        return joinedLobby;
    }

}
