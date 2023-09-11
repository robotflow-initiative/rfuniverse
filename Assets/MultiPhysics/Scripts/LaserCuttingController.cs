using UnityEngine;

public class LaserCuttingController : MonoBehaviour
{
    public GameObject light;
    public bool needActive = true;
    public float width;

    private SofaLaserModel laserModel;
    private ParticleSystem particleSystem;

    void Start()
    {
        laserModel = GetComponentInChildren<SofaLaserModel>();
        particleSystem = GetComponentInChildren<ParticleSystem>();

        if (needActive)
        {
            var psmain = particleSystem.main;
            psmain.startLifetime = laserModel.Length / psmain.startSpeed.constant;
        }
    }

    void FixedUpdate()
    {
        // This code segment must be in the Update() function;
        // If these code is in Start(), cutting will be somehow disabled.
        // Holy shit!!!!!!!! F**k SOFA!!!!!!!!
        if (needActive)
        {
            laserModel.ActivateRay = true;
            laserModel.ActivateTool = true;
            laserModel.DrawLaser = false;
            laserModel.LaserWidth = width;

            needActive = false;
        }
    }
}