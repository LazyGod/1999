using UnityEngine;
using System.Collections;

[RequireComponent (typeof (CharacterController))]

public class PlayerController : MonoBehaviour {

	//handling
	public float speedRotation = 450;
	public float walkSpeed = 5;
	public float runSpeed = 8;
	private float acceleration = 5;


	//Systeam
	private Quaternion targetRotation;
	private Vector3 currentVelocityMod;



	//Components
	private CharacterController controller;
	private Camera cam;
    private Animator anim;
	public Gun gun;

	void Start () 
	{

		controller = GetComponent<CharacterController>();
		cam = Camera.main;
        anim = GetComponent<Animator>();

	}
	

	void Update () 
	{

		ControlMouse ();
		//ControlWASD ();
		if(Input.GetButtonDown("Shoot")){
			gun.Shoot();
		}
		else if(Input.GetButton("Shoot")){
			gun.ShootContin();
		}

	}

	void ControlWASD()
	{


		Vector3 input = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical"));

		if (input != Vector3.zero) {
			targetRotation = Quaternion.LookRotation (input);
			transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y ,targetRotation.eulerAngles.y,speedRotation * Time.deltaTime);
		}

		currentVelocityMod = Vector3.MoveTowards(currentVelocityMod,input,acceleration * Time.deltaTime);
		Vector3 motion = input;
		motion *= (Mathf.Abs(input.x)==1 && Mathf.Abs(input.z)==1)?.7f:1;
		motion *= (Input.GetButton("Run"))?runSpeed:walkSpeed;
		motion += Vector3.up * -8;
		controller.Move (motion * Time.deltaTime);
        anim.SetFloat("Speed", Mathf.Sqrt(motion.x * motion.x + motion.z * motion.z));

	}

	void ControlMouse()
	{
		Vector3 input = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical"));
		Vector3 mousePos = Input.mousePosition;
		mousePos = cam.ScreenToWorldPoint (new Vector3 (mousePos.x, mousePos.y, cam.transform.position.y - transform.position.y));
		targetRotation = Quaternion.LookRotation (mousePos- new Vector3(transform.position.x,0,transform.position.z));
		transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y ,targetRotation.eulerAngles.y,speedRotation * Time.deltaTime);

	
		
		//Vector3 motion = input;
		currentVelocityMod = Vector3.MoveTowards(currentVelocityMod,input,acceleration * Time.deltaTime);
		Vector3 motion = input;

		motion *= (Mathf.Abs(input.x)==1 && Mathf.Abs(input.z)==1)?.7f:1;
		motion *= (Input.GetButton("Run"))?runSpeed:walkSpeed;
		motion += Vector3.up * -8;
		controller.Move (motion * Time.deltaTime);

        anim.SetFloat("Speed", Mathf.Sqrt(motion.x * motion.x + motion.z * motion.z));

	}
}
