using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// SO에 저장된 Sprite 배열만으로 플립북 재생
[DisallowMultipleComponent]
public class SpriteFlipbookAnimatorSO : MonoBehaviour
{
    [Header("재생 정의")]
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
        // 비활성화 시 재생 정리(유령 코루틴 방지)
        StopCurrent();
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
                Debug.LogWarning($"[SpriteFlipbookAnimatorSO] '{c.Name}' 프레임 비어있음. 에디터에서 스캔 필요", this);
                continue;
            }
            _clipSprites[c.Name] = frames;
        }
    }

    private bool CanAnimate() => isActiveAndEnabled && gameObject.activeInHierarchy;
    private Coroutine StartAnimSafe(IEnumerator routine) => CanAnimate() ? StartCoroutine(routine) : null;

    public void Play(string clipName, float fpsOverride = -1.0f)
    {
        if (!ValidateClip(clipName)) return;
        if (!CanAnimate()) return;            // 비활성 시 무시
        StopCurrent();
        _isPlayingOnce = false;
        _currentClip = clipName;
        _currentFps = fpsOverride > 0.0f ? fpsOverride : _clipLookup[clipName].FPS;
        _playRoutine = StartAnimSafe(CoPlayLoop(clipName));
    }

    public void PlayOnce(string clipName, Action onComplete = null, float fpsOverride = -1.0f)
    {
        if (!ValidateClip(clipName)) return;
        if (!CanAnimate()) return;            // 비활성 시 무시
        StopCurrent();
        _isPlayingOnce = true;
        _currentClip = clipName;
        _currentFps = fpsOverride > 0.0f ? fpsOverride : _clipLookup[clipName].FPS;
        _playRoutine = StartAnimSafe(CoPlayOnce(clipName, onComplete));
    }

    // 프레임 수 얻기
    public int GetFrameCount(string clipName)
    {
        return _clipSprites.TryGetValue(clipName, out var fr) && fr != null ? fr.Length : 0;
    }

    public void PlayOnceDuration(string clipName, float durationSec, System.Action onComplete = null)
    {
        int frames = GetFrameCount(clipName);
        float fps = -1f;
        if (durationSec > 0f && frames > 0)
            fps = Mathf.Clamp(frames / durationSec, 6f, 30f);
        PlayOnce(clipName, onComplete, fpsOverride: fps);
    }

    // (선택) 재시작 보장 버전
    public void PlayOnceDurationRestart(string clipName, float durationSec, System.Action onComplete = null)
    {
        StopCurrent();
        PlayOnceDuration(clipName, durationSec, onComplete);
    }

    // 크기 조절
    public void SetVisualScale(float scale)
    {
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
            t.x = Mathf.Abs(t.x) * (flip ? -1 : 1);
            uiImage.rectTransform.localScale = t;
        }
    }

    public void SetFpsForCurrent(float fps)
    {
        if (fps > 0f) _currentFps = fps;
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
            FireSimpleEventIfAny(def, _currentFrame);

            _currentFrame = (_currentFrame + 1) % frames.Length;

            if (!def.Loop && _currentFrame == 0) yield break;
            yield return new WaitForSeconds(frameTime);
        }
    }

    private IEnumerator CoPlayOnce(string clipName, Action onComplete)
    {
        if (!CanAnimate()) yield break;

        var def = _clipLookup[clipName];
        var frames = _clipSprites[clipName];
        float frameTime = 1.0f / Mathf.Max(1f, _currentFps);

        for (_currentFrame = 0; _currentFrame < frames.Length; _currentFrame++)
        {
            SetSprite(frames[_currentFrame]);
            FireSimpleEventIfAny(def, _currentFrame);
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

    private void FireSimpleEventIfAny(FlipbookClipSet.ClipDef def, int frame)
    {
        // 이벤트 프레임 기능을 쓰려면: def.EventFrames.Contains(frame) 체크 후 Invoke
    }

    public void SetClipSet(FlipbookClipSet set)
    {
        clipSet = set;
        BuildCaches();
        LogLoadedClips();
    }

    private void LogLoadedClips()
    {
#if UNITY_EDITOR
        foreach (var kv in _clipSprites)
            Debug.Log($"[Flipbook] Loaded clip '{kv.Key}' frames={kv.Value?.Length}", this);
#endif
    }

    private bool ValidateClip(string name)
    {
        if (!_clipSprites.ContainsKey(name) || !_clipLookup.ContainsKey(name))
        {
            Debug.LogWarning($"[SpriteFlipbookAnimatorSO] 클립 미존재: '{name}'. SO에 Name이 정확히 '{name}'인지, Sprites가 채워졌는지 확인.", this);
            return false;
        }
        if (_clipSprites[name] == null || _clipSprites[name].Length == 0)
        {
            Debug.LogWarning($"[SpriteFlipbookAnimatorSO] 클립 '{name}'의 프레임이 0개입니다.", this);
            return false;
        }
        return true;
    }
}
