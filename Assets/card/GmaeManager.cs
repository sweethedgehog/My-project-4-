using System.Collections.Generic;
using UnityEngine;

public class GmaeManager : MonoBehaviour
{
    public GameObject card;
    public int cardsCount = 5;
    public Sprite[] CardsTextures;
    private List<Card> cards;
    void Start()
    {
        for (int i = 0; i < cardsCount; i++)
        {
            GameObject obj = Instantiate(card, card.transform.position, Quaternion.identity);
            cards.Add(obj.GetComponent<Card>());
            cards[i].CardSprite = CardsTextures[Random.Range(0, CardsTextures.Length)];
        }
    }

    private void Spawn(Vector3 cardPos)
    {
        GameObject obj = Instantiate(card, cardPos, Quaternion.identity);
        Card buf = obj.GetComponent<Card>();
        buf.CardSprite = CardsTextures[Random.Range(0, CardsTextures.Length)];
    }
}
