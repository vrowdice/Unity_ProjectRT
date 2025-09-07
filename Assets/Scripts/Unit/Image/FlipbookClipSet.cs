using System;
using System.Collections.Generic;
using UnityEngine;

// �������� ���� �ٴ� "�ø��� Ŭ�� ��Ʈ"
[CreateAssetMenu(fileName = "FlipbookClipSet", menuName = "Battle/Animation/Flipbook Clip Set")]
public class FlipbookClipSet : ScriptableObject
{
    [Header("������ ��ĵ ��� ����(������Ʈ ��� ���)")]
    public string assetFolder = "Assets/Image/Unit";

    [Serializable]
    public class ClipDef
    {
        [Tooltip("�ڵ忡�� ����� �̸� (��: Standing, Move, Attack)")]
        public string Name = "Standing";

        [Header("�̸� ��Ģ")]
        [Tooltip("��������Ʈ �̸� ���λ� (��: standing_ / move_ / attack_)")]
        public string Prefix = "standing_";
        [Tooltip("Prefix �ڿ��� 0���� �����ϴ� ���� �ε����� �پ�� �� (standing_0, standing_1, ...)")]
        public bool UseExplicitIndexRange = false;
        public int MinIndex = 0;
        public int MaxIndex = 0;

        [Header("������ ���")]
        public List<Sprite> Sprites = new List<Sprite>();

        [Header("��� �ɼ�")]
        [Min(1)] public int FPS = 10;
        public bool Loop = true;

        [Header("������ �̺�Ʈ")]
        [Tooltip("Ư�� �����ӿ��� ��Ʈ/���� �� �̺�Ʈ ó���� ���� ��")]
        public List<int> EventFrames = new List<int>();
    }

    [Header("Ŭ����")]
    public List<ClipDef> Clips = new List<ClipDef>();
}
