using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    [SerializeField] private DeckSO deckSO;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform deckHolder;
    public List<Card> deck;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DeckInitiation(int playerID)
    {
        for(int i = 0; i < deckSO.deck.Count; i++)
        {
            GameObject card = Instantiate(cardPrefab, new Vector3(4.0f, -4.2f, 0.0f), Quaternion.identity, deckHolder);
            // card.transform.localScale = new Vector3(0.3f, 0.3f, 1);
            Card cardComponent = card.GetComponent<Card>();
            cardComponent.CardInitiation(deckSO.deck[i], playerID);
            deck.Add(cardComponent);
        }
    }

    public void SetCardIndex(int[] index)
    {
        List<Card> newDeck = new List<Card>();
        for(int i = 0; i < deck.Count; i++)
        {
            newDeck.Add(deck[index[i]]);
        }
        deck = newDeck;
    }
}