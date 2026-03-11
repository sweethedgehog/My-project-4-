using UnityEngine;

public class CandleFlameController : MonoBehaviour
{
    [Header("Настройки аниматора")]
    public Animator animator;

    [Header("Рассинхрон (offset) для разных свечей")]
    [Range(0f, 1f)] public float minOffset = 0f;
    [Range(0f, 1f)] public float maxOffset = 1f;

    private int targetLoops;     // сколько циклов low нужно сыграть
    private int currentLoops;    // сколько уже сыграно
    private bool wasInHigh;      // чтобы отследить возврат в low

    
    private const string LowState = "LowFire";
    private const string HighState = "HighFire";

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        float randomOffset = Random.Range(minOffset, maxOffset);
        animator.Play(0, -1, randomOffset);   // 0 = базовый layer, -1 = любой state

        StartNewCycle();
    }

    private void StartNewCycle()
    {
        targetLoops = Random.Range(2, 5);  
        currentLoops = 0;
    }

    public void OnLowCycleComplete()
    {
        currentLoops++;

        if (currentLoops >= targetLoops)
        {
            animator.SetTrigger("ToHigh");
        }
    }

    void Update()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        bool nowInLow = state.IsName(LowState);
        bool nowInHigh = state.IsName(HighState);

        if (wasInHigh && nowInLow)
        {
            StartNewCycle();
        }

        wasInHigh = nowInHigh;
    }
}