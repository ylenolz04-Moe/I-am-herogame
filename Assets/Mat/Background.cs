using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    [SerializeField] private float offsetSpeed;
    private Material mat;
    private Vector2 offset;
    private void Start()
    {
        mat = GetComponent<Renderer>().material;
        offset = mat.mainTextureOffset;
    }
    private void Update()
    {
        offset.x += offsetSpeed * Time.deltaTime;
        mat.mainTextureOffset = offset;
    }

}
