using UnityEngine;

public class Logger : MonoBehaviour
{
    public void Log(string str)
    {
        Debug.Log($"{Time.time}: {str}");
    }
}
