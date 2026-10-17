using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class A1VisualRefresh
{
    private const string CharacterRoot = "Assets/Art/A1/Characters";
    private static readonly string[] SceneNames = { "HomePage", "RockyShore", "Harbour", "Dunes", "MilasHome", "Credits" };

    [MenuItem("Tools/PROG2006 A1/Refresh transparent Mila sprites")]
    public static void Apply()
    {
        try
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ConfigureSprite("Mila_Listening_Transparent.png");
            ConfigureSprite("Mila_Curious_Transparent.png");
            ConfigureSprite("Mila_Relieved_Transparent.png");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var listening = LoadSprite("Mila_Listening_Transparent.png");
            var curious = LoadSprite("Mila_Curious_Transparent.png");
            var relieved = LoadSprite("Mila_Relieved_Transparent.png");

            foreach (var sceneName in SceneNames)
            {
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + sceneName + ".unity", OpenSceneMode.Single);
                var portraits = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Image>(true))
                    .Where(image => image.name == "MilaPortrait")
                    .ToArray();
                if (portraits.Length != 1)
                    throw new InvalidOperationException("Expected one MilaPortrait in " + sceneName + ", found " + portraits.Length);

                var image = portraits[0];
                image.sprite = listening;
                image.preserveAspect = true;
                image.color = Color.white;
                image.raycastTarget = false;

                var controller = image.GetComponent<MilaExpressionController>();
                if (controller == null)
                    controller = image.gameObject.AddComponent<MilaExpressionController>();
                controller.listening = listening;
                controller.curious = curious;
                controller.recognising = curious;
                controller.relieved = relieved;

                ApplyButtonTheme(scene);

                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("Could not save " + sceneName);
                Debug.Log("A1 transparent Mila sprite assigned: " + sceneName);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("A1_MILA_TRANSPARENCY_COMPLETE scenes=6 sprites=3 motionSpeed=2.5x");
            Rebuild();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }

    [MenuItem("Tools/PROG2006 A1/Rebuild enhanced WebGL preview")]
    public static void Rebuild()
    {
        var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
        if (scenes.Length == 0)
            throw new InvalidOperationException("Build Settings has no enabled scenes.");
        var root = Directory.GetParent(Application.dataPath).FullName;
        var path = Path.Combine(root, "Builds", "A1_EnhancedPreview");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = path,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        });
        Debug.Log("A1_VISUAL_REFRESH_BUILD=" + report.summary.result + " errors=" + report.summary.totalErrors);
        if (report.summary.result != BuildResult.Succeeded)
            EditorApplication.Exit(1);
    }

    private static void ApplyButtonTheme(Scene scene)
    {
        var buttons = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Button>(true))
            .ToArray();

        foreach (var button in buttons)
        {
            var name = button.name.ToLowerInvariant();
            Color baseColor = name.Contains("findclue")
                ? new Color32(202, 98, 74, 255)
                : name.Contains("back") || name.Contains("returnhome")
                    ? new Color32(75, 101, 117, 255)
                    : new Color32(45, 112, 105, 255);

            button.transition = Selectable.Transition.ColorTint;
            var background = button.targetGraphic as Image;
            if (background == null)
                background = button.GetComponent<Image>();
            if (background != null)
            {
                background.color = Color.white;
                button.targetGraphic = background;

                var shadow = background.GetComponent<Shadow>();
                if (shadow == null)
                    shadow = background.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(0.12f, 0.20f, 0.20f, 0.30f);
                shadow.effectDistance = new Vector2(0f, -3f);
                shadow.useGraphicAlpha = true;
            }

            var colors = button.colors;
            colors.normalColor = baseColor;
            colors.highlightedColor = Color.Lerp(baseColor, Color.white, 0.13f);
            colors.pressedColor = Color.Lerp(baseColor, Color.black, 0.20f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = Color.Lerp(baseColor, new Color32(143, 159, 154, 255), 0.65f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.12f;
            button.colors = colors;

            var cream = new Color32(255, 248, 229, 255);
            foreach (var childImage in button.GetComponentsInChildren<Image>(true))
            {
                if (childImage != background)
                    childImage.color = cream;
            }
            foreach (var label in button.GetComponentsInChildren<TMP_Text>(true))
                label.color = cream;
        }

        Debug.Log("A1 button theme applied: " + scene.name + " buttons=" + buttons.Length);
    }

    private static string AssetPath(string fileName)
    {
        return CharacterRoot + "/" + fileName;
    }

    private static void ConfigureSprite(string fileName)
    {
        var assetPath = AssetPath(fileName);
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
            throw new InvalidOperationException("Texture importer missing for " + assetPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100f;
        importer.maxTextureSize = 2048;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Compressed;
        importer.compressionQuality = 90;
        importer.SaveAndReimport();
    }

    private static Sprite LoadSprite(string fileName)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetPath(fileName));
        if (sprite == null)
            throw new InvalidOperationException("Sprite not imported: " + AssetPath(fileName));
        return sprite;
    }
}


