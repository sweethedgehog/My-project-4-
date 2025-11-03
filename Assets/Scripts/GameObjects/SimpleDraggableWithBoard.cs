using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CardGame.Cards;

namespace CardGame.GameObjects
{
    /// <summary>
    /// Enhanced draggable that works with CardBoard
    /// Automatically adds/removes cards from boards
    /// </summary>
    public class SimpleDraggableWithBoard : MonoBehaviour, 
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
    {
        private RectTransform rectTransform;
        private Canvas canvas;
        private CanvasGroup canvasGroup;
        private SimpleCard simpleCard;
        private CardBoard currentBoard;
        private CardBoard hoverBoard;
        private Transform originalParent;
        private bool isDragging = false;
        private Vector2 offset;
        
        [Header("Drag Settings")]
        [SerializeField] private float hoverDetectionDistance = 100f;
        
        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            simpleCard = GetComponent<SimpleCard>();
            
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            Image img = GetComponent<Image>();
            if (img != null)
            {
                img.raycastTarget = true;
            }
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("Card clicked!");
            
            Vector2 clickPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out clickPos
            );
            
            offset = rectTransform.anchoredPosition - clickPos;
            transform.SetAsLastSibling();
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log("Begin drag");
            isDragging = true;
            canvasGroup.blocksRaycasts = false;
            
            // Remember original parent
            originalParent = transform.parent;
            
            // Store current SCREEN position before parent change
            Vector3 worldPos = rectTransform.position;
            
            // Remove from current board if on one
            currentBoard = GetComponentInParent<CardBoard>();
            if (currentBoard != null)
            {
                currentBoard.RemoveCard(simpleCard);
            }
            
            // Move to canvas root for free dragging
            transform.SetParent(canvas.transform);
            transform.SetAsLastSibling();
            
            // Restore the world position after parent change
            rectTransform.position = worldPos;
            
            // Recalculate offset based on current position
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out mousePos
            );
            
            offset = rectTransform.anchoredPosition - mousePos;
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out mousePos
            );
            
            rectTransform.anchoredPosition = mousePos + offset;
            
            // Check if hovering over a board
            CheckBoardHover();
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log("End drag");
            isDragging = false;
            canvasGroup.blocksRaycasts = true;
            
            // Try to add to a board
            CardBoard targetBoard = FindNearestBoard();
            
            if (targetBoard != null)
            {
                targetBoard.AddCard(simpleCard);
                Debug.Log($"Card added to board: {targetBoard.name}");
            }
            else
            {
                Debug.Log("Card not dropped on any board");
            }
        }
        
        private void CheckBoardHover()
        {
            CardBoard[] allBoards = FindObjectsOfType<CardBoard>();
            CardBoard closestBoard = null;
            float closestDistance = float.MaxValue;
            
            foreach (CardBoard board in allBoards)
            {
                RectTransform boardRect = board.GetComponent<RectTransform>();
                float distance = Vector2.Distance(rectTransform.anchoredPosition, boardRect.anchoredPosition);
                
                if (distance < hoverDetectionDistance && distance < closestDistance)
                {
                    closestBoard = board;
                    closestDistance = distance;
                }
            }
            
            hoverBoard = closestBoard;
        }
        
        private CardBoard FindNearestBoard()
        {
            CardBoard[] allBoards = FindObjectsOfType<CardBoard>();
            CardBoard nearestBoard = null;
            float nearestDistance = float.MaxValue;
            
            foreach (CardBoard board in allBoards)
            {
                RectTransform boardRect = board.GetComponent<RectTransform>();
                float distance = Vector2.Distance(rectTransform.anchoredPosition, boardRect.anchoredPosition);
                
                if (distance < hoverDetectionDistance && distance < nearestDistance)
                {
                    nearestBoard = board;
                    nearestDistance = distance;
                }
            }
            
            return nearestBoard;
        }
    }
}