using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AdaptiveCollider class change the physics collider of the block depending on its display (sprite mask)
/// </summary>
public class AdaptiveCollider : MonoBehaviour
{
    // Collider2D (trigger) to know if the collider of the light is over the block
    private Collider2D trigger;

    // Vector2 positions of the block limits
    private Vector2 leftBottom;
    private Vector2 leftUp;
    private Vector2 rightBottom;
    private Vector2 rightUp;

    // Physic Collider2D of the block
    private PolygonCollider2D physicsCollider;

    /// <summary>
    /// At Start, fetches the component
    /// </summary>
    private void Start()
    {
        trigger = GetComponent<Collider2D>();

        leftBottom = (Vector2)trigger.bounds.min;
        leftUp = (Vector2)trigger.bounds.min + new Vector2(0f, trigger.bounds.size.y);
        rightBottom = (Vector2)trigger.bounds.min + new Vector2(trigger.bounds.size.x, 0f);
        rightUp = (Vector2)trigger.bounds.max;

        physicsCollider = transform.GetComponentInChildren<PolygonCollider2D>();
        physicsCollider.enabled = false;
    }

    /// <summary>
    /// OnCollisionEnter2D method, sets the physics collider2D
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If the collision is with the light (Player tag)
        if(collision.collider.CompareTag("Player") || collision.collider.CompareTag("Light"))
        {
            List<Vector2> _boundaries = new List<Vector2>();
            RaycastHit2D hit;

            // Get Lower left limit(s)
            if (collision.collider.OverlapPoint(leftBottom))
            {
                _boundaries.Add(leftBottom);
            }
            else
            {
                // Check for intersection on bottom border
                hit = Physics2D.Raycast(leftBottom, Vector2.right, trigger.bounds.size.x, LayerMask.GetMask("Light"));
                if (hit.collider != null) _boundaries.Add(hit.point);

                // Check for intersection on left border
                hit = Physics2D.Raycast(leftBottom, Vector2.up, trigger.bounds.size.y, LayerMask.GetMask("Light"));
                if (hit.collider != null) _boundaries.Add(hit.point);
            }

            // Get Upper left limit(s)
            if (collision.collider.OverlapPoint(leftUp))
            {
                _boundaries.Add(leftUp);
            }
            else
            {
                // Check for intersection on left border
                hit = Physics2D.Raycast(leftUp, Vector2.down, trigger.bounds.size.y, LayerMask.GetMask("Light"));
                if (hit.collider != null) _boundaries.Add(hit.point);

                // Check for intersection on up border
                hit = Physics2D.Raycast(leftUp, Vector2.right, trigger.bounds.size.x, LayerMask.GetMask("Light"));
                if (hit.collider != null) _boundaries.Add(hit.point);
            }

            // Get Upper right limit(s)
            if (collision.collider.OverlapPoint(rightUp))
            {
                _boundaries.Add(rightUp);
            }
            else
            {
                // Check for intersection on left border
                hit = Physics2D.Raycast(rightUp, Vector2.left, trigger.bounds.size.x, LayerMask.GetMask("Light"));
                if (hit.collider != null) _boundaries.Add(hit.point);

                // Check for intersection on up border
                hit = Physics2D.Raycast(rightUp, Vector2.down, trigger.bounds.size.y, LayerMask.GetMask("Light"));
                if (hit.collider != null) _boundaries.Add(hit.point);
            }

            // Get Lower right limit(s)
            if (collision.collider.OverlapPoint(rightBottom))
            {
                _boundaries.Add(rightBottom);
            }
            else
            {
                // Check for intersection on left border
                hit = Physics2D.Raycast(rightBottom, Vector2.up, trigger.bounds.size.y, LayerMask.GetMask("Light"));
                if (hit.collider != null) _boundaries.Add(hit.point);

                // Check for intersection on up border
                hit = Physics2D.Raycast(rightBottom, Vector2.left, trigger.bounds.size.x, LayerMask.GetMask("Light"));
                if (hit.collider != null) _boundaries.Add(hit.point);
            }

            // If there are only 2 points in the boundaries, add the center of the block (to get at least a triangle)
            if (_boundaries.Count < 3) _boundaries.Add((Vector2)transform.position);

            // If there are more than 2 points in the boundaries, set the physic Collider as with all the points of the boundaries
            if(_boundaries.Count > 2)
            {
                List<Vector2> _physicsBoundaries = new List<Vector2>();
                _boundaries.ForEach(delegate (Vector2 x) { _physicsBoundaries.Add(transform.InverseTransformPoint(x)); });
                physicsCollider.enabled = true;
                physicsCollider.points = _physicsBoundaries.ToArray();
            }
            else
            {
                physicsCollider.enabled = false;
            }
        }
    }

    /// <summary>
    /// OnCollisionStay2D method calls the OnCollisionEnter2D one to set the physic collider
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionStay2D(Collision2D collision)
    {
        OnCollisionEnter2D(collision);
    }

    /// <summary>
    /// OnCollisionExit2D method disables the physic collider
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player") || collision.collider.CompareTag("Light"))
        {
            if (!trigger.IsTouchingLayers(LayerMask.GetMask("Light")))
            {
                physicsCollider.enabled = false;
            }
        }
    }
}
