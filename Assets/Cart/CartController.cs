using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CartController : MonoBehaviour
{
    public Animator animFrontAxle;
    public Animator animRearAxle;
    public Animator animActor;

    public Rigidbody rigid;
    public Transform rayPoint;

    private float turnAngle;
    [Header("Torque")]
    public float torqueMax = 30f;
    public float rotIncreaseFactor = 1f;
    public float torquePow = 100f;

    [Header("Boost")]
    public float boostMax = 20f;
    public float boostDur = 5f;
    public float boostSpeedMultiplier = 4f;

    [Header("Speed And Accel")]
    public float accelMount = 1f;
    public float decelerateMount = 2f;
    public float maxSpeed = 10f;

    [Header("Drift")]
    public ForceMode driftForceType = ForceMode.Impulse;
    public float driftForceFactor = 10f;
    public float driftForceMax = 100;

    
    public Transform[] wheels;
    public float groudRayHeight = 1.6f;
    public float gravityForce = 500;
    public float defaultDrag = 3f;

    private float curSpeed = 0;
    private float speedInput;
    private float steerInput;

    private float driftForce = 0;
    private Vector3 driftDir = Vector3.zero;

    [Range(1, 999)]
    private float boost = 1;

    Vector3 startPos;
    Vector3 startEuler;
    bool bDrifting = false;

    private void Awake()
    {
        //rigid = GetComponent<Rigidbody>();
        startPos = transform.position;
        startEuler = transform.eulerAngles;
        rigid.transform.parent = null;
    }

    private void Update()
    {
        speedInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
        UpdateAnim();

        if (IsGround() && rigid.isKinematic)
            UpdateNonPhysics();

        if (Input.GetKey(KeyCode.Space))
        {
            boost = boostMax;
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            StopAllCoroutines();
            StartCoroutine(UpdateBoostCrtn(boostDur));
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            rigid.transform.position = startPos;
            transform.eulerAngles = startEuler;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.LeftShift))
        {
            driftDir = transform.forward;
            //driftForce = Mathf.Clamp(rigid.velocity.magnitude * driftForceMax, 0, driftForceMax);
            driftForce = Mathf.Clamp(curSpeed * driftForceFactor, 0, driftForceMax);
            bDrifting = true;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.LeftShift))
        {
            driftForce = 0;
            bDrifting = false;
        }

        if(IsGround())
        transform.rotation = Quaternion.Euler(
            transform.rotation.eulerAngles + new Vector3(0f, steerInput * torquePow * speedInput * Time.deltaTime, 0f));

        transform.position = rigid.transform.position;
    }

    private void FixedUpdate()
    {

        if (!rigid.isKinematic)
            UpdatePhysics();
    }

    void UpdateNonPhysics()
    {
        Vector3 dir = Vector3.zero;
        dir.x = -Input.acceleration.y;
        dir.z = Input.acceleration.x;
        if (dir.sqrMagnitude > 1)
            dir.Normalize();

        dir *= Time.deltaTime;
        transform.Translate(dir * maxSpeed);

        speedInput *= Time.deltaTime * maxSpeed * boost;
        steerInput *= Time.deltaTime * (speedInput < 0 ? -rotIncreaseFactor : rotIncreaseFactor);

        transform.Rotate(0, steerInput, 0);
        transform.Translate(0, 0, speedInput);
    }

    void UpdatePhysics()
    {
        UpdateSpeed();
        /*var force = Vector3.zero;
        var speedlimit = boost > 1 ? maxSpeed * boostSpeedMultiplier : maxSpeed;
        if (rigid.velocity.sqrMagnitude < speedlimit * speedlimit)
            force = transform.forward * speed * curTranslation * Time.deltaTime * boost;*/

        //Debug.Log($"Rigid velocity  {rigid.velocity}");
        //if (IsGround() && Mathf.Abs(curSpeed) > 1f)
        //{
        //    steerInput = steerInput * Mathf.Lerp(steerInput, speedInput < 0 ? -torqueMax : torqueMax, Time.fixedDeltaTime * torquePow) ;
        //    //Vector3 rotation = new Vector3(0, curAngle, 0);
        //    //Quaternion deltaRotation = Quaternion.Euler(rotation * Time.fixedDeltaTime);
        //    //rigid.MoveRotation(rigid.rotation * deltaRotation);

        //    rigid.AddRelativeTorque(transform.up * steerInput);
        //}

        bool grounded = false;
        RaycastHit hit;
        if (Physics.Raycast(rayPoint.position, -transform.up, out hit, groudRayHeight))
        {
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            grounded =  true;
        }


        if (bDrifting)
        {
            if (IsGround())
            {
                var force = driftDir * Time.fixedDeltaTime * driftForce;
                rigid.AddForce(force, driftForceType);
            }
        }
        else
        {

            //var newvel = IsGround() ? transform.forward * curSpeed * boost : transform.forward * curSpeed;
            //newvel.y = rigid.velocity.y;
            //rigid.velocity = newvel;

            var rigidvel = rigid.velocity.magnitude;
            if (grounded )
            {
                rigid.drag = defaultDrag;
                //var forcedelta = transform.forward * accelMount * (1 - curSpeed / maxSpeed) * Time.fixedDeltaTime;
                if (Mathf.Abs(rigidvel) < maxSpeed)
                {
                    var forcedelta = transform.forward * accelMount * speedInput * boost;
                    Debug.Log("---------------------------------");
                    Debug.Log($"forcedelta {forcedelta.ToString()}");
                    rigid.AddForce(forcedelta);
                }
            }
            else
            {
                rigid.drag = 0.1f;
                rigid.AddForce(transform.up * -gravityForce);
            }
            //rigid.MovePosition(transform.position + (transform.forward * Time.deltaTime * curSpeed * boost));
        }

        Debug.Log("---------------------------------");
        Debug.Log($"rigid.velocity.magnitude  { rigid.velocity.magnitude.ToString() }");
        Debug.Log($"Rigid Velocity = {rigid.velocity.ToString()}");
        Debug.Log($"Drift Force = {driftForce.ToString()}");
        Debug.Log($"CurSpeed = {curSpeed.ToString()}");
        //rigid.velocity = transform.forward * curTranslation * speed;
        //rigid.AddForce(transform.right * curAngle * (curTranslation < 0 ? -rotIncreaseFactor : rotIncreaseFactor) * Time.deltaTime, ForceMode.Impulse);

    }

    void UpdateAnim()
    {
        animFrontAxle.SetFloat("speed", speedInput);
        animFrontAxle.SetFloat("turn_angle", steerInput);

        animRearAxle.SetFloat("speed", speedInput);

        animActor.SetFloat("turn_angle", steerInput);
    }

    void UpdateSpeed()
    {
        if (speedInput != 0)
            curSpeed = Mathf.Clamp(curSpeed + (accelMount * Time.fixedDeltaTime * speedInput ), -maxSpeed, maxSpeed);
        else
        {
            if (IsGround())
                curSpeed = Mathf.Clamp(curSpeed - (decelerateMount * Time.fixedDeltaTime), 0, maxSpeed);
        }
    }

    bool IsGround()
    {
        RaycastHit hit;

        if (Physics.Raycast(rayPoint.position, -transform.up, out hit, groudRayHeight))
        {
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            return true;
        }
        
        return false;
        
        //int grounded = 0;
        //foreach (Transform t in wheels)
        //{
        //    if (Physics.Raycast(t.position, Vector3.down, height))
        //        grounded++;
        //}
        //return grounded > 0;
    }

    IEnumerator UpdateBoostCrtn(float dur)
    {
        boost = boostMax;
        float elapse = 0;
        while (elapse < dur)
        {
            boost = Mathf.Lerp(boost, 1, elapse / dur);
            elapse += Time.deltaTime;
            yield return null;
        }
        boost = 1;
    }
}
