using System;
using UnityEngine;
using System.Collections;


namespace GTRU.Scripts.Car
{

    public class CarController : MonoBehaviour
    {
        public enum CarDriveType
        {
            FrontWheelDrive,
            RearWheelDrive,
            FourWheelDrive
        }

        public enum SpeedType
        {
            MPH,
            KPH
        }

        public Transform centerOfMass;


        public CarDriveType m_CarDriveType = CarDriveType.FourWheelDrive;
        public SpeedType m_SpeedType;


        ////  LIGHTS

        public Light[] backLights = new Light[2];
        public Light[] forwardLights = new Light[2];
        //public Light[] backLights = new Light[2];


        ////////////


        public WheelCollider[] m_WheelColliders = new WheelCollider[4];
        public GameObject[] m_WheelMeshes = new GameObject[4];
        // public Vector3 m_CentreOfMassOffset;
        public float m_MaximumSteerAngle;
        [Range(0, 1)]
        public float m_SteerHelper; // 0 is raw physics , 1 the car will grip in the direction it is facing
        [Range(0, 1)]
        public float m_TractionControl; // 0 is no traction control, 1 is full interference
        public float m_FullTorqueOverAllWheels;
        public float m_ReverseTorque;
        public float m_MaxHandbrakeTorque;
        public float m_Downforce = 100f;
        public float m_Topspeed = 200;
        public static int NoOfGears = 5;
        public float m_RevRangeBoundary = 1f;
        public float m_SlipLimit;
        public float m_BrakeTorque;


        private float m_SteerAngle;
        private int m_GearNum;
        private float m_GearFactor;
        private float m_OldRotation;
        private float m_CurrentTorque;
        private Rigidbody m_Rigidbody;
        private const float k_ReversingThreshold = 0.01f;
        private int lightChek = 0;



        private float thrustTorque;


        private float kof;

        public bool handBreak = false;

        public bool Skidding { get; private set; }
        public float BrakeInput { get; private set; }
        public float CurrentSteerAngle { get { return m_SteerAngle; } }
        public float CurrentSpeed { get { return m_Rigidbody.velocity.magnitude * 2.23693629f; } }
        public float MaxSpeed { get { return m_Topspeed; } }
        public float Revs { get; private set; }
        public float AccelInput { get; private set; }


        void Start()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Rigidbody.centerOfMass = centerOfMass.localPosition;

            m_CurrentTorque = m_FullTorqueOverAllWheels - (m_TractionControl * m_FullTorqueOverAllWheels);

        }


        public void Move(float steering, float accel, float footbrake, float handbrake)
        {
            for (int i = 0; i < 4; i++)
            {
                Quaternion wheelRotate;
                Vector3 wheelPos;
                m_WheelColliders[i].GetWorldPose(out wheelPos, out wheelRotate);
                m_WheelMeshes[i].transform.position = wheelPos;
                m_WheelMeshes[i].transform.rotation = wheelRotate;

            }


            steering = Mathf.Clamp(steering, -1, 1);
            AccelInput = accel = Mathf.Clamp(accel, 0, 1);
            BrakeInput = footbrake = -1 * Mathf.Clamp(footbrake, -1, 0);
            handbrake = Mathf.Clamp(handbrake, 0, 1);

            m_SteerAngle = steering * m_MaximumSteerAngle;
            m_WheelColliders[0].steerAngle = m_SteerAngle;
            m_WheelColliders[3].steerAngle = m_SteerAngle;

            SteerHelper();
            ApplyDrive(accel, footbrake);
            CapSpeed();

            if (handbrake > 0f)
            {
                var hbTorque = handbrake * m_MaxHandbrakeTorque;
                m_WheelColliders[1].brakeTorque = hbTorque;
                m_WheelColliders[2].brakeTorque = hbTorque;
            }



            //CalculateRevs();
            AddDownForce();
            // CheckForWheelSpin();
            TractionControl();

            if (m_WheelColliders[0].rpm < -100)
            {
                //backLights[0].enabled = backLights[1].enabled = true;
                backLights[0].color = backLights[1].color = Color.white;
            }
            else
            {
                backLights[0].color = backLights[1].color = Color.red;
            }



            if (BrakeInput > 0f)
            {
                backLights[0].enabled = backLights[1].enabled = true;
            }
            else
            {
                backLights[0].enabled = backLights[1].enabled = false;
            }


        }

        private void CapSpeed()
        {
            float speed = m_Rigidbody.velocity.magnitude;
            switch (m_SpeedType)
            {
                case SpeedType.MPH:

                    speed *= 2.23693629f;
                    if (speed > m_Topspeed)
                        m_Rigidbody.velocity = (m_Topspeed / 2.23693629f) * m_Rigidbody.velocity.normalized;
                    break;

                case SpeedType.KPH:
                    speed *= 3.6f;
                    if (speed > m_Topspeed)
                        m_Rigidbody.velocity = (m_Topspeed / 3.6f) * m_Rigidbody.velocity.normalized;
                    break;
            }
        }

