using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private GameObject[] waypoints_1;
    [SerializeField] private GameObject[] waypoints_2;

    [SerializeField] private Transform target;
    [SerializeField] private Rigidbody2D targetRB;
    [SerializeField] private Rigidbody2D enemyRB;
    [SerializeField] private Rigidbody2D eagleRB;
    [SerializeField] private Rigidbody2D frogRB;
    [SerializeField] private Rigidbody2D opossumRB;
    [SerializeField] private SpriteRenderer enemyGFX;
    [SerializeField] private Animator frogAnim;
    [SerializeField] private Animator opossumAnim;
    [SerializeField] private BoxCollider2D frogColl;
    private enum movementState { idle, jump, fall, walk }

    private movementState frogState;
    private movementState opossumState;

    [SerializeField] private float speed = 200f;
    [SerializeField] private float nextWaypointDistance = 1f;
    [SerializeField] private float speed_OF = 3f;

    [SerializeField] private LayerMask jumpableGround;

    private Path path;
    private Vector2 direction;
    private Vector2 force;
    private float distance;
    private int currentWaypoint = 0;
    private int currentWaypoint_OF_1 = 0;
    private int currentWaypoint_OF_2 = 0;
    private bool reachedEndOfPath = false;

    private Seeker seeker;

    private void Start()
    {
        seeker = GetComponent<Seeker>();

        InvokeRepeating("UpdatePath", 0f, .5f);
    }

    private void Update()
    {

        if (gameObject.name.Equals("Opossum (1)") || (gameObject.name.Equals("Opossum (2)")))
        {
            UpdateAnimationOpossumState();
        }
        else if (gameObject.name.Equals("Frog (1)") || (gameObject.name.Equals("Frog (2)")))
        {
            UpdateAnimationFrogState();
        }

        if (distance > nextWaypointDistance && IsGrounded())
        { 
            frogRB.velocity = new Vector2 (frogRB.velocity.x, 10f);
        }

        if (Vector2.Distance(waypoints_1[currentWaypoint_OF_1].transform.position, opossumRB.transform.position) < .1f)
        {
            currentWaypoint_OF_1++;

            if (currentWaypoint_OF_1 >= waypoints_1.Length)
            {
                currentWaypoint_OF_1 = 0;
            }

        }
        else if (Vector2.Distance(waypoints_2[currentWaypoint_OF_2].transform.position, opossumRB.transform.position) < .1f)
        {
            currentWaypoint_OF_2++;

            if (currentWaypoint_OF_2 >= waypoints_2.Length)
            {
                currentWaypoint_OF_2 = 0;
            }

        }

        if (opossumRB.bodyType == RigidbodyType2D.Static)
        {
            opossumRB.constraints = RigidbodyConstraints2D.FreezePositionX;
        }
        else
        {
            if (gameObject.name.Equals("Opossum (1)"))
            {
                opossumRB.transform.position = Vector2.MoveTowards(opossumRB.transform.position, waypoints_1[currentWaypoint_OF_1].transform.position, Time.deltaTime * speed_OF);
            }
            else
            {
                opossumRB.transform.position = Vector2.MoveTowards(opossumRB.transform.position, waypoints_2[currentWaypoint_OF_2].transform.position, Time.deltaTime * speed_OF);
            }
        }

        opossumState = movementState.walk;
    }

    private void UpdatePath()
    {
        seeker.StartPath(enemyRB.position, target.position, OnPathComplete);
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    private void FixedUpdate()
    {
        if (path == null)
            return;

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else { reachedEndOfPath = false; }

        direction = ((Vector2)path.vectorPath[currentWaypoint] - enemyRB.position).normalized;
        force = direction * speed * Time.deltaTime;

        enemyRB.AddForce(force);

        distance = Vector2.Distance(enemyRB.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        if (force.x >= 0.01f)
        {
            enemyGFX.flipX = true;
        }
        else if (force.x <= -0.01f)
        {
            enemyGFX.flipX = false;
        }

        if (targetRB.bodyType == RigidbodyType2D.Static)
        {
            eagleRB.bodyType = RigidbodyType2D.Static;
            frogRB.bodyType = RigidbodyType2D.Static;
            opossumRB.bodyType = RigidbodyType2D.Static;
        }
    }

    private void UpdateAnimationFrogState()
    {
        if (frogRB.velocity.y > .1f)
        {
            frogState = movementState.jump;
        }
        else if (frogRB.velocity.y < -.1f)
        {
            frogState = movementState.fall;
        }
        else { frogState = movementState.idle; }

        frogAnim.SetInteger("state", (int)frogState);
    }

    private void UpdateAnimationOpossumState()
    {
        if (currentWaypoint_OF_1 == 1 || currentWaypoint_OF_2 == 1)
        {
            enemyGFX.flipX = true;
        }
        else if (currentWaypoint_OF_1 == 0 || currentWaypoint_OF_2 == 0)
        {
            enemyGFX.flipX = false;
        }

        opossumAnim.SetInteger("state", (int)opossumState);
    }
    private bool IsGrounded()
    {
        return Physics2D.BoxCast(frogColl.bounds.center, frogColl.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
    }
}