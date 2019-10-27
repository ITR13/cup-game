using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Play : MonoBehaviour
{
    public event Action OnPlayClicked;

    private void OnMouseDown()
    {
        OnPlayClicked?.Invoke();
    }

    public void Show()
    {
        GetComponent<Renderer>().enabled = true;
        GetComponent<Collider2D>().enabled = true;
    }

    public void Hide()
    {
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
    }
}
