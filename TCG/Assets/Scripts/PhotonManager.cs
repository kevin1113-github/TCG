using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private TitleScene title_scene;

    void Awake()
    {
        Screen.SetResolution(1920, 1080, false);
        if(!PhotonNetwork.IsConnected) PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        if(title_scene != null) title_scene.OnMasterConnected();
    }

    public override void OnJoinedRoom()
    {
        GameObject PI = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList) 
    { 
        // 룸 리스트 콜백은 로비에 접속했을때 자동으로 호출된다. 
        // 로비에서만 호출할 수 있음... 
        // Debug.Log($"룸 리스트 업데이트 ::::::: 현재 방 갯수 : {roomList.Count}");

        List<string> room_list = new List<string>();
        foreach(var room in roomList)
        {
            if(!room.RemovedFromList) room_list.Add(room.Name);
        }
        title_scene.RefreshRoomList(room_list);
    } 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateRoom(string room_name)
    {
        PhotonNetwork.CreateRoom(room_name, new RoomOptions { MaxPlayers = 4 });
    }

    public void _JoinRoom(string room_name)
    {
        PhotonNetwork.JoinRoom(room_name);
        // PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 2 }, null);
    }

    public void _LeaveRoom()
    {
        // if(PhotonNetwork.CurrentRoom.PlayerCount == 1) PhotonNetwork.CurrentRoom.
        PhotonNetwork.LeaveRoom();
    }

    public void _JoinLobby()
    {
        PhotonNetwork.JoinLobby();
    }

    public void _LeaveLobby()
    {
        PhotonNetwork.LeaveLobby();
    }
}
