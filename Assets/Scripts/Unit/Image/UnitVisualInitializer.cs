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

        if (stat.flipbook) _anim.SetClipSet(stat.flipbook);

        _anim.Play("Standing");

        bool faceRight = stat.spawnFacingRight;
        _anim.SetFlipX(!faceRight);

        if (stat.flipbook) _anim.SetClipSet(stat.flipbook);

        // Å©±â
        _anim.SetVisualScale(Mathf.Max(0.1f, stat.visualScale));
    }
}
