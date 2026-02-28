using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Rendering.DebugUI;

public class Graple : MonoBehaviour
{
    Rigidbody _rb;
    [SerializeField]private GameObject _player;
    [SerializeField]private GameObject _target;
    public AudioSource clank;
    public AudioSource chainShot;
    public AudioSource chainLock;
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
        _rb = _player.GetComponent<Rigidbody>();

        StartCoroutine(GrapleShot());
        
    }
    

    //characterAnimator.SetBool(GrapleHash,true);


    //handles the graple going forwards when shot
    IEnumerator GrapleShot()
    {
        chainShot.Play();
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
        chainShot.Play();
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
        chainShot.Stop();
        this.gameObject.gameObject.SetActive(false);
    }
    IEnumerator DragPickup(GameObject Pickup)
    {
        chainShot.Play();
        if (_player == null)
        {
            Debug.Log("player was not added correctly");

            this.gameObject.SetActive(false);


        }
        while (Vector3.Distance(this.gameObject.transform.position, _player.transform.position) > 0.9f)
        {

            Debug.Log("returning");
            this.gameObject.transform.position = Vector3.MoveTowards(this.gameObject.transform.position, _player.transform.position, _travelSpeed * 2 * Time.deltaTime);
            Pickup.transform.position = this.transform.position;
            yield return null;
        }
        if (Vector3.Distance(this.gameObject.transform.position, _player.transform.position) <= 0.9f)
        {
            Debug.Log("Returned with pickup");
        }
        chainShot.Stop();
        this.gameObject.gameObject.SetActive(false);
    }
    IEnumerator GoToGraple(Transform graplelocation,Transform playerStartLocation,float lerpTimer)
    {
        //makes sure grapling is not chopy once built
        _rb.useGravity = false;
        
        chainShot.Play();
        if (_player == null)
        {
            Debug.Log("player was not added correctly");

            this.gameObject.SetActive(false);


        }
        while (Vector3.Distance(_player.transform.position, graplelocation.position) > 0.9f)
        {
            lerpTimer += Time.deltaTime;
            this.gameObject.transform.position = graplelocation.position;
            Debug.Log("Moving to graple");
            _rb.MovePosition( Vector3.MoveTowards(_player.transform.position, graplelocation.position, _travelSpeed * 1.2f * Time.deltaTime));
            yield return null;
        }
        if (Vector3.Distance(_player.transform.position, graplelocation.position) <= 0.9f)
        {
            Debug.Log("palyer reached point");
        }
        chainShot.Stop();
        
        _rb.useGravity = true;
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
            chainShot.Stop();
            Debug.Log("hit pickup");
            chainLock.Play();
            StartCoroutine(DragPickup(collision.gameObject));
        }
        else if (collision.gameObject.CompareTag("Pushable")|| collision.gameObject.CompareTag("GraplePoint"))
        {
            chainShot.Stop();
            Debug.Log("hit pushable");
            chainLock.Play();
            StartCoroutine(GoToGraple(collision.transform,_player.transform,0));
            
        }
        //returns garple if it hits something other than the player or not solid objects
        else 
        {
            chainShot.Stop();
            Debug.Log("hit " + collision.gameObject.name);
            clank.Play();
            StartCoroutine(GrapleReturn());
        }
    }
    
    
}
