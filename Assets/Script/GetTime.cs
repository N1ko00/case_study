using UnityEngine;


// データを保持するためだけのクラスですわ
public class TimeKeeper
{
    // staticをつけるのが最大のポイントですの！
    public static float clearTime;
}

public class GetTime : MonoBehaviour
{
    void Update()
    {
        // ゲーム開始から5秒以上経過したかチェックしますわ
        if (Time.time > 5.0f)
        {
            Debug.Log("お坊ちゃま、ゲーム開始から5秒が経過いたしましたわ！");
            TimeKeeper.clearTime = Time.time;
            Debug.Log("お坊ちゃまのクリアタイムは：" + TimeKeeper.clearTime + "秒ですわ！");

        }
    }
}
