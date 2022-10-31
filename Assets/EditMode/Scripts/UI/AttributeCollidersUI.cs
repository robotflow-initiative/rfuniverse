using Michsky.UI.ModernUIPack;
using RFUniverse.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UI;
using TMPro;

namespace RFUniverse.EditMode
{
    public class AttributeCollidersUI : AttributeBaseUI
    {
        const string uiName = "Colliders";
        public List<Toggle> collidersToggle = new List<Toggle>();
        public CustomDropdown colliderType;
        public CustomInputField positionX;
        public CustomInputField positionY;
        public CustomInputField positionZ;
        public CustomInputField rotationX;
        public CustomInputField rotationY;
        public CustomInputField rotationZ;
        public CustomInputField scaleX;
        public CustomInputField scaleY;
        public CustomInputField scaleZ;
        public CustomInputField radius;
        public CustomInputField height;
        public CustomDropdown direction;
        public CustomInputField bounciness;
        public CustomInputField dynamicFriction;
        public CustomInputField staticFriction;
        public CustomDropdown bounceCombine;
        public CustomDropdown frictionCombine;

        List<ColliderData> colliderDatas;

        int currentSelectedCollider = 0;
        int CurrentSelectedCollider
        {
            get { return currentSelectedCollider; }
            set
            {
                if (colliderDatas.Count == 0) return;
                if (value >= colliderDatas.Count)
                    value = colliderDatas.Count - 1;
                currentSelectedCollider = value;
                OnAttributeChange(uiName, currentSelectedCollider);
                collidersToggle[currentSelectedCollider].SetIsOnWithoutNotify(true);
                CurrentSelectedType = colliderDatas[currentSelectedCollider].type;
                positionX.inputText.text = colliderDatas[currentSelectedCollider].position[0].ToString();
                positionY.inputText.text = colliderDatas[currentSelectedCollider].position[1].ToString();
                positionZ.inputText.text = colliderDatas[currentSelectedCollider].position[2].ToString();
                rotationX.inputText.text = colliderDatas[currentSelectedCollider].rotation[0].ToString();
                rotationY.inputText.text = colliderDatas[currentSelectedCollider].rotation[1].ToString();
                rotationZ.inputText.text = colliderDatas[currentSelectedCollider].rotation[2].ToString();
                scaleX.inputText.text = colliderDatas[currentSelectedCollider].scale[0].ToString();
                scaleY.inputText.text = colliderDatas[currentSelectedCollider].scale[1].ToString();
                scaleZ.inputText.text = colliderDatas[currentSelectedCollider].scale[2].ToString();
                radius.inputText.text = colliderDatas[currentSelectedCollider].radius.ToString();
                height.inputText.text = colliderDatas[currentSelectedCollider].height.ToString();
                direction.selectedItemIndex = colliderDatas[currentSelectedCollider].direction;
                direction.SetupDropdown();
                bounciness.inputText.text = colliderDatas[currentSelectedCollider].physicMateria.bounciness.ToString();
                dynamicFriction.inputText.text = colliderDatas[currentSelectedCollider].physicMateria.dynamicFriction.ToString();
                staticFriction.inputText.text = colliderDatas[currentSelectedCollider].physicMateria.staticFriction.ToString();
                bounceCombine.selectedItemIndex = (int)colliderDatas[currentSelectedCollider].physicMateria.bounceCombine;
                direction.SetupDropdown();
                frictionCombine.selectedItemIndex = (int)colliderDatas[currentSelectedCollider].physicMateria.frictionCombine;
                direction.SetupDropdown();
            }
        }
        ColliderType CurrentSelectedType
        {
            set
            {
                colliderType.selectedItemIndex = (int)value;
                colliderType.SetupDropdown();
                colliderDatas[CurrentSelectedCollider].type = value;

                radius.gameObject.SetActive(false);
                height.gameObject.SetActive(false);
                direction.gameObject.SetActive(false);
                switch ((ColliderType)colliderType.selectedItemIndex)
                {
                    case ColliderType.None:
                        break;
                    case ColliderType.Box:
                        break;
                    case ColliderType.Sphere:
                        radius.gameObject.SetActive(true);
                        break;
                    case ColliderType.Capsule:
                        radius.gameObject.SetActive(true);
                        height.gameObject.SetActive(true);
                        direction.gameObject.SetActive(true);
                        break;
                    case ColliderType.Mesh:
                        break;
                    case ColliderType.Original:
                        break;
                }
            }
        }


