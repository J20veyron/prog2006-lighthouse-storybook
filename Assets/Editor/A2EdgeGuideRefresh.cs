using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class A2EdgeGuideRefresh
{
    private const string ButtonPillPath = "Assets/Art/A1/UI/ButtonPill.png";
    private static readonly string[] SceneNames =
    {
        "HomePage", "RockyShore", "Harbour", "Dunes", "MilasHome", "Credits"
    };

    private sealed class GuideSpec
    {
        public readonly string Scene;
        public readonly string Title;
        public readonly string Subtitle;
        public readonly string IconPath;

        public GuideSpec(string scene, string title, string subtitle, string iconPath)
        {
            Scene = scene;
            Title = title;
            Subtitle = subtitle;
            IconPath = iconPath;
        }
    }

    private static readonly GuideSpec[] Guides =
    {
        new GuideSpec("HomePage", "MILA'S SOUND HUNT", "Tap BEGIN to start Mila's journey",
            "Assets/Art/A1/Props/PaperLantern.png"),
        new GuideSpec("RockyShore", "LOOK  •  TAP  •  LISTEN", "Touch a story clue to hear its sound",
            "Assets/Art/A1/Props/Seashell_Spiral.png"),
        new GuideSpec("Harbour", "LOOK  •  TAP  •  LISTEN", "Touch a story clue to hear its sound",
            "Assets/Art/A1/Props/HarbourBell.png"),
        new GuideSpec("Dunes", "LOOK  •  TAP  •  LISTEN", "Touch a story clue to hear its sound",
            "Assets/Art/A1/Props/Seagull_Flying.png"),
        new GuideSpec("MilasHome", "LOOK  •  TAP  •  LISTEN", "Touch a story clue to hear its sound",
            "Assets/Art/A1/Props/PaperLantern.png"),
        new GuideSpec("Credits", "FOUR SOUNDS RESTORED", "Play again to revisit the coast",
            "Assets/Art/A2/Props/Lighthouse_Standalone.png")
    };

    [MenuItem("Tools/PROG2006 A2/Refresh edge guide and rebuild")]
    public static void ApplyAndBuild()
    {
        try
        {
            var pill = AssetDatabase.LoadAssetAtPath<Sprite>(ButtonPillPath);
            if (pill == null)
                throw new InvalidOperationException("Guide background sprite is missing: " + ButtonPillPath);

            foreach (var spec in Guides)
            {
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + spec.Scene + ".unity", OpenSceneMode.Single);
                var backdrop = FindSceneObject(scene, "CoastalScene");
                var storyPanel = FindSceneObject(scene, "StoryPanel");
                var pageTitle = FindSceneObject(scene, "PageTitle");

                if (backdrop == null || storyPanel == null || pageTitle == null)
                    throw new InvalidOperationException("CoastalScene, StoryPanel, or PageTitle is missing from " + spec.Scene);
                var canvas = backdrop.transform.parent;
                if (canvas == null || storyPanel.transform.parent != canvas || pageTitle.transform.parent != canvas)
                    throw new InvalidOperationException("The page layout objects do not share a common UI parent in " + spec.Scene);

                SetStretch(backdrop.GetComponent<RectTransform>(), new Vector2(0.02f, 0.11f), new Vector2(0.66f, 0.84f));
                SetStretch(storyPanel.GetComponent<RectTransform>(), new Vector2(0.68f, 0.11f), new Vector2(0.98f, 0.84f));
                SetStretch(pageTitle.GetComponent<RectTransform>(), new Vector2(0.025f, 0.855f), new Vector2(0.975f, 0.985f));
                MoveNavigationToBottom("BackButton");
                MoveNavigationToBottom("NextButton");
                MoveNavigationToBottom("ReturnHomeButton");
                CreateOrUpdateGuide(canvas, spec, pill, FindText(scene, "PageTitle"));

                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("Could not save scene " + spec.Scene);
                Debug.Log("A2 edge guide and expanded layout saved: " + spec.Scene);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("A2_EDGE_GUIDE_SAVED=6 footerGuides=6 nonBlocking=true layoutExpanded=true");
            A2ProjectBuild.ApplyAndBuild();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            if (Application.isBatchMode)
                EditorApplication.Exit(1);
        }
    }

    private static void CreateOrUpdateGuide(Transform canvas, GuideSpec spec, Sprite pill, TMP_Text fontReference)
    {
        if (fontReference == null || fontReference.font == null)
            throw new InvalidOperationException("Page title font is missing in " + spec.Scene);

        var root = canvas.Find("A2EdgeGuide");
        if (root == null)
        {
            var guide = new GameObject("A2EdgeGuide", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            guide.transform.SetParent(canvas, false);
            root = guide.transform;
        }

        var rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.055f);
        rootRect.anchorMax = new Vector2(0.5f, 0.055f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = Vector2.zero;
        rootRect.sizeDelta = new Vector2(416f, 48f);

        var background = root.GetComponent<Image>();
        background.sprite = pill;
        background.type = Image.Type.Sliced;
        background.color = new Color32(37, 79, 83, 248);
        background.raycastTarget = false;

        var shadow = background.GetComponent<Shadow>();
        if (shadow == null) shadow = background.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0.06f, 0.13f, 0.15f, 0.28f);
        shadow.effectDistance = new Vector2(0f, -2f);
        shadow.useGraphicAlpha = true;

        var outline = background.GetComponent<Outline>();
        if (outline == null) outline = background.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.89f, 0.64f, 0.72f);
        outline.effectDistance = new Vector2(1f, -1f);
        outline.useGraphicAlpha = true;

        var badge = EnsureImageChild(root, "A2EdgeGuideIconBadge");
        var badgeRect = badge.GetComponent<RectTransform>();
        badgeRect.anchorMin = new Vector2(0f, 0.5f);
        badgeRect.anchorMax = new Vector2(0f, 0.5f);
        badgeRect.pivot = new Vector2(0.5f, 0.5f);
        badgeRect.anchoredPosition = new Vector2(23f, 0f);
        badgeRect.sizeDelta = new Vector2(36f, 36f);
        var badgeImage = badge.GetComponent<Image>();
        badgeImage.sprite = pill;
        badgeImage.type = Image.Type.Sliced;
        badgeImage.color = new Color32(255, 207, 111, 255);
        badgeImage.raycastTarget = false;

        var icon = EnsureImageChild(badge.transform, "A2EdgeGuideIcon");
        var iconRect = icon.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.sizeDelta = new Vector2(28f, 28f);
        var iconImage = icon.GetComponent<Image>();
        iconImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spec.IconPath);
        if (iconImage.sprite == null)
            throw new InvalidOperationException("Guide icon is missing: " + spec.IconPath);
        iconImage.preserveAspect = true;
        iconImage.color = Color.white;
        iconImage.raycastTarget = false;

        var main = EnsureTextChild(root, "A2EdgeGuideTitle", fontReference);
        SetGuideText(main, spec.Title, 17f, new Color32(255, 246, 218, 255),
            new Vector2(0.15f, 0.49f), new Vector2(0.98f, 0.98f), TextAlignmentOptions.Center);

        var subtitle = EnsureTextChild(root, "A2EdgeGuideSubtitle", fontReference);
        SetGuideText(subtitle, spec.Subtitle, 12.5f, new Color32(238, 245, 230, 255),
            new Vector2(0.15f, 0.03f), new Vector2(0.98f, 0.52f), TextAlignmentOptions.Center);

        root.SetAsLastSibling();
    }

    private static GameObject EnsureImageChild(Transform parent, string name)
    {
        var child = parent.Find(name);
        if (child != null) return child.gameObject;
        var created = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        created.transform.SetParent(parent, false);
        return created;
    }

    private static TextMeshProUGUI EnsureTextChild(Transform parent, string name, TMP_Text fontReference)
    {
        var child = parent.Find(name);
        TextMeshProUGUI text;
        if (child == null)
        {
            var created = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            created.transform.SetParent(parent, false);
            text = created.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            text = child.GetComponent<TextMeshProUGUI>();
            if (text == null) text = child.gameObject.AddComponent<TextMeshProUGUI>();
        }

        text.font = fontReference.font;
        text.fontSharedMaterial = fontReference.fontSharedMaterial;
        text.raycastTarget = false;
        return text;
    }

    private static void SetGuideText(TMP_Text text, string value, float size, Color color,
        Vector2 anchorMin, Vector2 anchorMax, TextAlignmentOptions alignment)
    {
        var rect = text.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.enableAutoSizing = false;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.margin = Vector4.zero;
    }

    private static void SetStretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
    {
        if (rect == null)
            throw new InvalidOperationException("A required layout object has no RectTransform.");
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void MoveNavigationToBottom(string objectName)
    {
        var navigation = GameObject.Find(objectName);
        if (navigation == null) return;
        var rect = navigation.GetComponent<RectTransform>();
        if (rect == null) return;
        rect.anchorMin = new Vector2(rect.anchorMin.x, 0.055f);
        rect.anchorMax = new Vector2(rect.anchorMax.x, 0.055f);
    }

    private static TMP_Text FindText(Scene scene, string name)
    {
        var go = FindSceneObject(scene, name);
        return go != null ? go.GetComponent<TMP_Text>() : null;
    }

    private static GameObject FindSceneObject(Scene scene, string name)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            var match = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);
            if (match != null) return match.gameObject;
        }
        return null;
    }
}
