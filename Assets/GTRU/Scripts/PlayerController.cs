using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]

public class PlayerController : MonoBehaviour
{

    //handling
    public GameObject tazBone;
    public GameObject hold;
    public float speedRotation = 450;
    public float walkSpeed = 5;
    public float runSpeed = 8;
    private float acceleration = 5;
    public Gun[] guns;




    //Systeam
    private Quaternion targetRotation;
    private Vector3 currentVelocityMod;
    private Quaternion dir;
    private bool reloading;



    //Components
    private CharacterController controller;
    private AnimatorTransitionInfo armsTransitionInfo;
    private Camera cam;
    private Animator anim;
    private Gun currentGun;
    public Transform handHold;

    void Start()
    {

        controller = GetComponent<CharacterController>();
        cam = Camera.main;
        anim = GetComponentInChildren<Animator>();
        armsTransitionInfo = anim.GetAnimatorTransitionInfo(1);
        EquipGun(0);
    }


    void Update()
    {
        
        ControlMouse();
        //ControlWASD ();
        if (currentGun)
        {
            if (Input.GetButtonDown("Shoot"))
            {
                currentGun.Shoot();
            }
            else if (Input.GetButton("Shoot"))
            {
                currentGun.ShootContin();
            }

            if (Input.GetButton("Aim"))
            {
                //anim.SetFloat("Weapon Aim", 1);
                anim.SetLayerWeight(1, 1);
            }
            else
            {
                anim.SetLayerWeight(1, 0);
            }

            if (Input.GetButtonDown("Reload"))
            {
                if (currentGun.Reload())
                {
                    anim.SetTrigger("Reload");
                    reloading = true;
                }
            }

            if (reloading)
            {
                if (armsTransitionInfo.nameHash == Animator.StringToHash("Arms.Reload -> Arms.Weapon"))
                {
                    currentGun.FinishReload();
                    reloading = false;
                }
            }
        }


        for (int i = 0; i < guns.Length; i++)
        {

            if ((Input.GetKeyDown((i + 1) + "")) || (Input.GetKeyDown("[" + (i + 1) + "]")))
            {
                EquipGun(i);
                break;

            }
        }

    }

    void EquipGun(int i)
    {
        if (currentGun)
        {
            Destroy(currentGun.gameObject);
        }

        currentGun = Instantiate(guns[i], handHold.position, handHold.rotation) as Gun;
        currentGun.transform.parent = handHold;
        anim.SetFloat("Weapon ID", currentGun.gunID);

    }

    void ControlWASD()
    {


        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        

        if (input != Vector3.zero)
        {
            targetRotation = Quaternion.LookRotation(input);
            transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, speedRotation * Time.deltaTime);
        }

        currentVelocityMod = Vector3.MoveTowards(currentVelocityMod, input, acceleration * Time.deltaTime);
        Vector3 motion = input;
        motion *= (Mathf.Abs(input.x) == 1 && Mathf.Abs(input.z) == 1) ? .7f : 1;
        motion *= (Input.GetButton("Run")) ? runSpeed : walkSpeed;
        motion += Vector3.up * -8;
        controller.Move(motion * Time.deltaTime);
        anim.SetFloat("Speed", Mathf.Sqrt(motion.x * motion.x + motion.z * motion.z));

    }

    void ControlMouse()
    {

        float ix = Input.GetAxisRaw("Horizontal");
        float iy = Input.GetAxisRaw("Vertical");

        anim.SetFloat("Speed_X", ix);
        anim.SetFloat("Speed_Y", iy);
        Debug.Log("X" + ix);
        Debug.Log("Y" + iy);

        Vector3 input = new Vector3(ix, 0, iy);
        Vector3 mousePos = Input.mousePosition;
        mousePos = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.transform.position.y - transform.position.y));
        targetRotation = Quaternion.LookRotation(mousePos - new Vector3(transform.position.x, 0, transform.position.z));
        transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, speedRotation * Time.deltaTime);

        ///Hold Rotation
        hold.transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, speedRotation * Time.deltaTime);



        currentVelocityMod = Vector3.MoveTowards(currentVelocityMod, input, acceleration * Time.deltaTime);
        Vector3 motion = input;

        motion *= (Mathf.Abs(input.x) == 1 && Mathf.Abs(input.z) == 1) ? .7f : 1;
        motion *= (Input.GetButton("Run")) ? runSpeed : walkSpeed;
        motion += Vector3.up * -8;
        controller.Move(motion * Time.deltaTime);

        anim.SetFloat("Speed", Mathf.Sqrt(motion.x * motion.x + motion.z * motion.z));

        //    tazBone.transform.rotation = Quaternion.LookRotation(input);


    }

}
