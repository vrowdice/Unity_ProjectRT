using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequestPanel : MonoBehaviour, IUIPanel
{
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
        gameObject.SetActive(true);
    }

    public void OnClose()
    {

    }
}
