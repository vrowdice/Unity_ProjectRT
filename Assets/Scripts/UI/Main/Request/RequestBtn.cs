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

    int m_index = 0;
    RequestPanel m_requestPanel = null;

    public void Initialize(
        List<ResourceAmount> argResourceAmountList,
        RequestPanel argPanel,
        Sprite argClientImage,
        int argIndex,
        int argFactionLike,
        string argName,
        string argExp)
    {
        m_requestPanel = argPanel;

        foreach (ResourceAmount item in argResourceAmountList)
        {
            GameObject _obj = Instantiate(m_requestPanel.MainUIManager.ResourceIconTextPrefeb, m_resourceContentTrans);

            _obj.GetComponent<ResourceIconText>().InitializeMainText(item.m_type, item.m_amount);
        }

        m_clientImage.sprite = argClientImage;
        m_index = argIndex;
        m_completeConditionText.text = "+ " + argFactionLike.ToString();
        m_nameText.text = argName;
        m_expText.text = argExp;
    }

    public void Click()
    {
        m_requestPanel.AcceptRequest(m_index);
    }
}
