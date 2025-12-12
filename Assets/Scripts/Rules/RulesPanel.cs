using UnityEngine;

namespace CardGame.UI
{
    public enum RulesCords
    {
        Open = 300,
        Closed = 1550  // Fixed capitalization
    }
    
    /// <summary>
    /// Controls the sliding animation of the rules panel
    /// </summary>
    public class RulesPanel : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float speedScale = 2f;
        [SerializeField] private float arrivalThreshold = 1f;  // Distance to consider "arrived"
        
        private RectTransform rectTransform;
        private RulesPanelSound panelSound;
        private float localY;
        private Vector2 targetPos;
        private bool isMoving = false;
        
        /// <summary>
        /// Is the panel currently moving?
        /// </summary>
        public bool IsMoving => isMoving;
        
        /// <summary>
        /// Current target position state
        /// </summary>
        public RulesCords CurrentTarget { get; private set; }
        
        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            panelSound = GetComponent<RulesPanelSound>();
        }
        
        void Start()
        {
            localY = rectTransform.anchoredPosition.y;
            targetPos = rectTransform.anchoredPosition;
            
            // Determine initial state based on position
            CurrentTarget = rectTransform.anchoredPosition.x < 1000 ? RulesCords.Open : RulesCords.Closed;
        }
        
        void Update()
        {
            float distanceToTarget = Vector2.Distance(targetPos, rectTransform.anchoredPosition);
            
            // Check if we've arrived
            if (distanceToTarget <= arrivalThreshold)
            {
                if (isMoving)
                {
                    // Snap to exact position
                    rectTransform.anchoredPosition = targetPos;
                    isMoving = false;
                }
                return;
            }
            
            // Move towards target
            isMoving = true;
            Vector2 direction = (targetPos - rectTransform.anchoredPosition).normalized;
            float moveAmount = speedScale * Time.deltaTime * distanceToTarget;
            rectTransform.anchoredPosition += direction * moveAmount;
        }
        
        /// <summary>
        /// Move the panel to a specific position (open or closed)
        /// </summary>
        public void MoveTo(RulesCords targetState)
        {
            CurrentTarget = targetState;
            targetPos = new Vector2((float)targetState, localY);
            
            // Notify sound component
            if (panelSound != null)
            {
                panelSound.PlaySoundForState(targetState);
            }
        }
        
        /// <summary>
        /// Toggle between open and closed states
        /// </summary>
        public void Toggle()
        {
            if (CurrentTarget == RulesCords.Open)
            {
                MoveTo(RulesCords.Closed);
            }
            else
            {
                MoveTo(RulesCords.Open);
            }
        }
        
        /// <summary>
        /// Open the panel
        /// </summary>
        public void Open()
        {
            MoveTo(RulesCords.Open);
        }
        
        /// <summary>
        /// Close the panel
        /// </summary>
        public void Close()
        {
            MoveTo(RulesCords.Closed);
        }
    }
}