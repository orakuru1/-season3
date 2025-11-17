using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetItemPopUi : MonoBehaviour
{
    public static GetItemPopUi instance;

    [SerializeField] private GameObject popupObj;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Text itemText;

    private void Awake()
    {
        instance = this;
        popupObj.SetActive(false);
    }

    public void Show(string itemName, Sprite icon)
    {
        gameObject.SetActive(true);

        StartCoroutine(ShowPopup(itemName, icon));
    }

    IEnumerator ShowPopup(string itemName, Sprite icon)
    {
        popupObj.SetActive(true);
        itemIcon.sprite = icon;
        itemText.text = $"{itemName}を手に入れた";

        //2秒表示
        yield return new WaitForSeconds(2f);

        popupObj.SetActive(false);
    }
}
