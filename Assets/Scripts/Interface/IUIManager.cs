using UnityEngine;

public interface IUIManager
{
    Transform CanvasTrans { get; }

    void SetAllResourceText();

    void Initialize();
}
