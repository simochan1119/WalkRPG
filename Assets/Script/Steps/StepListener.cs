using UnityEngine;

public class StepListener : AndroidJavaProxy
{
    public int CurrentSteps { get; private set; }

    public StepListener()
        : base("android.hardware.SensorEventListener")
    {
    }

    public void onSensorChanged(AndroidJavaObject sensorEvent)
    {
        float[] values = sensorEvent.Get<float[]>("values");

        CurrentSteps = (int)values[0];

        Debug.Log("ÉZÉìÉTÅ[ï‡êî: " + CurrentSteps);
    }

    public void onAccuracyChanged(AndroidJavaObject sensor, int accuracy)
    {
    }
}