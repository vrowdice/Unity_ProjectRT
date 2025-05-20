using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPanel : MonoBehaviour, IUIPanel
{
    private GameDataManager m_gameDataManager = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnOpen(GameDataManager argDataManager)
    {
        m_gameDataManager = argDataManager;
        gameObject.SetActive(true);
    }

    public void OnClose()
    {

    }
}
