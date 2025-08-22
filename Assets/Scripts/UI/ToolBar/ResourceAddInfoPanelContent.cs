using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResourceAddInfoPanelContent : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI m_titleText = null;
    [SerializeField]
    Transform m_contentTrans = null;
    [SerializeField]
    GameObject m_expTextPrefab = null;

    public void Init(List<string> argExplainTexts, string argTextTitle)
    {
        GameObjectUtils.ClearChildren(m_contentTrans);

        m_titleText.text = argTextTitle;

        foreach(string item in argExplainTexts)
        {
            TextMeshProUGUI _text = Instantiate(m_expTextPrefab, m_contentTrans).GetComponent<TextMeshProUGUI>();
            _text.text = item;
        }

    }

    public void Init(List<string> argExplainTexts)
    {
        GameObjectUtils.ClearChildren(m_contentTrans);

        foreach (string item in argExplainTexts)
        {
            TextMeshProUGUI _text = Instantiate(m_expTextPrefab, m_contentTrans).GetComponent<TextMeshProUGUI>();
            _text.text = item;
        }
    }
}