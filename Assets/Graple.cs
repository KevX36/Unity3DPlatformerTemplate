using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Rendering.DebugUI;

public class Graple : MonoBehaviour
{
    
    [SerializeField]private GameObject _player;
    [SerializeField]private GameObject _target;
    private Vector3 _direction;
    public float _range = 10;
    public float _travelSpeed = 5;
    
    //sets hook to this gameObject
    private void Start()
    {
        
    }
    // handles setting vriables for graple shot
    public void ShootGraple()
    {
        this.gameObject.transform.position = _player.transform.position;
        this.gameObject.transform.rotation = Quaternion.Euler(90, _player.transform.rotation.eulerAngles.y, 0);
        _direction = _player.transform.forward * _range;
        _direction = _direction + _player.transform.position;
        _target.transform.position = _direction;



        StartCoroutine(GrapleShot());
        
    }


    
    
    //handles the graple going forwards when shot
    IEnumerator GrapleShot()
    {
        
        if (_player == null)
        {
            Debug.Log("player was not added correctly");
            
            this.gameObject.SetActive(false);
            
            
        }
        while (Vector3.Distance(this.gameObject.transform.position, _target.transform.position) > 0.9f)
        {
            
            Debug.Log("shooting");
            this.gameObject.transform.position = Vector3.MoveTowards(this.gameObject.transform.position, _target.transform.position, _travelSpeed*Time.deltaTime);
            yield return null;
        }
        if(Vector3.Distance(this.gameObject.transform.position, _target.transform.position) <= 0.9f)
        {
            Debug.Log("reached point");
        }

        StartCoroutine(GrapleReturn());
    }
    IEnumerator GrapleReturn()
    {
        if (_player == null)
        {
            Debug.Log("player was not added correctly");

            this.gameObject.SetActive(false);


        }
        while (Vector3.Distance(this.gameObject.transform.position, _player.transform.position) > 0.9f)
        {

            Debug.Log("returning");
            this.gameObject.transform.position = Vector3.MoveTowards(this.gameObject.transform.position, _player.transform.position, _travelSpeed * 2 * Time.deltaTime);
            yield return null;
        }
        if (Vector3.Distance(this.gameObject.transform.position, _player.transform.position) <= 0.9f)
        {
            Debug.Log("reached player");
        }

        this.gameObject.gameObject.SetActive(false);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(_player == null)
        {
            Debug.Log("player was not added correctly");
            this.gameObject.SetActive(false);
            
        }
        
        
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Respawn") || collision.gameObject.CompareTag("Finish") || collision.gameObject.CompareTag("Water"))
        {
            
            return;
        }
        else if (collision.gameObject.CompareTag("Pickup"))
        {
            Debug.Log("hit pickup");
            //add drag back here
        }
        else if (collision.gameObject.CompareTag("Pushable"))
        {
            //add move to object here
            Debug.Log("hit pushable");
        }
        //returns garple if it hits something other than the player or not solid objects
        else 
        {
            Debug.Log("hit " + collision.gameObject.name);
            StartCoroutine(GrapleReturn());
        }
    }
    
    
}
