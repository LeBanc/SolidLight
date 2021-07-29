using UnityEngine;

public class JumpTrigger : MonoBehaviour
{
    public delegate void JumpTriggerEventHandler();
    public event JumpTriggerEventHandler OnJump;

    private Collider2D col2D;

    private void Start()
    {
        col2D = GetComponent<Collider2D>();
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if(!col2D.IsTouchingLayers(LayerMask.GetMask("Trigger")))
        {
            OnJump?.Invoke();
        }
    }
}
