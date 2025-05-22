using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRequestData", menuName = "Request Data")]
public class RequestData : ScriptableObject
{
    [HideInInspector]
    public string m_code;
    public string m_name;
    [TextArea] public string m_description;

    public int m_timeLimite;

    public List<int> m_condition = new List<int>();

    public RequestType m_type;
}
