using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Card : MonoBehaviour
{
    [SerializeField] private CardSO cardObj;

    [SerializeField] private SpriteRenderer image;
    [SerializeField] private TextMeshPro cardName;
    [SerializeField] private TextMeshPro description;
    [SerializeField] private TextMeshPro costText;
    [SerializeField] private TextMeshPro healthText;
    [SerializeField] private TextMeshPro attackText;

    public GameObject[] hitEffect = new GameObject[3];
    public int health = 1;
    public int attack = 0;
    public int cost = 0;
    public int attackCount = 0;
    public int cardOwnerID = -1;
    public GameObject backImage;
    public GameObject canAttackImage;
    public PlayerCtrl playerCtrl;

    public bool isBack = true;
    // public Transform cardHolder;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        CardUI();
        if(health <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    // 카드 데이터 적용
    public void CardInitiation(CardSO cardObj, int playerID)
    {
        this.cardObj = cardObj;
        if(cardObj.cardImage) image.sprite = cardObj.cardImage;
        cardName.text = cardObj.cardName;
        description.text = cardObj.description;
        health = cardObj.health;
        attack = cardObj.attack;
        cost = cardObj.cost;
        attackCount = 0;
        cardOwnerID = playerID;
        playerCtrl = transform.root.GetComponent<PlayerCtrl>();

        CardUI();
    }

    // private void CardUI()
    // {
    //     backImage.SetActive(isBack);

    //     canAttackImage.SetActive((attackCount > 0
    //         && transform.parent.CompareTag("CardHolder"))
    //         || (playerCtrl.isMyTurn
    //             && transform.parent.CompareTag("Hand")
    //             && cost <= playerCtrl.currentCost
    //             && playerCtrl.PV.IsMine));
        
    //     costText.text = cost.ToString();
    //     healthText.text = health.ToString();
    //     attackText.text = attack.ToString();
    // }
    private void CardUI()
    {
        backImage.SetActive(isBack);

        canAttackImage.SetActive((playerCtrl.isMyTurn &&
            attackCount > 0
            && transform.parent.CompareTag("CardHolder"))
            || (playerCtrl.isMyTurn
                && transform.parent.CompareTag("Hand")
                && cost <= playerCtrl.currentCost
                && playerCtrl.PV.IsMine));
        
        costText.text = cost.ToString();
        healthText.text = health.ToString();
        attackText.text = attack.ToString();
    }

    public void DamageEffect()
    {
        GameObject effectInstance = Instantiate(hitEffect[Random.Range(0, 3)]);
        effectInstance.transform.position = this.transform.position;
        Destroy(effectInstance, 0.45f);
    }

    public void ResetAttackCount()
    {
        attackCount = cardObj.attackCount;
    }
}