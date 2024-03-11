using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class TitleScene : MonoBehaviour
{
    [SerializeField] private PhotonManager pun;

    [SerializeField] private Transform titleView;
    [SerializeField] private Transform startView;
    [SerializeField] private Transform createRoomView;


    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;

    [SerializeField] private Button startPlayButton;
    [SerializeField] private Button startCancelButton;
    [SerializeField] private Button createRoomButton;

    [SerializeField] private Button createRoomCancelButton;
    [SerializeField] private Button createRoomPlayButton;


    [SerializeField] private TextMeshProUGUI roomName;
    [SerializeField] private TextMeshProUGUI nickNameInput;
    [SerializeField] private TextMeshProUGUI createRoomRoomNameInput;

    [SerializeField] private GameObject roomPrefab;
    [SerializeField] private Transform roomListContentContainer;

    private List<string> roomList;

    // Start is called before the first frame update
    void Start()
    {
        exitButton.onClick.AddListener(OnClickExitButton);
        startPlayButton.onClick.AddListener(OnClickStartPlayButton);
        startCancelButton.onClick.AddListener(OnClickStartCancelButton);
        createRoomButton.onClick.AddListener(OnClickCreateRoomButton);
        createRoomCancelButton.onClick.AddListener(OnClickCreateRoomCancelButton);
        createRoomPlayButton.onClick.AddListener(OnClickCreateRoomPlayButton);
    }

    public void OnMasterConnected()
    {
        startButton.onClick.AddListener(OnClickStartButton);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnClickStartButton()
    {
        pun._JoinLobby();
        titleView.gameObject.SetActive(false);
        startView.gameObject.SetActive(true);
    }

    private void OnClickExitButton()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void OnClickStartPlayButton()
    {
        if(roomName.text != "" && roomName.text != "방을 선택해주세요")
        {
            pun._JoinRoom(roomName.text);
            SceneManager.LoadScene(1);
        }
        else
        {
            Debug.Log("There is no room " + roomName.text);
        }
    }

    private void OnClickStartCancelButton()
    {
        pun._LeaveLobby();
        startView.gameObject.SetActive(false);
        titleView.gameObject.SetActive(true);
    }

    private void OnClickCreateRoomButton()
    {
        createRoomView.gameObject.SetActive(true);
    }

    private void OnClickCreateRoomCancelButton()
    {
        createRoomRoomNameInput.text = "";
        createRoomView.gameObject.SetActive(false);
    }

    private void OnClickCreateRoomPlayButton()
    {
        pun.CreateRoom(createRoomRoomNameInput.text);
        SceneManager.LoadScene(1);
    }

    public void RefreshRoomList(List<string> roomList)
    {
        // Debug.Log(roomList.Count);
        for(int i = roomListContentContainer.childCount-1; i >= 0; i--)
        {
            Destroy(roomListContentContainer.GetChild(i).gameObject);
        }
        this.roomList = roomList;
        foreach(var room in roomList)
        {
            GameObject tempRoom = Instantiate(roomPrefab);
            tempRoom.GetComponentInChildren<TextMeshProUGUI>().text = room;
            tempRoom.GetComponent<Button>().onClick.AddListener(() => SetRoomName(room));
            tempRoom.transform.SetParent(roomListContentContainer, false);
        }
    }

    private void SetRoomName(string name)
    {
        roomName.text = name;
    }
}
