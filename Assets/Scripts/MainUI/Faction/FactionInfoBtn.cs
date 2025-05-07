using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FactionInfoBtn : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI m_nameText = null;
    [SerializeField]
    Image m_image = null;

    public void Setting(string argName, Sprite argSprite)
    {
        m_nameText.text = argName;
        m_image.sprite = argSprite;
    }
}
