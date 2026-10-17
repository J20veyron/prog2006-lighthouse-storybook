using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public static class A2ProjectBuild
{
    private const string AudioRoot = "Assets/Audio/A2";
    private const string AmbientRoot = "Assets/FromAssetStore/Nature Sounds";
    private const string ButtonSpritePath = "Assets/Art/A1/UI/ButtonPill.png";
    private const string ButtonTextMaterialPath = "Assets/Materials/A2_ButtonText_White.mat";
    private const string MemoryLanternPath = "Assets/Art/A1/Props/PaperLantern.png";
    private static readonly string[] SceneNames = { "HomePage", "RockyShore", "Harbour", "Dunes", "MilasHome", "Credits" };

    private sealed class PageSpec
    {
        public string Scene;
        public int MemoryId;
        public string Title;
        public string Banner;
        public string Intro;
        public string Hint;
        public string Restored;
        public string Ambient;
        public float AmbientVolume;
        public string SecondaryAmbient;
        public float SecondaryVolume;
        public string Sfx;

        public PageSpec(string scene, int id, string title, string banner, string intro, string hint,
            string restored, string ambient, float ambientVolume, string secondaryAmbient, float secondaryVolume, string sfx)
        {
            Scene = scene; MemoryId = id; Title = title; Banner = banner; Intro = intro; Hint = hint;
            Restored = restored; Ambient = ambient; AmbientVolume = ambientVolume;
            SecondaryAmbient = secondaryAmbient; SecondaryVolume = secondaryVolume; Sfx = sfx;
        }
    }

    [MenuItem("Tools/PROG2006 A2/Apply polished interactive storybook and build")]
    public static void ApplyAndBuild()
    {
        try
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ConfigureShortAudio("TideRipple.wav");
            ConfigureShortAudio("HarbourBell.wav");
            ConfigureShortAudio("GullCall.wav");
            ConfigureShortAudio("LanternChime.wav");
            ConfigureShortAudio("Crab_Click.wav");
            ConfigureShortAudio("Starfish_Sparkle.wav");
            ConfigureShortAudio("Skiff_Splash.wav");
            ConfigureShortAudio("DuneGrass_Rustle.wav");
            ConfigureShortAudio("DuneFlowers_Chime.wav");
            ConfigureShortAudio("Lighthouse_Beam.wav");
            ConfigureShortAudio("Lantern_Twinkle.wav");
            ConfigureShortAudio("Cottage_Knock.wav");
            ConfigureShortAudio("WoodTable_Tap.wav");
            ConfigureShortAudio("Fireplace_Crackle.wav");
            ConfigureMusicAudio("Lighthouse_Theme.wav");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var buttonSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ButtonSpritePath);
            if (buttonSprite == null)
                throw new InvalidOperationException("Button pill sprite could not be loaded: " + ButtonSpritePath);

            foreach (var sceneName in SceneNames)
            {
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + sceneName + ".unity", OpenSceneMode.Single);
                var page = GetPage(sceneName);
                ApplyCopyAndBanner(scene, sceneName, page, buttonSprite);
                ApplyButtonTheme(scene);
                ApplyAmbient(scene, sceneName);
                ApplyBackgroundMusic(scene);
                if (sceneName == "HomePage")
                    ApplyHomeInteractions(scene);
                if (page != null)
                {
                    ApplyStoryPage(scene, page);
                    ApplyMemoryMarkerIcons(scene, false);
                }
                if (sceneName == "Credits")
                {
                    ApplyMemoryMarkerIcons(scene, true);
                    CleanCreditsNavigation(scene);
                }

                NormaliseTextMaterials(scene);

                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("Could not save scene " + sceneName);
                Debug.Log("A2 scene updated: " + sceneName);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("A2_STORYBOOK_SCENES_SAVED=6 interactiveObjects=25 persistentMemories=4");
            RebuildPreview();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            if (Application.isBatchMode)
                EditorApplication.Exit(1);
        }
    }

    private static PageSpec GetPage(string sceneName)
    {
        switch (sceneName)
        {
            case "RockyShore":
                return new PageSpec("RockyShore", 1, "Tide-Pool Music", "SOUND 1  •  ROCKY SHORE",
                    "The tide pools are quiet. Tap the shell or crab to wake the bubbling wave-song.",
                    "Try the spiral shell beside the tide pool.",
                    "The shell bubbles, the crab clicks, and the tide-pool song returns!",
                    "Ambiance_Sea_Loop_Stereo.wav", 0.10f, "Ambiance_Stream_Calm_Loop_Stereo.wav", 0.035f, "TideRipple.wav");
            case "Harbour":
                return new PageSpec("Harbour", 2, "The Silent Bell", "SOUND 2  •  HARBOUR",
                    "The skiff rocks beside a silent bell. Tap the bell or boat to bring back the harbour chime.",
                    "Look beside the dock and tap the hanging bell.",
                    "Ding! The harbour chime returns, and the skiff rocks home!",
                    "Ambiance_Sea_Loop_Stereo.wav", 0.09f, "Ambiance_Wind_Calm_Loop_Stereo.wav", 0.035f, "HarbourBell.wav");
            case "Dunes":
                return new PageSpec("Dunes", 3, "The Gull's Answer", "SOUND 3  •  COASTAL DUNES",
                    "Grass sways, but no gull answers. Tap a gull or flowers to invite back the wind's call.",
                    "Watch above the dune grass and tap the flying gull.",
                    "A gull calls back! The dunes are singing with the wind again!",
                    "Ambiance_Forest_Birds_Loop_Stereo.wav", 0.075f, "Ambiance_Wind_Calm_Loop_Stereo.wav", 0.045f, "GullCall.wav");
            case "MilasHome":
                return new PageSpec("MilasHome", 4, "Mila's Hidden Tune", "SOUND 4  •  LANTERN FESTIVAL",
                    "At home, Mila's family hums a familiar tune. Tap a lantern to discover the missing sound.",
                    "Tap the lantern nearest to Mila.",
                    "Mila hums. The lanterns glow: her own voice was woven through all four sounds!",
                    "Ambiance_Night_Loop_Stereo.wav", 0.075f, "Ambiance_Sea_Loop_Stereo.wav", 0.035f, "LanternChime.wav");
            default:
                return null;
        }
    }

    private static void ApplyCopyAndBanner(Scene scene, string sceneName, PageSpec page, Sprite pill)
    {
        var pageTitle = FindText(scene, "PageTitle");
        var intro = FindText(scene, "IntroCopy");
        var festival = FindText(scene, "FestivalLabel");
        var progress = FindText(scene, "ProgressLabel");

        if (sceneName == "HomePage")
        {
            SetText(pageTitle, "The Lighthouse That Remembers Sounds", 29f, 23f, 32f, new Color32(34, 52, 64, 255), TextAlignmentOptions.TopLeft, FontStyles.Bold);
            SetText(festival, "LANTERN FESTIVAL NIGHT", 15f, 12f, 16f, new Color32(255, 233, 181, 255), TextAlignmentOptions.Center, FontStyles.Bold);
            SetText(intro, "On Lantern Festival night, the coast falls quiet. Help Mila find four missing sounds. Tap glowing objects on each page; use HINT if you get stuck.", 18f, 16f, 20f, new Color32(35, 53, 62, 255), TextAlignmentOptions.TopLeft, FontStyles.Normal);
            SetText(FindText(scene, "StartButtonLabel"), "BEGIN", 21f, 18f, 23f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
        }
        else if (page != null)
        {
            SetText(pageTitle, page.Title, 28f, 23f, 31f, new Color32(34, 52, 64, 255), TextAlignmentOptions.TopLeft, FontStyles.Bold);
            SetText(festival, page.Banner, 14f, 12f, 16f, new Color32(255, 235, 186, 255), TextAlignmentOptions.Center, FontStyles.Bold);
            SetText(intro, page.Intro, 18f, 15f, 20f, new Color32(37, 54, 64, 255), TextAlignmentOptions.TopLeft, FontStyles.Normal);
            SetText(progress, "SOUNDS FOUND: 0 / 4", 16f, 14f, 17f, new Color32(38, 64, 69, 255), TextAlignmentOptions.Center, FontStyles.Bold);
            SetText(FindText(scene, "BackButtonLabel"), "BACK", 19f, 17f, 21f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            SetText(FindText(scene, "NextButtonLabel"), sceneName == "MilasHome" ? "FINISH" : "NEXT", 19f, 17f, 21f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            SetText(FindText(scene, "FindClueLabel"), "HINT", 17f, 15f, 18f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
        }
        else if (sceneName == "Credits")
        {
            SetText(pageTitle, "The Coast Remembers", 30f, 23f, 32f, new Color32(34, 52, 64, 255), TextAlignmentOptions.TopLeft, FontStyles.Bold);
            SetText(festival, "THE END", 16f, 14f, 18f, new Color32(255, 235, 186, 255), TextAlignmentOptions.Center, FontStyles.Bold);
            SetText(intro, "The four sounds were never lost; they were waiting in Mila's memories. When she hummed, the coast could hear them again. Thank you for listening!", 19f, 16f, 21f, new Color32(35, 53, 62, 255), TextAlignmentOptions.TopLeft, FontStyles.Normal);
            SetText(progress, "FOUR MEMORIES RESTORED", 16f, 14f, 18f, new Color32(38, 64, 69, 255), TextAlignmentOptions.Center, FontStyles.Bold);
            SetText(FindText(scene, "ReturnHomeButtonLabel"), "PLAY AGAIN", 18f, 16f, 20f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
        }

        AddStoryBanner(scene, AssetDatabase.LoadAssetAtPath<Sprite>(ButtonSpritePath));

        if (pageTitle != null) pageTitle.transform.SetAsLastSibling();

        var panel = FindSceneObject(scene, "StoryPanel");
        if (panel != null)
        {
            var banner = panel.transform.Find("A2StoryBanner");
            if (banner == null)
            {
                var go = new GameObject("A2StoryBanner", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(panel.transform, false);
                banner = go.transform;
            }
            var rect = banner.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.04f, 0.79f);
            rect.anchorMax = new Vector2(0.96f, 0.98f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            var image = banner.GetComponent<Image>();
            image.sprite = pill;
            image.type = Image.Type.Sliced;
            image.color = new Color32(36, 78, 82, 248);
            image.raycastTarget = false;
            banner.SetAsFirstSibling();
        }
    }

    private static void ApplyButtonTheme(Scene scene)
    {
        var buttons = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Button>(true))
            .ToArray();

        foreach (var button in buttons)
        {
            var name = button.name.ToLowerInvariant();
            Color32 baseColor;
            if (name.Contains("back")) baseColor = new Color32(211, 47, 47, 255);
            else if (name.Contains("next")) baseColor = new Color32(21, 128, 61, 255);
            else if (name.Contains("findclue")) baseColor = new Color32(181, 101, 35, 255);
            else baseColor = new Color32(44, 105, 99, 255);

            button.transition = Selectable.Transition.ColorTint;
            var background = button.targetGraphic as Image;
            if (background == null) background = button.GetComponent<Image>();
            if (background != null)
            {
                background.color = Color.white;
                button.targetGraphic = background;
                var shadow = background.GetComponent<Shadow>();
                if (shadow == null) shadow = background.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(0.10f, 0.18f, 0.18f, 0.32f);
                shadow.effectDistance = new Vector2(0f, -3f);
                shadow.useGraphicAlpha = true;
                var outline = background.GetComponent<Outline>();
                if (outline == null) outline = background.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(1f, 0.94f, 0.76f, 0.72f);
                outline.effectDistance = new Vector2(1f, -1f);
                outline.useGraphicAlpha = true;
            }

            var colors = button.colors;
            colors.normalColor = baseColor;
            colors.highlightedColor = Color.Lerp(baseColor, Color.white, 0.04f);
            colors.pressedColor = Color.Lerp(baseColor, Color.black, 0.18f);
            colors.selectedColor = colors.highlightedColor;
            if (name.Contains("next"))
                colors.disabledColor = new Color32(11, 79, 42, 255);
            else if (name.Contains("back"))
                colors.disabledColor = new Color32(127, 31, 44, 255);
            else
                colors.disabledColor = new Color32(82, 105, 102, 255);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.12f;
            button.colors = colors;

            if (name == "backbutton" || name == "nextbutton")
            {
                var rect = button.GetComponent<RectTransform>();
                if (rect != null) rect.sizeDelta = new Vector2(146f, 54f);
            }
            else if (name == "findcluebutton")
            {
                var rect = button.GetComponent<RectTransform>();
                if (rect != null) rect.sizeDelta = new Vector2(178f, 50f);
            }

            var cream = Color.white;
            foreach (var image in button.GetComponentsInChildren<Image>(true))
                if (image != background) image.color = cream;
            foreach (var label in button.GetComponentsInChildren<TMP_Text>(true))
            {
                label.fontSharedMaterial = GetWhiteButtonTextMaterial(label.font);
                label.color = Color.white;
                label.faceColor = new Color32(255, 255, 255, 255);
                label.fontStyle = FontStyles.Bold;
                var textOutline = label.GetComponent<Outline>();
                if (textOutline == null) textOutline = label.gameObject.AddComponent<Outline>();
                textOutline.effectColor = new Color(0.10f, 0.16f, 0.18f, 0.88f);
                textOutline.effectDistance = new Vector2(1f, -1f);
                textOutline.useGraphicAlpha = true;
            }
        }
    }

    private static Material GetWhiteButtonTextMaterial(TMP_FontAsset fontAsset)
    {
        if (fontAsset == null || fontAsset.material == null)
            return null;

        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        var material = AssetDatabase.LoadAssetAtPath<Material>(ButtonTextMaterialPath);
        if (material == null)
        {
            material = new Material(fontAsset.material) { name = "A2_ButtonText_White" };
            AssetDatabase.CreateAsset(material, ButtonTextMaterialPath);
        }

        material.SetColor("_FaceColor", Color.white);
        material.SetColor("_OutlineColor", new Color32(12, 30, 40, 255));
        material.SetFloat("_OutlineWidth", 0f);
        material.SetFloat("_OutlineSoftness", 0f);
        material.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 0f));
        material.DisableKeyword("OUTLINE_ON");
        material.DisableKeyword("UNDERLAY_ON");
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void ApplyWhiteFaceMaterial(TMP_Text text)
    {
        if (text == null) return;
        var whiteFace = GetWhiteFaceMaterial(text.font);
        if (whiteFace == null) return;
        text.fontSharedMaterial = whiteFace;
        text.fontMaterial = whiteFace;
    }

    /// <summary>
    /// White-face copy of a font material, stored as an asset so it survives the scene
    /// save and the WebGL build. Text colour then comes from the vertex colour only.
    /// </summary>
    private static Material GetWhiteFaceMaterial(TMP_FontAsset fontAsset)
    {
        if (fontAsset == null || fontAsset.material == null)
            return null;

        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        var safeName = new string(fontAsset.name.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());
        var path = "Assets/Materials/A2_Text_White_" + safeName + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(fontAsset.material) { name = "A2_Text_White_" + safeName };
            AssetDatabase.CreateAsset(material, path);
        }

        material.SetColor("_FaceColor", Color.white);
        material.SetColor("_OutlineColor", new Color32(12, 30, 40, 255));
        material.SetFloat("_OutlineWidth", 0f);
        material.SetFloat("_OutlineSoftness", 0f);
        material.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 0f));
        material.DisableKeyword("OUTLINE_ON");
        material.DisableKeyword("UNDERLAY_ON");
        EditorUtility.SetDirty(material);
        return material;
    }

    /// <summary>
    /// Repairs labels still pointing at a material instance saved inside the scene
    /// (the ones the previous build script created). Asset materials are left alone.
    /// </summary>
    private static void NormaliseTextMaterials(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                var shared = text.fontSharedMaterial;
                if (shared == null || AssetDatabase.Contains(shared))
                    continue;

                ApplyWhiteFaceMaterial(text);
                Debug.Log("A2_TEXT_MATERIAL_FIXED=" + text.name + " text=" + text.text);
            }
        }
    }

    private static void ApplyMemoryMarkerIcons(Scene scene, bool allRestored)
    {
        var lantern = AssetDatabase.LoadAssetAtPath<Sprite>(MemoryLanternPath);
        if (lantern == null)
            throw new InvalidOperationException("Memory lantern sprite is missing: " + MemoryLanternPath);

        var inactive = new Color32(190, 204, 206, 190);
        var restored = new Color32(255, 194, 61, 255);
        for (var i = 1; i <= A2StoryProgress.TotalMemories; i++)
        {
            var mark = FindSceneObject(scene, "MemoryMark" + i);
            if (mark == null) continue;
            var image = mark.GetComponent<Image>();
            var rect = mark.GetComponent<RectTransform>();
            if (image == null || rect == null) continue;

            image.sprite = lantern;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.color = allRestored ? restored : inactive;
            rect.sizeDelta = new Vector2(22f, 28f);
        }
    }
    private static void ApplyHomeInteractions(Scene scene)
    {
        var managers = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<ButtonManager>(true))
            .ToArray();
        if (managers.Length == 0)
            throw new InvalidOperationException("ButtonManager is missing from HomePage.");

        var managerObject = managers[0].gameObject;
        var controller = managerObject.GetComponent<A2StoryPageController>();
        if (controller == null) controller = managerObject.AddComponent<A2StoryPageController>();
        controller.memoryId = 0;
        controller.hintText = string.Empty;
        controller.restoredText = string.Empty;
        controller.replayClip = LoadClip("LanternChime.wav");

        var audio = managerObject.GetComponent<AudioSource>();
        if (audio == null) audio = managerObject.AddComponent<AudioSource>();
        audio.clip = null;
        audio.playOnAwake = false;
        audio.loop = false;
        audio.volume = 0.88f;
        audio.spatialBlend = 0f;
        audio.dopplerLevel = 0f;

        ConfigureStoryObject(scene, "A1Prop_Lantern_Home", false,
            "The festival lantern gives a soft chime. Mila is ready to listen.", "LanternChime.wav");
        ConfigureStoryObject(scene, "A1Prop_Gull_Home_A", false,
            "A gull calls across the water as Mila begins her search.", "GullCall.wav");
        ConfigureStoryObject(scene, "A2Prop_Lighthouse_Standalone", false,
            "The lighthouse sweeps the quiet shore with a warm beacon tone.", "Lighthouse_Beam.wav");
        ConfigureStoryObject(scene, "A2Modular_Cottage_Home", false,
            "A gentle knock comes from the cottage by the shore.", "Cottage_Knock.wav");
    }

    private static void ApplyStoryPage(Scene scene, PageSpec spec)
    {
        var managers = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<ButtonManager>(true))
            .ToArray();
        if (managers.Length == 0)
            throw new InvalidOperationException("ButtonManager is missing from " + spec.Scene);

        var managerObject = managers[0].gameObject;
        var controller = managerObject.GetComponent<A2StoryPageController>();
        if (controller == null) controller = managerObject.AddComponent<A2StoryPageController>();
        controller.memoryId = spec.MemoryId;
        controller.hintText = spec.Hint;
        controller.restoredText = spec.Restored;
        controller.replayClip = LoadClip(spec.Sfx);

        var audio = managerObject.GetComponent<AudioSource>();
        if (audio == null) audio = managerObject.AddComponent<AudioSource>();
        audio.clip = null;
        audio.playOnAwake = false;
        audio.loop = false;
        audio.volume = 0.88f;
        audio.spatialBlend = 0f;
        audio.dopplerLevel = 0f;

        if (FindText(scene, "FindClueLabel") == null || FindText(scene, "IntroCopy") == null)
            throw new InvalidOperationException("Story UI text is incomplete in " + spec.Scene);

        switch (spec.Scene)
        {
            case "RockyShore":
                ConfigureStoryObject(scene, "A1Prop_Shell_Rocky", true, "The shell bubbles, and the tide-pool song returns!", "TideRipple.wav");
                ConfigureStoryObject(scene, "A1Prop_Crab_RockPool", false, "The crab clicks, but the wave-song is still quiet. Try the glowing shell.", "Crab_Click.wav");
                ConfigureStoryObject(scene, "A1Prop_Starfish_Rocky", false, "The starfish shimmers, but the wave-song is missing. Find the shell by the pool.", "Starfish_Sparkle.wav");
                ConfigureStoryObject(scene, "A1Prop_Gull_Rocky", false, "A gull answers from above the tide pools.", "GullCall.wav");
                ConfigureStoryObject(scene, "A2Prop_Lighthouse_Standalone", false, "The lighthouse beam sweeps across the rocky pool.", "Lighthouse_Beam.wav");
                break;
            case "Harbour":
                ConfigureStoryObject(scene, "A1Prop_Bell_Harbour", true, "Ding! The harbour chime returns, and the skiff rocks home!", "HarbourBell.wav");
                ConfigureStoryObject(scene, "A1Prop_Skiff_Harbour", false, "The skiff splashes, but the bell is still silent. Tap the bell by the dock.", "Skiff_Splash.wav");
                ConfigureStoryObject(scene, "A1Prop_Gull_Harbour_A", false, "The gull calls, but the harbour chime is missing. Find the bell by the dock.", "GullCall.wav");
                ConfigureStoryObject(scene, "A1Prop_Gull_Harbour_B", false, "A second gull circles over the harbour roofs.", "GullCall.wav");
                ConfigureStoryObject(scene, "A2Modular_Cottage_Harbour_Right", false, "A soft knock answers from the cottage beside the harbour.", "Cottage_Knock.wav");
                ConfigureStoryObject(scene, "A2Prop_Lighthouse_Standalone", false, "The beacon answers the silent harbour with a low, warm tone.", "Lighthouse_Beam.wav");
                break;
            case "Dunes":
                ConfigureStoryObject(scene, "A1Prop_Gull_Dunes_A", true, "A gull calls back! The dunes are singing with the wind again!", "GullCall.wav");
                ConfigureStoryObject(scene, "A1Prop_Grass_Dunes", false, "The grass rustles, but the gull-call is missing. Look up for the flying gull.", "DuneGrass_Rustle.wav");
                ConfigureStoryObject(scene, "A1Prop_Flowers_Dunes", false, "The flowers chime in the breeze, but the gull is still quiet. Tap the gull above them.", "DuneFlowers_Chime.wav");
                ConfigureStoryObject(scene, "A1Prop_Gull_Dunes_B", false, "A second gull sweeps above the sandy ridge.", "GullCall.wav");
                ConfigureStoryObject(scene, "A2Prop_Lighthouse_Standalone", false, "The lighthouse glimmers at the edge of the dunes.", "Lighthouse_Beam.wav");
                break;
            case "MilasHome":
                ConfigureStoryObject(scene, "A1Prop_Lantern_Home_Left", true, "Mila hums. The lanterns glow: her own voice was woven through all four sounds!", "LanternChime.wav");
                ConfigureStoryObject(scene, "A1Prop_Lantern_Home_Right", false, "The lantern twinkles, but the last sound is still missing. Tap the lantern beside Mila.", "Lantern_Twinkle.wav");
                ConfigureStoryObject(scene, "A1Prop_Flowers_Home", false, "The petals chime to Mila's tune, but the last sound is missing. Find the lantern beside her.", "DuneFlowers_Chime.wav");
                ConfigureStoryObject(scene, "A2Modular_Table_Home", false, "The wooden table gives a gentle hollow tap.", "WoodTable_Tap.wav");
                ConfigureStoryObject(scene, "A2Modular_Stove_Home", false, "A cozy crackle comes from the little stove.", "Fireplace_Crackle.wav");
                break;
        }
    }

    private static void ConfigureStoryObject(Scene scene, string objectName, bool collectsMemory, string feedback, string sfxName)
    {
        var go = FindSceneObject(scene, objectName);
        if (go == null)
            throw new InvalidOperationException("Interactive story object not found: " + scene.name + "/" + objectName);
        var image = go.GetComponent<Image>();
        if (image == null)
            throw new InvalidOperationException("Interactive story object has no Image: " + objectName);
        image.raycastTarget = true;
        image.color = Color.white;

        var trigger = go.GetComponent<A2StoryObjectTrigger>();
        if (trigger == null) trigger = go.AddComponent<A2StoryObjectTrigger>();
        trigger.collectsMemory = collectsMemory;
        trigger.feedbackText = feedback;
        trigger.feedbackClip = LoadClip(sfxName);

        var outline = image.GetComponent<Outline>();
        if (outline == null) outline = image.gameObject.AddComponent<Outline>();
        outline.effectColor = collectsMemory
            ? new Color(1f, 0.79f, 0.28f, 0.82f)
            : new Color(1f, 0.86f, 0.58f, 0.62f);
        outline.effectDistance = collectsMemory ? new Vector2(2f, -2f) : new Vector2(1.5f, -1.5f);
        outline.useGraphicAlpha = true;
    }

    private static void ApplyAmbient(Scene scene, string sceneName)
    {
        string[] clips;
        float[] volumes;
        switch (sceneName)
        {
            case "HomePage":
                clips = new[] { "Ambiance_Night_Loop_Stereo.wav", "Ambiance_Sea_Loop_Stereo.wav" };
                volumes = new[] { 0.07f, 0.035f };
                break;
            case "RockyShore":
                clips = new[] { "Ambiance_Sea_Loop_Stereo.wav", "Ambiance_Stream_Calm_Loop_Stereo.wav" };
                volumes = new[] { 0.08f, 0.04f };
                break;
            case "Harbour":
                clips = new[] { "Ambiance_Sea_Loop_Stereo.wav", "Ambiance_Wind_Calm_Loop_Stereo.wav" };
                volumes = new[] { 0.075f, 0.035f };
                break;
            case "Dunes":
                clips = new[] { "Ambiance_Forest_Birds_Loop_Stereo.wav", "Ambiance_Wind_Calm_Loop_Stereo.wav" };
                volumes = new[] { 0.07f, 0.045f };
                break;
            case "MilasHome":
                clips = new[] { "Ambiance_Night_Loop_Stereo.wav", "Ambiance_Sea_Loop_Stereo.wav" };
                volumes = new[] { 0.065f, 0.03f };
                break;
            default:
                clips = new[] { "Ambiance_Night_Loop_Stereo.wav", "Ambiance_Sea_Loop_Stereo.wav" };
                volumes = new[] { 0.06f, 0.035f };
                break;
        }

        var root = FindSceneObject(scene, "A2AmbientAudio");
        if (root == null)
        {
            root = new GameObject("A2AmbientAudio");
            SceneManager.MoveGameObjectToScene(root, scene);
        }
        var sources = root.GetComponents<AudioSource>();
        while (sources.Length < clips.Length)
        {
            root.AddComponent<AudioSource>();
            sources = root.GetComponents<AudioSource>();
        }
        for (var i = 0; i < sources.Length; i++)
        {
            var source = sources[i];
            if (i >= clips.Length)
            {
                source.Stop();
                source.enabled = false;
                continue;
            }
            source.enabled = true;
            source.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AmbientRoot + "/" + clips[i]);
            if (source.clip == null)
                throw new InvalidOperationException("Ambient audio clip missing: " + clips[i]);
            source.loop = true;
            source.playOnAwake = true;
            source.volume = volumes[i];
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            source.priority = 160;
        }
    }

    private static void ApplyBackgroundMusic(Scene scene)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioRoot + "/Lighthouse_Theme.wav");
        if (clip == null)
            throw new InvalidOperationException("Background music asset was not imported.");

        var musicObject = FindSceneObject(scene, "A2BackgroundMusic");
        if (musicObject == null)
        {
            musicObject = new GameObject("A2BackgroundMusic");
            SceneManager.MoveGameObjectToScene(musicObject, scene);
        }

        var source = musicObject.GetComponent<AudioSource>();
        if (source == null) source = musicObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.playOnAwake = false;
        source.volume = 0.20f;
        source.spatialBlend = 0f;
        source.dopplerLevel = 0f;

        var music = musicObject.GetComponent<A2BackgroundMusic>();
        if (music == null) music = musicObject.AddComponent<A2BackgroundMusic>();
        music.themeClip = clip;
        music.volume = 0.20f;
    }

    private static void CleanCreditsNavigation(Scene scene)
    {
        foreach (var name in new[] { "BackButton", "FindClueButton" })
        {
            var go = FindSceneObject(scene, name);
            if (go != null) go.SetActive(false);
        }
        SetText(FindText(scene, "ReturnHomeButtonLabel"), "PLAY AGAIN", 18f, 16f, 20f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
    }

    private static void AddStoryBanner(Scene scene, Sprite sprite)
    {
        var panel = FindSceneObject(scene, "StoryPanel");
        if (panel == null) return;
        var transform = panel.transform.Find("A2StoryBanner");
        if (transform == null)
        {
            var banner = new GameObject("A2StoryBanner", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            banner.transform.SetParent(panel.transform, false);
            transform = banner.transform;
        }
        var rect = transform.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.04f, 0.79f);
        rect.anchorMax = new Vector2(0.96f, 0.98f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        var image = transform.GetComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.color = new Color32(37, 79, 83, 250);
        image.raycastTarget = false;
        transform.SetAsFirstSibling();
    }

    private static void SetText(TMP_Text text, string value, float size, float minSize, float maxSize,
        Color color, TextAlignmentOptions alignment, FontStyles style)
    {
        if (text == null) return;
        text.text = value;
        text.color = color;
        text.fontStyle = style;
        text.alignment = alignment;
        text.enableWordWrapping = true;
        text.enableAutoSizing = true;
        text.fontSize = size;
        text.fontSizeMin = minSize;
        text.fontSizeMax = maxSize;

        // The storybook font carries a coloured face in its material, which multiplies
        // the vertex colour set above. The earlier version wrote the requested colour
        // back into that face instead, so every label rendered as colour x colour and
        // the pale story banner turned olive. Labels now share a white-face material
        // asset, so the colour requested here is the colour the reader sees.
        ApplyWhiteFaceMaterial(text);
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

    private static AudioClip LoadClip(string name)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioRoot + "/" + name);
        if (clip == null) throw new InvalidOperationException("Story sound was not imported: " + name);
        return clip;
    }

    private static void ConfigureShortAudio(string fileName)
    {
        var path = AudioRoot + "/" + fileName;
        var importer = AssetImporter.GetAtPath(path) as AudioImporter;
        if (importer == null) throw new InvalidOperationException("Audio importer not found: " + path);
        var settings = importer.defaultSampleSettings;
        settings.loadType = AudioClipLoadType.DecompressOnLoad;
        settings.compressionFormat = AudioCompressionFormat.Vorbis;
        settings.quality = 0.8f;
        importer.defaultSampleSettings = settings;
        importer.forceToMono = true;
        importer.loadInBackground = false;
        importer.SaveAndReimport();
    }

    private static void ConfigureMusicAudio(string fileName)
    {
        var path = AudioRoot + "/" + fileName;
        var importer = AssetImporter.GetAtPath(path) as AudioImporter;
        if (importer == null) throw new InvalidOperationException("Music importer not found: " + path);
        var settings = importer.defaultSampleSettings;
        settings.loadType = AudioClipLoadType.CompressedInMemory;
        settings.compressionFormat = AudioCompressionFormat.Vorbis;
        settings.quality = 0.72f;
        importer.defaultSampleSettings = settings;
        importer.forceToMono = false;
        importer.loadInBackground = true;
        importer.SaveAndReimport();
    }

    private static void RebuildPreview()
    {
        var scenePaths = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        if (scenePaths.Length != 6)
            throw new InvalidOperationException("A2 preview expects exactly six enabled scenes; found " + scenePaths.Length);
        PlayerSettings.productName = "The Lighthouse That Remembers Sounds";
        PlayerSettings.WebGL.decompressionFallback = true;
        var projectRoot = Directory.GetParent(Application.dataPath).FullName;
        var outputPath = Path.Combine(projectRoot, "Builds", "A2_FinalPreview");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenePaths,
            locationPathName = outputPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        });
        Debug.Log("A2_FINAL_WEBGL_BUILD=" + report.summary.result + " errors=" + report.summary.totalErrors + " warnings=" + report.summary.totalWarnings + " decompressionFallback=" + PlayerSettings.WebGL.decompressionFallback);
        if (report.summary.result != BuildResult.Succeeded)
            throw new InvalidOperationException("A2 WebGL build did not succeed.");
    }
}
