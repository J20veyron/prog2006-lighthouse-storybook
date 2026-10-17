using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public void LoadMyScene(int sceneNumber)
    {
        A2BackgroundMusic.EnsurePlaying();
        if (SceneManager.GetActiveScene().name == "HomePage" && sceneNumber == 1)
            A2StoryProgress.ResetJourney();

        SceneManager.LoadScene(sceneNumber);
    }

    public void FindClue(int ignoredLegacyCount)
    {
        var page = FindObjectOfType<A2StoryPageController>();
        if (page != null)
            page.ShowHint();
    }
}
