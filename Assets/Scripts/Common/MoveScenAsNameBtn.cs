using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveScenAsNameBtn : MonoBehaviour
{
    public void Click(string arsScenName)
    {
        GameManager.Instance.LoadScene(arsScenName);
    }
}
