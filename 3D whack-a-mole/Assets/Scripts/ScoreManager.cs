using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField]
    public int curSocre;
    [SerializeField]
    public int maxScore = 0;
    public TMP_Text curScoreText;
    public TMP_Text maxScoreText;
    public TMP_Text endScoreText;

    public static ScoreManager instance;

    void Awake ()
    {
        curSocre = 0;
        instance = this;
    }

    void Update ()
    {
        this.curScoreText.text = "当前分数： " + curSocre;
    }

    public void SetMaxScoreView(int value)
    {
        maxScoreText.text ="最高得分："+value.ToString();
    }

    public void SetEndScoreView(int value)
    {
        endScoreText.text = "本次得分：" + value.ToString();
    }
}
