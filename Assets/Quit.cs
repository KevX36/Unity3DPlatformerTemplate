using UnityEngine;

public class Quit : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnExit()
    {
        Application.Quit();
        Debug.Log("tried to close game");
    }
}
