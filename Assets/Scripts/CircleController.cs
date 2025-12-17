using UnityEngine;
using TMPro;

public class CircleController : MonoBehaviour
{
    public int nodeId;
    private SpriteRenderer sr;
    private TextMeshPro scoreText;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        
        // Create TextMeshPro for score display
        GameObject textObj = new GameObject("ScoreText");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = Vector3.zero;
        textObj.transform.localScale = Vector3.one;
        
        scoreText = textObj.AddComponent<TextMeshPro>();
        scoreText.alignment = TextAlignmentOptions.Center;
        scoreText.fontSize = 3;
        scoreText.color = Color.black;
        scoreText.sortingOrder = 10;
    }

    public void SetColorAndScore(int score, byte owner)
    {
        // owner: 0 neutral -> white, 1 player1 -> red-ish, 2 player2 -> blue-ish
        if (sr != null)
        {
            if (owner == 1)
                sr.color = Color.Lerp(Color.white, Color.red, Mathf.Clamp01(Mathf.Abs(score) / 10f + 0.1f));
            else if (owner == 2)
                sr.color = Color.Lerp(Color.white, Color.blue, Mathf.Clamp01(Mathf.Abs(score) / 10f + 0.1f));
            else
                sr.color = Color.white;
        }
        
        // Update score text
        if (scoreText != null)
        {
            scoreText.text = Mathf.Abs(score).ToString();
            
            // Change text color based on owner
            if (owner == 1)
                scoreText.color = new Color(0.5f, 0, 0); // dark red
            else if (owner == 2)
                scoreText.color = new Color(0, 0, 0.5f); // dark blue
            else
                scoreText.color = Color.black;
        }
    }
}
