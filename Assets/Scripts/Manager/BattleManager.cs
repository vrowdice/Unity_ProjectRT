using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 전투 시스템을 관리하는 매니저 클래스
/// 현재는 기본 구조만 구현되어 있음
/// 향후 전투 로직 구현 시 확장 예정
/// </summary>
public class BattleManager : MonoBehaviour
{

    [Header("배틀 로딩 매니저")]
    [SerializeField] private BattleLoadingManager m_battleLoadingManager = null;
    public bool isSettingClear = false;
    public bool isAttackField = true;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(m_battleLoadingManager.InitializeBattleScene());
    }

    // Update is called once per frame
    void Update()
    {
        if(isSettingClear == true)
        {
            MoveField();
        }
    }

    private void MoveField()
    {
        if(isAttackField != true)
        {
            m_battleLoadingManager.mainCamra.transform.position = m_battleLoadingManager.defenseCameraPoint;
            Destroy(m_battleLoadingManager.deploymentUI);
        }
        else
        {
            m_battleLoadingManager.mainCamra.transform.position = m_battleLoadingManager.attackCameraPoint;
        }
    }
}
