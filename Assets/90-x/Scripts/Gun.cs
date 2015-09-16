using UnityEngine;
using System.Collections;

[RequireComponent (typeof (AudioSource))]

public class Gun : MonoBehaviour {
	
	//Systeam
	private float secondsBetweenShoots;
	private float nextPossibleShootTime;
	public float rpm;
    public float gunID;
    public float damage;
    public LayerMask collisionMask;

	//Components
	public Transform spawn;
	public Transform shellEjectionPoint;
	private AudioSource audio;
	private LineRenderer tracer;
	public Rigidbody shell;
	public enum GunType{Semi,Burst,Auto};
	public GunType gunType;


	void Start()
	{
		secondsBetweenShoots = 60 / rpm;
		audio = GetComponent<AudioSource> ();

		if (GetComponent<LineRenderer> ()) {
			tracer = GetComponent<LineRenderer> ();
		}
	}

	public void Shoot()
	{

		if (CanShoot ()) {
			Ray ray = new Ray (spawn.position, spawn.forward);
			RaycastHit Hit;

			float shootDistance = 20;
			if (Physics.Raycast (ray, out Hit, shootDistance, collisionMask))
			{
				shootDistance = Hit.distance;

                if (Hit.collider.GetComponent<Entity>())
                {
                    Hit.collider.GetComponent<Entity>().TakeDamage(damage);
                }
			}

			nextPossibleShootTime = Time.time + secondsBetweenShoots;

			audio.Play();

			if(tracer)
			{
				StartCoroutine("RenderTracer", ray.direction * shootDistance);
			}

			Rigidbody newShell = Instantiate(shell,shellEjectionPoint.position,Quaternion.identity) as Rigidbody;
			newShell.AddForce(shellEjectionPoint.forward * Random.Range(150f,200f) + spawn.forward * Random.Range (-10f,10f));
	}
	}

	public void ShootContin()
	{
		if (gunType == GunType.Auto)
		{
			Shoot ();
		}
	}

	private bool CanShoot()
	{
		bool canShoot = true;

		if (Time.time < nextPossibleShootTime) 
		{
		canShoot = false;
		}

		return canShoot;
	}
	IEnumerator RenderTracer(Vector3 hitPoint)
	{
		tracer.enabled = true;
		tracer.SetPosition(0,spawn.position);
		tracer.SetPosition(1,spawn.position + hitPoint);

		yield return null;
		tracer.enabled = false;
	}


}
