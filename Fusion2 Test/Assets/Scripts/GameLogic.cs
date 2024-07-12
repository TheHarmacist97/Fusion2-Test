using Fusion;
using Fusion.Addons.KCC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum GameState
{
    Waiting,
    Playing
}

public class GameLogic : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private Transform spawnPoint, spawnPointPivot;

    [Networked] private Player Winner { get; set; }
    [Networked, OnChangedRender(nameof(OnGameStateChanged))] private GameState State { get; set; }

    [SerializeField] private NetworkPrefabRef playerPrefab;
    [Networked, Capacity(12)] private NetworkDictionary<PlayerRef, Player> Players => default;

    public override void Spawned()
    {
        Winner = null;
        State = GameState.Waiting;
        UIManager.Instance.SetWaitUI(State, Winner);
    }

    //incoming player is just "player"
    public void PlayerJoined(PlayerRef player)
    {
        if (!HasStateAuthority)
            return;
        
        GetNextSpawnPoint(90f, out Vector3 position, out Quaternion rotation);

        //spawn a Networked player using the current referenced player
        NetworkObject playerObject = Runner.Spawn(playerPrefab, position, rotation, player);

        //add currently networked player and playerLogic to dictionary
        Players.Add(player, playerObject.GetComponent<Player>());
    }

    public override void FixedUpdateNetwork()
    {
        if(Players.Count == 0) return;
        if (!Runner.IsServer || State != GameState.Waiting) return;

        bool isEveryoneReady = true;
        foreach (KeyValuePair<PlayerRef, Player> player in Players)
        {
            if (!player.Value.isReady)
            {
                isEveryoneReady = false;
                break;
            }
        }

        //new game or everybody is ready
        if(isEveryoneReady)
        {
            Winner = null;
            State = GameState.Playing;
            PreparePlayers();
        }
    }

    private void PreparePlayers()
    {
        float spacingAngle = 360 / Players.Count;
        spawnPointPivot.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        foreach (KeyValuePair<PlayerRef, Player> Player in Players)
        {
            GetNextSpawnPoint(spacingAngle, out Vector3 position, out Quaternion rotation);
            Player.Value.Teleport(position, rotation);
        }
    }

    private void GetNextSpawnPoint(float spacingAngle, out Vector3 position, out Quaternion rotation)
    {
        position = spawnPoint.position;
        rotation = spawnPoint.rotation;
        spawnPointPivot.Rotate(0f, spacingAngle, 0f);
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (!HasStateAuthority)
            return;
        if(Players.TryGet(player, out Player playerBehaviour))
        {
            Players.Remove(player);
            Runner.Despawn(playerBehaviour.Object);
        }   
    }

    public void OnGameStateChanged()
    {
        Debug.Log("Called");
        UIManager.Instance.SetWaitUI(State, Winner);   
    }

    private void UnreadyAll()
    {
        foreach (KeyValuePair<PlayerRef, Player> player in Players)
        {
            player.Value.isReady = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered");
        if (!Runner.IsServer || Winner != null || other.attachedRigidbody == null) return;

        Debug.Log("Checks Cleared");
        if (other.attachedRigidbody.TryGetComponent(out Player player))
        {
            UnreadyAll();
            Winner = player;
            State = GameState.Waiting;
            Debug.Log("Game Complete");
        }
    }
}
