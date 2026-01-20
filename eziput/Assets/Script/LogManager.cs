using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogManager : MonoBehaviour
{
    public static LogManager Instance;

    [Header("UI")]
    public ScrollRect scrollRect;
    public RectTransform content;
    public GameObject logPrefab;

    [Header("Settings")]
    public int maxLogCount = 20;

    [Header("Fade Settings")]
    public float visibleTime = 3f;   // 表示される時間
    public float fadeTime = 1.5f;    // フェード時間

    [Header("UI Root")]
    public GameObject logUIRoot;   // ScrollViewの親
    private int fadingLogCount = 0; // フェード中ログ数

    [Header("Get Item Popup UI")]
    [SerializeField] private GameObject popupObj;
    [SerializeField] private Transform itemLogParent;

    private readonly Queue<GameObject> logs = new();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        logUIRoot.SetActive(false);
    }

    public void AddLog(string message)
    {
        // ★ 表示する
        if (logUIRoot != null && !logUIRoot.activeSelf)
            logUIRoot.SetActive(true);

        GameObject log = Instantiate(logPrefab, content);
        log.transform.SetAsLastSibling();

        log.GetComponentInChildren<Text>().text = message;
        logs.Enqueue(log);

        if (logs.Count > maxLogCount)
        {
            GameObject old = logs.Dequeue();
            Destroy(old);
        }

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;

        // ★ フェード管理
        fadingLogCount++;
        StartCoroutine(FadeAndRemove(log));
    }

    public void AddItemLog(string itemName, Sprite icon)
    {
        // ★ 表示する
        if (logUIRoot != null && !logUIRoot.activeSelf)
            logUIRoot.SetActive(true);

        GameObject log = Instantiate(popupObj, itemLogParent);
        log.transform.SetAsLastSibling();

        Image iconImage = log.transform.Find("ItemIcon").GetComponent<Image>();
        Text itemText = log.transform.Find("ItemText").GetComponent<Text>();

        iconImage.sprite = icon;
        itemText.text = itemName;

        logs.Enqueue(log);

        if (logs.Count > maxLogCount)
        {
            GameObject old = logs.Dequeue();
            Destroy(old);
        }

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;

        // ★ フェード管理
        fadingLogCount++;
        StartCoroutine(FadeAndRemove(log));
    }

    private System.Collections.IEnumerator FadeAndRemove(GameObject log)
    {
        CanvasGroup cg = log.GetComponent<CanvasGroup>();
        if (cg == null)
            yield break;

        cg.alpha = 1f;

        yield return new WaitForSeconds(visibleTime);

        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
            yield return null;
        }

        // Queue から除去
        if (logs.Contains(log))
        {
            var newQueue = new Queue<GameObject>();
            foreach (var l in logs)
                if (l != log) newQueue.Enqueue(l);

            logs.Clear();
            foreach (var l in newQueue)
                logs.Enqueue(l);
        }

        Destroy(log);

        // ★ フェード完了
        fadingLogCount--;

        // ★ 全部消えたらUI非表示
        if (fadingLogCount <= 0 && logUIRoot != null)
        {
            logUIRoot.SetActive(false);
        }
    }



    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            AddLog("This is a test log message.");
        }
    }

    public static string ColorText(string text, string color)
    {
        return $"<color={color}>{text}</color>";
    }
}
