using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RequestLineTemplate")]
public class RequestLineTemplate : ScriptableObject
{
    public RequestType.TYPE m_type = RequestType.TYPE.Hunt;
    public string m_titleTemplate = string.Empty;
    public List<string> m_contentTemplates;
}
