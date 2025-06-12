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
    RequestPanel m_requestPanel = null;
    RequestState m_requestState = null;

    public void Initialize(
        bool argIsAcceptable,
        RequestPanel argPanel,
        Sprite argClientImage,
        Sprite argRequestTypeImage,
        RequestState argState)
    {
        m_isAcceptable = argIsAcceptable;
        m_requestPanel = argPanel;
        m_clientImage.sprite = argClientImage;
        m_typeImage.sprite = argRequestTypeImage;
        m_requestState = argState;
        m_completeConditionText.text = "+ " + argState.m_factionAddLike.ToString();
        m_nameText.text = argState.m_title;
        m_expText.text = argState.m_description;

        foreach (ResourceAmount item in argState.m_resourceRewardList)
        {
            GameObject _obj = Instantiate(m_requestPanel.MainUIManager.ResourceIconTextPrefeb, m_resourceContentTrans);

            _obj.GetComponent<ResourceIconText>().InitializeMainText(item.m_type, item.m_amount);
        }
    }

    public void Click()
    {
        m_requestPanel.OpenRequestDetailPanel(m_isAcceptable, m_requestState);
    }
}
