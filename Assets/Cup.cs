using System;
using System.Collections;
using UnityEngine;

public class Cup : MonoBehaviour
{
    [SerializeField] private SpriteRenderer whiteCup;
    [SerializeField] private SpriteRenderer coloredCup;
    [SerializeField] private Color color;

    public event Action<Color> OnClick;
    public Color Color => color;

    private void Reset()
    {
        color = ColorUtility.TryParseHtmlString(
            gameObject.name, 
            out var htmlColor
        ) ? htmlColor : Color.black;

        whiteCup = transform.GetChild(0).GetComponent<SpriteRenderer>();
        coloredCup = transform.GetChild(1).GetComponent<SpriteRenderer>();
        
        SetVisible(true);
        coloredCup.color = color;
    }

    private void OnMouseDown()
    {
        Debug.Log($"Cup {color} was clicked");
        OnClick?.Invoke(color);
    }

    public IEnumerator Blink()
    {
        for (var i = 0; i < 2; i++)
        {
            SetVisible(false);
            yield return new WaitForSeconds(0.3f);
            SetVisible(true);
            yield return new WaitForSeconds(0.3f);
        }
        SetVisible(false);

        yield return new WaitForSeconds(0.1f);
    }

    public void SetVisible(bool colored)
    {
        whiteCup.enabled = !colored;
        coloredCup.enabled = colored;
    }

    public IEnumerator MoveTo(Vector3 endPosition, float time)
    {
        var startPosition = transform.position;
        for (var t = 0f; t < time; t += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(
                startPosition,
                endPosition,
                t / time
            );
            yield return null;
        }

        transform.position = endPosition;
    }
}
