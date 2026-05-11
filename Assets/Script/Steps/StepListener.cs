using UnityEngine;

public class StepListener :
    AndroidJavaProxy
{
    public int CurrentSteps { get; private set; }

    public StepListener()
        : base("android.hardware.SensorEventListener")
    {
    }

    void onSensorChanged(AndroidJavaObject sensorEvent)
    {
        float[] values =
            sensorEvent.Get<float[]>("values");

        CurrentSteps = (int)values[0];
    }

    void onAccuracyChanged(
        AndroidJavaObject sensor,
        int accuracy
    )
    {
    }
}