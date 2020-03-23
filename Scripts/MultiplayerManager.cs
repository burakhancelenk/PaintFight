using System ;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun ;
using Photon.Realtime ;
using UnityEditor ;
using UnityEngine;
using UnityEngine.SceneManagement ;

public class MultiplayerManager : MonoBehaviourPunCallbacks
{
    private byte numberOfPlayers ;
    private byte duration ;
    public enum GameStatus
    {
        InGame,InMenu
    }

    public static GameStatus CurrentStatus ;
    public static bool DisconnectionAttempt ;
    public static bool DisconnectForReconnection ;
    public static bool ConnectedToMaster ;
    public static bool IsFriendlyMatch ;
    public static bool RoomCreatedByMe ;
    public MainMenu MM ;

    private GamePlayUI gamePlayUI ;

    private void Start()
    {
        gamePlayUI = GameObject.Find("Canvas").GetComponent<GamePlayUI>() ;
        PhotonNetwork.SendRate = 60 ;
        PhotonNetwork.SerializationRate = 60 ;
        DontDestroyOnLoad(gameObject);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true ;
        if (CurrentStatus == GameStatus.InMenu && !ConnectedToMaster)
        {
            MM.ConnectionToMasterEstablishedInMenu();
            ConnectedToMaster = true ;
        }
        else if (CurrentStatus == GameStatus.InMenu && DisconnectForReconnection)
        {
            MM.OpenLastOpenedMenuObject();
            DisconnectForReconnection = false ;
        }
        else if (CurrentStatus == GameStatus.InGame)
        {
            gamePlayUI.OpenCloseDisconnectionPanel(false);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (CurrentStatus == GameStatus.InMenu)
        {
            if (!DisconnectionAttempt)
            {
                MM.DisconnectedFromMasterInMenu(); 
            }
            else
            {
                DisconnectionAttempt = false ;
                if (DisconnectForReconnection)
                {
                    PhotonNetwork.ConnectUsingSettings() ;
                }
            }
        }
        else if (CurrentStatus == GameStatus.InGame)
        {
            if (DisconnectionAttempt)
            {
                DisconnectionAttempt = false ;
                CurrentStatus = GameStatus.InMenu ;
            }
            else
            {
                gamePlayUI.OpenCloseDisconnectionPanel(true);
                PhotonNetwork.ReconnectAndRejoin() ;
            }
        }
    }

    public override void OnJoinedRoom()
    {
        duration = Convert.ToByte(PhotonNetwork.CurrentRoom.CustomProperties["d"]) ;
        numberOfPlayers = PhotonNetwork.CurrentRoom.MaxPlayers ;
        if (IsFriendlyMatch)
        {
            if (!RoomCreatedByMe)
            {
                MM.JoinedRoom(); 
            }
            else
            {
                RoomCreatedByMe = false ;
            }
            
            if (PhotonNetwork.CurrentRoom.PlayerCount == numberOfPlayers && numberOfPlayers > 1)
            {
                MM.FMGameFound();
            }
        }
        else
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == numberOfPlayers && numberOfPlayers > 1)
            {
                MM.QMGameFound();
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (IsFriendlyMatch)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == numberOfPlayers && numberOfPlayers > 1)
            {
                MM.FMGameFound();
                CurrentStatus = GameStatus.InGame ;
            }
        }
        else
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == numberOfPlayers && numberOfPlayers > 1)
            {
                MM.QMGameFound();
                CurrentStatus = GameStatus.InGame ;
            }
        }
    }

    public override void OnJoinRandomFailed(short returnCode , string message)
    {
        RoomOptions roomOptions = new RoomOptions();
        ExitGames.Client.Photon.Hashtable customRoomOptions = new ExitGames.Client.Photon.Hashtable();
        customRoomOptions.Add("d",duration);
        roomOptions.MaxPlayers = numberOfPlayers ;
        roomOptions.IsVisible = true ;
        roomOptions.CustomRoomProperties = customRoomOptions ;
        PhotonNetwork.CreateRoom(null,roomOptions) ;
    }

    public override void OnJoinRoomFailed(short returnCode , string message)
    {
        if (!RoomCreatedByMe)
        {
            if (CurrentStatus == GameStatus.InMenu)
            {
                MM.JoinRoomFailed();
            }
            else if (CurrentStatus == GameStatus.InGame)
            {
                SceneManager.LoadScene(0) ;
            }
        }
        else
        {
            if (CurrentStatus == GameStatus.InMenu)
            {
                RoomCreatedByMe = false ;
            }
            else if (CurrentStatus == GameStatus.InGame)
            {
                SceneManager.LoadScene(0) ;
            }
        }
    }

    public override void OnCreatedRoom()
    {
        if (IsFriendlyMatch)
        {
            MM.RoomCreated();
        }
    }

    public override void OnCreateRoomFailed(short returnCode , string message)
    {
        if (IsFriendlyMatch)
        {
            MM.RoomCreationFailed();
        }
        else
        {
            RequestQuickMatch(numberOfPlayers,duration);
        }
    }

    public void ConnectToServer()
    {
        PhotonNetwork.ConnectUsingSettings() ;
        
    }

    public void DisconnectFromServer()
    {
        PhotonNetwork.Disconnect();
    }

    public void RequestQuickMatch(byte numberOfPlayers, byte duration)
    {
        this.numberOfPlayers = numberOfPlayers ;
        this.duration = duration ;
        IsFriendlyMatch = false ;
        ExitGames.Client.Photon.Hashtable customOptions = new ExitGames.Client.Photon.Hashtable();
        customOptions.Add("d",duration);
        PhotonNetwork.JoinRandomRoom(customOptions,numberOfPlayers) ;
    }

    public void JoinFriendsRoom(string roomName)
    {
        IsFriendlyMatch = true ;
        PhotonNetwork.JoinRoom(roomName) ;
    }

    public void CreateRoomForFriends(string roomName, byte numberOfPlayers, byte duration)
    {
        this.numberOfPlayers = numberOfPlayers ;
        this.duration = duration ;
        IsFriendlyMatch = true ;
        RoomOptions roomOptions = new RoomOptions();
        ExitGames.Client.Photon.Hashtable customRoomOptions = new ExitGames.Client.Photon.Hashtable();
        customRoomOptions.Add("d",duration);
        roomOptions.MaxPlayers = numberOfPlayers ;
        roomOptions.CustomRoomProperties = customRoomOptions ;
        roomOptions.IsVisible = false ;
        PhotonNetwork.CreateRoom(roomName,roomOptions) ;
    }

    public static int SetViewId(int id , Transform player)
    {
        player.GetComponent<PhotonView>().ViewID = id + 1 ;
        if (PhotonNetwork.CurrentRoom.Players[id+1].Equals(PhotonNetwork.LocalPlayer))
        {
            player.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.CurrentRoom.Players[id+1]);
            return id ;
        }

        return -1 ;
    }

    public int[] TakeRoomInformations()
    {
        int[] roomInfs = {Convert.ToInt32(PhotonNetwork.CurrentRoom.CustomProperties["d"]), PhotonNetwork.CurrentRoom.MaxPlayers} ;
        return roomInfs ;
    }
}
