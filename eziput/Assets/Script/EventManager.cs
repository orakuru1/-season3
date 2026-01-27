using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;
    public GameObject targetMap;
    public GameObject returnMap;


    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator TriggerEvent(string id, Unit actor)
    {
        Debug.Log($"イベント発生: {id}");

        switch (id)
        {
            case "Treasure":
                Debug.Log($"{actor.name} は宝箱を開けた！");
                break;

            case "Trap":
                Debug.Log($"{actor.name} は罠を踏んだ！");
                actor.TakeDamage(10, actor);
                break;

            case "Next":
                Debug.Log($"{actor.name} は出口に着いた！");

                if (ItemUIManager.instance.HasItem("アヌビスの仮面"))
                {
                    Debug.Log("特殊アイテム所持 → クリア判定");

                    // 消費
                    ItemUIManager.instance.UseSelectedItem("アヌビスの通行証", "item");

                    // ★ フラグを立てる
                    GameManager.Instance.IsItemCrafted = true;

                    GameManager.Instance.SavePlayerState(actor);
                    GameManager.Instance.TryStageClear();
                }
                else
                {
                    Debug.Log("特殊アイテムを持っていません");
                }
                break;


            case "Boss":
                Debug.Log("ボス戦開始！");
                yield return new WaitForSeconds(3f);
                break;

            default:
                Debug.Log("未定義イベント");
                break;
        }
        actor.isEvent = false;
    }
}
