using UnityEngine;

class Timer : MonoBehaviour
{
    public float ResetTime = 5f; //5 second default why not?
    public bool IsRunning = false;
    public bool IsComplete = false;
    private float currentTime;

    void Update()
    {
        if (IsRunning) {
            currentTime -= Time.deltaTime;
            if (currentTime < 0f)
            {
                IsRunning = false;
                IsComplete = true;
            }
        }
    }

    public void StartTimer()
    {
        IsRunning = true;
        IsComplete = false;
        currentTime = ResetTime;
    }

    public bool CheckForDone() {
        if (IsComplete) {
            IsComplete = false; //They already know about it once.
            return true;
        }
        return false;
    }
}
