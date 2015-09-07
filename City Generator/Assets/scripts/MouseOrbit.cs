using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MouseOrbit : MonoBehaviour
{

    public Transform target;
    public float distance = 10.0f;
    public float xSpeed = 250.0f;
    public float ySpeed = 120.0f;
    public float zSpeed = 10.0f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;
    public float moveSpeed = 1.0f;
    private float x = 0.0f;
    private float y = 0.0f;

    public static bool IsMouseBusy = false;

    void Start()
    {
        var angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    
        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;
    }

    void LateUpdate()
    {
    
        bool mouse = false;
        if (Application.platform == RuntimePlatform.Android)
            mouse = Input.GetTouch(0).phase == TouchPhase.Moved;
        else
            mouse = Input.GetAxis("Fire1") > 0 && Input.GetAxis("Fire2") > 0;

        MouseOrbit.IsMouseBusy = mouse;

        if (target)
        {
            if (mouse)
            {
                if(Application.platform == RuntimePlatform.Android)
                {
                    x += Input.GetTouch(0).deltaPosition.x * xSpeed * 0.0025f;
                    y -= Input.GetTouch(0).deltaPosition.y * ySpeed * 0.0025f;
                }
                else
                {
                    x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                    y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
                }

                y = ClampAngle(y, yMinLimit, yMaxLimit);
            }
        
            float zoom = Input.GetAxis("Mouse ScrollWheel") * zSpeed;
            this.distance += zoom;
        
            Quaternion rotation = Quaternion.Euler(y, x, 0);
            Vector3 position = rotation * new Vector3(0.0f, 0.0f, -distance) + target.position;
        
            transform.rotation = rotation;
            transform.position = position;// Vector3.Lerp(transform.position,position, 0.5f);
        }

        if (Application.platform == RuntimePlatform.Android)
            return;
    
        float movementHor = Input.GetAxis("Horizontal") * moveSpeed;
        float movementVer = Input.GetAxis("Vertical") * moveSpeed;
    
        if (movementVer != 0)
        {
            Vector3 v0 = this.transform.position;
            v0.y = 0;
        
            Vector3 targetCamVar = Vector3.Normalize(this.target.transform.position - v0);

            Vector3 newTargetPos = this.target.position;
            newTargetPos.x += targetCamVar.x * movementVer;
            newTargetPos.z += targetCamVar.z * movementVer;
            this.target.position = newTargetPos;

            Vector3 newPos = this.transform.position;
            newPos.x += targetCamVar.x * movementVer;
            newPos.z += targetCamVar.z * movementVer;
            this.transform.position = newPos;
        }
    
        if (movementHor != 0)
        {
            Vector3 v1 = this.transform.position;
            v1.y = 0;
        
            Vector3 targetCamVarB = Vector3.Normalize(this.target.transform.position - v1);
            targetCamVarB = Quaternion.Euler(0, 90, 0) * targetCamVarB;

            Vector3 newTargetPos = this.target.position;
            newTargetPos.x += targetCamVarB.x * movementHor;
            newTargetPos.z += targetCamVarB.z * movementHor;
            this.target.position = newTargetPos;
            
            Vector3 newPos = this.transform.position;
            newPos.x += targetCamVarB.x * movementHor;
            newPos.z += targetCamVarB.z * movementHor;
            this.transform.position = newPos;
        }
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}