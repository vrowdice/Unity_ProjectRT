using UnityEngine;

[DisallowMultipleComponent]
public class UnitVisualInitializer : MonoBehaviour
{
    private UnitBase _unit;
    private SpriteFlipbookAnimatorSO _anim;

    private void Awake()
    {
        _unit = GetComponent<UnitBase>();
        _anim = GetComponentInChildren<SpriteFlipbookAnimatorSO>();
    }

    private void Start() => TryApply();

    public void TryApply()
    {
        if (!_unit || !_anim) return;

        var stat = _unit.UnitStat;
        if (stat == null) return;

        // 클립셋 연결
        if (stat.flipbook) _anim.SetClipSet(stat.flipbook);

        // 기본 대기 재생(Standing 클립 FPS/Loop는 SO에 기록됨)
        _anim.Play("Standing");

        // 방향
        bool faceRight = stat.spawnFacingRight;
        _anim.SetFlipX(!faceRight); 

        // 비주얼 스케일
        _anim.SetVisualScale(Mathf.Max(0.1f, stat.visualScale));
    }
}
