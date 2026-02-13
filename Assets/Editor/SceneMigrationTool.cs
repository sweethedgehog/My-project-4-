using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using TMPro;
using CardGame.GameObjects;
using CardGame.Scoring;
using CardGame.Managers;

/// <summary>
/// Editor tool that migrates TutorialScene gameplay objects from Canvas (ScreenSpace-Overlay)
/// to world-space with SpriteRenderers and plain Transforms.
/// Run via menu: Tools > Migrate Scene to World-Space
/// Full Undo support — Ctrl+Z reverts everything.
/// </summary>
public class SceneMigrationTool : Editor
{
    // World-space positions matching MainScene
    private static readonly Vector3 CardBoardPos = new Vector3(3.308f, -1.108f, 0f);
    private static readonly Vector3 CardBoardHandPos = new Vector3(3.296f, -3.094f, 0f);
    private static readonly Vector3 SimpleDeckPos = new Vector3(-1.085f, -2.010f, 0f);
    private static readonly Vector3 GoalPanelPos = new Vector3(-1.0846f, 0.310771f, 0f);
    private static readonly Vector3 BallLocalPos = new Vector3(-0.05f, -0.3f, 0f);
    private static readonly Vector3 AdviceGlowLocalPos = new Vector3(0f, 0.02f, 0f);
    private static readonly Vector3 AdviceGlowScale = new Vector3(1.02f, 1.02f, 1.02f);

