using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillerController : MonoBehaviour, IAlive
{
    private Transform killerTransform;

    private List<Vector3> detectorList = new List<Vector3>()
    {
        new Vector3(-0.45f, -0.5f),
        new Vector3(0.45f, -0.5f)
    };

    [SerializeField] private float detectRadius = 0.3f;


    public struct KillZone
    {
        public KillZone(Vector3 pos, Vector3 dir)
        {
            origin = pos;
            direction = dir;
        }

        public Vector3 origin;
        public Vector3 direction;
    }

    private List<KillZone> killZoneList = new List<KillZone>()
    {
        new KillZone(new Vector3(-0.52f, 0, 0), Vector3.left),
        new KillZone(new Vector3(0, 0.52f, 0), Vector3.up),
        new KillZone(new Vector3(0.52f, 0, 0), Vector3.right)
    };


    private float horVelocity = 4;
    
    // Start is called before the first frame update
    void Start()
    {
        killerTransform = this.transform;
    }

    private bool leftGrounded = true;

    private bool rightGrounded = true;

    private bool drop = false;
    // Update is called once per frame
    void FixedUpdate()
    {
        leftGrounded = OnGround(0);
        rightGrounded = OnGround(1);
        if (!leftGrounded)
        {
            horVelocity = 1;
        }else if (!rightGrounded)
        {
            horVelocity = -1;
        }

        if ((!leftGrounded && !rightGrounded) || drop)
            ApplyGravity();
        else
        {
            ApplyVelocity();
        }
        
        Kill();

    }
    
    private float detectDistance = .1f;
    private RaycastHit2D detectHit;
    private Vector3 detectDir = Vector3.down;
    private Vector3 detectOrigin;
    private bool OnGround(int index)
    {
        detectOrigin = killerTransform.position + detectorList[index];
        detectHit = Physics2D.Raycast(detectOrigin, detectDir, detectDistance);
        if (detectHit.collider != null)
        {
            if (detectHit.collider.GetComponentInParent<ChunkClass>()?.inRotateProcedure == true)
                drop = true;
            return true;
        }

        return false;
    }


    private float killDistance = 0.1f;
    private RaycastHit2D killHit;
    private Vector3 killOrigin;
    private void Kill()
    {
        foreach (var killZone in killZoneList)
        {
            killOrigin = killerTransform.position + killZone.origin;
            killHit = Physics2D.Raycast(killOrigin, killZone.direction, killDistance);
            if (killHit.collider != null)
            {
                killHit.collider.GetComponent<IAlive>()?.GotAttacked();
            }
                
        }
    }
    
    

    private void ApplyVelocity()
    {
        killerTransform.position += new Vector3(horVelocity*Time.fixedDeltaTime, 0, 0);
    }
    
    private void ApplyGravity()
    {
        killerTransform.position += new Vector3(0,-6*Time.fixedDeltaTime, 0);
    }

    public void GotAttacked()
    {
        gameObject.SetActive(false);
    }
}
