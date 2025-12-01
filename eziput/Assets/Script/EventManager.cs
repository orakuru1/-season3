using System.Collections;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

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

            case "BossStart":
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
