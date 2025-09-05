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

        // Ŭ���� ����
        if (stat.flipbook) _anim.SetClipSet(stat.flipbook);

        // �⺻ ��� ���(Standing Ŭ�� FPS/Loop�� SO�� ��ϵ�)
        _anim.Play("Standing");

        // ����
        bool faceRight = stat.spawnFacingRight;
        _anim.SetFlipX(!faceRight); 

        // ���־� ������
        _anim.SetVisualScale(Mathf.Max(0.1f, stat.visualScale));
    }
}
