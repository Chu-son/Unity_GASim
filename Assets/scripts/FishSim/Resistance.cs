using UnityEngine;
using System.Collections;

public class Resistance : MonoBehaviour {

    private Rigidbody rigid;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }

	void FixedUpdate()
    {
        rigid.AddForce( -100 * rigid.velocity );
        //rigid.AddTorque(-100 * rigid.angularVelocity);
    }
}
