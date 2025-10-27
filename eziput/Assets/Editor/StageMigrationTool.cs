#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class StageMigrationTool : MonoBehaviour
{
    [MenuItem("Tools/Migrate StageMake to StageParent")]
    static void MigrateStage()
    {
        var stageMake = GameObject.Find("StageMake");
        if (stageMake == null)
        {
            Debug.LogWarning("StageMake が見つかりません。");
            return;
        }

        GameObject stageParent = GameObject.Find("StageParent");
        if (stageParent == null)
        {
            stageParent = new GameObject("StageParent");
            stageParent.transform.position = Vector3.zero;
            Debug.Log("StageParent を作成しました。");
        }

        // 全子を StageParent に移動
        int childCount = stageMake.transform.childCount;
        var children = new GameObject[childCount];
        for (int i = 0; i < childCount; i++) children[i] = stageMake.transform.GetChild(i).gameObject;
        foreach (var c in children)
        {
            c.transform.SetParent(stageParent.transform, true);
        }
        Debug.Log($"StageMake の {childCount} 個の子を StageParent に移動しました。");

        // StageMake を無効化（安全のため削除は手動で）
        stageMake.SetActive(false);
        Debug.Log("StageMake を無効化しました（削除する場合は手動で行ってください）。");
    }
}
#endif
