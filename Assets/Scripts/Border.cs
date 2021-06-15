using UnityEngine;

/// <summary>
/// Border class defines the border of the map to destroy (or respawn) the character
/// </summary>
public class Border : MonoBehaviour
{
    /// <summary>
    /// OnCollisionEnter2D method removes the character objects
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Character"))
        {
            Destroy(collision.gameObject);
        }        
    }
}
