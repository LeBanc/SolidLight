using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.InputSystem;

/// <summary>
/// PlayerController class defines the Player Controller (light)
/// </summary>
public class PlayerController : MonoBehaviour
{
    // private components
    private Transform maskTransform;
    private SpriteMask spriteMask;
    private CircleCollider2D lightCollider;
    private Light2D spotLight;

    // public parameters
    public float expendSpeed = 4f;
    public float lightMinRadius = 0.32f;
    public float lightMaxRadius = 2.1f;

    /// <summary>
    /// At Start, fetches the components and inits them 
    /// </summary>
    void Start()
    {
        lightCollider = GetComponentInChildren<CircleCollider2D>();
        spriteMask = GetComponentInChildren<SpriteMask>();
        spotLight = GetComponentInChildren<Light2D>();
        maskTransform = transform.GetChild(0);

        // Disable the mask and the collider
        lightCollider.enabled = false;
        spriteMask.enabled = false;

        // Set the size of the components
        spotLight.transform.localScale = lightMinRadius * Vector3.one;
        maskTransform.transform.localScale = Vector3.zero;

        // Hide the cursor
        Cursor.visible = false;
    }

    /// <summary>
    /// At Update, move the element at mouse position
    /// </summary>
    void Update()
    {
        transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }

    /// <summary>
    /// OnPress method is called when the mouse button is pressed to show the sprite mask and make it grow
    /// </summary>
    void OnPress()
    {
        StopAllCoroutines();
        spriteMask.enabled = true;
        lightCollider.enabled = true;
        StartCoroutine(ShowLight());
    }

    /// <summary>
    /// ShowLight coroutine makes the sprite mask and the light grow
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowLight()
    {
        float _size = maskTransform.transform.localScale.x;

        while(_size < 1f)
        {
            _size += expendSpeed * Time.deltaTime;
            maskTransform.transform.localScale = new Vector3(_size, _size, _size);
            spotLight.transform.localScale = Mathf.Lerp(lightMinRadius, lightMaxRadius, _size) * Vector3.one;
            yield return null;
        }
        maskTransform.transform.localScale = Vector3.one;
        spotLight.transform.localScale = lightMaxRadius * Vector3.one;
    }

    /// <summary>
    /// OnRelease method is called when the mouse button is released to shrink the sprite mask and hide it
    /// </summary>
    void OnRelease()
    {
        StopAllCoroutines();
        lightCollider.enabled = false;
        StartCoroutine(HideLight());
    }

    /// <summary>
    /// HideLight coroutine makes the sprite mask and the light shrink
    /// </summary>
    /// <returns></returns>
    IEnumerator HideLight()
    {
        float _size = maskTransform.transform.localScale.x;

        while (_size > 0f)
        {
            _size -= expendSpeed * Time.deltaTime;
            maskTransform.transform.localScale = new Vector3(_size, _size, _size);
            spotLight.transform.localScale = Mathf.Lerp(lightMinRadius, lightMaxRadius, _size) * Vector3.one;
            yield return null;
        }
        maskTransform.transform.localScale = Vector3.zero;
        spriteMask.enabled = false;
        spotLight.transform.localScale = lightMinRadius * Vector3.one;
    }
}
