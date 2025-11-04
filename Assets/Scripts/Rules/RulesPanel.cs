using UnityEngine;

public enum RulesCords
{
    Open = 300,
    closed = 1550
}
public class RulesPanel : MonoBehaviour
{
    public float speedScale = 2;
    private RectTransform rulesPanel;
    private float localY;
    private Vector2 targetPos;
    void Start()
    {
        rulesPanel = GetComponent<RectTransform>();
        localY = rulesPanel.anchoredPosition.y;
        targetPos = rulesPanel.anchoredPosition;
    }
    void Update()
    {
        if (targetPos.x == rulesPanel.anchoredPosition.x) return;
        Vector2 target = targetPos - rulesPanel.anchoredPosition;
        rulesPanel.anchoredPosition += target.normalized * speedScale * Time.deltaTime * target.magnitude;
    }
    public void moveTo(RulesCords rulesCords)
    {
        targetPos = new Vector2((float)rulesCords, localY);
    }
}
