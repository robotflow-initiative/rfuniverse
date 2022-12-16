using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArticulationGraph : MonoBehaviour
{
    public ArticulationBody articulationBody;
    [Range(50,500)]
    public int showFrame = 500;
    public UILineGraph position;
    public UILineGraph velocity;
    public UILineGraph acceleration;
    void Start()
    {
        position.Init("POS",Color.blue);
        velocity.Init("VEL", Color.green);
        acceleration.Init("ACC", Color.red);
    }

    List<float> pos = new List<float>();
    List<float> vel = new List<float>();
    List<float> acc = new List<float>();
    void FixedUpdate()
    {
        pos.Add(articulationBody.jointPosition[0]);
        vel.Add(articulationBody.jointVelocity[0]);
        acc.Add(articulationBody.jointAcceleration[0]);
        List<float> posData = new(pos);
        List<float> velData = new(vel);
        List<float> accData = new(acc);

        if (showFrame > 0)
        {
            int max = Mathf.Min(pos.Count, showFrame);
            posData = pos.ToArray()[^max..].ToList();
            velData = vel.ToArray()[^max..].ToList();
            accData = acc.ToArray()[^max..].ToList();
        }
        position.SetData(posData);
        velocity.SetData(velData);
        acceleration.SetData(accData);
    }
}
