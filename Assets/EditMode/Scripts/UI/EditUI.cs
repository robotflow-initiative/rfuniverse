using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using RFUniverse.EditMode;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;
using System.IO;
using TMPro;

namespace RFUniverse.EditMode
{
    public class EditUI : MonoBehaviour
    {
        public ButtonManagerBasicWithIcon create;
        public ButtonManagerBasicWithIcon select;
        public ButtonManagerBasicWithIcon move;
        public ButtonManagerBasicWithIcon rotate;
        public ButtonManagerBasicWithIcon scale;
        public ButtonManagerBasicWithIcon parent;
        public ButtonManagerBasicWithIcon attr;
        public GameObject transformField;
        public CustomInputField transformFieldX;
        public CustomInputField transformFieldY;
        public CustomInputField transformFieldZ;
        public ButtonManagerBasicIcon load;
        public ButtonManagerBasicIcon save;
        public ButtonManagerBasicIcon exit;
        public WindowManager createWindow;
        public CustomDropdown objectList;
        public ModalWindowManager quitWindow;
        public TextMeshProUGUI tips;
        public AttributeWindowUI attribute;
        public ListViewUI parentList;
        public FileSelectUI fileList;
        public CustomToggle groundToggle;
        public void Init(List<EditTypeData> typeData,
            Action<EditMode> OnChangeMode,
            Action<string> OnSelectCreate,
            Action<int> OnSelectUnitIndex,
            Action OnDeleteUnit,
            Action<string, string, object, int> OnValueChange,
            Action<Vector3> OnTransformSubmit,
            Action<string, int> OnAttributeChange,
            string filePath,
            Action<string, bool> OnSelectFile,
            Action<bool> OnGroundChange
            )
        {
            transformFieldX.inputText.onSubmit.AddListener((s) =>
            {
                OnTransformSubmit(new Vector3(float.Parse(transformFieldX.inputText.text), float.Parse(transformFieldY.inputText.text), float.Parse(transformFieldZ.inputText.text)));
            });
            transformFieldY.inputText.onSubmit.AddListener((s) =>
            {
                OnTransformSubmit(new Vector3(float.Parse(transformFieldX.inputText.text), float.Parse(transformFieldY.inputText.text), float.Parse(transformFieldZ.inputText.text)));
            });
            transformFieldZ.inputText.onSubmit.AddListener((s) =>
            {
                OnTransformSubmit(new Vector3(float.Parse(transformFieldX.inputText.text), float.Parse(transformFieldY.inputText.text), float.Parse(transformFieldZ.inputText.text)));
            });
            transformFieldX.inputText.onDeselect.AddListener((s) =>
            {
                OnTransformSubmit(new Vector3(float.Parse(transformFieldX.inputText.text), float.Parse(transformFieldY.inputText.text), float.Parse(transformFieldZ.inputText.text)));
            });
            transformFieldY.inputText.onDeselect.AddListener((s) =>
            {
                OnTransformSubmit(new Vector3(float.Parse(transformFieldX.inputText.text), float.Parse(transformFieldY.inputText.text), float.Parse(transformFieldZ.inputText.text)));
            });
            transformFieldZ.inputText.onDeselect.AddListener((s) =>
            {
                OnTransformSubmit(new Vector3(float.Parse(transformFieldX.inputText.text), float.Parse(transformFieldY.inputText.text), float.Parse(transformFieldZ.inputText.text)));
            });
            create.clickEvent.AddListener(() =>
            {
                OnChangeMode(EditMode.Create);
            });
            select.clickEvent.AddListener(() =>
            {
                OnChangeMode(EditMode.Select);
            });
            move.clickEvent.AddListener(() =>
            {
                OnChangeMode(EditMode.Move);
            });
            rotate.clickEvent.AddListener(() =>
            {
                OnChangeMode(EditMode.Rotate);
            });
            scale.clickEvent.AddListener(() =>
            {
                OnChangeMode(EditMode.Scale);
            });
            parent.clickEvent.AddListener(() =>
            {
                OnChangeMode(EditMode.Parent);
            });
            attr.clickEvent.AddListener(() =>
            {
                OnChangeMode(EditMode.Attr);
            });
            load.clickEvent.AddListener(() =>
            {
                fileList.Open(true, filePath);
            });
            save.clickEvent.AddListener(() =>
            {
                fileList.Open(false, filePath);
            });
            exit.clickEvent.AddListener(() =>
            {
                quitWindow.OpenWindow();
            });
            quitWindow.cancelButton.onClick.AddListener(() =>
            {
                quitWindow.CloseWindow();
            });
            quitWindow.confirmButton.onClick.AddListener(() =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                Application.Quit();
            });
            groundToggle.toggleObject.onValueChanged.AddListener((b) =>
            {
                OnGroundChange(b);
            });
            InitCreateWindow(typeData, OnSelectCreate);
            RefeshObjectList(new List<EditableUnit>());
            objectList.dropdownEvent.AddListener((i) => OnSelectUnitIndex(i));
            attribute.Init(OnDeleteUnit, OnValueChange, OnAttributeChange);
            fileList.Init(OnSelectFile);
            fileList.gameObject.SetActive(false);
        }
        public void GroundChange(bool b)
        {
            groundToggle.toggleObject.SetIsOnWithoutNotify(b);
        }
        public void ModeChange(EditMode editMode, EditableUnit unit)
        {
            parentList.gameObject.SetActive(false);
            createWindow.gameObject.SetActive(false);
            attribute.gameObject.SetActive(false);
            transformField.SetActive(false);
            switch (editMode)
            {
                case EditMode.Create:
                    createWindow.gameObject.SetActive(true);
                    break;
                case EditMode.Select:
                    break;
                case EditMode.Move:
                    transformField.SetActive(true);
                    break;
                case EditMode.Rotate:
                    transformField.SetActive(true);
                    break;
                case EditMode.Scale:
                    transformField.SetActive(true);
                    break;
                case EditMode.Parent:
                    parentList.gameObject.SetActive(true);
                    break;
                case EditMode.Attr:
                    attribute.gameObject.SetActive(true);
                    attribute.SetAttr(unit.attr);
                    break;
            }
        }
        public void UnitChange(EditableUnit unit)
        {

        }
        public void TransformChange(Vector3 vector3)
        {
            transformFieldX.inputText.text = vector3.x.ToString();
            transformFieldY.inputText.text = vector3.y.ToString();
            transformFieldZ.inputText.text = vector3.z.ToString();
        }

