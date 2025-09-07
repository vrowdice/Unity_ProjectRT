using System;
using System.Collections.Generic;
using UnityEngine;

// 프리팹이 물고 다닐 "플립북 클립 세트"
[CreateAssetMenu(fileName = "FlipbookClipSet", menuName = "Battle/Animation/Flipbook Clip Set")]
public class FlipbookClipSet : ScriptableObject
{
    [Header("에디터 스캔 대상 폴더(프로젝트 상대 경로)")]
    public string assetFolder = "Assets/Image/Unit";

    [Serializable]
    public class ClipDef
    {
        [Tooltip("코드에서 재생할 이름 (예: Standing, Move, Attack)")]
        public string Name = "Standing";

        [Header("이름 규칙")]
        [Tooltip("스프라이트 이름 접두사 (예: standing_ / move_ / attack_)")]
        public string Prefix = "standing_";
        [Tooltip("Prefix 뒤에는 0부터 시작하는 정수 인덱스가 붙어야 함 (standing_0, standing_1, ...)")]
        public bool UseExplicitIndexRange = false;
        public int MinIndex = 0;
        public int MaxIndex = 0;

        [Header("프레임 목록")]
        public List<Sprite> Sprites = new List<Sprite>();

        [Header("재생 옵션")]
        [Min(1)] public int FPS = 10;
        public bool Loop = true;

        [Header("프레임 이벤트")]
        [Tooltip("특정 프레임에서 히트/사운드 등 이벤트 처리용 간단 훅")]
        public List<int> EventFrames = new List<int>();
    }

    [Header("클립들")]
    public List<ClipDef> Clips = new List<ClipDef>();
}
