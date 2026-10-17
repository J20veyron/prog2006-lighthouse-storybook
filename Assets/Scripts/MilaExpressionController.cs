using UnityEngine;
using UnityEngine.UI;

public class MilaExpressionController : MonoBehaviour
{
    public Sprite listening;
    public Sprite curious;
    public Sprite recognising;
    public Sprite relieved;

    public void ShowForClue(int foundCount)
    {
        var portrait = GetComponent<Image>();
        if (portrait == null)
            return;

        Sprite expression = relieved;
        if (foundCount <= 1)
            expression = listening;
        else if (foundCount == 2)
            expression = curious;
        else if (foundCount == 3)
            expression = recognising;

        if (expression != null)
            portrait.sprite = expression;
    }
}
