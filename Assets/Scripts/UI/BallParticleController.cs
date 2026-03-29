using UnityEngine;
using UnityEngine.VFX;
using CardGame.Core;

namespace CardGame.UI
{
    public class BallParticleController : MonoBehaviour
    {
        [Header("Suit Colors (HDR)")]
        [ColorUsage(true, true)]
        [SerializeField] private Color coinsColor = Color.green;
        [ColorUsage(true, true)]
        [SerializeField] private Color crownsColor = Color.yellow;
        [ColorUsage(true, true)]
        [SerializeField] private Color skullsColor = Color.blue;
        [ColorUsage(true, true)]
        [SerializeField] private Color rosesColor = Color.red;

        [Header("Emission Rates")]
        [SerializeField] private int passedRate = 50;
        [SerializeField] private int unpassedRate = 5;

        private VisualEffect vfx;

        private void Awake()
        {
            vfx = GetComponent<VisualEffect>();
        }

        public void SetSuitColor(Suits suit)
        {
            if (vfx == null) return;

            vfx.SetVector4("Partical_color", GetColorForSuit(suit));
        }

        public void SetGoalComplete(bool complete)
        {
            if (vfx == null) return;

            vfx.SetInt("Partical_rate", complete ? passedRate : unpassedRate);
        }

        private Color GetColorForSuit(Suits suit)
        {
            switch (suit)
            {
                case Suits.Coins: return coinsColor;
                case Suits.Crowns: return crownsColor;
                case Suits.Skulls: return skullsColor;
                case Suits.Roses: return rosesColor;
                default: return Color.white;
            }
        }
    }
}