        private void ApplyDrive(float accel, float footbrake)
        {

            float thrustTorque;
            switch (m_CarDriveType)
            {
                case CarDriveType.FourWheelDrive:
                    thrustTorque = accel * (m_CurrentTorque / 4f);
                    for (int i = 0; i < 4; i++)
                    {
                        m_WheelColliders[i].motorTorque = thrustTorque;
                    }
                    break;

                case CarDriveType.FrontWheelDrive:
                    thrustTorque = accel * (m_CurrentTorque / 2f);
                    m_WheelColliders[1].motorTorque = m_WheelColliders[3].motorTorque = thrustTorque;
                    break;

                case CarDriveType.RearWheelDrive:
                    thrustTorque = accel * (m_CurrentTorque / 2f);
                    m_WheelColliders[0].motorTorque = m_WheelColliders[2].motorTorque = thrustTorque;
                    break;

            }

            for (int i = 0; i < 4; i++)
            {
                if (CurrentSpeed > 5 && Vector3.Angle(transform.forward, m_Rigidbody.velocity) < 50f)
                {
                    m_WheelColliders[i].brakeTorque = m_BrakeTorque * footbrake;
                }
                else if (footbrake > 0)
                {
                    m_WheelColliders[i].brakeTorque = 0f;
                    m_WheelColliders[i].motorTorque = -m_ReverseTorque * footbrake;
                }
            }
        }

        private static float CurveFactor(float factor)
        {
            return 1 - (1 - factor) * (1 - factor);
        }


        private static float ULerp(float from, float to, float value)
        {
            return (1.0f - value) * from + value * to;
        }

        private void SteerHelper()
        {
            for (int i = 0; i < 4; i++)
            {
                WheelHit wheelhit;
                m_WheelColliders[i].GetGroundHit(out wheelhit);
                if (wheelhit.normal == Vector3.zero)
                    return; // wheels arent on the ground so dont realign the rigidbody velocity
            }

            // this if is needed to avoid gimbal lock problems that will make the car suddenly shift direction
            if (Mathf.Abs(m_OldRotation - transform.eulerAngles.y) < 10f)
            {
                var turnadjust = (transform.eulerAngles.y - m_OldRotation) * m_SteerHelper;
                Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
                m_Rigidbody.velocity = velRotation * m_Rigidbody.velocity;
            }
            m_OldRotation = transform.eulerAngles.y;
        }

        private void AddDownForce()
        {
            m_WheelColliders[0].attachedRigidbody.AddForce(-transform.up * m_Downforce *
                                                         m_WheelColliders[0].attachedRigidbody.velocity.magnitude);
        }


        void Update()
        {

            if (Input.GetKey(KeyCode.Q))
            {
                Time.timeScale = 0.5f;
            }

            if (Input.GetKey(KeyCode.E))
            {
                Time.timeScale = 1f;
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                lightChek++;

                if(lightChek == forwardLights.Length + 1)
                {
                    lightChek = 0;
                }
            }


            if (lightChek == 0)
            {
                forwardLights[0].enabled = forwardLights[1].enabled = false;
            }
            else if(lightChek == 1)
            {
                forwardLights[0].enabled = forwardLights[1].enabled = true;
                forwardLights[0].range = forwardLights[1].range = 2;
            }
            else if (lightChek == 2)
            {
                forwardLights[0].enabled = forwardLights[1].enabled = true;
                forwardLights[0].range = forwardLights[1].range = 5;
            }



        }


        private void TractionControl()
        {
            WheelHit wheelHit;
            switch (m_CarDriveType)
            {
                case CarDriveType.FourWheelDrive:
                    // loop through all wheels
                    for (int i = 0; i < 4; i++)
                    {
                        m_WheelColliders[i].GetGroundHit(out wheelHit);

                        AdjustTorque(wheelHit.forwardSlip);
                    }
                    break;

                case CarDriveType.RearWheelDrive:
                    m_WheelColliders[2].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);

                    m_WheelColliders[1].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);
                    break;

                case CarDriveType.FrontWheelDrive:
                    m_WheelColliders[0].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);

                    m_WheelColliders[3].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);
                    break;
            }
        }

        private void AdjustTorque(float forwardSlip)
        {
            if (forwardSlip >= m_SlipLimit && m_CurrentTorque >= 0)
            {
                m_CurrentTorque -= 10 * m_TractionControl;
            }
            else
            {
                m_CurrentTorque += 10 * m_TractionControl;
                if (m_CurrentTorque > m_FullTorqueOverAllWheels)
                {
                    m_CurrentTorque = m_FullTorqueOverAllWheels;
                }
            }
        }

        void OnGUI()
        {
            GUI.Box(new Rect(Screen.width - 100, Screen.height - 50, 100, 50), "RPM - " + Mathf.RoundToInt(m_WheelColliders[0].rpm * 3.6f));
        }
    }
}