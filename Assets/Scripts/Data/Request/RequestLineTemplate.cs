using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RequestTemplate")]
public class RequestLineTemplate : ScriptableObject
{
    public string m_titleTemplate = string.Empty;
    public List<string> m_contentTemplates = new List<string>();
}
