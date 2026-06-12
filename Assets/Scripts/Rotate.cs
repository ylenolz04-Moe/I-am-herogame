using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private float rotateSpeed = 2f;
    // Update is called once per frame
    private void Update()
    {
        transform.Rotate(0,0,360*rotateSpeed*Time.deltaTime);
    }
}

