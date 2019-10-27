using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Playables;

public class GameController : MonoBehaviour
{
    [SerializeField] private Cup[] cups;
    [SerializeField] private Play play;
    [SerializeField] private TextMesh text;

    private int _moves = 5;
    private int _wins = 0;
    private int _losses = 0;
    private float _speed = 0.5f;

    private void Reset()
    {
        cups = GetComponentsInChildren<Cup>();
        play = GetComponentInChildren<Play>();
        text = GetComponentInChildren<TextMesh>();
    }

    private void Start()
    {
        play.Show();
        play.OnPlayClicked += StartGame;
        foreach (var cup in cups)
        {
            cup.SetVisible(true);
        }

        text.text = "Press Play\nto Play!";
    }

    private void StartGame()
    {
        play.Hide();
        _moves++;
        text.text = "";
        StartCoroutine(PlayGame(_speed));
    }

    private IEnumerator PlayGame(float time)
    {
        var startAnim =
            cups.Select(cup => StartCoroutine(cup.Blink())).ToArray();
        foreach (var coroutine in startAnim)
        {
            yield return coroutine;
        }

        for (var i = 0; i < _moves; i++)
        {
            var firstR = Random.Range(0, cups.Length);
            var secondR = (firstR + Random.Range(1, cups.Length)) % cups.Length;

            yield return Swap(cups[firstR], cups[secondR], time);
        }


        yield return new WaitForSeconds(_speed);


        var correct = cups[Random.Range(0, cups.Length)];
        var name = correct.gameObject.name;
        text.text =
            $"Press the\n<color={name}>{name}</color>\n cup!";


        bool? won = null;

        void Select(Color color)
        {
            won = color == correct.Color;
        }

        foreach (var cup in cups)
        {
            cup.OnClick += Select;
        }

        while (won == null)
        {
            yield return null;
        }


        foreach (var cup in cups)
        {
            cup.SetVisible(true);
            cup.OnClick -= Select;
        }

        // ReSharper disable once PossibleInvalidOperationException
        if (won.Value)
        {
            _wins++;
            _speed *= 0.9f;
            text.text = $"You won!\n{_wins} wins\n{_losses} losses";
        }
        else
        {
            _losses++;
            _speed *= 1.1f;
            text.text = $"You lost!\n{_wins} wins\n{_losses} losses";
        }

        play.Show();
    }

    private IEnumerator Swap(Cup first, Cup second, float time)
    {
        var firstPosition = first.transform.position;
        var secondPosition = second.transform.position;

        var firstCoroutine = 
            StartCoroutine(first.MoveTo(secondPosition, time));
        var secondCoroutine =
            StartCoroutine(second.MoveTo(firstPosition, time));

        yield return firstCoroutine;
        yield return secondCoroutine;
        yield return new WaitForSeconds(time / 10f);
    }
}
