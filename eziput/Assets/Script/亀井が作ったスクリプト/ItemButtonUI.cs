using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemButtonUI : MonoBehaviour
{
    public Text nameText;
    public Text countText;

    public void Set(string itemName, int count)
    {
        nameText.text = itemName;
        countText.text = count > 1 ? $"×{count}" : "";
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
