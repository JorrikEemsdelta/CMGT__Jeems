using System.Collections;
using UnityEngine;

// This custom class lets us group multiple waypoints together in the Inspector
[System.Serializable]
public class WalkRoute1
{
    public string routeName = "New Route";
    [Tooltip("Drag your waypoint empty GameObjects here in the order the character should walk them.")]
    public Transform[] waypoints;
    
    [Space(5)]
    [Tooltip("If true, the character will turn to match the blue arrow (Forward Z-axis) of the FINAL waypoint when they stop.")]
    public bool faceFinalWaypointDirection = true;
}

public class CharacterMover : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag the Animator component here.")]
    public Animator characterAnimator;

    [Header("Settings")]
    [Tooltip("How fast the character moves forward.")]
    public float walkSpeed = 3f;
    [Tooltip("How fast the character turns when facing a new waypoint.")]
    public float turnSpeed = 5f; 

    [Space(10)]
    [Header("Route Setup")]
    [Tooltip("Create multiple routes here. Button 1 will call Route 0, Button 2 calls Route 1, etc.")]
    public WalkRoute[] availableRoutes;

    // UI Buttons will call this function and pass a number (0, 1, 2, etc.)
    public void StartWalkingRoute(int routeIndex)
    {
        // Safety check to make sure the route actually exists
        if (routeIndex < 0 || routeIndex >= availableRoutes.Length)
        {
            Debug.LogWarning("Route index " + routeIndex + " does not exist!");
            return;
        }

        // Stop current movement and start the new route
        StopAllCoroutines(); 
        StartCoroutine(FollowRouteRoutine(availableRoutes[routeIndex]));
    }

    private IEnumerator FollowRouteRoutine(WalkRoute route)
    {
        // Don't do anything if there are no points in the route
        if (route.waypoints.Length == 0) yield break;

        // Trigger the walk animation
        if (characterAnimator != null)
        {
            characterAnimator.SetBool("isWalking", true);
        }

        // Loop through every single waypoint in the route, one by one
        foreach (Transform waypoint in route.waypoints)
        {
            if (waypoint == null) continue;

            // Keep moving until we are close enough to the current waypoint
            while (Vector3.Distance(transform.position, waypoint.position) > 0.05f)
            {
                // 1. Smoothly rotate towards the current waypoint
                Vector3 direction = (waypoint.position - transform.position).normalized;
                direction.y = 0; // Ignore height so the character doesn't tilt upward
                
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
                }

                // 2. Move forward towards the waypoint
                transform.position = Vector3.MoveTowards(
                    transform.position, 
                    waypoint.position, 
                    walkSpeed * Time.deltaTime
                );

                // Wait for the next frame
                yield return null; 
            }

            // Snap exactly to the waypoint position to prevent drifting off-course over time
            transform.position = waypoint.position;
        }

        // --- NEW: Face the final direction ---
        if (route.faceFinalWaypointDirection)
        {
            Transform finalWaypoint = route.waypoints[route.waypoints.Length - 1];
            
            // Ignore the Y tilt of the waypoint so the character doesn't look up/down into the floor
            Vector3 finalEuler = finalWaypoint.eulerAngles;
            Quaternion flatFinalRotation = Quaternion.Euler(0, finalEuler.y, 0);

            // Smoothly pivot in place until the character matches the final waypoint's rotation
            while (Quaternion.Angle(transform.rotation, flatFinalRotation) > 0.5f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, flatFinalRotation, turnSpeed * Time.deltaTime);
                yield return null;
            }
            
            // Snap the rotation perfectly at the end
            transform.rotation = flatFinalRotation;
        }

        // We finished the loop and the final turn, trigger Idle animation
        if (characterAnimator != null)
        {
            characterAnimator.SetBool("isWalking", false);
        }
    }
}