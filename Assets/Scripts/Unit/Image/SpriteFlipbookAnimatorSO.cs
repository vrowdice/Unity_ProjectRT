using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// SO에 저장된 Sprite 배열을 FPS/Loop 옵션 그대로 재생하는 간단 애니메이터
[DisallowMultipleComponent]
public class SpriteFlipbookAnimatorSO : MonoBehaviour
{
    [Header("재생 정의(SO)")]
    [SerializeField] private FlipbookClipSet clipSet;

    [Header("출력 대상")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Image uiImage;

    private readonly Dictionary<string, Sprite[]> _clipSprites = new();
    private readonly Dictionary<string, FlipbookClipSet.ClipDef> _clipLookup = new();

    // 상태
    private Coroutine _playRoutine;
    private string _currentClip;
    private bool _isPlayingOnce;
    private int _currentFrame;
    private float _currentFps = -1.0f;

    public string CurrentClip => _currentClip;
    public int CurrentFrame => _currentFrame;
    public bool IsPlayingOnce => _isPlayingOnce;

    private void Awake()
    {
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (!uiImage) uiImage = GetComponentInChildren<Image>();
        if (!spriteRenderer && !uiImage)
            Debug.LogError("[SpriteFlipbookAnimatorSO] SpriteRenderer나 UI.Image 필요", this);

        BuildCaches();
    }

    private void OnDisable()
    {
        StopCurrent(); // 유령 코루틴 방지
    }

    private void BuildCaches()
    {
        _clipSprites.Clear();
        _clipLookup.Clear();

        if (!clipSet)
        {
            Debug.LogError("[SpriteFlipbookAnimatorSO] ClipSet이 비어있음", this);
            return;
        }

        foreach (var c in clipSet.Clips)
        {
            if (string.IsNullOrEmpty(c.Name)) continue;
            _clipLookup[c.Name] = c;
            var frames = c.Sprites?.Where(s => s != null).ToArray();
            if (frames == null || frames.Length == 0)
            {
                Debug.LogWarning($"[Flipbook] '{c.Name}' 프레임 비어있음. 에디터에서 채워주세요.", this);
                continue;
            }
            _clipSprites[c.Name] = frames;
        }
    }

    private bool CanAnimate() => isActiveAndEnabled && gameObject.activeInHierarchy;
    private Coroutine StartAnimSafe(IEnumerator routine) => CanAnimate() ? StartCoroutine(routine) : null;

    public void Play(string clipName, float fpsOverride = -1.0f)
    {
        if (!ValidateClip(clipName) || !CanAnimate()) return;
        StopCurrent();
        _isPlayingOnce = false;
        _currentClip = clipName;
        _currentFps = (fpsOverride > 0.0f) ? fpsOverride : _clipLookup[clipName].FPS;
        _playRoutine = StartAnimSafe(CoPlayLoop(clipName));
    }

    public void PlayOnce(string clipName, Action onComplete = null, float fpsOverride = -1.0f)
    {
        if (!ValidateClip(clipName) || !CanAnimate()) return;
        StopCurrent();
        _isPlayingOnce = true;
        _currentClip = clipName;
        _currentFps = (fpsOverride > 0.0f) ? fpsOverride : _clipLookup[clipName].FPS;
        _playRoutine = StartAnimSafe(CoPlayOnce(clipName, onComplete));
    }

    // 크기 조절(비주얼만)
    public void SetVisualScale(float scale)
    {
        scale = Mathf.Max(0.1f, scale);
        if (spriteRenderer) spriteRenderer.transform.localScale = Vector3.one * scale;
        if (uiImage) uiImage.rectTransform.localScale = Vector3.one * scale;
    }

    public void StopCurrent()
    {
        if (_playRoutine != null) { StopCoroutine(_playRoutine); _playRoutine = null; }
        _currentClip = null;
        _isPlayingOnce = false;
    }

    public void SetFlipX(bool flip)
    {
        if (spriteRenderer) spriteRenderer.flipX = flip;
        if (uiImage)
        {
            var t = uiImage.rectTransform.localScale;
            t.x = Mathf.Abs(t.x) * (flip ? -1.0f : 1.0f);
            uiImage.rectTransform.localScale = t;
        }
    }

    private IEnumerator CoPlayLoop(string clipName)
    {
        if (!CanAnimate()) yield break;

        var def = _clipLookup[clipName];
        var frames = _clipSprites[clipName];
        float frameTime = 1.0f / Mathf.Max(1.0f, _currentFps);

        _currentFrame = 0;
        while (true)
        {
            SetSprite(frames[_currentFrame]);

            _currentFrame = (_currentFrame + 1) % frames.Length;
            if (!def.Loop && _currentFrame == 0) yield break;

            yield return new WaitForSeconds(frameTime);
        }
    }

    private IEnumerator CoPlayOnce(string clipName, Action onComplete)
    {
        if (!CanAnimate()) yield break;

        var frames = _clipSprites[clipName];
        float frameTime = 1.0f / Mathf.Max(1.0f, _currentFps);

        for (_currentFrame = 0; _currentFrame < frames.Length; _currentFrame++)
        {
            SetSprite(frames[_currentFrame]);
            yield return new WaitForSeconds(frameTime);
        }

        _playRoutine = null;
        _isPlayingOnce = false;
        _currentClip = null;
        onComplete?.Invoke();
    }

    private void SetSprite(Sprite s)
    {
        if (spriteRenderer) spriteRenderer.sprite = s;
        if (uiImage) uiImage.sprite = s;
    }

    public void SetClipSet(FlipbookClipSet set)
    {
        clipSet = set;
        BuildCaches();
#if UNITY_EDITOR
        foreach (var kv in _clipSprites)
            Debug.Log($"[Flipbook] Loaded '{kv.Key}' frames={kv.Value?.Length}", this);
#endif
    }

    private bool ValidateClip(string name)
    {
        if (!_clipSprites.ContainsKey(name) || !_clipLookup.ContainsKey(name))
        {
            Debug.LogWarning($"[Flipbook] 클립 '{name}' 없음 또는 프레임 비어있음", this);
            return false;
        }
        return _clipSprites[name] != null && _clipSprites[name].Length > 0;
    }
}
