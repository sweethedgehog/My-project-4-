using System.Collections;
using UnityEngine;

public class CatAnimationController : MonoBehaviour
{
    public Animator anim;

    private bool isTalking = false;

    void Start()
    {
        CatIdle();
    }

    /// <summary>
    /// Sets the cat to idle state (default state)
    /// </summary>
    public void CatIdle()
    {
        isTalking = false;
        anim.SetBool("AnimSpeech", false);
    }

    /// <summary>
    /// Sets the cat to talking state
    /// </summary>
    public void CatTalk()
    {
        isTalking = true;
        anim.SetBool("AnimSpeech", true);
    }

    /// <summary>
    /// Makes the cat talk for a specified duration, then returns to idle
    /// </summary>
    public void CatTalkForDuration(float duration)
    {
        StartCoroutine(TalkForDurationCoroutine(duration));
    }

    private IEnumerator TalkForDurationCoroutine(float duration)
    {
        CatTalk();
        yield return new WaitForSeconds(duration);
        CatIdle();
    }

    public bool IsTalking()
    {
        return isTalking;
    }
}
