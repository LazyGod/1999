using UnityEngine;
using System.Collections;

public class CarController : MonoBehaviour
{
    public enum CarDriveType
    {
        FrontWheelDrive,
        RearWheelDrive,
        FourWheelDrive
    }

    public Transform centerOfMass;

    public float maxTorque = 50f;
    public float torquePower = 25f;
    public float steerForce = 2f;
    public float handBrakeForce = 10f;

    private Rigidbody m_RBody;


    public WheelCollider[] wheelCollider = new WheelCollider[4];
    public Transform[] wheelMeshes = new Transform[4];


    [SerializeField]
    private CarDriveType m_CarDriveType = CarDriveType.FourWheelDrive;
    private float thrustTorque;

    [SerializeField]
    private float currentBreak;
    private float kof;

    public bool handBreak = false;



    void UpdateMeshesPos()
    {
        for (int i = 0; i < 4; i++)
        {
            Quaternion wheelRotate;
            Vector3 wheelPos;
            wheelCollider[i].GetWorldPose(out wheelPos, out wheelRotate);
            wheelMeshes[i].position = wheelPos;
            wheelMeshes[i].rotation = wheelRotate;

        }
    }

    void Start()
    {
        m_RBody = GetComponent<Rigidbody>();
       m_RBody.centerOfMass = centerOfMass.localPosition;
        
    }

    void Update()
    {
        UpdateMeshesPos();

        /// CONTROL CAR
        currentBreak = wheelCollider[0].brakeTorque;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            handBreak = !handBreak;
        }

        if (handBreak)
        {
            for (int i = 0; i < 4; i++)
            {
                wheelCollider[i].brakeTorque = handBrakeForce;
                //wheelCollider[i].brakeTorque = 0;
            }

        }
        if (Input.GetKey(KeyCode.Q))
        {
            Time.timeScale = 0.5f;
        }

        if (Input.GetKey(KeyCode.E))
        {
            Time.timeScale = 1f;
        }
         
         


    }
    void FixedUpdate()
    {
        float steer = Input.GetAxis("Horizontal");
        float accel = Input.GetAxis("Vertical");

        float wheelAngle = steer * torquePower;

        wheelCollider[1].steerAngle = wheelAngle;
        wheelCollider[3].steerAngle = wheelAngle;




        switch (m_CarDriveType)
        {
            case CarDriveType.FourWheelDrive:
                thrustTorque = accel * (maxTorque / 4f);
                for (int i = 0; i < 4; i++)
                {
                    wheelCollider[i].motorTorque = thrustTorque;
                }
                break;

            case CarDriveType.FrontWheelDrive:
                thrustTorque = accel * (maxTorque / 2f);
                wheelCollider[1].motorTorque = wheelCollider[3].motorTorque = thrustTorque;
                break;

            case CarDriveType.RearWheelDrive:
                thrustTorque = accel * (maxTorque / 2f);
                wheelCollider[0].motorTorque = wheelCollider[2].motorTorque = thrustTorque;
                break;

        }

    }
}
