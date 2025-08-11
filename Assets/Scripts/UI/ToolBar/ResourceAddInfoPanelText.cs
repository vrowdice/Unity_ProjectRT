using UnityEngine;
using TMPro;

public class ResourceAddInfoPanelText : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI m_resourceAddInfoText = null;

    public void Init(string argTextTitle, string argExplainText)
    {
        m_resourceAddInfoText.text = argTextTitle + "\n" + argExplainText;
    }
}