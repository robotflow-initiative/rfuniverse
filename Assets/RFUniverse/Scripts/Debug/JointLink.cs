using RFUniverse;
using RFUniverse.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JointLink : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public List<Canvas> jointText;
    List<ArticulationBody> articulations = new List<ArticulationBody>();
    ControllerAttr target;
    public ControllerAttr Target
    {
        get { return target; }
        set
        {
            target = value;
            if (target != null)
            {
                articulations = target.moveableJoints;
                while (jointText.Count < articulations.Count)
                {
                    jointText.Add(GameObject.Instantiate(jointText[0], jointText[0].transform.parent));
                }
                lineRenderer.positionCount = articulations.Count;
            }
        }
    }
    void FixedUpdate()
    {
        if (target && target.gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);

            for (int i = 0; i < articulations.Count; i++)
            {
                jointText[i].transform.rotation = Camera.main.transform.rotation;
                jointText[i].transform.position = articulations[i].transform.position;
                jointText[i].GetComponentInChildren<TextMeshProUGUI>().text = (Mathf.Rad2Deg * articulations[i].jointPosition[0]).ToString("f2");
                lineRenderer.SetPosition(i, articulations[i].transform.position);

            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
