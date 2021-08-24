using UnityEngine;

public class GroundTrigger : MonoBehaviour
{
    public delegate void GroundTriggerEventHandler();
    public event GroundTriggerEventHandler OnGrounded;
    public event GroundTriggerEventHandler OnAir;

    private Collider2D col2D;

    private void Start()
    {
        col2D = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(col2D.IsTouchingLayers(LayerMask.GetMask("Obstacle")))
        {
            OnGrounded?.Invoke();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        OnTriggerEnter2D(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!col2D.IsTouchingLayers(LayerMask.GetMask("Obstacle")))
        {
            OnAir?.Invoke();
        }
    }
}