    [MenuItem("Tools/Migrate Scene to World-Space")]
    public static void MigrateCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != "TutorialScene")
        {
            if (!EditorUtility.DisplayDialog("Warning",
                $"This tool is designed for TutorialScene but current scene is '{sceneName}'. Continue anyway?",
                "Continue", "Cancel"))
                return;
        }

        if (!EditorUtility.DisplayDialog("Migrate to World-Space",
            "This will restructure gameplay objects from Canvas to world-space.\n\n" +
            "Make sure you have committed or saved your changes.\n" +
            "You can undo with Ctrl+Z if anything goes wrong.",
            "Migrate", "Cancel"))
            return;

        Undo.SetCurrentGroupName("Migrate Scene to World-Space");
        int undoGroup = Undo.GetCurrentGroup();

        try
        {
            // Phase 1: Fix camera
            FixCamera();

            // Phase 2: Create Gameplay Root
            GameObject gameplayRoot = CreateGameplayRoot();

            // Phase 3: Migrate CardBoard and CardBoardHand
            var (newCardBoard, newCardBoardCB, newScorer) = MigrateCardBoard(
                gameplayRoot.transform, "CardBoard", CardBoardPos, 5.92f, 1.58f, 1.0f, false);

            var (newCardBoardHand, newCardBoardHandCB, _) = MigrateCardBoard(
                gameplayRoot.transform, "CardBoardHand", CardBoardHandPos, 5.95f, 1.70f, 1.0f, true);

            // Phase 4: Migrate SimpleDeck
            var (newDeck, newDeckObj) = MigrateSimpleDeck(gameplayRoot.transform, newCardBoardHandCB);

            // Phase 5: Migrate GoalPanel
            var (goalPanel, ballSR, numberTMP, suitTMP) = MigrateGoalPanel(gameplayRoot.transform);

            // Phase 6: Migrate Background
            MigrateBackground(gameplayRoot.transform);

            // Phase 7: Rewire TutorialManager references
            RewireTutorialManager(newDeckObj, newCardBoardHandCB, newCardBoardCB, newScorer,
                goalPanel, numberTMP, suitTMP, ballSR);

            // Phase 8: Clean up old Canvas children
            CleanupOldCanvasChildren();

            // Phase 9: Fix CardBoard edgeExtension values on any remaining old references
            // (handled during migration)

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            Debug.Log("[Migration] Scene migration complete! Review the scene and save with Ctrl+S.");
            Debug.Log("[Migration] If anything looks wrong, undo with Ctrl+Z.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Migration] Error during migration: {e.Message}\n{e.StackTrace}");
            Debug.LogError("[Migration] Use Ctrl+Z to undo partial changes.");
            Undo.CollapseUndoOperations(undoGroup);
        }
    }

    private static void FixCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = Object.FindFirstObjectByType<Camera>();
        }

        if (cam == null)
        {
            Debug.LogWarning("[Migration] No camera found!");
            return;
        }

        Undo.RecordObject(cam, "Fix Camera");
        Undo.RecordObject(cam.transform, "Fix Camera Transform");

        cam.orthographic = true;
        cam.orthographicSize = 5.4f;
        cam.transform.position = new Vector3(0, 0, -10);

        Debug.Log($"[Migration] Camera fixed: orthographic size=5.4, pos=(0,0,-10)");
    }

    private static GameObject CreateGameplayRoot()
    {
        GameObject root = new GameObject("Gameplay Root");
        root.transform.position = Vector3.zero;
        root.layer = 0; // Default
        Undo.RegisterCreatedObjectUndo(root, "Create Gameplay Root");

        Debug.Log("[Migration] Created Gameplay Root at (0,0,0)");
        return root;
    }

    private static (GameObject, CardBoard, CardScorer) MigrateCardBoard(
        Transform parent, string objectName, Vector3 position,
        float boardWidth, float boardHeight, float edgeExtension, bool isHandBoard)
    {
        // Find old object
        CardBoard[] allBoards = Object.FindObjectsByType<CardBoard>(FindObjectsSortMode.None);
        CardBoard oldBoard = null;
        foreach (var b in allBoards)
        {
            if (b.gameObject.name == objectName)
            {
                oldBoard = b;
                break;
            }
        }

        if (oldBoard == null)
        {
            Debug.LogWarning($"[Migration] Could not find old {objectName}!");
            // Create from scratch
            GameObject newGO = new GameObject(objectName);
            newGO.transform.SetParent(parent);
            newGO.transform.localPosition = position;
            newGO.layer = 0;
            var sr = newGO.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Gameplay";
            var cb = newGO.AddComponent<CardBoard>();
            Undo.RegisterCreatedObjectUndo(newGO, $"Create {objectName}");
            return (newGO, cb, null);
        }

        // Create new world-space object
        GameObject newObj = new GameObject(objectName);
        newObj.transform.SetParent(parent);
        newObj.transform.localPosition = position;
        newObj.layer = 0; // Default

        // Add SpriteRenderer for board visual
        SpriteRenderer boardSR = newObj.AddComponent<SpriteRenderer>();
        boardSR.sortingLayerName = "Gameplay";

        // Add CardBoard component and copy serialized data
        CardBoard newBoard = newObj.AddComponent<CardBoard>();
        EditorUtility.CopySerialized(oldBoard, newBoard);

        // Override with correct world-space values
        SerializedObject so = new SerializedObject(newBoard);
        so.FindProperty("boardWidth").floatValue = boardWidth;
        so.FindProperty("boardHeight").floatValue = boardHeight;
        so.FindProperty("edgeExtension").floatValue = edgeExtension;
        // Fix frozenAlpha -> frozenBrightness (field was renamed, old scene still has frozenAlpha)
        var brightnessField = so.FindProperty("frozenBrightness");
        if (brightnessField != null)
            brightnessField.floatValue = 0.65f;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Re-parent children (like CardScorer) from old board to new board
        CardScorer scorer = null;
        for (int i = oldBoard.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = oldBoard.transform.GetChild(i);
            Undo.SetTransformParent(child, newObj.transform, "Reparent board child");
            scorer = child.GetComponent<CardScorer>();
        }

        // Wire scorer reference on the new board
        if (scorer != null)
        {
            so = new SerializedObject(newBoard);
            so.FindProperty("scorer").objectReferenceValue = scorer;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        Undo.RegisterCreatedObjectUndo(newObj, $"Create {objectName}");

        Debug.Log($"[Migration] Migrated {objectName} to world-space at {position}, " +
                  $"boardWidth={boardWidth}, boardHeight={boardHeight}, edgeExtension={edgeExtension}" +
                  (scorer != null ? ", CardScorer re-parented" : ""));

        return (newObj, newBoard, scorer);
    }

    private static (GameObject, SimpleDeckObject) MigrateSimpleDeck(
        Transform parent, CardBoard newHandBoard)
    {
        // Find old deck
        SimpleDeckObject oldDeck = Object.FindFirstObjectByType<SimpleDeckObject>();

        // Create new world-space deck
        GameObject newObj = new GameObject("SimpleDeck");
        newObj.transform.SetParent(parent);
        newObj.transform.localPosition = SimpleDeckPos;
        newObj.layer = 0; // Default

        // Add SpriteRenderer (required by SimpleDeckObject)
        SpriteRenderer deckSR = newObj.AddComponent<SpriteRenderer>();
        deckSR.sortingLayerName = "Gameplay";

        // Add BoxCollider2D for OnMouseDown click detection and Physics2D.Raycast
        BoxCollider2D col = newObj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.035f, 1.72f); // Match deck sprite size

        // Add SimpleDeckObject and copy serialized data
        SimpleDeckObject newDeck = newObj.AddComponent<SimpleDeckObject>();
        if (oldDeck != null)
        {
            EditorUtility.CopySerialized(oldDeck, newDeck);
        }

        // Override references for world-space
        SerializedObject so = new SerializedObject(newDeck);
        so.FindProperty("targetBoard").objectReferenceValue = newHandBoard;
        so.FindProperty("cardCountText").objectReferenceValue = null; // Not used in world-space
        so.ApplyModifiedPropertiesWithoutUndo();

        // Create AdviceGlow child
        GameObject adviceGlow = CreateAdviceGlow(newObj.transform);

        // Wire adviceGlow on deck
        so = new SerializedObject(newDeck);
        so.FindProperty("adviceGlow").objectReferenceValue = adviceGlow;
        so.ApplyModifiedPropertiesWithoutUndo();

        Undo.RegisterCreatedObjectUndo(newObj, "Create SimpleDeck");

        // Copy sprite from old deck if available
        if (oldDeck != null)
        {
            SpriteRenderer oldSR = oldDeck.GetComponent<SpriteRenderer>();
            Image oldImage = oldDeck.GetComponent<Image>();

            if (oldSR != null && oldSR.sprite != null)
            {
                deckSR.sprite = oldSR.sprite;
            }
            else if (oldImage != null && oldImage.sprite != null)
            {
                deckSR.sprite = oldImage.sprite;
            }
        }

        Debug.Log($"[Migration] Migrated SimpleDeck to world-space at {SimpleDeckPos} with AdviceGlow child");
        return (newObj, newDeck);
    }

    private static GameObject CreateAdviceGlow(Transform deckTransform)
    {
        GameObject glow = new GameObject("AdviceGlow");
        glow.transform.SetParent(deckTransform);
        glow.transform.localPosition = AdviceGlowLocalPos;
        glow.transform.localScale = AdviceGlowScale;
        glow.layer = 5; // Matching MainScene

        // SpriteRenderer
        SpriteRenderer sr = glow.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Cards";
        sr.sortingOrder = 0;

        // Try to load glow sprite
        Sprite glowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/cards/card_glow.png");
        if (glowSprite != null)
        {
            sr.sprite = glowSprite;
        }
        else
        {
            Debug.LogWarning("[Migration] Could not load card_glow.png sprite. Assign manually.");
        }

        // Animator
        RuntimeAnimatorController controller =
            AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/animaion/Highlight_Deck.controller");
        if (controller != null)
        {
            Animator anim = glow.AddComponent<Animator>();
            anim.runtimeAnimatorController = controller;
        }
        else
        {
            Debug.LogWarning("[Migration] Could not load Highlight_Deck.controller. Assign manually.");
        }

        Undo.RegisterCreatedObjectUndo(glow, "Create AdviceGlow");
        return glow;
    }

    private static (GameObject, SpriteRenderer, TextMeshPro, TextMeshPro) MigrateGoalPanel(
        Transform parent)
    {
        // Create GoalPanel container
        GameObject goalPanel = new GameObject("GoalPanel");
        goalPanel.transform.SetParent(parent);
        goalPanel.transform.localPosition = GoalPanelPos;
        goalPanel.layer = 0;
        Undo.RegisterCreatedObjectUndo(goalPanel, "Create GoalPanel");

        // Create Ball child with SpriteRenderer
        GameObject ball = new GameObject("Ball");
        ball.transform.SetParent(goalPanel.transform);
        ball.transform.localPosition = BallLocalPos;
        ball.layer = 0;

        SpriteRenderer ballSR = ball.AddComponent<SpriteRenderer>();
        ballSR.sortingLayerName = "Gameplay";
        ballSR.sortingOrder = 1;

        // Copy Ball sprite from old Ball if it exists
        CopySpriteFromOldObject("Ball", ballSR);

        Undo.RegisterCreatedObjectUndo(ball, "Create Ball");

        // Create Number child with TextMeshPro 3D
        // Note: TextMeshPro adds RectTransform automatically, but under a plain Transform parent
        // it acts as world-space text (not Canvas text)
        GameObject number = new GameObject("Number");
        number.transform.SetParent(goalPanel.transform);
        number.transform.localPosition = Vector3.zero;
        number.transform.localScale = new Vector3(0.05f, 0.05f, 1f);
        number.layer = 0;

        TextMeshPro numberTMP = number.AddComponent<TextMeshPro>();
        numberTMP.alignment = TextAlignmentOptions.Center;
        numberTMP.fontSize = 90;

        // Load font
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Arkhip_font SDF.asset");
        if (font != null)
        {
            numberTMP.font = font;
        }
        else
        {
            Debug.LogWarning("[Migration] Could not load Arkhip_font SDF. Assign font manually on Number.");
        }

        // Set RectTransform to match MainScene
        RectTransform numberRT = number.GetComponent<RectTransform>();
        if (numberRT != null)
        {
            numberRT.anchoredPosition = new Vector2(-0.0102f, -0.0451f);
            numberRT.sizeDelta = new Vector2(24.0059f, 23.4275f);
        }

        // Add SortingGroup to control render order
        SortingGroup numberSG = number.AddComponent<SortingGroup>();
        numberSG.sortingLayerName = "Cards";
        numberSG.sortingOrder = 0;

        Undo.RegisterCreatedObjectUndo(number, "Create Number");

        // Create Suit child with TextMeshPro 3D
        GameObject suit = new GameObject("Suit");
        suit.transform.SetParent(goalPanel.transform);
        suit.transform.localPosition = Vector3.zero;
        suit.transform.localScale = new Vector3(0.05f, 0.05f, 1f);
        suit.layer = 0;

        TextMeshPro suitTMP = suit.AddComponent<TextMeshPro>();
        suitTMP.alignment = TextAlignmentOptions.Center;
        suitTMP.fontSize = 90;

        if (font != null)
        {
            suitTMP.font = font;
        }

        RectTransform suitRT = suit.GetComponent<RectTransform>();
        if (suitRT != null)
        {
            suitRT.anchoredPosition = Vector2.zero;
            suitRT.sizeDelta = new Vector2(24f, 24f);
        }

        SortingGroup suitSG = suit.AddComponent<SortingGroup>();
        suitSG.sortingLayerName = "Cards";
        suitSG.sortingOrder = 0;

        Undo.RegisterCreatedObjectUndo(suit, "Create Suit");

        Debug.Log($"[Migration] Created GoalPanel at {GoalPanelPos} with Ball, Number, Suit children");
        return (goalPanel, ballSR, numberTMP, suitTMP);
    }

    private static void CopySpriteFromOldObject(string objectName, SpriteRenderer targetSR)
    {
        // Try to find old object and copy its sprite
        GameObject oldObj = GameObject.Find(objectName);
        if (oldObj == null) return;

        // Check for SpriteRenderer first
        SpriteRenderer oldSR = oldObj.GetComponent<SpriteRenderer>();
        if (oldSR != null && oldSR.sprite != null)
        {
            targetSR.sprite = oldSR.sprite;
            return;
        }

        // Check for Image (Canvas component)
        Image oldImage = oldObj.GetComponent<Image>();
        if (oldImage != null && oldImage.sprite != null)
        {
            targetSR.sprite = oldImage.sprite;
        }
    }

    private static void MigrateBackground(Transform parent)
    {
        // Find old background
        GameObject oldBG = GameObject.Find("Background");
        Sprite bgSprite = null;

        if (oldBG != null)
        {
            Image bgImage = oldBG.GetComponent<Image>();
            if (bgImage != null && bgImage.sprite != null)
            {
                bgSprite = bgImage.sprite;
            }

            SpriteRenderer bgSR = oldBG.GetComponent<SpriteRenderer>();
            if (bgSR != null && bgSR.sprite != null)
            {
                bgSprite = bgSR.sprite;
            }
        }

        // Create new world-space background
        GameObject newBG = new GameObject("Background");
        newBG.transform.SetParent(parent);
        newBG.transform.localPosition = Vector3.zero;
        // Move to first child so it renders behind everything
        newBG.transform.SetAsFirstSibling();
        newBG.layer = 0;

        SpriteRenderer sr = newBG.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Background";
        sr.sortingOrder = 0;

        if (bgSprite != null)
        {
            sr.sprite = bgSprite;
        }
        else
        {
            Debug.LogWarning("[Migration] Could not find Background sprite. Assign manually.");
        }

        Undo.RegisterCreatedObjectUndo(newBG, "Create Background");
        Debug.Log("[Migration] Created world-space Background");
    }

    private static void RewireTutorialManager(
        SimpleDeckObject newDeck, CardBoard newHandBoard, CardBoard newTargetBoard,
        CardScorer newScorer, GameObject goalPanel,
        TextMeshPro numberTMP, TextMeshPro suitTMP, SpriteRenderer ballSR)
    {
        TutorialManager tm = Object.FindFirstObjectByType<TutorialManager>();
        if (tm == null)
        {
            Debug.LogWarning("[Migration] TutorialManager not found! Skipping reference wiring.");
            return;
        }

        Undo.RecordObject(tm, "Rewire TutorialManager");

        SerializedObject so = new SerializedObject(tm);

        SetPropertyIfExists(so, "deck", newDeck);
        SetPropertyIfExists(so, "handBoard", newHandBoard);
        SetPropertyIfExists(so, "targetBoard", newTargetBoard);

        if (newScorer != null)
            SetPropertyIfExists(so, "scorer", newScorer);

        SetPropertyIfExists(so, "goalDisplay", goalPanel);
        SetPropertyIfExists(so, "goalValueText", numberTMP);
        SetPropertyIfExists(so, "goalSuitText", suitTMP);
        SetPropertyIfExists(so, "ballImage", ballSR);

        so.ApplyModifiedProperties();

        Debug.Log("[Migration] Rewired TutorialManager references: deck, handBoard, targetBoard, scorer, " +
                  "goalDisplay, goalValueText, goalSuitText, ballImage");
    }

    private static void SetPropertyIfExists(SerializedObject so, string propertyName, Object value)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop != null)
        {
            prop.objectReferenceValue = value;
        }
        else
        {
            Debug.LogWarning($"[Migration] Property '{propertyName}' not found on {so.targetObject.GetType().Name}");
        }
    }

    private static void CleanupOldCanvasChildren()
    {
        // Find the Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[Migration] Canvas not found! Skipping cleanup.");
            return;
        }

        Transform canvasTransform = canvas.transform;
        string[] objectsToRemove = { "CardBoard", "CardBoardHand", "SimpleDeck", "GoalPanel", "Background" };

        foreach (string name in objectsToRemove)
        {
            Transform old = canvasTransform.Find(name);
            if (old != null)
            {
                Undo.DestroyObjectImmediate(old.gameObject);
                Debug.Log($"[Migration] Removed old Canvas child: {name}");
            }
        }
    }
}
