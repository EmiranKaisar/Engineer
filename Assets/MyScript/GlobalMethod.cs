using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalMethod
{
    public static void OperateUIDirection(GameObject obj, int toolDir)
    {
        if (toolDir <= 3)
        {
            obj.transform.rotation = Quaternion.AngleAxis(90 * toolDir, Vector3.forward);
        }
        else
        {
            obj.transform.rotation = Quaternion.AngleAxis(180, Vector3.up);
        }
    }
}
