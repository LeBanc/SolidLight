using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallJumpDetectionTrigger : MonoBehaviour
{
    public delegate void WallTriggerEventHandler();
    public event WallTriggerEventHandler OnHang;
    public event WallTriggerEventHandler OnFall;

    private Collider2D col2D;

    private void Start()
    {
        col2D = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (col2D.IsTouchingLayers(LayerMask.GetMask("Trigger")))
        {
            OnHang?.Invoke();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!col2D.IsTouchingLayers(LayerMask.GetMask("Trigger")))
        {
            OnFall?.Invoke();
        }
    }
}
