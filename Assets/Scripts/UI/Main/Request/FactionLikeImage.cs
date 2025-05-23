using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FactionLikeImage : MonoBehaviour
{
    [SerializeField]
    Image m_image = null;
    [SerializeField]
    TextMeshProUGUI m_likeText = null;
    [SerializeField]
    TextMeshProUGUI m_nameText = null;

    public FactionType m_factionType = FactionType.None;

    public void Initialize(FactionType argFactionType, Sprite argSprite, string argName, int arglike)
    {
        m_factionType = argFactionType;
        m_image.sprite = argSprite;
        m_nameText.text = argName;
        m_likeText.text = arglike.ToString();
    }

    public void SetLikeText(int arglike)
    {
        m_likeText.text = arglike.ToString();
    }
}
