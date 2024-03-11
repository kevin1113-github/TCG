using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerCtrl : MonoBehaviour
{
    public PhotonView PV;
    [SerializeField] private Deck deck;
    [SerializeField] private List<Card> hand;
    [SerializeField] private Card[] field = new Card[5];
    [SerializeField] private GameObject fieldHolder;

    public bool isMyTurn = false;
    public int playerID;
    public int health = 1;
    public int maxCost = 0;
    public int currentCost;
    public GameObject attackArrow;
    GameObject arrowInstance;

    [SerializeField] private HandCurve handCurve;

    bool isCardHold = false;

    Transform originParent, focusCard;
    Vector3 originCardRot;
    Vector3 originCardPos;
    int attackOriginIndex = -1;
    int attackTargetIndex = -1;

    [SerializeField] private TextMeshPro healthText;
    [SerializeField] private TextMeshPro costText;
    public GameObject[] hitEffect = new GameObject[3];

    [SerializeField] private GameObject loadingUI;

    public bool isDead = false;

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        if(PV.IsMine) GameObject.Find("GameManager").GetComponent<GameManager>().playerPV = PV;
        GameObject.Find("GameManager").GetComponent<GameManager>().AppendPlayerList(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        PlayerUI();
        
        // 디버깅
        // if(Input.GetKeyDown(KeyCode.I)) Initiation(0);
        // if(Input.GetKeyDown(KeyCode.A)) Draw();
        // if(Input.GetKeyDown(KeyCode.C)) { maxCost++; currentCost = maxCost; }
        // ControlCard();
        
        if (PV.IsMine)
        {
            if(isMyTurn)
            {
                ControlCard();
            }

            if(health <= 0 && !isDead)
            {
                PV.RPC("Death", RpcTarget.All);
            }
        }
    }

    // 카드 컨트롤
    private void ControlCard()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hitCard = Physics2D.Raycast(mousePos, Vector2.zero, 0f, LayerMask.GetMask("Card"));

        if(mousePos.y < -3.0f && mousePos.x > -3.0f && mousePos.x < 3.0f)
        {
            transform.GetChild(1).transform.localPosition = new Vector3(0.0f, 1f, 0.0f);
        }
        else
        {
            transform.GetChild(1).transform.localPosition = Vector3.zero;
        }

        // Debug.Log(mousePos);
        if (hitCard)
        {
            // 내 핸드 -> 내 필드 위
            if (hitCard.collider.CompareTag("Card") && hitCard.collider.transform.parent.CompareTag("Hand"))
            {
                // 카드를 현재 집고있는 상태가 아니면서 마우스 다운
                if (Input.GetMouseButtonDown(0) && !isCardHold && hitCard.collider.GetComponent<Card>().cardOwnerID == playerID)
                {
                    focusCard = hitCard.collider.transform;
                    isCardHold = true;

                    originParent = focusCard.parent;
                    originCardRot = focusCard.rotation.eulerAngles;
                    originCardPos = focusCard.position;

                    focusCard.eulerAngles = Vector3.zero;
                    focusCard.localScale = new Vector3(2.0f, 2.0f, 0.0f);
                }

                // 카드를 내려놓았을때, 해당 카드홀더 범위와 자식 수(이미 카드가 놔져있는상태면 자식으로 들어감)를 비교하여 자리에 넣기
                if (Input.GetMouseButtonUp(0) && isCardHold)
                {
                    Vector2 mousePosCardHolder = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    RaycastHit2D hitCardHolder = Physics2D.Raycast(mousePosCardHolder, Vector2.zero, 0f, LayerMask.GetMask("CardHolder"));

                    bool backToOrigin = false;
                    // 카드 홀더에 놓으면
                    if (hitCardHolder && hitCardHolder.collider.transform.childCount == 0 && hitCardHolder.collider.transform.parent.parent.GetComponent<PlayerCtrl>().PV.IsMine)
                    {
                        Card cardScript = focusCard.GetComponent<Card>();
                        Transform cardHolder = hitCardHolder.collider.transform;

                        int handIndex = -1;
                        handIndex = hand.FindIndex(card => System.Object.ReferenceEquals(card, cardScript));
                        int fieldIndex = -1;
                        for (int i = 0; i < 5; i++)
                        {
                            if (System.Object.ReferenceEquals(fieldHolder.transform.GetChild(i), cardHolder))
                            {
                                fieldIndex = i;
                                break;
                            }
                        }
                        if(currentCost >= hand[handIndex].cost)
                        {
                            focusCard.GetComponent<AudioSource>().Play();
                            PV.RPC("Spawn", RpcTarget.All, handIndex, fieldIndex);
                        }
                        else
                        {
                            Debug.Log("비용이 부족합니다!");
                            backToOrigin = true;
                        }
                    }
                    else
                    {
                        backToOrigin = true;
                    }


                    if(backToOrigin)
                    {
                        focusCard.SetParent(originParent);
                        focusCard.eulerAngles = originCardRot;
                        focusCard.position = originCardPos;
                        handCurve.reCalculate();
                    }

                    focusCard.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    focusCard = null;
                    isCardHold = false;
                }
            }

            // 내 필드 위 카드로 공격
            if (hitCard.collider.CompareTag("Card") && hitCard.collider.transform.parent.CompareTag("CardHolder") && hitCard.collider.GetComponent<Card>().attackCount > 0)
            {
                if(Input.GetMouseButtonDown(0) && !isCardHold && hitCard.collider.GetComponent<Card>().cardOwnerID == playerID)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (System.Object.ReferenceEquals(field[i], hitCard.collider.GetComponent<Card>()))
                        {
                            attackOriginIndex = i;
                            break;
                        }
                    }
                    arrowInstance = Instantiate(attackArrow, mousePos, Quaternion.identity);
                }
            }
        }

        // 공격하는중 마우스를 뗐을때
        if (attackOriginIndex != -1 && Input.GetMouseButtonUp(0))
        {
            Vector2 mousePosTargetCard = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hitTargetCard = Physics2D.Raycast(mousePosTargetCard, Vector2.zero, 0f, LayerMask.GetMask("Card"));

            // 공격 타겟이 상대 필드 위 카드인지
            if(hitTargetCard && hitTargetCard.collider.CompareTag("Card") && hitTargetCard.collider.transform.parent.CompareTag("CardHolder") && hitTargetCard.collider.GetComponent<Card>().cardOwnerID != playerID && !hitTargetCard.collider.transform.root.GetComponent<PlayerCtrl>().isDead)
            {
                Card[] targetField = hitTargetCard.collider.transform.root.GetComponent<PlayerCtrl>().field;

                attackTargetIndex = -1;
                for (int i = 0; i < 5; i++)
                {
                    if (System.Object.ReferenceEquals(targetField[i], hitTargetCard.collider.GetComponent<Card>()))
                    {
                        attackTargetIndex = i;
                        break;
                    }
                }

                Camera.main.GetComponent<AudioSource>().Play();
                PV.RPC("AttackCard", RpcTarget.All, attackOriginIndex, hitTargetCard.collider.GetComponent<Card>().cardOwnerID, attackTargetIndex);
            }
            // 아니라면
            else
            {
                Vector2 mousePosCharacter = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hitCharacter = Physics2D.Raycast(mousePosCharacter, Vector2.zero, 0f, LayerMask.GetMask("Character"));

                if(hitCharacter)
                {
                    PlayerCtrl tempCtrl = hitCharacter.collider.transform.root.GetComponent<PlayerCtrl>();
                    // 공격 타겟이 상대 플레이어 캐릭터인지
                    if(tempCtrl.playerID != playerID && !tempCtrl.isDead)
                    {
                        Camera.main.GetComponent<AudioSource>().Play();
                        PV.RPC("AttackPlayer", RpcTarget.All, attackOriginIndex, tempCtrl.playerID);
                    }
                }
            }
            Destroy(arrowInstance);
            attackOriginIndex = -1;
            attackTargetIndex = -1;
        }

        // 집고있는 동안 카드가 마우스 포지션 따라다님
        if (isCardHold)
        {
            focusCard.position = mousePos;

            focusCard.GetComponent<UnityEngine.Rendering.SortingGroup>().sortingOrder = 10;
        }
    }

    // 드로우
    [PunRPC] public void Draw(int n = 1)
    {
        // 덱에서 카드 n장을 손패로 드로우
        for(int i = 0; i < n; i++)
        {
            if(deck.deck.Count <= 0)
            {
                Debug.Log("덱에 카드 없음.");
                return;
            }
            deck.deck[0].gameObject.GetComponent<AudioSource>().Play();
            handCurve.AppendCard(deck.deck[0].gameObject);
            hand.Add(deck.deck[0]);
            deck.deck.RemoveAt(0);
            hand[hand.Count-1].isBack = hand[hand.Count-1].cardOwnerID != GameObject.Find("GameManager").GetComponent<GameManager>().thisPlayerID;
        }
    }

    // 턴 종료
    // [PunRPC] public void EndTurn()
    // {
    //     GameObject.Find("GameManager").GetComponent<GameManager>().NextTurn();
    // }
    
    // 몬스터 소환
    [PunRPC] public void Spawn(int handIndex, int fieldIndex)
    {
        currentCost -= hand[handIndex].cost;
        field[fieldIndex] = (hand[handIndex]);
        hand.RemoveAt(handIndex);
        handCurve.RemoveCardAt(handIndex);
        Transform cardHolder = fieldHolder.transform.GetChild(fieldIndex);
        field[fieldIndex].transform.SetParent(cardHolder);
        field[fieldIndex].transform.position = cardHolder.position;
        field[fieldIndex].transform.localEulerAngles = Vector3.zero;
        field[fieldIndex].gameObject.GetComponent<UnityEngine.Rendering.SortingGroup>().sortingOrder = 2;
        field[fieldIndex].gameObject.GetComponent<Card>().isBack = false;
    }

    // 카드 -> 카드 공격
    [PunRPC] public void AttackCard(int originCardIndex, int targetPlayerID, int targetCardIndex)
    {
        field[originCardIndex].attackCount--;
        Card target = GameObject.Find("GameManager").GetComponent<GameManager>().playerList[targetPlayerID].GetComponent<PlayerCtrl>().field[targetCardIndex];
        field[originCardIndex].health -= target.attack;
        field[originCardIndex].DamageEffect();
        target.health -= field[originCardIndex].attack;
        target.DamageEffect();
    }

    // 카드 -> 플레이어 공격
    [PunRPC] public void AttackPlayer(int originCardIndex, int targetPlayerID)
    {
        field[originCardIndex].attackCount--;
        PlayerCtrl tempCtrl = GameObject.Find("GameManager").GetComponent<GameManager>().playerList[targetPlayerID].GetComponent<PlayerCtrl>();
        tempCtrl.health -= field[originCardIndex].attack;
        tempCtrl.DamageEffect();
    }

    public void Initiation(int playerID)
    {
        this.playerID = playerID;
        if(PV.IsMine) GameObject.Find("GameManager").GetComponent<GameManager>().thisPlayerID = playerID;
        deck.DeckInitiation(playerID);
    }

    private void PlayerUI()
    {
        loadingUI.SetActive(isMyTurn);
        costText.text = currentCost.ToString();
        healthText.text = health.ToString();
    }

    public void DamageEffect()
    {
        GameObject effectInstance = Instantiate(hitEffect[Random.Range(0, 3)], transform.Find("Character").position, Quaternion.identity);
        Destroy(effectInstance, 0.45f);
    }

    [PunRPC] public void Death()
    {
        isDead = true;
        if(isMyTurn)
        {
            GameObject.Find("GameManager").GetComponent<GameManager>().NextTurn();
        }
    }

    // 덱 셔플
    public void Shuffle()
    {
        int[] index = new int[deck.deck.Count];
        for(int i = 0; i < deck.deck.Count; i++)
        {
            index[i] = i;
        }
        for(int i = 0; i < deck.deck.Count - 2; i++)
        {
            int ranNum = Random.Range(i, deck.deck.Count);

            int temp = index[i];
            index[i] = index[ranNum];
            index[ranNum] = temp;
        }

        PV.RPC("SetCardIndex", RpcTarget.All, index);
    }

    [PunRPC] private void SetCardIndex(int[] index)
    {
        deck.SetCardIndex(index);
    }

    public void ResetAttackCount()
    {
        for(int i = 0; i < 5; i++)
        {
            if(field[i] != null)
            {
                field[i].ResetAttackCount();
            }
        }
    }
}