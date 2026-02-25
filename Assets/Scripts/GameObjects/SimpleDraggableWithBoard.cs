using System.Collections;
using UnityEngine;
using CardGame.Cards;
using CardGame.UI;

namespace CardGame.GameObjects
{
    /// <summary>
    /// World-space draggable that works with CardBoard
    /// Uses OnMouse* callbacks with BoxCollider2D for drag detection
    /// Automatically adds/removes cards from boards
    /// </summary>
    public class SimpleDraggableWithBoard : MonoBehaviour
    {
        private SimpleCard simpleCard;
        private CardBoard currentBoard;
        private CardBoard hoverBoard;
        private bool isDragging = false;
        private Vector3 offset;
        private Camera mainCamera;
        private SpriteRenderer spriteRenderer;
        private int originalSortingOrder;
        private CardBoard[] cachedBoards;
        private Vector3 originalScale;
        private Coroutine scaleCoroutine;

        [Header("Drag Settings")]
        [SerializeField] private int dragSortingOrder = 100;
        [SerializeField] private bool showDebugInfo = false;

        [Header("Drag Effects")]
        [SerializeField] private Vector3 dragScale = new Vector3(1.15f, 1.15f, 1f);
        [SerializeField] private float scaleTime = 0.1f;
        [SerializeField] private GameObject shadowObject;

        void Awake()
        {
            simpleCard = GetComponent<SimpleCard>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            cachedBoards = FindObjectsOfType<CardBoard>();
            originalScale = transform.localScale;
        }

        void OnMouseDown()
        {
            if (!simpleCard.CanInteract()) return;

            mainCamera = Camera.main;
            Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = transform.position.z;
            offset = transform.position - mouseWorld;
            isDragging = true;

            // Stop smooth movement
            SmoothCardMover mover = GetComponent<SmoothCardMover>();
            if (mover != null)
            {
                mover.Stop();
            }

            // Remove from current board
            currentBoard = GetComponentInParent<CardBoard>();
            if (currentBoard != null)
            {
                currentBoard.RemoveCard(simpleCard);
            }

            // Reparent to scene root for free dragging
            transform.SetParent(null);

            // Raise sorting order so dragged card is on top
            originalSortingOrder = spriteRenderer.sortingOrder;
            spriteRenderer.sortingOrder = dragSortingOrder;

            // Also raise overlay child
            Transform overlayTransform = transform.Find("Overlay");
            if (overlayTransform != null)
            {
                SpriteRenderer osr = overlayTransform.GetComponent<SpriteRenderer>();
                if (osr != null) osr.sortingOrder = dragSortingOrder + 1;
            }

            // Disable collider so it doesn't block board detection
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            if (col != null) col.enabled = false;

            // Scale up and show shadow
            if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
            scaleCoroutine = StartCoroutine(ScaleTo(dragScale));
            if (shadowObject != null) shadowObject.SetActive(true);

            // Play pickup sound
            CardSound cardSound = GetComponent<CardSound>();
            if (cardSound != null) cardSound.PlayPickup();
        }

        void OnMouseDrag()
        {
            if (!isDragging) return;

            Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = transform.position.z;
            transform.position = mouseWorld + offset;

            CheckBoardHover();
        }

        void OnMouseUp()
        {
            if (!isDragging) return;
            isDragging = false;

            // Restore sorting order
            spriteRenderer.sortingOrder = originalSortingOrder;
            Transform overlayTransform = transform.Find("Overlay");
            if (overlayTransform != null)
            {
                SpriteRenderer osr = overlayTransform.GetComponent<SpriteRenderer>();
                if (osr != null) osr.sortingOrder = originalSortingOrder + 1;
            }

            // Re-enable collider
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            if (col != null) col.enabled = true;

            // Scale back and hide shadow
            if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
            scaleCoroutine = StartCoroutine(ScaleTo(originalScale));
            if (shadowObject != null) shadowObject.SetActive(false);

            // Play drop sound
            CardSound cardSound = GetComponent<CardSound>();
            if (cardSound != null) cardSound.PlayDrop();

            // Find target board
            CardBoard targetBoard = FindNearestBoard();

            if (targetBoard != null)
            {
                targetBoard.AddCard(simpleCard);
            }
            else
            {
                // No board found - return to original board or find any board
                if (currentBoard != null)
                {
                    currentBoard.AddCard(simpleCard);
                }
                else
                {
                    if (cachedBoards.Length > 0)
                    {
                        cachedBoards[0].AddCard(simpleCard);
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }

        private void CheckBoardHover()
        {
            CardBoard closestBoard = null;

            Vector2 cardScreenPos = Input.mousePosition;

            foreach (CardBoard board in cachedBoards)
            {
                if (board.IsPositionNearBoard(cardScreenPos))
                {
                    closestBoard = board;
                    break;
                }
            }

            if (hoverBoard != closestBoard)
            {
                hoverBoard = closestBoard;
                if (showDebugInfo && hoverBoard != null)
                {
                    Debug.Log($"Hovering over: {hoverBoard.name}");
                }
            }
        }

        private CardBoard FindNearestBoard()
        {
            CardBoard nearestBoard = null;
            float nearestDistance = float.MaxValue;

            Vector2 cardScreenPos = Input.mousePosition;

            foreach (CardBoard board in cachedBoards)
            {
                if (board.IsPositionNearBoard(cardScreenPos))
                {
                    float distance = Vector2.Distance(transform.position, board.transform.position);

                    if (distance < nearestDistance)
                    {
                        nearestBoard = board;
                        nearestDistance = distance;
                    }
                }
            }

            if (showDebugInfo && nearestBoard != null)
            {
                Debug.Log($"Found board: {nearestBoard.name} at distance {nearestDistance}");
            }
            else if (showDebugInfo)
            {
                Debug.Log("No board found at current position");
            }

            return nearestBoard;
        }

        private IEnumerator ScaleTo(Vector3 target)
        {
            Vector3 start = transform.localScale;
            float elapsed = 0f;
            while (elapsed < scaleTime)
            {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(start, target, elapsed / scaleTime);
                yield return null;
            }
            transform.localScale = target;
        }
    }
}
