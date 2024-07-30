using System;
using UnityEngine;

public class DiceSide : MonoBehaviour
{
    bool onGround;
    public bool OnGround => onGround;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            onGround = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            onGround = false;
        }
    }
    public int SideValue()
    {
        int value = Int32.Parse(name);
        return value;
    }
}
