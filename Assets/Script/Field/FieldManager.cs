using UnityEngine;

public class FieldManager : MonoBehaviour
{
    void Start()
    {
        FirebaseManager.Instance.HealPlayerFull();
    }
}