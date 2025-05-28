using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RequestDetailPanel : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI m_titleText = null;
    [SerializeField]
    TextMeshProUGUI m_descriptionText = null;
    [SerializeField]
    TextMeshProUGUI m_conditionText = null;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnOpen()
    {
        m_conditionText.text = "";

        gameObject.SetActive(true);
    }
}
