using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CardSequence : MonoBehaviour
{
    public GameObject card;
    public int StartCardsCount = 5;
    public int CardsMustBe = 5;
    public float MinX = -7.5f, MaxX = 7.5f;
    public Sprite[] CardsTextures;
    public int yPos = 2;
    public int terminator = 0;
    private List<Card> cards = new List<Card>();
    void Start()
    {
        for (int i = 0; i < StartCardsCount; i++)
        {
            GameObject obj = Instantiate(card, card.transform.position, Quaternion.identity);
            cards.Add(obj.GetComponent<Card>());
            cards[i].CardSprite = CardsTextures[Random.Range(0, CardsTextures.Length)];
            cards[i].cardValue = i
            cards[i].setParent(this);
        }
        rebaseAll();
    }

    public void addCard(Card card)
    {
        if (cards.Count == 0) cards.Add(card);
        else
        {
            float minDif = -1f;
            int index = 0;
            for (int i = 0; i < cards.Count; i++)
            {
                float buf = Math.Abs(card.transform.position.x - getCoolX(i));
                if (buf < minDif || minDif <= -0.5f)
                {
                    minDif = buf;
                    index = i;
                }
                else break;
            }

            if (cards[index].transform.position.x < card.transform.position.x) index++;
            cards.Insert(index, card);
        }
        rebaseAll();
    }
    public void removeCard(Card card)
    {
        cards.Remove(card);
        rebaseAll();
    }

    private void rebaseAll()
    {
        for (int i = 0; i < cards.Count; i++) cards[i].setHomePos(new Vector3(getCoolX(i), yPos, 0));
    }
    private float getCoolX(int i)
    {
        float mid = MinX + (MaxX - MinX) / 2;
        float offset = mid - MinX;
        float x = (float) cards.Count / CardsMustBe;
        return map(i, 0, cards.Count - 1, mid - offset * x, mid + offset * x);
    }
    private float map(float x, float in_min, float in_max, float out_min, float out_max)
    {
        if (in_min == in_max) return out_min + (out_max - out_min) / 2f;
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }

}
