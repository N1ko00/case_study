using UnityEngine;
using TMPro; 

public class TimeDisplayer : MonoBehaviour
{
    
    [SerializeField] private TMP_Text timeText;

    void Start()
    {
        // TimeKeeperから保存されたタイムを読み取り
        float rawTime = TimeKeeper.clearTime;

        // 全体の秒数から「分」を計算
        int minutes = Mathf.FloorToInt(rawTime / 60f);

        // 60秒で割った余りから「秒」を計算
        int seconds = Mathf.FloorToInt(rawTime % 60f);

        int milliseconds = Mathf.FloorToInt((rawTime * 100f) % 100f);

        string formattedTime = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);

        timeText.text = "Clear Time\n" + formattedTime;
    }
}
