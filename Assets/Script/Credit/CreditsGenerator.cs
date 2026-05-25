using UnityEngine;
using TMPro;

public class CreditsGenerator : MonoBehaviour
{
    [Header("データ")]
    [SerializeField]
    private CreditsData creditsData;

    [Header("生成先")]
    [SerializeField]
    private Transform creditsRoot;

    [Header("Prefab")]
    [SerializeField]
    private TextMeshProUGUI sectionTitlePrefab;

    [SerializeField]
    private TextMeshProUGUI namePrefab;

    [Header("余白")]
    [SerializeField]
    private float sectionSpacing = 100f;

    private void Start()
    {
        Generate();
    }

    public void Generate()
    {
        foreach (Transform child in creditsRoot)
        {
            Destroy(child.gameObject);
        }

        foreach (var section in creditsData.sections)
        {
            CreateSection(section);
        }
    }

    private void CreateSection(CreditSection section)
    {
        // セクションタイトル
        var title = Instantiate(
            sectionTitlePrefab,
            creditsRoot
        );

        title.text = section.sectionTitle;

        // 名前一覧
        foreach (var name in section.names)
        {
            var text = Instantiate(
                namePrefab,
                creditsRoot
            );

            text.text = name;
        }

        // セクション余白
        var spacer = new GameObject("Spacer");

        spacer.transform.SetParent(
            creditsRoot,
            false
        );

        var layout =
            spacer.AddComponent<UnityEngine.UI.LayoutElement>();

        layout.preferredHeight = sectionSpacing;
    }
}