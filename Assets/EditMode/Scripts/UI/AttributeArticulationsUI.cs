using Michsky.UI.ModernUIPack;
using RFUniverse.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RFUniverse.EditMode
{
    public class AttributeArticulationsUI : AttributeBaseUI
    {
        const string uiName = "Articulations";
        public List<Toggle> articulationsToggle = new List<Toggle>();
        public CustomDropdown jointType;
        public Toggle xDrive;
        public Toggle yDrive;
        public Toggle zDrive;
        public CustomDropdown jointLimit;
        public CustomInputField lowerLimit;
        public CustomInputField upperLimit;
        public CustomInputField stiffness;
        public CustomInputField damping;

        List<ArticulationData> articulationDatas;

        int currentSelectedArticuliation = 0;
        int CurrentSelectedArticulation
        {
            get { return currentSelectedArticuliation; }
            set
            {
                currentSelectedArticuliation = value;
                OnAttributeChange(uiName, currentSelectedArticuliation);
                articulationsToggle[currentSelectedArticuliation].SetIsOnWithoutNotify(true);
                CurrentSelectedJointType = articulationDatas[currentSelectedArticuliation].JointType;
            }
        }
        ArticulationJointType CurrentSelectedJointType
        {
            get { return (ArticulationJointType)jointType.selectedItemIndex; }
            set
            {
                jointType.selectedItemIndex = (int)value;
                jointType.SetupDropdown();
                articulationDatas[CurrentSelectedArticulation].JointType = value;

                switch ((ArticulationJointType)jointType.selectedItemIndex)
                {
                    case ArticulationJointType.FixedJoint:
                        xDrive.gameObject.SetActive(false);
                        yDrive.gameObject.SetActive(false);
                        zDrive.gameObject.SetActive(false);
                        jointLimit.gameObject.SetActive(false);
                        lowerLimit.gameObject.SetActive(false);
                        upperLimit.gameObject.SetActive(false);
                        stiffness.gameObject.SetActive(false);
                        damping.gameObject.SetActive(false);
                        break;
                    case ArticulationJointType.PrismaticJoint:
                        xDrive.gameObject.SetActive(true);
                        yDrive.gameObject.SetActive(true);
                        zDrive.gameObject.SetActive(true);
                        jointLimit.gameObject.SetActive(true);
                        lowerLimit.gameObject.SetActive(true);
                        upperLimit.gameObject.SetActive(true);
                        stiffness.gameObject.SetActive(true);
                        damping.gameObject.SetActive(true);

                        if (articulationDatas[CurrentSelectedArticulation].linearLockZ != ArticulationDofLock.LockedMotion)
                        {
                            CurrentSelectedAxis = 2;
                        }
                        else if (articulationDatas[CurrentSelectedArticulation].linearLockY != ArticulationDofLock.LockedMotion)
                        {
                            CurrentSelectedAxis = 1;
                        }
                        else
                        {
                            CurrentSelectedAxis = 0;
                        }
                        break;
                    case ArticulationJointType.RevoluteJoint:

                        CurrentSelectedAxis = 0;

                        xDrive.gameObject.SetActive(true);
                        yDrive.gameObject.SetActive(false);
                        zDrive.gameObject.SetActive(false);
                        jointLimit.gameObject.SetActive(true);
                        lowerLimit.gameObject.SetActive(true);
                        upperLimit.gameObject.SetActive(true);
                        stiffness.gameObject.SetActive(true);
                        damping.gameObject.SetActive(true);
                        break;
                    case ArticulationJointType.SphericalJoint:
                        xDrive.gameObject.SetActive(true);
                        yDrive.gameObject.SetActive(true);
                        zDrive.gameObject.SetActive(true);
                        jointLimit.gameObject.SetActive(true);
                        lowerLimit.gameObject.SetActive(true);
                        upperLimit.gameObject.SetActive(true);
                        stiffness.gameObject.SetActive(true);
                        damping.gameObject.SetActive(true);
                        break;
                    default:
                        break;
                }
            }
        }

        int CurrentSelectedAxis
        {
            get
            {
                if (xDrive.isOn)
                    return 0;
                else if (yDrive.isOn)
                    return 1;
                else
                    return 2;
            }
            set
            {
                MyArticulationDrive currentDrive = articulationDatas[CurrentSelectedArticulation].xDrive;
                switch (value)
                {
                    case 0:
                        switch (articulationDatas[CurrentSelectedArticulation].JointType)
                        {
                            case ArticulationJointType.PrismaticJoint:
                                articulationDatas[CurrentSelectedArticulation].linearLockX = ArticulationDofLock.LimitedMotion;
                                articulationDatas[CurrentSelectedArticulation].linearLockY = ArticulationDofLock.LockedMotion;
                                articulationDatas[CurrentSelectedArticulation].linearLockZ = ArticulationDofLock.LockedMotion;
                                jointLimit.selectedItemIndex = (int)articulationDatas[CurrentSelectedArticulation].linearLockX;
                                break;
                            case ArticulationJointType.RevoluteJoint:
                            case ArticulationJointType.SphericalJoint:
                                jointLimit.selectedItemIndex = (int)articulationDatas[CurrentSelectedArticulation].twistLock;
                                break;
                        }
                        currentDrive = articulationDatas[CurrentSelectedArticulation].xDrive;

                        xDrive.SetIsOnWithoutNotify(true);
                        break;
                    case 1:
                        switch (articulationDatas[CurrentSelectedArticulation].JointType)
                        {
                            case ArticulationJointType.PrismaticJoint:
                                articulationDatas[CurrentSelectedArticulation].linearLockX = ArticulationDofLock.LockedMotion;
                                articulationDatas[CurrentSelectedArticulation].linearLockY = ArticulationDofLock.LimitedMotion;
                                articulationDatas[CurrentSelectedArticulation].linearLockZ = ArticulationDofLock.LockedMotion;
                                jointLimit.selectedItemIndex = (int)articulationDatas[CurrentSelectedArticulation].linearLockY;
                                break;
                            case ArticulationJointType.RevoluteJoint:
                            case ArticulationJointType.SphericalJoint:
                                jointLimit.selectedItemIndex = (int)articulationDatas[CurrentSelectedArticulation].swingYLock;
                                break;
                        }
                        currentDrive = articulationDatas[CurrentSelectedArticulation].yDrive;
                        yDrive.SetIsOnWithoutNotify(true);
                        break;
                    case 2:
                        switch (articulationDatas[CurrentSelectedArticulation].JointType)
                        {
                            case ArticulationJointType.PrismaticJoint:
                                articulationDatas[CurrentSelectedArticulation].linearLockX = ArticulationDofLock.LockedMotion;
                                articulationDatas[CurrentSelectedArticulation].linearLockY = ArticulationDofLock.LockedMotion;
                                articulationDatas[CurrentSelectedArticulation].linearLockZ = ArticulationDofLock.LimitedMotion;
                                jointLimit.selectedItemIndex = (int)articulationDatas[CurrentSelectedArticulation].linearLockZ;
                                break;
                            case ArticulationJointType.RevoluteJoint:
                            case ArticulationJointType.SphericalJoint:
                                jointLimit.selectedItemIndex = (int)articulationDatas[CurrentSelectedArticulation].swingZLock;
                                break;
                        }
                        currentDrive = articulationDatas[CurrentSelectedArticulation].zDrive;
                        zDrive.SetIsOnWithoutNotify(true);
                        break;
                }
                jointLimit.SetupDropdown();
                lowerLimit.inputText.text = currentDrive.lowerLimit.ToString();
                upperLimit.inputText.text = currentDrive.upperLimit.ToString();
                stiffness.inputText.text = currentDrive.stiffness.ToString();
                damping.inputText.text = currentDrive.damping.ToString();
            }
        }

        public override void Init(BaseAttr baseAttr, PropertyInfo info, Action<string, string, object, int> OnValueChange, Action<string, int> OnAttributeChanged)
        {
            base.Init(baseAttr, info, OnValueChange, OnAttributeChanged);
            articulationDatas = (List<ArticulationData>)info.GetValue(baseAttr);
            // if (articulationDatas.Count == 0)
            // {
            //     jointType.gameObject.SetActive(false);
            //     xDrive.gameObject.SetActive(false);
            //     yDrive.gameObject.SetActive(false);
            //     zDrive.gameObject.SetActive(false);
            //     jointLimit.gameObject.SetActive(false);
            //     lowerLimit.gameObject.SetActive(false);
            //     upperLimit.gameObject.SetActive(false);
            //     stiffness.gameObject.SetActive(false);
            //     damping.gameObject.SetActive(false);
            // }
            for (int i = 0; i < articulationDatas.Count; i++)
            {
                if (articulationsToggle.Count <= i)
                {
                    articulationsToggle.Add(Instantiate(articulationsToggle[0], articulationsToggle[0].transform.parent));
                }
                int index = i;
                articulationsToggle[i].onValueChanged.AddListener((b) =>
                {
                    if (b)
                    {
                        CurrentSelectedArticulation = index;
                    }
                });
                articulationsToggle[i].GetComponentInChildren<TextMeshProUGUI>().text = (i + 1).ToString();
            }
            jointType.dropdownEvent.AddListener((i) =>
            {
                CurrentSelectedJointType = (ArticulationJointType)i;
                ValueChanged(OnValueChange);
            });
            xDrive.onValueChanged.AddListener((b) =>
            {
                if (b)
                    CurrentSelectedAxis = 0;
                ValueChanged(OnValueChange);
            });
            yDrive.onValueChanged.AddListener((b) =>
            {
                if (b)
                    CurrentSelectedAxis = 1;
                ValueChanged(OnValueChange);
            });
            zDrive.onValueChanged.AddListener((b) =>
            {
                if (b)
                    CurrentSelectedAxis = 2;
                ValueChanged(OnValueChange);
            });
            jointLimit.dropdownEvent.AddListener((i) =>
            {
                switch (CurrentSelectedJointType)
                {
                    case ArticulationJointType.PrismaticJoint:
                        switch (CurrentSelectedAxis)
                        {
                            case 0:
                                articulationDatas[CurrentSelectedArticulation].linearLockX = (ArticulationDofLock)i;
                                break;
                            case 1:
                                articulationDatas[CurrentSelectedArticulation].linearLockY = (ArticulationDofLock)i;
                                break;
                            case 2:
                                articulationDatas[CurrentSelectedArticulation].linearLockZ = (ArticulationDofLock)i;
                                break;
                        }
                        break;
                    case ArticulationJointType.RevoluteJoint:
                    case ArticulationJointType.SphericalJoint:
                        switch (CurrentSelectedAxis)
                        {
                            case 0:
                                articulationDatas[CurrentSelectedArticulation].twistLock = (ArticulationDofLock)i;
                                break;
                            case 1:
                                articulationDatas[CurrentSelectedArticulation].swingYLock = (ArticulationDofLock)i;
                                break;
                            case 2:
                                articulationDatas[CurrentSelectedArticulation].swingZLock = (ArticulationDofLock)i;
                                break;
                        }
                        break;
                }

                ValueChanged(OnValueChange);
            });
            lowerLimit.inputText.onSubmit.AddListener((s) => { LowerLimitChange(s, OnValueChange); });
            lowerLimit.inputText.onDeselect.AddListener((s) => { LowerLimitChange(s, OnValueChange); });
            upperLimit.inputText.onSubmit.AddListener((s) => { UpperLimitChange(s, OnValueChange); });
            upperLimit.inputText.onDeselect.AddListener((s) => { UpperLimitChange(s, OnValueChange); });
            stiffness.inputText.onSubmit.AddListener((s) => { StiffnessChange(s, OnValueChange); });
            stiffness.inputText.onDeselect.AddListener((s) => { StiffnessChange(s, OnValueChange); });
            damping.inputText.onSubmit.AddListener((s) => { DampingChange(s, OnValueChange); });
            damping.inputText.onDeselect.AddListener((s) => { DampingChange(s, OnValueChange); });
            CurrentSelectedArticulation = CurrentSelectedArticulation;
        }

        public override void ReSet()
        {
            CurrentSelectedArticulation = 0;
        }
        void LowerLimitChange(string s, Action<string, string, object, int> OnValueChange)
        {
            float f = 0;
            try
            {
                f = float.Parse(s);
            }
            catch
            {
                lowerLimit.inputText.SetTextWithoutNotify(f.ToString());
            }
            switch (CurrentSelectedAxis)
            {
                case 0:
                    articulationDatas[CurrentSelectedArticulation].xDrive.lowerLimit = f;
                    break;
                case 1:
                    articulationDatas[CurrentSelectedArticulation].yDrive.lowerLimit = f;
                    break;
                case 2:
                    articulationDatas[CurrentSelectedArticulation].zDrive.lowerLimit = f;
                    break;
            }
            ValueChanged(OnValueChange);
        }
        void UpperLimitChange(string s, Action<string, string, object, int> OnValueChange)
        {
            float f = 0;
            try
            {
                f = float.Parse(s);
            }
            catch
            {
                upperLimit.inputText.SetTextWithoutNotify(f.ToString());
            }
            switch (CurrentSelectedAxis)
            {
                case 0:
                    articulationDatas[CurrentSelectedArticulation].xDrive.upperLimit = f;
                    break;
                case 1:
                    articulationDatas[CurrentSelectedArticulation].yDrive.upperLimit = f;
                    break;
                case 2:
                    articulationDatas[CurrentSelectedArticulation].zDrive.upperLimit = f;
                    break;
            }
            ValueChanged(OnValueChange);
        }
        void StiffnessChange(string s, Action<string, string, object, int> OnValueChange)
        {
            float f = 0;
            try
            {
                f = float.Parse(s);
            }
            catch
            {
                stiffness.inputText.SetTextWithoutNotify(f.ToString());
            }
            switch (CurrentSelectedAxis)
            {
                case 0:
                    articulationDatas[CurrentSelectedArticulation].xDrive.stiffness = f;
                    break;
                case 1:
                    articulationDatas[CurrentSelectedArticulation].yDrive.stiffness = f;
                    break;
                case 2:
                    articulationDatas[CurrentSelectedArticulation].zDrive.stiffness = f;
                    break;
            }
            ValueChanged(OnValueChange);
        }
        void DampingChange(string s, Action<string, string, object, int> OnValueChange)
        {
            float f = 0;
            try
            {
                f = float.Parse(s);
            }
            catch
            {
                damping.inputText.SetTextWithoutNotify(f.ToString());
            }
            switch (CurrentSelectedAxis)
            {
                case 0:
                    articulationDatas[CurrentSelectedArticulation].xDrive.damping = f;
                    break;
                case 1:
                    articulationDatas[CurrentSelectedArticulation].yDrive.damping = f;
                    break;
                case 2:
                    articulationDatas[CurrentSelectedArticulation].zDrive.damping = f;
                    break;
            }
            ValueChanged(OnValueChange);
        }

        protected override void ValueChanged(Action<string, string, object, int> OnValueChange)
        {
            OnValueChange(uiName, propertieName, articulationDatas, CurrentSelectedArticulation);
        }
    }
}