        public override void Init(BaseAttr baseAttr, PropertyInfo info, Action<string, string, object, int> OnValueChange, Action<string, int> OnAttributeChanged)
        {
            base.Init(baseAttr, info, OnValueChange, OnAttributeChanged);
            colliderDatas = (List<ColliderData>)info.GetValue(baseAttr);
            // if (colliderDatas.Count == 0)
            // {
            //     colliderType.gameObject.SetActive(false);
            //     positionX.gameObject.SetActive(false);
            //     positionY.gameObject.SetActive(false);
            //     positionZ.gameObject.SetActive(false);
            //     rotationX.gameObject.SetActive(false);
            //     rotationY.gameObject.SetActive(false);
            //     rotationZ.gameObject.SetActive(false);
            //     scaleX.gameObject.SetActive(false);
            //     scaleY.gameObject.SetActive(false);
            //     scaleZ.gameObject.SetActive(false);
            //     radius.gameObject.SetActive(false);
            //     height.gameObject.SetActive(false);
            //     direction.gameObject.SetActive(false);
            //     bounciness.gameObject.SetActive(false);
            //     dynamicFriction.gameObject.SetActive(false);
            //     staticFriction.gameObject.SetActive(false);
            //     bounceCombine.gameObject.SetActive(false);
            //     frictionCombine.gameObject.SetActive(false);
            // }
            for (int i = 0; i < colliderDatas.Count; i++)
            {
                if (collidersToggle.Count <= i)
                {
                    collidersToggle.Add(Instantiate(collidersToggle[0], collidersToggle[0].transform.parent));
                }
                int index = i;
                collidersToggle[i].onValueChanged.AddListener((b) =>
                {
                    if (b)
                    {
                        CurrentSelectedCollider = index;
                    }
                });
                collidersToggle[i].GetComponentInChildren<TextMeshProUGUI>().text = (i + 1).ToString();
            }
            colliderType.dropdownEvent.AddListener((i) =>
            {
                CurrentSelectedType = (ColliderType)i;
                ValueChanged(OnValueChange);
            });
            positionX.inputText.onSubmit.AddListener((s) => { PositionXChange(s, OnValueChange); });
            positionX.inputText.onDeselect.AddListener((s) => { PositionXChange(s, OnValueChange); });
            positionY.inputText.onSubmit.AddListener((s) => { PositionYChange(s, OnValueChange); });
            positionY.inputText.onDeselect.AddListener((s) => { PositionYChange(s, OnValueChange); });
            positionZ.inputText.onSubmit.AddListener((s) => { PositionZChange(s, OnValueChange); });
            positionZ.inputText.onDeselect.AddListener((s) => { PositionZChange(s, OnValueChange); });
            rotationX.inputText.onSubmit.AddListener((s) => { RotationXChange(s, OnValueChange); });
            rotationX.inputText.onDeselect.AddListener((s) => { RotationXChange(s, OnValueChange); });
            rotationY.inputText.onSubmit.AddListener((s) => { RotationYChange(s, OnValueChange); });
            rotationY.inputText.onDeselect.AddListener((s) => { RotationYChange(s, OnValueChange); });
            rotationZ.inputText.onSubmit.AddListener((s) => { RotationZChange(s, OnValueChange); });
            rotationZ.inputText.onDeselect.AddListener((s) => { RotationZChange(s, OnValueChange); });
            scaleX.inputText.onSubmit.AddListener((s) => { ScaleXChange(s, OnValueChange); });
            scaleX.inputText.onDeselect.AddListener((s) => { ScaleXChange(s, OnValueChange); });
            scaleY.inputText.onSubmit.AddListener((s) => { ScaleYChange(s, OnValueChange); });
            scaleY.inputText.onDeselect.AddListener((s) => { ScaleYChange(s, OnValueChange); });
            scaleZ.inputText.onSubmit.AddListener((s) => { ScaleZChange(s, OnValueChange); });
            scaleZ.inputText.onDeselect.AddListener((s) => { ScaleZChange(s, OnValueChange); });
            radius.inputText.onSubmit.AddListener((s) => { RadiusChange(s, OnValueChange); });
            radius.inputText.onDeselect.AddListener((s) => { RadiusChange(s, OnValueChange); });
            height.inputText.onSubmit.AddListener((s) => { HeightChange(s, OnValueChange); });
            height.inputText.onDeselect.AddListener((s) => { HeightChange(s, OnValueChange); });
            direction.dropdownEvent.AddListener((i) =>
            {
                colliderDatas[CurrentSelectedCollider].direction = i;
                ValueChanged(OnValueChange);
            });
            bounciness.inputText.onSubmit.AddListener((s) => { BouncinessChange(s, OnValueChange); });
            bounciness.inputText.onDeselect.AddListener((s) => { BouncinessChange(s, OnValueChange); });
            dynamicFriction.inputText.onSubmit.AddListener((s) => { DynamicFrictionChange(s, OnValueChange); });
            dynamicFriction.inputText.onDeselect.AddListener((s) => { DynamicFrictionChange(s, OnValueChange); });
            staticFriction.inputText.onSubmit.AddListener((s) => { StaticFrictionChange(s, OnValueChange); });
            staticFriction.inputText.onDeselect.AddListener((s) => { StaticFrictionChange(s, OnValueChange); });
            positionY.inputText.onSubmit.AddListener((s) =>
            {
                float f = 0;
                try
                {
                    f = float.Parse(s);
                }
                catch
                {
                    positionX.inputText.SetTextWithoutNotify(f.ToString());
                }
                colliderDatas[CurrentSelectedCollider].position[1] = f;
                ValueChanged(OnValueChange);
            });
            positionZ.inputText.onSubmit.AddListener((s) =>
            {

            });
            CurrentSelectedCollider = CurrentSelectedCollider;
        }
        public override void ReSet()
        {
            CurrentSelectedCollider = 0;
        }
        void PositionXChange(string s, Action<string, string, object, int> OnValueChange)
        {
            float f = 0;
            try
            {
                f = float.Parse(s);
            }
            catch
            {
                positionX.inputText.SetTextWithoutNotify(f.ToString());
            }
            colliderDatas[CurrentSelectedCollider].position[0] = f;
            ValueChanged(OnValueChange);
        }
        void PositionYChange(string s, Action<string, string, object, int> OnValueChange)
        {
            float f = 0;
            try
            {
                f = float.Parse(s);
            }
            catch
            {
                positionY.inputText.SetTextWithoutNotify(f.ToString());
            }
            colliderDatas[CurrentSelectedCollider].position[1] = f;
            ValueChanged(OnValueChange);
        }
        void PositionZChange(string s, Action<string, string, object, int> OnValueChange)
        {
            float f = 0;
            try
            {
                f = float.Parse(s);
            }
            catch
            {
                positionZ.inputText.SetTextWithoutNotify(f.ToString());
            }
            colliderDatas[CurrentSelectedCollider].position[2] = f;
            ValueChanged(OnValueChange);
        }
        void RotationXChange(string s, Action<string, string, object, int> OnValueChange)
        {
            float f = 0;
            try
            {
                f = float.Parse(s);
            }
            catch
            {
                rotationX.inputText.SetTextWithoutNotify(f.ToString());
            }
            colliderDatas[CurrentSelectedCollider].rotation[0] = f;
            ValueChanged(OnValueChange);
        }
        void RotationYChange(string s, Action<string, string, object, int> OnValueChange)
        {
            float f = 0;
            try
            {
                f = float.Parse(s);
            }
            catch
            {
                rotationY.inputText.SetTextWithoutNotify(f.ToString());
            }
            colliderDatas[CurrentSelectedCollider].rotation[1] = f;
            ValueChanged(OnValueChange);
        }
        void RotationZChange(string s, Action<string, string, object, int> OnValueChange)
        {
            float f = 0;
            try
            {
                f = float.Parse(s);
            }
            catch
            {
                rotationZ.inputText.SetTextWithoutNotify(f.ToString());
            }
            colliderDatas[CurrentSelectedCollider].rotation[2] = f;
            ValueChanged(OnValueChange);
        }
        void ScaleXChange(string s, Action<string, string, object, int> OnValueChange)
        {
            float f = 0;
            try
            {
                f = float.Parse(s);
            }
            catch
            {
                scaleX.inputText.SetTextWithoutNotify(f.ToString());
            }
            colliderDatas[CurrentSelectedCollider].scale[0] = f;
            ValueChanged(OnValueChange);
        }
        void ScaleYChange(string s, Action<string, string, object, int> OnValueChange)
        {
            float f = 0;
            try
            {
                f = float.Parse(s);
            }
            catch
            {
                scaleY.inputText.SetTextWithoutNotify(f.ToString());
            }
            colliderDatas[CurrentSelectedCollider].scale[1] = f;
            ValueChanged(OnValueChange);
        }
        void ScaleZChange(string s, Action<string, string, object, int> OnValueChange)
        {
            float f = 0;
            try
            {
                f = float.Parse(s);
            }
            catch
            {
                scaleZ.inputText.SetTextWithoutNotify(f.ToString());
            }
            colliderDatas[CurrentSelectedCollider].scale[2] = f;
            ValueChanged(OnValueChange);
        }
        void RadiusChange(string s, Action<string, string, object, int> OnValueChange)
        {
            float f = 0;
            try
            {
                f = float.Parse(s);
            }
            catch
            {
                radius.inputText.SetTextWithoutNotify(f.ToString());
            }
            colliderDatas[CurrentSelectedCollider].radius = f;
            ValueChanged(OnValueChange);
        }
        void HeightChange(string s, Action<string, string, object, int> OnValueChange)
        {
            float f = 0;
            try
            {
                f = float.Parse(s);
            }
            catch
            {
                height.inputText.SetTextWithoutNotify(f.ToString());
            }
            colliderDatas[CurrentSelectedCollider].height = f;
            ValueChanged(OnValueChange);
        }
        void BouncinessChange(string s, Action<string, string, object, int> OnValueChange)
        {
            float f = 0;
            try
            {
                f = float.Parse(s);
            }
            catch
            {
                bounciness.inputText.SetTextWithoutNotify(f.ToString());
            }
            colliderDatas[CurrentSelectedCollider].physicMateria.bounciness = f;
            ValueChanged(OnValueChange);
        }
        void DynamicFrictionChange(string s, Action<string, string, object, int> OnValueChange)
        {
            float f = 0;
            try
            {
                f = float.Parse(s);
            }
            catch
            {
                dynamicFriction.inputText.SetTextWithoutNotify(f.ToString());
            }
            colliderDatas[CurrentSelectedCollider].physicMateria.dynamicFriction = f;
            ValueChanged(OnValueChange);
        }
        void StaticFrictionChange(string s, Action<string, string, object, int> OnValueChange)
        {
            float f = 0;
            try
            {
                f = float.Parse(s);
            }
            catch
            {
                staticFriction.inputText.SetTextWithoutNotify(f.ToString());
            }
            colliderDatas[CurrentSelectedCollider].physicMateria.staticFriction = f;
            ValueChanged(OnValueChange);
        }
        protected override void ValueChanged(Action<string, string, object, int> OnValueChange)
        {
            OnValueChange(uiName, propertieName, colliderDatas, CurrentSelectedCollider);
        }
    }
}
