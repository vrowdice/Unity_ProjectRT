using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// SO�� ����� Sprite �迭������ �ø��� ���
[DisallowMultipleComponent]
public class SpriteFlipbookAnimatorSO : MonoBehaviour
{
    [Header("��� ����")]
    [SerializeField] private FlipbookClipSet clipSet;

    [Header("��� ���")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Image uiImage;

    private readonly Dictionary<string, Sprite[]> _clipSprites = new();
    private readonly Dictionary<string, FlipbookClipSet.ClipDef> _clipLookup = new();

    // ����
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
            Debug.LogError("[SpriteFlipbookAnimatorSO] SpriteRenderer�� UI.Image �ʿ�", this);

        BuildCaches();
    }

    private void OnDisable()
    {
        // ��Ȱ��ȭ �� ��� ����(���� �ڷ�ƾ ����)
        StopCurrent();
    }

    private void BuildCaches()
    {
        _clipSprites.Clear();
        _clipLookup.Clear();

        if (!clipSet)
        {
            Debug.LogError("[SpriteFlipbookAnimatorSO] ClipSet�� �������", this);
            return;
        }

        foreach (var c in clipSet.Clips)
        {
            if (string.IsNullOrEmpty(c.Name)) continue;
            _clipLookup[c.Name] = c;

            var frames = c.Sprites?.Where(s => s != null).ToArray();
            if (frames == null || frames.Length == 0)
            {
                Debug.LogWarning($"[SpriteFlipbookAnimatorSO] '{c.Name}' ������ �������. �����Ϳ��� ��ĵ �ʿ�", this);
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
        if (!CanAnimate()) return;            // ��Ȱ�� �� ����
        StopCurrent();
        _isPlayingOnce = false;
        _currentClip = clipName;
        _currentFps = fpsOverride > 0.0f ? fpsOverride : _clipLookup[clipName].FPS;
        _playRoutine = StartAnimSafe(CoPlayLoop(clipName));
    }

    public void PlayOnce(string clipName, Action onComplete = null, float fpsOverride = -1.0f)
    {
        if (!ValidateClip(clipName)) return;
        if (!CanAnimate()) return;            // ��Ȱ�� �� ����
        StopCurrent();
        _isPlayingOnce = true;
        _currentClip = clipName;
        _currentFps = fpsOverride > 0.0f ? fpsOverride : _clipLookup[clipName].FPS;
        _playRoutine = StartAnimSafe(CoPlayOnce(clipName, onComplete));
    }

    // ������ �� ���
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

    // (����) ����� ���� ����
    public void PlayOnceDurationRestart(string clipName, float durationSec, System.Action onComplete = null)
    {
        StopCurrent();
        PlayOnceDuration(clipName, durationSec, onComplete);
    }

    // ũ�� ����
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
        // �̺�Ʈ ������ ����� ������: def.EventFrames.Contains(frame) üũ �� Invoke
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
            Debug.LogWarning($"[SpriteFlipbookAnimatorSO] Ŭ�� ������: '{name}'. SO�� Name�� ��Ȯ�� '{name}'����, Sprites�� ä�������� Ȯ��.", this);
            return false;
        }
        if (_clipSprites[name] == null || _clipSprites[name].Length == 0)
        {
            Debug.LogWarning($"[SpriteFlipbookAnimatorSO] Ŭ�� '{name}'�� �������� 0���Դϴ�.", this);
            return false;
        }
        return true;
    }
}
