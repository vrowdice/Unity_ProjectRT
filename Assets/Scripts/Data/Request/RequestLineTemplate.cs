using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RequestLineTemplateData", menuName = "Request Line Template Data")]
public class RequestLineTemplate : ScriptableObject
{
    public RequestType.TYPE m_type = RequestType.TYPE.Battle;
    public string m_titleTemplate = string.Empty;
    public List<string> m_contentTemplates;
}
