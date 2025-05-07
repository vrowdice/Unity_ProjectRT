using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public List<FactionData> FactionData => m_factionDataList;

    [SerializeField]
    List<FactionData> m_factionDataList = new List<FactionData>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
