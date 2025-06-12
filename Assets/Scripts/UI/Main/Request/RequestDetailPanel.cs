using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RequestDetailPanel : MonoBehaviour
{
    [SerializeField]
    Transform m_resourceContentTrans = null;
    [SerializeField]
    Transform m_tokenContentTrans = null;
    [SerializeField]
    TextMeshProUGUI m_titleText = null;
    [SerializeField]
    TextMeshProUGUI m_descriptionText = null;

    private RequestPanel m_requestPanel;
    private RequestState m_nowRequestState;

    public void OnOpen(RequestPanel argRequestPanel, bool argIsAcceptable, RequestState argState)
    {
        m_requestPanel = argRequestPanel;
        m_nowRequestState = argState;

        GameObjectUtils.ClearChildren(m_resourceContentTrans);
        foreach (ResourceAmount item in argState.m_resourceRewardList)
        {
            GameObject _obj = Instantiate(m_requestPanel.MainUIManager.ResourceIconTextPrefeb, m_resourceContentTrans);

            _obj.GetComponent<ResourceIconText>().InitializeMainText(item.m_type, item.m_amount);
        }

        GameObjectUtils.ClearChildren(m_tokenContentTrans);
        foreach (TokenAmount item in argState.m_tokenRewardList)
        {
            GameObject _obj = Instantiate(m_requestPanel.MainUIManager.ResourceIconTextPrefeb, m_tokenContentTrans);

            _obj.GetComponent<ResourceIconText>().InitializeMainText(item.m_type, item.m_amount);
        }

        m_titleText.text = argState.m_title;
        m_descriptionText.text = argState.m_description;

        gameObject.SetActive(true);
    }

    public void ClickAcceptBtn()
    {
        m_requestPanel.AcceptRequest(m_nowRequestState);

        gameObject.SetActive(false);
    }

    public void ClickRefuseBtn()
    {
        gameObject.SetActive(false);
    }
}
