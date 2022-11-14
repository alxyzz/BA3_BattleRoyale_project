using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class Timer : MonoBehaviour
{
    
    public float timeDuration;

    private float timer;

    [SerializeField] private TextMeshProUGUI firstMinute;
    [SerializeField] private TextMeshProUGUI secondMinute;
    [SerializeField] private TextMeshProUGUI divider;
    [SerializeField] private TextMeshProUGUI firstSecond;
    [SerializeField] private TextMeshProUGUI secondSecond;

    // Start is called before the first frame update
    void Start()
    {
        
        ResetTimer();
    }

    // Update is called once per frame
    void Update()
    {
        //timeDuration = zoneBehaviour.GetComponent<ZoneBehaviour>().pauseTime;

        if (timer > 0)
        {
            timer -= Time.deltaTime;
            UpdateTimerToDisplay(timer);
        }
        else
        {
            Flash();
        }
    }

    private void Flash()
    {
        if (timer != 0)
        {
            timer = 0;
            UpdateTimerToDisplay(timer);
        }
    }

    public void ResetTimer()
    {
        timer = timeDuration;
    }

    private void UpdateTimerToDisplay(float time)
    {
        float minutes = Mathf.FloorToInt(time / 60);
        float seconds  = Mathf.FloorToInt(time % 60);

        string currentTime = string.Format("{00:00}{1:00}", minutes, seconds);

        firstMinute.text = currentTime[0].ToString();
        secondMinute.text = currentTime[1].ToString();
        firstSecond.text = currentTime[2].ToString();
        secondSecond.text = currentTime[3].ToString();
    }

    /*private void SetTextDisplay(bool enabled)
    {
        firstMinute.enabled = enabled;
        secondMinute.enabled = enabled;
        divider.enabled = enabled;
        firstSecond.enabled = enabled;
        secondSecond.enabled = enabled;
    }*/

    
}