        void InitCreateWindow(List<EditTypeData> typeData, Action<string> OnSelectCreate)
        {
            createWindow.onWindowChange.AddListener((i) => OnSelectCreate(null));
            GameObject button = createWindow.windows[0].buttonObject;
            GameObject window = createWindow.windows[0].windowObject;
            createWindow.windows.Clear();
            for (int i = 0; i < typeData.Count; i++)
            {
                WindowManager.WindowItem item = new WindowManager.WindowItem();
                item.windowName = typeData[i].name;
                item.buttonObject = Instantiate(button, button.transform.parent);
                item.windowObject = Instantiate(window, window.transform.parent);
                item.buttonObject.GetComponent<CreateButtonUI>().SetType(typeData[i].image, typeData[i].name);
                item.windowObject.GetComponent<CreateWindowUI>().SetAttr(typeData[i].attrs, OnSelectCreate);
                createWindow.windows.Add(item);
            }
            Destroy(button);
            Destroy(window);
            createWindow.InitializeWindows();
        }
        public void RefeshObjectList(List<EditableUnit> units)
        {
            objectList.dropdownItems.Clear();
            foreach (var item in units)
            {
                CustomDropdown.Item oneUnit = new CustomDropdown.Item();
                oneUnit.itemName = item.data.id + "  " + item.data.name;
                oneUnit.itemIcon = item.image;
                objectList.dropdownItems.Add(oneUnit);
            }
            objectList.SetupDropdown();
        }


        public void RefeshParentList(List<string> parents, Action<string> OnSelectParent)
        {
            parentList.RefeshList(parents, OnSelectParent);
        }

        public void SetTips(string s)
        {
            tips.text = s;
        }
    }
}
