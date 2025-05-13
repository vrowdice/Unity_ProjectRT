using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSetAcitveBtn : MonoBehaviour
{
    public void Click(GameObject argObject)
    {
        if (argObject.activeSelf)
        {
            argObject.SetActive(false);
        }
        else
        {
            argObject.SetActive(true);
        }
    }
}
