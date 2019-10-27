using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.Playables;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    [SerializeField] private Cup[] cups;
    [SerializeField] private Play play;
    [SerializeField] private TextMesh text;

    private Cup[] _currentCups = new Cup[0];
    private int _moves = 4;
    private int _score = 0;
    private int _lives = 3;
    private float _speed = 0.5f;

    private void Reset()
    {
        cups = GetComponentsInChildren<Cup>();
        play = GetComponentInChildren<Play>();
        text = GetComponentInChildren<TextMesh>();
        SetCups(cups.Length);
    }

    private void Start()
    {
        play.Show();
        play.OnPlayClicked += StartGame;
        SetCups(3);
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
            _currentCups.Select(cup => StartCoroutine(cup.Blink())).ToArray();
        foreach (var coroutine in startAnim)
        {
            yield return coroutine;
        }

        for (var i = 0; i < _moves; i++)
        {
            yield return RandomMove(time);
        }

        yield return new WaitForSeconds(_speed);


        var correct = _currentCups[Random.Range(0, _currentCups.Length)];
        var correctName = correct.gameObject.name;
        text.text =
            $"Press the\n<color={correctName}>{correctName}</color>\ncup!";


        bool? won = null;

        void Select(Color color)
        {
            won = color == correct.Color;
        }

        foreach (var cup in _currentCups)
        {
            cup.OnClick += Select;
        }

        while (won == null)
        {
            yield return null;
        }


        foreach (var cup in _currentCups)
        {
            cup.SetVisible(true);
            cup.OnClick -= Select;
        }

        // ReSharper disable once PossibleInvalidOperationException
        if (won.Value)
        {
            _score++;
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
            SetCups(3);
        }else if (_score % 7 == 0 && _currentCups.Length < cups.Length)
        {
            _speed = 0.5f;
            SetCups(_currentCups.Length + 1);
        }

        play.Show();
        Debug.Log($"Speed: {_speed}");
    }

    private IEnumerator RandomMove(float time)
    {
        if (Random.Range(0, 4) == 0)
        {
            yield return Swirl(time);
        }
        else
        {

            yield return Swap(time);
        }
    }

    private IEnumerator Swap(float time)
    {
        var firstR = Random.Range(0, _currentCups.Length);
        var secondR = Random.Range(1, _currentCups.Length);
        secondR += firstR;
        secondR %= _currentCups.Length;

        var first = _currentCups[firstR];
        var second = _currentCups[secondR];
        _currentCups[firstR] = second;
        _currentCups[secondR] = first;

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

    private IEnumerator Swirl(float time)
    {
        var reverse = Random.Range(0, 2) == 0;
        var positions = _currentCups
            .Select(cup => cup.transform.position)
            .ToArray();

        var routines = new Coroutine[_currentCups.Length];
        for (var i = 0; i < _currentCups.Length; i++)
        {
            var next = reverse ? i + positions.Length - 1 : i + 1;
            next %= positions.Length;

            SetZ(_currentCups[i].transform, positions[next].z);
            routines[i] = StartCoroutine(
                _currentCups[i].MoveTo(positions[next], time)
            );
        }

        if (reverse)
        {
            var temp = new Cup[_currentCups.Length];
            Array.Copy(_currentCups, 1, temp, 0, _currentCups.Length - 1);
            temp[_currentCups.Length - 1] = _currentCups[0];
            _currentCups = temp;
        }
        else
        {
            var temp = new Cup[_currentCups.Length];
            Array.Copy(_currentCups, 0, temp, 1, _currentCups.Length - 1);
            temp[0] = _currentCups[_currentCups.Length - 1];
            _currentCups = temp;
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

    private void SetCups(int count)
    {
        cups = _currentCups.Concat(cups.Skip(_currentCups.Length)).ToArray();

        _currentCups = cups.Take(count).ToArray();
        for (var i = 0; i < cups.Length; i++)
        {
            cups[i].gameObject.SetActive(i < count);
        }
    }
}
