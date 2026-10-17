using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class A2StoryProgress
{
    public const int TotalMemories = 4;
    private const string Prefix = "A2_SOUND_MEMORY_";

    public static string Key(int memoryId) => Prefix + memoryId;

    public static bool HasMemory(int memoryId)
    {
        return memoryId > 0 && PlayerPrefs.GetInt(Key(memoryId), 0) == 1;
    }

    public static int Count
    {
        get
        {
            var count = 0;
            for (var i = 1; i <= TotalMemories; i++)
                if (HasMemory(i)) count++;
            return count;
        }
    }

    public static void Record(int memoryId)
    {
        if (memoryId < 1 || memoryId > TotalMemories) return;
        PlayerPrefs.SetInt(Key(memoryId), 1);
        PlayerPrefs.Save();
    }

    public static void ResetJourney()
    {
        for (var i = 1; i <= TotalMemories; i++)
            PlayerPrefs.DeleteKey(Key(i));
        PlayerPrefs.Save();
    }
}

public class A2StoryPageController : MonoBehaviour
{
    public int memoryId;
    [TextArea(2, 4)] public string hintText;
    [TextArea(2, 4)] public string restoredText;
    public AudioClip replayClip;

    private TMP_Text introCopy;
    private TMP_Text progressLabel;
    private TMP_Text hintButtonLabel;
    private Button nextButton;
    private AudioSource feedbackAudio;
    private MilaExpressionController milaExpression;

    private void Awake()
    {
        introCopy = FindText("IntroCopy");
        progressLabel = FindText("ProgressLabel");
        hintButtonLabel = FindText("FindClueLabel");
        var nextObject = GameObject.Find("NextButton");
        if (nextObject != null) nextButton = nextObject.GetComponent<Button>();
        var portrait = GameObject.Find("MilaPortrait");
        if (portrait != null) milaExpression = portrait.GetComponent<MilaExpressionController>();
        feedbackAudio = GetComponent<AudioSource>();
    }

    private void Start()
    {
        RefreshPageState();
        if (A2StoryProgress.HasMemory(memoryId) && introCopy != null && !string.IsNullOrWhiteSpace(restoredText))
            introCopy.text = restoredText;
    }

    public void ShowHint()
    {
        if (memoryId <= 0) return;

        if (A2StoryProgress.HasMemory(memoryId))
        {
            if (introCopy != null && !string.IsNullOrWhiteSpace(restoredText))
                introCopy.text = restoredText;
            Play(replayClip);
        }
        else if (introCopy != null && !string.IsNullOrWhiteSpace(hintText))
        {
            introCopy.text = hintText;
        }

        RefreshPageState();
    }

    public void CollectMemory(string responseText, AudioClip clip)
    {
        if (memoryId <= 0) return;

        A2StoryProgress.Record(memoryId);
        if (introCopy != null && !string.IsNullOrWhiteSpace(responseText))
            introCopy.text = responseText;
        if (milaExpression != null)
            milaExpression.ShowForClue(A2StoryProgress.Count);
        Play(clip != null ? clip : replayClip);
        RefreshPageState();
    }

    public void ShowObjectFeedback(string responseText, AudioClip clip)
    {
        if (introCopy != null && !string.IsNullOrWhiteSpace(responseText))
            introCopy.text = responseText;
        Play(clip);
    }

    private void RefreshPageState()
    {
        var found = A2StoryProgress.HasMemory(memoryId);
        if (progressLabel != null)
            progressLabel.text = "SOUNDS FOUND: " + A2StoryProgress.Count + " / " + A2StoryProgress.TotalMemories;
        if (hintButtonLabel != null)
            hintButtonLabel.text = found ? "LISTEN AGAIN" : "HINT";
        if (nextButton != null)
            nextButton.interactable = found;

        for (var i = 1; i <= A2StoryProgress.TotalMemories; i++)
        {
            var markObject = GameObject.Find("MemoryMark" + i);
            if (markObject == null) continue;
            var image = markObject.GetComponent<Image>();
            if (image != null)
                image.color = A2StoryProgress.HasMemory(i)
                    ? new Color32(255, 194, 61, 255)
                    : new Color32(190, 205, 205, 170);
        }
    }

    private void Play(AudioClip clip)
    {
        A2BackgroundMusic.EnsurePlaying();
        if (clip != null && feedbackAudio != null)
            feedbackAudio.PlayOneShot(clip, 0.82f);
    }

    private static TMP_Text FindText(string objectName)
    {
        var found = GameObject.Find(objectName);
        return found != null ? found.GetComponent<TMP_Text>() : null;
    }
}
