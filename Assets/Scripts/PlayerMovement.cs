using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;
public class PlayerMovement : MonoBehaviour
{

    public float sensitivity = 3f;
    public Transform playerbody;
    float xRotation = 0f;
    public static bool stopRotation = false;
    
    float horizontalRotation;
    float verticalRotation;
    [Header ("Cam Shake")]
    public Transform headTransform;
    public Transform camTransform;

    public float bobFrequency = 5f;
    public float bobHorizontalAmplitude = 0.01f;
    public float bobVerticalAmplitude = 0.01f;
    [Range(0,1)] public float headBobSmoothing = 0.1f;

    private float walkingTime;
    private Vector3 targetCamPos;

    [Header("Movement")]
    public CharacterController controller;
    public float speed = 12f;
    Vector3 velocity;
    public float gravity =-9.81f;
    public static bool stopMovement = false;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private SerialPort data_stream = new SerialPort("COM7", 9600);
    private Thread readThread;
    private char receivedChar;
    float horizontalMovement;
    float verticalMovement;
    [SerializeField]GameObject cam;

    bool isGrounded;

    void Start()
    {
        data_stream.WriteTimeout = 300;
        data_stream.ReadTimeout = 5000;
        data_stream.DtrEnable = true;
        data_stream.RtsEnable = true;
        data_stream.Open();
        if (data_stream.IsOpen)
        {
            readThread = new Thread(ReadThread);
            readThread.Start();

        }
    }
    void Update()
    {   

            float mouseY;
            
            /*horizontalRotation = Input.GetAxis("Mouse X"); //mouse da kullanılabilir
            mouseY = 2 * Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

           */

            if(receivedChar=='L'){
                horizontalRotation = -1;
            }
            else if(receivedChar=='R'){
                horizontalRotation = +1;
            }
            else horizontalRotation=0;

            if(receivedChar=='U') mouseY = sensitivity * Time.deltaTime;
            else if(receivedChar=='D') mouseY = -sensitivity * Time.deltaTime;
            else mouseY=0;
            float mouseX = horizontalRotation * sensitivity * Time.deltaTime;
            

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation,-90f,90f);
            if(!stopRotation){
                cam.transform.localRotation = Quaternion.Euler(xRotation,0f,0f);
                playerbody.Rotate(Vector3.up * mouseX);
            }




            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            if(isGrounded && velocity.y < 0){
                velocity.y = -4.0f;
            }
            /*horizontalMovement = Input.GetAxis("Horizontal");
            verticalMovement = Input.GetAxis("Vertical");*/

            if(receivedChar=='F') verticalMovement=1;
            else if(receivedChar=='B') verticalMovement=-1;
            else verticalMovement=0;


            if(Input.GetKey(KeyCode.LeftShift)){
                horizontalMovement*=1.8f;
                verticalMovement *=1.8f;
            }
            if(Input.GetKeyUp(KeyCode.LeftShift)){
                horizontalMovement/=1.8f;
                verticalMovement/=1.8f;
            }


        Vector3 move = transform.right * horizontalMovement + transform.forward * verticalMovement;
        if(move == Vector3.zero){
            walkingTime = 0;
        }
        else{
            walkingTime +=Time.deltaTime;

        }
        if(!stopMovement){
            controller.Move(move * speed * Time.deltaTime);
        }

        targetCamPos = headTransform.position + CalculateHeadBobOffset(walkingTime);
        camTransform.position = Vector3.Lerp(camTransform.position,targetCamPos,headBobSmoothing);
        if((camTransform.position - targetCamPos).magnitude <=0.001)
            camTransform.position = targetCamPos;

        velocity.y +=  gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private Vector3 CalculateHeadBobOffset(float walkingTime)
    {
        float horizontalOffset = 0;
        float verticalOffset = 0;
        Vector3 offset = Vector3.zero;

        if(walkingTime > 0){
            horizontalOffset = Mathf.Cos(walkingTime * bobFrequency) * bobHorizontalAmplitude;
            verticalOffset = Mathf.Sin(walkingTime*bobFrequency*2) * bobVerticalAmplitude;
            offset = headTransform.right * horizontalOffset + headTransform.up * verticalOffset;


        }
        return offset;
    }

    void ReadThread()
    {
        while (data_stream.IsOpen)
        {
            receivedChar = (char)data_stream.ReadChar();
        }
    }

}

