using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class DebugText : MonoBehaviour
{
    private static DebugText instance;
    private Text debugText;    

    private void Start()
    {
        if(instance == null)
        {
            instance = this;
            debugText = GetComponent<Text>();
        }
        else
        {
            Destroy(this.gameObject);
            Debug.LogError("There is already a DebugText!");
        }        
    }

    public static void ChangeDebugText(string _text)
    {
        if(instance != null)
        {
            if(instance.debugText != null) instance.debugText.text = _text;
        }        
    }
}
