using UnityEngine;
using System;

// 子オブジェクトのトリガーイベントを親 (LockerDoor) に流すだけのやつ
// 操作範囲と隠れゾーン、トリガー2つ使いたいので分けてる
public class LockerTriggerRelay : MonoBehaviour
{
    public Action<Collider> OnEnter;
    public Action<Collider> OnExit;

    private void OnTriggerEnter(Collider other) => OnEnter?.Invoke(other);
    private void OnTriggerExit(Collider other) => OnExit?.Invoke(other);
}