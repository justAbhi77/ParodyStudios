using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject TimerPanel;
    [SerializeField] TextMeshProUGUI TimerText;
    [SerializeField] GameObject GameOverPanel;
    [SerializeField] TextMeshProUGUI CubesText;

    [Space]
    [SerializeField] float CurrentTimeSec;
    [SerializeField] float MaxTimeMin;

    [Space]
    [SerializeField] float CubesCollected = 0;

    public UnityEvent OnTimeExhausted;

    float minutes, seconds; 

    bool GameOver = false;

    void Start()
    {
        MaxTimeMin *= 60;

        minutes = Mathf.FloorToInt(MaxTimeMin / 60);
        seconds = Mathf.FloorToInt(MaxTimeMin % 60);

        TimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        CubesText.text = string.Format("Cubes Collected : {0}", CubesCollected);

        TimerPanel.SetActive(true);
        GameOverPanel.SetActive(false);

        InvokeRepeating("Timer", 3, 1);
    }

    void Timer()
    {
        if (GameOver)
            return;

        if (CurrentTimeSec > MaxTimeMin)
        {
            PauseInputs();
            OnTimeExhausted.Invoke();
            return;
        }

        CurrentTimeSec += 1;

        minutes = Mathf.FloorToInt(CurrentTimeSec / 60);
        seconds = Mathf.FloorToInt(CurrentTimeSec % 60);

        TimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void PauseInputs()
    {
        GameOver = true;
        GameOverPanel.SetActive(true);
    }

    public void OnCubeCollected()
    {
        CubesCollected++;

        CubesText.text = string.Format("Cubes Collected : {0}", CubesCollected);
    }
}
