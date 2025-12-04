using UnityEngine;
using CardGame.Core;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    
    [SerializeField] private AudioClip rosesActivatingSound;
    [SerializeField] private AudioClip coinsActivatingSound;
    [SerializeField] private AudioClip crownsActivatingSound;
    [SerializeField] private AudioClip skullsActivatingSound;

    public void ActivateSuitSound(Suits suit)
    {
        if (suit == Suits.Coins)
        {
            sfxSource.PlayOneShot(coinsActivatingSound);
        }
        else if (suit == Suits.Crowns)
        {
            sfxSource.PlayOneShot(crownsActivatingSound);
        }
        else if (suit == Suits.Roses)
        {
            sfxSource.PlayOneShot(rosesActivatingSound);
        }
        else if (suit == Suits.Skulls)
        {
            sfxSource.PlayOneShot(skullsActivatingSound);
        }
    }
    
    public void ActivateValueSound()
    {}
    
    public void HoverSound()
    {}
    
    public void HoldingCardSound()
    {}
    
    public void ClickSound()
    {}
    
    public void RollOutRulesSound()
    {}
    
    public void PutCardSound()
    {}
    
    public void ActivatingZoneSound()
    {}
}
