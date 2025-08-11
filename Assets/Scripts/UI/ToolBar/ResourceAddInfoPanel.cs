using UnityEngine;

public class ResourceAddInfoPanel : MonoBehaviour
{
    [SerializeField]
    GameObject m_resourceAddInfoPanel = null;
    [SerializeField]
    ResourceAddInfoPanelText m_resourceAddInfoPanelTextPrefab = null;
    [SerializeField]
    Transform m_resourceAddInfoContent = null;

    public void OpenResourceAddInfoPanel(ResourceType.TYPE argType, EffectOperationType.TYPE argEffectOperationType, int argEffectValue)
    {
        m_resourceAddInfoPanel.SetActive(true);
        ResourceAddInfoPanelText text = Instantiate(m_resourceAddInfoPanelTextPrefab, m_resourceAddInfoContent);
        text.Init(argType.ToString(), "Explain Text");
    }
}