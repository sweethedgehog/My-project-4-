using System.Collections;
using UnityEngine;

public class Test_anim : MonoBehaviour
{
    public Animator anim;

    void Start()
    {
        StartCoroutine(AnimationSequence());
    }

    IEnumerator AnimationSequence()
    {
        yield return new WaitForSeconds(2f);
        anim.SetBool("AnimSpeech", true);
        Debug.Log("Говорим");

       
        yield return new WaitForSeconds(2f);

     
        anim.SetBool("AnimSpeech", false);
        Debug.Log("Моргаем");


        yield return new WaitForSeconds(15f);
        anim.SetBool("AnimSpeech", true);
        Debug.Log("Снова Говорим");

        yield return new WaitForSeconds(2f);


        anim.SetBool("AnimSpeech", false);
        Debug.Log("Моргаем");
    }

}
