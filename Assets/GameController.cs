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

    private int _moves = 4;
    private int _score = 0;
    private int _lives = 3;
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
            yield return RandomMove(time);
        }

        yield return new WaitForSeconds(_speed);


        var correct = cups[Random.Range(0, cups.Length)];
        var correctName = correct.gameObject.name;
        text.text =
            $"Press the\n<color={correctName}>{correctName}</color>\ncup!";


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

        _score++;
        // ReSharper disable once PossibleInvalidOperationException
        if (won.Value)
        {
            _speed *= 0.85f;
            text.text = $"You won!\nScore: {_score}\nLives: {_lives}";
        }
        else
        {
            _lives--;
            text.text = $"You lost!\nScore: {_score}\nLives: {_lives}";
        }

        if (_lives == 0)
        {
            var prevscore = PlayerPrefs.GetInt("highscore", 0);
            if (_score > prevscore)
            {
                PlayerPrefs.SetInt("highscore", _score);
                PlayerPrefs.Save();
            }

            text.text =
                $"Game over!\nFinal Score: {_score}\nHighscore: {prevscore}";

            _lives = 3;
            _score = 0;
            _speed = 0.5f;
            _moves = 4;
        }

        play.Show();
        Debug.Log($"Speed: {_speed}");
    }

    private IEnumerator RandomMove(float time)
    {
        if (Random.Range(0, 4) == 0)
        {
            yield return Swirl(Random.Range(0, 2) == 0, time);
        }
        else
        {
            var firstR = Random.Range(0, cups.Length);
            var secondR = Random.Range(1, cups.Length);
            secondR += firstR;
            secondR %= cups.Length;

            yield return Swap(cups[firstR], cups[secondR], time);
        }
    }

    private IEnumerator Swap(Cup first, Cup second, float time)
    {
        var firstPosition = first.transform.position;
        var secondPosition = second.transform.position;

        // ReSharper disable once Unity.InefficientPropertyAccess
        SetZ(first.transform, secondPosition.z);
        SetZ(second.transform, firstPosition.z);

        var firstCoroutine = 
            StartCoroutine(first.MoveTo(secondPosition, time));
        var secondCoroutine =
            StartCoroutine(second.MoveTo(firstPosition, time));

        yield return firstCoroutine;
        yield return secondCoroutine;

        yield return new WaitForSeconds(time / 10f);
    }

    private IEnumerator Swirl(bool reverse, float time)
    {
        var all = reverse ? cups.Reverse().ToArray() : cups;
        var positions = all.Select(cup => cup.transform.position).ToArray();

        var routines = new Coroutine[all.Length];
        for (var i = 0; i < all.Length; i++)
        {
            var next = (i + 1) % positions.Length;
            SetZ(all[i].transform, positions[next].z);

            routines[i] = StartCoroutine(
                all[i].MoveTo(positions[next], time)
            );
        }

        foreach (var coroutine in routines)
        {
            yield return coroutine;
        }
        yield return new WaitForSeconds(time / 10f);
    }

    private void SetZ(Transform t, float z)
    {
        var pos = t.position;
        pos.z = z;
        t.position = pos;
    }
}
