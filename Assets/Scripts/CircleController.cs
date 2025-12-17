using UnityEngine;

public class CircleController : MonoBehaviour
{
    public int nodeId;
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetColorAndScore(int score, byte owner)
    {
        // owner: 0 neutral -> white, 1 player1 -> red-ish, 2 player2 -> blue-ish
        if (sr == null) return;
        if (owner == 1)
            sr.color = Color.Lerp(Color.white, Color.red, Mathf.Clamp01(score / 10f + 0.1f));
        else if (owner == 2)
            sr.color = Color.Lerp(Color.white, Color.blue, Mathf.Clamp01(-score / 10f + 0.1f));
        else
            sr.color = Color.white;

    }
}
