using UnityEngine;

public class CloseTutorial : MonoBehaviour
{
    public GameObject tutorial;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            tutorial.gameObject.SetActive(false);
            this.gameObject.SetActive(false);
        }
    }
}
