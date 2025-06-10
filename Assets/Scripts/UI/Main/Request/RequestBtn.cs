using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RequestBtn : MonoBehaviour
{
    [SerializeField]
    Transform m_resourceContentTrans = null;
    [SerializeField]
    Image m_clientImage = null;
    [SerializeField]
    Image m_typeImage = null;
    [SerializeField]
    TextMeshProUGUI m_nameText = null;
    [SerializeField]
    TextMeshProUGUI m_expText = null;
    [SerializeField]
    TextMeshProUGUI m_completeConditionText = null;

    bool m_isAcceptable = true;
    int m_index = 0;
    RequestPanel m_requestPanel = null;

    public void Initialize(
        bool argIsAcceptable,
        RequestPanel argPanel,
        int argIndex,
        Sprite argClientImage,
        Sprite argRequestTypeImage,
        RequestState argState)
    {
        m_isAcceptable = argIsAcceptable;

        foreach (ResourceAmount item in argState.m_resourceReward)
        {
            GameObject _obj = Instantiate(m_requestPanel.MainUIManager.ResourceIconTextPrefeb, m_resourceContentTrans);

            _obj.GetComponent<ResourceIconText>().InitializeMainText(item.m_type, item.m_amount);
        }

        m_requestPanel = argPanel;
        m_index = argIndex;
        m_clientImage.sprite = argClientImage;
        m_typeImage.sprite = argRequestTypeImage;
        m_completeConditionText.text = "+ " + argState.m_factionAddLike.ToString();
        m_nameText.text = argState.m_factionType.ToString();
        m_expText.text = argState.m_requestType.ToString();
    }

    public void Click()
    {
        m_requestPanel.OpenRequestDetailPanel(m_isAcceptable, m_index);
    }
}
