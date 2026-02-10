using UnityEngine;

public class OpenTutorial : MonoBehaviour
{
    public GameObject tutorial;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            tutorial.gameObject.SetActive(true);
            this.gameObject.SetActive(false);
        }
    }
}
