using UnityEngine;

public interface IUIManager
{
    Transform CanvasTrans { get; }
    
    // 공통 UI 프리팹들
    GameObject ResourceIconTextPrefeb { get; }
    GameObject ConditionPanelTextPrefeb { get; }
    GameObject ResourceIconImagePrefeb { get; }

    void UpdateAllMainText();

    void Initialize(GameManager argGameManager, GameDataManager argGameDataManager);
}
