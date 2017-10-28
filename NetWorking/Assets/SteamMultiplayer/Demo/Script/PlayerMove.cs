
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float Speed=3;

	void Update () {
	    if (Input.GetKey(KeyCode.A))
	    {
	        transform.Translate(new Vector3(-Speed*Time.deltaTime,0,0));
	    }
	    if (Input.GetKey(KeyCode.D))
	    {
	        transform.Translate(new Vector3(Speed * Time.deltaTime, 0, 0));
	    }
	    if (Input.GetKey(KeyCode.W))
	    {
	        transform.Translate(new Vector3(0, Speed * Time.deltaTime,  0));
	    }
	    if (Input.GetKey(KeyCode.S))
	    {
	        transform.Translate(new Vector3(0, -Speed * Time.deltaTime, 0));
	    }
    }
}
