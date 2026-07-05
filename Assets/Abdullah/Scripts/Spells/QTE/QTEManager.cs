using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class QTEManager : MonoBehaviour
{
    public static QTEManager Instance;

    [Header("UI References")]
    public GameObject qtePanel;
    public Transform buttonIconContainer;  // horizontal layout group
    public GameObject buttonIconPrefab;    // prefab with an Image + TMP label
    public TextMeshProUGUI gradeText;
    public TextMeshProUGUI timerText;

    [Header("QTE Settings")]
    public int sequenceLength = 5;
    public float timePerInput = 1.5f;

    // all keys that can appear in the sequence
    private readonly Key[] possibleKeys = new Key[]
    {
        Key.E, Key.R, Key.F, Key.G, Key.X, Key.V
    };

    private List<Key> sequence = new List<Key>();
    private int currentIndex = 0;
    private int correctCount = 0;
    private float inputTimer = 0f;
    private bool isRunning = false;
    private Action<QTEResult> onComplete;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        qtePanel.SetActive(false);
    }

    private void Update()
    {
        if (!isRunning) return;

        inputTimer -= Time.deltaTime;
        if (timerText != null)
            timerText.text = inputTimer.ToString("F1");

        // time ran out for this input
        if (inputTimer <= 0f)
            AdvanceSequence(false);

        // check key presses
        foreach (Key key in possibleKeys)
        {
            if (Keyboard.current[key].wasPressedThisFrame)
            {
                bool correct = key == sequence[currentIndex];
                AdvanceSequence(correct);
                break;
            }
        }
    }

    /// <summary>
    /// Start a QTE sequence. onComplete is called with the result when done.
    /// </summary>
    public void StartQTE(Action<QTEResult> onComplete, int length = -1)
    {
        if (isRunning) return;

        this.onComplete = onComplete;
        sequenceLength = length > 0 ? length : sequenceLength;

        GenerateSequence();
        StartCoroutine(RunQTE());
    }

    private void GenerateSequence()
    {
        sequence.Clear();
        for (int i = 0; i < sequenceLength; i++)
        {
            Key randomKey = possibleKeys[UnityEngine.Random.Range(0, possibleKeys.Length)];
            sequence.Add(randomKey);
        }
    }

    private IEnumerator RunQTE()
    {
        isRunning = true;
        currentIndex = 0;
        correctCount = 0;

        qtePanel.SetActive(true);
        BuildIconDisplay();

        inputTimer = timePerInput;

        // wait for Update() to handle inputs — coroutine just waits for finish
        yield return new WaitUntil(() => !isRunning);
    }

    private void BuildIconDisplay()
    {
        // clear old icons
        foreach (Transform child in buttonIconContainer)
            Destroy(child.gameObject);

        // spawn one icon per key in sequence
        foreach (Key key in sequence)
        {
            GameObject icon = Instantiate(buttonIconPrefab, buttonIconContainer);
            TextMeshProUGUI label = icon.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = key.ToString();

            // store reference so we can color it later
            icon.name = key.ToString();
        }
    }

    private void AdvanceSequence(bool wasCorrect)
    {
        if (wasCorrect)
        {
            correctCount++;
            HighlightIcon(currentIndex, Color.green);
        }
        else
        {
            HighlightIcon(currentIndex, Color.red);
        }

        currentIndex++;
        inputTimer = timePerInput;

        if (currentIndex >= sequence.Count)
            FinishQTE();
    }

    private void HighlightIcon(int index, Color color)
    {
        if (index >= buttonIconContainer.childCount) return;

        Transform icon = buttonIconContainer.GetChild(index);
        Image img = icon.GetComponent<Image>();
        if (img != null) img.color = color;
    }

    private void FinishQTE()
    {
        isRunning = false;
        QTEResult result = new QTEResult(correctCount, sequence.Count);

        // show grade briefly then hide
        StartCoroutine(ShowGradeAndClose(result));
    }

    private IEnumerator ShowGradeAndClose(QTEResult result)
    {
        if (gradeText != null)
        {
            gradeText.text = result.grade.ToString();
            gradeText.color = result.grade switch
            {
                QTEGrade.Perfect => Color.yellow,
                QTEGrade.Good    => Color.green,
                QTEGrade.Partial => Color.cyan,
                QTEGrade.Fail    => Color.red,
                _                => Color.white
            };
        }

        yield return new WaitForSeconds(1.2f);

        qtePanel.SetActive(false);
        onComplete?.Invoke(result);
    }
}