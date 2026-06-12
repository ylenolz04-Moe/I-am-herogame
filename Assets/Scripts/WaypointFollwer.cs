using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointFollwer : MonoBehaviour
{
    
    [SerializeField] private Transform[] waypoints;
    private int CurrentWaypointIndex = 0;
    [SerializeField] private float speed = 2f;
    

    // Update is called once per frame
    private void Update()
    {
        if(Vector2.Distance(waypoints[CurrentWaypointIndex].transform.position,transform.position) < 0.1f)
        {
            CurrentWaypointIndex++;
            if (CurrentWaypointIndex >= waypoints.Length)
            {
                CurrentWaypointIndex = 0;
            }
        }
        transform.position = Vector2.MoveTowards(transform.position, waypoints[CurrentWaypointIndex].transform.position, speed * Time.deltaTime);
    }
}
