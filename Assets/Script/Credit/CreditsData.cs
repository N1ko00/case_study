using UnityEngine;
using System.Collections.Generic;

//クレジットデータを定義
[CreateAssetMenu(
    fileName = "CreditsData",
    menuName = "Game/Credits Data"
)]
public class CreditsData : ScriptableObject
{
    public List<CreditSection> sections = new();
}