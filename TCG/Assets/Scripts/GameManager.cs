using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private PhotonView PV;
    public PhotonView playerPV;
    public bool isStarted = false;
    public bool[] isReady = {false, false, false, false};

    public GameObject[] playerList = new GameObject[4];
    private int playerCount = 0;
    public int currentTurn = 0;
    public int thisPlayerID = -1;

    [SerializeField] private Button endTurnButton;
    [SerializeField] private GameObject winView;
    [SerializeField] private GameObject loseView;

    [SerializeField] private Button winViewBackToTitleButton;
    [SerializeField] private Button loseViewObserveButton;

    private bool isGameOver = false;

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        endTurnButton.onClick.AddListener(OnClickEndTurn);
        endTurnButton.gameObject.SetActive(false);
        winViewBackToTitleButton.onClick.AddListener(OnClickWinViewBackToTitleButton);
        loseViewObserveButton.onClick.AddListener(OnClickLoseViewObserveButton);
    }

    // Update is called once per frame
    void Update()
    {
        // 게임 레디
        if(PV && !isStarted && PhotonNetwork.CurrentRoom != null && thisPlayerID == -1)
        {
            if(PhotonNetwork.CurrentRoom.PlayerCount == 4 && playerCount == 4)
            {
                SortPlayerList();
                PV.RPC("Ready", RpcTarget.All, thisPlayerID);
            }
        }

        // 게임 시작
        if(CheckReady() && thisPlayerID == 3 && !isStarted) PV.RPC("GameStart", RpcTarget.All);

        if(isStarted)
        {
            if(PhotonNetwork.CurrentRoom.PlayerCount < 4)
            {
                PhotonNetwork.LeaveRoom();    
                SceneManager.LoadScene(0);
            }
            else
            {
                if(!isGameOver && CheckWin())
                {
                    winView.SetActive(true);
                }
                if(!isGameOver && CheckLose())
                {
                    isGameOver = true;
                    loseView.SetActive(true);
                }
            }
        }
    }

    [PunRPC] public void Ready(int playerID)
    {
        isReady[playerID] = true;
    }

    private bool CheckReady()
    {
        for(int i = 0; i < 4; i++)
        {
            if(!isReady[i]) return false;
        }
        return true;
    }

    private bool CheckWin()
    {
        for(int i = 0; i < 4; i++)
        {
            if(playerList[i].GetComponent<PlayerCtrl>().isDead == (i == thisPlayerID)) return false;
        }
        return true;
    }

    private bool CheckLose()
    {
        return playerList[thisPlayerID].GetComponent<PlayerCtrl>().isDead;
    }

    [PunRPC] public void GameStart()
    {
        isStarted = true;
        // 모든 플레이어 덱 셔플
        playerPV.GetComponent<PlayerCtrl>().Shuffle();

        // 카드 3장 드로우
        playerPV.RPC("Draw", RpcTarget.All, 3);
        // 시작 플레이어 정하기

        for (int i = 0; i < 4; i++)
        {
            playerList[(thisPlayerID + i)%4].transform.Rotate(new Vector3(0.0f, 0.0f, -90.0f * i));
            if(i == 1)
            {
                playerList[(thisPlayerID + i)%4].transform.position = new Vector3(-2.5f, 0, 0);
            }
            if(i == 3)
            {
                playerList[(thisPlayerID + i)%4].transform.position = new Vector3(2.5f, 0, 0);
            }
        }

        endTurnButton.gameObject.SetActive(true);

        PlayerCtrl tempCtrl = playerList[currentTurn].GetComponent<PlayerCtrl>();
        tempCtrl.isMyTurn = true;
        tempCtrl.maxCost++;
        tempCtrl.currentCost = tempCtrl.maxCost;
        if(tempCtrl.PV.IsMine) tempCtrl.PV.RPC("Draw", RpcTarget.All, 1);
        tempCtrl.ResetAttackCount();
    }

    // 턴 넘기기
    [PunRPC] public void NextTurn()
    {
        playerList[currentTurn].GetComponent<PlayerCtrl>().isMyTurn = false;

        do
        {
            currentTurn++;
            if(currentTurn > 3)
            {
                currentTurn = 0;
            }
        }
        while(playerList[currentTurn].GetComponent<PlayerCtrl>().isDead);

        PlayerCtrl tempCtrl = playerList[currentTurn].GetComponent<PlayerCtrl>();
        tempCtrl.maxCost++;
        tempCtrl.currentCost = tempCtrl.maxCost;
        tempCtrl.isMyTurn = true;
        // 드로우
        if(tempCtrl.PV.IsMine) tempCtrl.PV.RPC("Draw", RpcTarget.All, 1);
        tempCtrl.ResetAttackCount();
    }

    public void AppendPlayerList(GameObject obj)
    {
        playerList[playerCount] = obj;
        playerCount++;
    }

    private void SortPlayerList()
    {
        for(int i = 0; i < 4; i++)
        {
            for(int j = i+1; j < 4; j++)
            {
                if(playerList[i].GetComponent<PhotonView>().ViewID > playerList[j].GetComponent<PhotonView>().ViewID)
                {
                    GameObject temp = playerList[i];
                    playerList[i] = playerList[j];
                    playerList[j] = temp;
                }
            }
        }
        for(int i = 0; i < 4; i++)
        {
            playerList[i].GetComponent<PlayerCtrl>().Initiation(i);
        }
    }

    private void OnClickEndTurn()
    {
        // 턴 종료
        if (currentTurn == thisPlayerID)
        {
            Debug.Log("턴 종료");
            PV.RPC("NextTurn", RpcTarget.All);
        }
        else
        {
            Debug.Log("당신의 턴이 아닙니다!");
        }
    }

    private void OnClickWinViewBackToTitleButton()
    {
        PhotonNetwork.LeaveRoom();    
        SceneManager.LoadScene(0);
    }

    private void OnClickLoseViewObserveButton()
    {
        loseView.SetActive(false);
    }
}
