using UnityEngine;
using System.Collections;

public class CreditsScroll : MonoBehaviour
{
    public RectTransform target;
    public float speed = 50f;

    //制御しやすいので、コルーチンでスクロールさせる
    IEnumerator Start()
    {
        while (true)
        {
            target.anchoredPosition += Vector2.up * speed * Time.deltaTime;
            yield return null;
        }
    }
}