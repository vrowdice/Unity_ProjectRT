using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BarrackUnitDetailPanel : MonoBehaviour
{
    [Header("Unit Header Area")]
    [SerializeField] private Image m_unitImage;              // UnitImage (illustration/large image)
    [SerializeField] private Image m_factionImage;           // UnitImage/FactionImage
    [SerializeField] private TextMeshProUGUI m_unitText;     // UnitImage/UnitText (name)
    [SerializeField] private TextMeshProUGUI m_unitClassText;// UnitImage/UnitClassText
    [SerializeField] private TextMeshProUGUI m_useFoodText;  // UnitImage/UseFoodText

    [Header("Panels")]
    [SerializeField] private GameObject m_abilityPanel;          // AbilityPanel
    [SerializeField] private GameObject m_researchPanel;  // ResearchPanel

    [Header("AbilityPanel/AbilityPanelContent Texts")]
    [SerializeField] private TextMeshProUGUI m_atkText;       // AtkText
    [SerializeField] private TextMeshProUGUI m_defText;       // DefText
    [SerializeField] private TextMeshProUGUI m_hpText;        // HPText
    [SerializeField] private TextMeshProUGUI m_maxMPText;     // MaxMPText
    [SerializeField] private TextMeshProUGUI m_recoverMPText; // RecoverMPText
    [SerializeField] private TextMeshProUGUI m_secPerMPText;  // SecPerMPText
    [SerializeField] private TextMeshProUGUI m_langeText;     // LangeText (attack range)

    [Header("SkillPanel Texts")]
    [SerializeField] private TextMeshProUGUI m_basicAtkText;  // BasicAtkText
    [SerializeField] private TextMeshProUGUI m_atkSkillText;  // AtkSkillText
    [SerializeField] private TextMeshProUGUI m_passiveText;   // PassiveText

    private UnitData m_currentUnit;

    void Start()
    {
        gameObject.SetActive(false);
    }

    public void Show(UnitData unit)
    {
        if (unit == null)
        {
            Debug.LogWarning("BarrackUnitDetailPanel.Show called with null UnitData");
            return;
        }

        m_currentUnit = unit;
        UpdateUI(unit);
        gameObject.SetActive(true);
        // Default to ability panel active
        SetTabByIndex(0);
    }

    private void UpdateUI(UnitData unit)
    {
        if (m_unitText != null) m_unitText.text = unit.unitName;
        if (m_unitImage != null)
        {
            m_unitImage.sprite = unit.unitIllustration != null ? unit.unitIllustration : unit.unitIcon;
            m_unitImage.preserveAspect = true;
        }
        if (m_factionImage != null)
        {
            // If there is a separate faction icon, replace it; currently show unit icon instead
            m_factionImage.sprite = unit.unitIcon;
        }
        if (m_unitClassText != null) m_unitClassText.text = unit.unitTagType.ToString();
        if (m_useFoodText != null) m_useFoodText.text = string.Empty;

        // Ability stats mapping
        if (m_atkText != null) m_atkText.text = Mathf.RoundToInt(unit.attackPower).ToString();
        if (m_defText != null) m_defText.text = Mathf.RoundToInt(unit.defensePower).ToString();
        if (m_hpText != null) m_hpText.text = Mathf.RoundToInt(unit.maxHealth).ToString();
        if (m_maxMPText != null) m_maxMPText.text = Mathf.RoundToInt(unit.maxMana).ToString();
        if (m_recoverMPText != null) m_recoverMPText.text = unit.manaRecoveryOnAttack.ToString("0.##");
        if (m_secPerMPText != null) m_secPerMPText.text = unit.manaRecoveryPerSecond.ToString("0.##");
        if (m_langeText != null) m_langeText.text = unit.attackRange.ToString("0.##");

        // Skill descriptions (single line)
        if (m_basicAtkText != null)
        {
            var dps = unit.attackSpeed > 0 ? unit.damageCoefficient * unit.attackCount * unit.attackSpeed : 0f;
            m_basicAtkText.text = $"Basic Attack: {unit.attackCount} hits, Coeff {unit.damageCoefficient:0.##}, APS {unit.attackSpeed:0.##}, est. DPS {dps:0.##}";
        }
        if (m_atkSkillText != null)
        {
            m_atkSkillText.text = string.IsNullOrEmpty(unit.active.displayName)
                ? "Active: No info"
                : $"Active '{unit.active.displayName}': Cost {unit.active.manaCost:0.##}, Coeff {unit.active.damageCoeff:0.##}, Range {unit.active.multiTargetRange:0.##}";
        }
        if (m_passiveText != null)
        {
            m_passiveText.text = string.IsNullOrEmpty(unit.passive.passiveLogic)
                ? "Passive: No info"
                : $"Passive: Effect {unit.passive.passiveLogic} (Coeff {unit.passive.passiveCoeff1:0.##})";
        }
    }

    // Call this from a toggle button, passing an int parameter (0/1)
    public void OnClickTabToggle(int tabIndex)
    {
        SetTabByIndex(tabIndex);
    }

    private void SetTabByIndex(int tabIndex)
    {
        // 0: Ability (stats/skills) panel, 1: Research panel
        bool isAbility = tabIndex == 0;
        if (m_abilityPanel != null) m_abilityPanel.SetActive(isAbility);
        if (m_researchPanel != null) m_researchPanel.SetActive(!isAbility);
    }
}
