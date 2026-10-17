using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class A2LayeredSceneRefresh
{
    private const string LighthousePath = "Assets/Art/A2/Props/Lighthouse_Standalone.png";
    private const string GullPath = "Assets/Art/A1/Props/Seagull_Flying.png";
    private const string FlowersPath = "Assets/Art/A1/Props/Dune_Wildflowers.png";
    private const string StarfishPath = "Assets/Art/A1/Props/Starfish_Orange.png";
    private const string PinePath = "Assets/Art/A1/Props/Coastal_PineTree.png";
    private const string CliffPath = "Assets/Art/A2/Modular/Props/Coastal_Cliff.png";
    private const string CottagePath = "Assets/Art/A2/Modular/Props/Harbour_Cottage.png";
    private const string DockPath = "Assets/Art/A2/Modular/Props/Harbour_DockSegment.png";
    private const string TablePath = "Assets/Art/A2/Modular/Props/Interior_Table.png";
    private const string ChairPath = "Assets/Art/A2/Modular/Props/Interior_Chair.png";
    private const string BookcasePath = "Assets/Art/A2/Modular/Props/Interior_Bookcase.png";
    private const string SofaPath = "Assets/Art/A2/Modular/Props/Interior_Sofa.png";
    private const string StovePath = "Assets/Art/A2/Modular/Props/Interior_WoodStove.png";
    private const string RugPath = "Assets/Art/A2/Modular/Props/Interior_Rug.png";

    private static readonly string[] SceneNames =
    {
        "HomePage", "RockyShore", "Harbour", "Dunes", "MilasHome", "Credits"
    };

    private static readonly string[] BackgroundPaths =
    {
        "Assets/Art/A2/Modular/Backgrounds/BG_HomePage_Base.png",
        "Assets/Art/A2/Modular/Backgrounds/BG_RockyShore_Base.png",
        "Assets/Art/A2/Modular/Backgrounds/BG_Harbour_Base.png",
        "Assets/Art/A2/Modular/Backgrounds/BG_Dunes_Base.png",
        "Assets/Art/A2/Modular/Backgrounds/BG_MilasHome_Base.png",
        "Assets/Art/A2/Modular/Backgrounds/BG_Credits_Base.png"
    };

    private sealed class ModularAssets
    {
        public Sprite Pine;
        public Sprite Cliff;
        public Sprite Cottage;
        public Sprite Dock;
        public Sprite Table;
        public Sprite Chair;
        public Sprite Bookcase;
        public Sprite Sofa;
        public Sprite Stove;
        public Sprite Rug;
    }

    [MenuItem("Tools/PROG2006 A2/Apply modular scenery and rebuild")]
    public static void ApplyLayeredSceneryAndBuild()
    {
        try
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (var path in BackgroundPaths) ConfigureSprite(path, false, 1024);
            ConfigureSprite(LighthousePath, true, 1024);
            foreach (var path in new[] { CliffPath, CottagePath, DockPath, TablePath, ChairPath, BookcasePath, SofaPath, StovePath, RugPath })
                ConfigureSprite(path, true, 1024);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var backgrounds = new Sprite[BackgroundPaths.Length];
            for (var i = 0; i < BackgroundPaths.Length; i++)
            {
                backgrounds[i] = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPaths[i]);
                if (backgrounds[i] == null)
                    throw new InvalidOperationException("Background sprite is missing: " + BackgroundPaths[i]);
            }

            var lighthouse = AssetDatabase.LoadAssetAtPath<Sprite>(LighthousePath);
            if (lighthouse == null)
                throw new InvalidOperationException("Standalone lighthouse sprite is missing: " + LighthousePath);

            var modular = new ModularAssets
            {
                Pine = LoadSprite(PinePath),
                Cliff = LoadSprite(CliffPath),
                Cottage = LoadSprite(CottagePath),
                Dock = LoadSprite(DockPath),
                Table = LoadSprite(TablePath),
                Chair = LoadSprite(ChairPath),
                Bookcase = LoadSprite(BookcasePath),
                Sofa = LoadSprite(SofaPath),
                Stove = LoadSprite(StovePath),
                Rug = LoadSprite(RugPath)
            };

            var modularObjectCount = 0;

            for (var i = 0; i < SceneNames.Length; i++)
            {
                var sceneName = SceneNames[i];
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + sceneName + ".unity", OpenSceneMode.Single);
                var backdrop = FindSceneObject(scene, "CoastalScene");
                if (backdrop == null)
                    throw new InvalidOperationException("CoastalScene is missing from " + sceneName);

                modularObjectCount += ApplyBackdrop(sceneName, backdrop, backgrounds[i], lighthouse, modular, i);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("Could not save scene " + sceneName);
                Debug.Log("A2 modular scene saved; separate scenery and furniture objects: " + sceneName);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("A2_MODULAR_SCENES_SAVED=6 independentEnvironmentObjects=" + modularObjectCount + " existingInteractivePropsPreserved=true");
            A2ProjectBuild.ApplyAndBuild();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            if (Application.isBatchMode)
                EditorApplication.Exit(1);
        }
    }

    private static void ConfigureSprite(string path, bool alphaIsTransparency, int maxTextureSize)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            throw new InvalidOperationException("Texture importer not found: " + path);

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = alphaIsTransparency;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.maxTextureSize = maxTextureSize;
        importer.textureCompression = TextureImporterCompression.Compressed;
        importer.compressionQuality = 85;
        importer.SaveAndReimport();
    }

    private static int ApplyBackdrop(string sceneName, GameObject backdrop, Sprite background, Sprite lighthouse, ModularAssets modular, int phase)
    {
        var parent = backdrop.transform;
        RemoveGeneratedChildren(parent);
        RemoveLegacyBackdropChildren(parent);
        ApplyLayout(backdrop);

        var backgroundImage = backdrop.GetComponent<Image>();
        if (backgroundImage == null) backgroundImage = backdrop.AddComponent<Image>();
        backgroundImage.sprite = background;
        backgroundImage.type = Image.Type.Simple;
        backgroundImage.preserveAspect = false;
        backgroundImage.color = Color.white;
        backgroundImage.raycastTarget = false;

        var nextSibling = AddModularEnvironment(sceneName, parent, modular, 0, phase);
        AddLighthouse(parent, lighthouse, sceneName, nextSibling++);
        ApplyExistingPropPlacements(sceneName, parent);
        if (sceneName == "Credits") AddCreditsDecorations(parent, nextSibling, phase);
        return nextSibling + (sceneName == "Credits" ? 3 : 0);
    }

    private static int AddModularEnvironment(string sceneName, Transform parent, ModularAssets assets, int siblingIndex, int phase)
    {
        var index = siblingIndex;
        switch (sceneName)
        {
            case "HomePage":
                AddSpriteObject(parent, "A2Modular_Cliff_Home_Left", assets.Cliff, new Vector2(-152f, 18f), new Vector2(292f, 98f), index++, false);
                AddSpriteObject(parent, "A2Modular_Cliff_Home_Right", assets.Cliff, new Vector2(210f, 50f), new Vector2(190f, 64f), index++, true);
                AddSpriteObject(parent, "A2Modular_Cottage_Home", assets.Cottage, new Vector2(218f, 106f), new Vector2(88f, 76f), index++, false);
                AddSpriteObject(parent, "A2Modular_Pine_Home_Back", assets.Pine, new Vector2(-238f, 72f), new Vector2(48f, 80f), index++, false, true, phase);
                break;
            case "RockyShore":
                AddSpriteObject(parent, "A2Modular_Cliff_Rocky_Left", assets.Cliff, new Vector2(-158f, 35f), new Vector2(294f, 98f), index++, false);
                AddSpriteObject(parent, "A2Modular_Cliff_Rocky_Right", assets.Cliff, new Vector2(222f, 34f), new Vector2(170f, 57f), index++, true);
                AddSpriteObject(parent, "A2Modular_Pine_Rocky", assets.Pine, new Vector2(-222f, 113f), new Vector2(47f, 78f), index++, false, true, phase + 1);
                break;
            case "Harbour":
                AddSpriteObject(parent, "A2Modular_Cliff_Harbour_Left", assets.Cliff, new Vector2(-190f, 54f), new Vector2(250f, 84f), index++, false);
                AddSpriteObject(parent, "A2Modular_Cliff_Harbour_Center", assets.Cliff, new Vector2(0f, 54f), new Vector2(150f, 50f), index++, true);
                AddSpriteObject(parent, "A2Modular_Cliff_Harbour_Right", assets.Cliff, new Vector2(190f, 54f), new Vector2(220f, 74f), index++, true);
                AddSpriteObject(parent, "A2Modular_Cottage_Harbour_Left", assets.Cottage, new Vector2(-177f, 93f), new Vector2(72f, 62f), index++, false);
                AddSpriteObject(parent, "A2Modular_Cottage_Harbour_Right", assets.Cottage, new Vector2(190f, 96f), new Vector2(88f, 75f), index++, false);
                AddSpriteObject(parent, "A2Modular_Dock_Harbour", assets.Dock, new Vector2(153f, -92f), new Vector2(246f, 124f), index++, false);
                break;
            case "Dunes":
                AddSpriteObject(parent, "A2Modular_Cliff_Dunes_Horizon", assets.Cliff, new Vector2(208f, 26f), new Vector2(168f, 56f), index++, true);
                // DuneRidge, grasses, flowers and pines are already separate scene objects.
                break;
            case "MilasHome":
                AddSpriteObject(parent, "A2Modular_WindowCliff_Home", assets.Cliff, new Vector2(0f, 44f), new Vector2(148f, 50f), index++, false);
                AddSpriteObject(parent, "A2Modular_WindowPine_Home", assets.Pine, new Vector2(-101f, 53f), new Vector2(25f, 42f), index++, false);
                AddSpriteObject(parent, "A2Modular_Bookcase_Home", assets.Bookcase, new Vector2(-222f, -8f), new Vector2(78f, 118f), index++, false);
                AddSpriteObject(parent, "A2Modular_Sofa_Home", assets.Sofa, new Vector2(-123f, -88f), new Vector2(146f, 96f), index++, false);
                AddSpriteObject(parent, "A2Modular_Rug_Home", assets.Rug, new Vector2(20f, -148f), new Vector2(224f, 86f), index++, false);
                AddSpriteObject(parent, "A2Modular_Stove_Home", assets.Stove, new Vector2(235f, -19f), new Vector2(76f, 116f), index++, false);
                AddSpriteObject(parent, "A2Modular_Table_Home", assets.Table, new Vector2(22f, -100f), new Vector2(186f, 116f), index++, false);
                AddSpriteObject(parent, "A2Modular_Chair_Home_Left", assets.Chair, new Vector2(-99f, -142f), new Vector2(54f, 82f), index++, false);
                AddSpriteObject(parent, "A2Modular_Chair_Home_Right", assets.Chair, new Vector2(142f, -142f), new Vector2(54f, 82f), index++, false);
                break;
            case "Credits":
                AddSpriteObject(parent, "A2Modular_Cliff_Credits", assets.Cliff, new Vector2(170f, 32f), new Vector2(245f, 82f), index++, true);
                AddSpriteObject(parent, "A2Modular_Pine_Credits", assets.Pine, new Vector2(-225f, 82f), new Vector2(45f, 75f), index++, false, true, phase + 3);
                break;
        }

        return index - siblingIndex;
    }

    private static void ApplyExistingPropPlacements(string sceneName, Transform parent)
    {
        if (sceneName != "Harbour") return;
        SetChildRect(parent, "A1Prop_Pine_Harbour", new Vector2(-262f, 116f), new Vector2(43f, 75f));
        SetChildRect(parent, "A1Prop_Bell_Harbour", new Vector2(154f, -25f), new Vector2(50f, 65f));
    }

    private static void SetChildRect(Transform parent, string name, Vector2 position, Vector2 size)
    {
        foreach (var rect in parent.GetComponentsInChildren<RectTransform>(true))
        {
            if (rect.gameObject.name != name) continue;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return;
        }
        throw new InvalidOperationException("Expected separate prop is missing: " + name);
    }

    private static void AddSpriteObject(Transform parent, string name, Sprite sprite, Vector2 position, Vector2 size, int siblingIndex, bool flipX, bool animate = false, int phase = 0)
    {
        if (sprite == null)
            throw new InvalidOperationException("Sprite is missing for modular object " + name);

        var prop = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        prop.transform.SetParent(parent, false);
        var rect = prop.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        if (flipX) rect.localScale = new Vector3(-1f, 1f, 1f);
        var image = prop.GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
        prop.transform.SetSiblingIndex(Mathf.Clamp(siblingIndex, 0, parent.childCount - 1));

        if (animate)
        {
            var motion = prop.AddComponent<A1SpriteMotion>();
            motion.driftRange = new Vector2(0.8f, 0.45f);
            motion.cyclesPerSecond = 0.13f;
            motion.swayDegrees = 0.9f;
            motion.phase = phase * 0.7f;
        }
    }

    private static void ApplyLayout(GameObject backdrop)
    {
        SetStretch(backdrop.GetComponent<RectTransform>(), new Vector2(0.02f, 0.11f), new Vector2(0.66f, 0.84f));

        var storyPanel = GameObject.Find("StoryPanel");
        if (storyPanel == null)
            throw new InvalidOperationException("StoryPanel is missing from the active scene.");
        SetStretch(storyPanel.GetComponent<RectTransform>(), new Vector2(0.68f, 0.11f), new Vector2(0.98f, 0.84f));

        var pageTitle = GameObject.Find("PageTitle");
        if (pageTitle != null)
            SetStretch(pageTitle.GetComponent<RectTransform>(), new Vector2(0.025f, 0.855f), new Vector2(0.975f, 0.985f));

        MoveNavigationToBottom("BackButton");
        MoveNavigationToBottom("NextButton");
        MoveNavigationToBottom("ReturnHomeButton");
    }

    private static void SetStretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
    {
        if (rect == null)
            throw new InvalidOperationException("A required UI element has no RectTransform.");
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

    private static void AddLighthouse(Transform parent, Sprite sprite, string sceneName, int siblingIndex)
    {
        var landmark = new GameObject("A2Prop_Lighthouse_Standalone", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        landmark.transform.SetParent(parent, false);
        var rect = landmark.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = GetLighthouseSize(sceneName);
        rect.anchoredPosition = GetLighthousePosition(sceneName);
        var image = landmark.GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
        image.color = sceneName == "Credits" ? Color.white : new Color32(224, 230, 226, 245);
        landmark.transform.SetSiblingIndex(Mathf.Clamp(siblingIndex, 0, parent.childCount - 1));
    }

    private static Vector2 GetLighthouseSize(string sceneName)
    {
        switch (sceneName)
        {
            case "HomePage": return new Vector2(42f, 63f);
            case "RockyShore": return new Vector2(38f, 57f);
            case "Harbour": return new Vector2(34f, 51f);
            case "Dunes": return new Vector2(31f, 47f);
            case "MilasHome": return new Vector2(22f, 33f);
            default: return new Vector2(40f, 60f);
        }
    }

    private static Vector2 GetLighthousePosition(string sceneName)
    {
        switch (sceneName)
        {
            case "HomePage": return new Vector2(-150f, 82f);
            case "RockyShore": return new Vector2(-100f, 91f);
            case "Harbour": return new Vector2(0f, 87f);
            case "Dunes": return new Vector2(212f, 61f);
            case "MilasHome": return new Vector2(74f, 54f);
            default: return new Vector2(194f, 82f);
        }
    }

    private static void AddCreditsDecorations(Transform parent, int siblingIndex, int phase)
    {
        AddMovingProp(parent, "A2Prop_Credits_Gull", GullPath, new Vector2(-190f, 142f), new Vector2(56f, 38f), new Vector2(6f, 2f), 0.10f, 1.1f, siblingIndex++, phase);
        AddMovingProp(parent, "A2Prop_Credits_Flowers", FlowersPath, new Vector2(-205f, -142f), new Vector2(72f, 68f), new Vector2(1.5f, 0f), 0.08f, 0.8f, siblingIndex++, phase + 1);
        AddMovingProp(parent, "A2Prop_Credits_Starfish", StarfishPath, new Vector2(-82f, -151f), new Vector2(32f, 28f), new Vector2(0.5f, 0f), 0.08f, 1.6f, siblingIndex, phase + 2);
    }

    private static void AddMovingProp(Transform parent, string name, string assetPath, Vector2 position, Vector2 size, Vector2 drift, float cyclesPerSecond, float swayDegrees, int siblingIndex, int phase)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite == null)
            throw new InvalidOperationException("Sprite asset is missing: " + assetPath);

        var prop = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        prop.transform.SetParent(parent, false);
        var rect = prop.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        var image = prop.GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
        prop.transform.SetSiblingIndex(Mathf.Clamp(siblingIndex, 0, parent.childCount - 1));

        var motion = prop.AddComponent<A1SpriteMotion>();
        motion.driftRange = drift;
        motion.cyclesPerSecond = cyclesPerSecond;
        motion.swayDegrees = swayDegrees;
        motion.phase = phase * 0.7f;
    }

    private static void RemoveGeneratedChildren(Transform parent)
    {
        for (var i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            if (child.name.StartsWith("A2Backdrop_") || child.name.StartsWith("A2Modular_") || child.name == "A2Prop_Lighthouse_Standalone" || child.name.StartsWith("A2Prop_Credits_"))
                UnityEngine.Object.DestroyImmediate(child.gameObject);
        }
    }

    private static void RemoveLegacyBackdropChildren(Transform parent)
    {
        var legacyNames = new[] { "SeaBand", "ShoreBand", "RockLarge", "RockSmall", "HomeWall", "HomeWindow", "HomeTable", "HarbourPier" };
        for (var i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            if (Array.IndexOf(legacyNames, child.name) >= 0)
                UnityEngine.Object.DestroyImmediate(child.gameObject);
        }
    }

    private static Sprite LoadSprite(string path)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
            throw new InvalidOperationException("Sprite asset is missing: " + path);
        return sprite;
    }

    private static GameObject FindSceneObject(Scene scene, string name)
    {
        return GameObject.Find(name);
    }
}
