using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBuildingData", menuName = "Building Data")]
public class BuildingData : ScriptableObject
{
    [System.Serializable]
    public class ResourceAmount
    {
        public ResourceType resource;
        public int amount;
    }

    [HideInInspector]
    public string m_code;
    public string m_name;
    [TextArea] public string m_description;

    public List<ResourceAmount> m_productionList = new List<ResourceAmount>();
}
