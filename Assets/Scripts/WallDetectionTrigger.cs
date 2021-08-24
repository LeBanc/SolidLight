using UnityEngine;

public class WallDetectionTrigger : MonoBehaviour
{
    public delegate void WallTriggerEventHandler();
    public event WallTriggerEventHandler OnJump;

    private Collider2D col2D;

    private void Start()
    {
        col2D = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(col2D.IsTouchingLayers(LayerMask.GetMask("Obstacle")))
        {
            OnJump?.Invoke();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        OnTriggerEnter2D(collision);
    }
}
