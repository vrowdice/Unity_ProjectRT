using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WarningPanel : MonoBehaviour
{
    /// <summary>
    /// �ִϸ��̼�
    /// </summary>
    public Animator m_ani = null;

    // Start is called before the first frame update
    void Start()
    {
        m_ani.SetTrigger("Open");
    }

    /// <summary>
    /// ������Ʈ �ı�
    /// </summary>
    void DestroyObj()
    {
        Destroy(gameObject);
    }
}
