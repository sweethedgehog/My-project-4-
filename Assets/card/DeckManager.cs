using System.Collections.Generic;
using DefaultNamespace;
using TMPro;
using Unity.Collections;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public GameObject CardOld;
    public Sprite[] yellowCards;
    public Sprite[] blueCards;
    public Sprite[] redCards;
    public Sprite[] greenCards;
    public Vector3 startPos;
    private List<Pair<int, int>> cards = new ();
    private TMP_Text cardsLeft;
    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                for (int k = 0; k < 3 - j; k++)
                {
                    cards.Add(new Pair<int, int>(i, j));
                }
            }
        }
        ShuffleList(cards);
        cardsLeft = GameObject.Find("Cards Counter").GetComponent<TextMeshProUGUI>();
        Draw();
    }

    public void GetNextSequence()
    {
        GameObject obj = GameObject.Find("Upper Sequence");
        CardSequence cardSequence = obj.GetComponent<CardSequence>();
        int score = cardSequence.getScore();
        Debug.Log(score);
        Draw();
    }
    public void Draw()
    {
        GameObject obj = GameObject.Find("Downer Sequence");
        CardSequence cardSequence = obj.GetComponent<CardSequence>();
        int buf = cardSequence.CardsMustBe - cardSequence.cards.Count;
        for (int i = 0; i < buf; ++i)
        {
            if (cards.Count <= 0) break;
            GameObject card = Instantiate(CardOld, startPos, Quaternion.identity);
            cardSequence.cards.Add(card.GetComponent<CardOld>());
            switch (cards[cards.Count - 1].first)
            {
                case 0:
                    cardSequence.cards[cardSequence.cards.Count - 1].cardSprite = yellowCards[cards[cards.Count - 1].second];
                    break;
                case 1:
                    cardSequence.cards[cardSequence.cards.Count - 1].cardSprite = blueCards[cards[cards.Count - 1].second];
                    break;
                case 2:
                    cardSequence.cards[cardSequence.cards.Count - 1].cardSprite = greenCards[cards[cards.Count - 1].second];
                    break;
                default:
                    cardSequence.cards[cardSequence.cards.Count - 1].cardSprite = redCards[cards[cards.Count - 1].second];
                    break;
            }
            cardSequence.cards[cardSequence.cards.Count - 1].setParent(cardSequence);
            cards.RemoveAt(cards.Count - 1);
        }
        cardSequence.rebaseAll();
        UpdateText();
    }
    private void UpdateText() => cardsLeft.text = cards.Count + " Cards left";
    void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            int k = Random.Range(0, n--);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }
}
