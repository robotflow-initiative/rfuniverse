using Newtonsoft.Json;
using RFUniverse.Attributes;
using RFUniverse.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Obi;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RFUniverse.EditMode
{
    public enum EditMode
    {
        Create,//创建模式
        Select,//选择模式
        Move,//移动模式
        Rotate,//旋转模式
        Scale,//缩放模式
        Parent,//父物体模式
        Attr//属性编辑模式
    }

    public class EditMain : RFUniverseMain
    {
        EditMode currentEditMode = EditMode.Select;
        public EditMode CurrentEditMode
        {
            get
            {
                return currentEditMode;
            }
            set
            {
                if (value == EditMode.Move || value == EditMode.Rotate || value == EditMode.Scale || value == EditMode.Parent || value == EditMode.Attr)
                    if (CurrentSelectedUnit == null)
                        value = EditMode.Select;
                if (currentEditMode == value) return;
                currentEditMode = value;
                jointLimitView.gameObject.SetActive(false);
                ui.SetTips($"EditMode : {currentEditMode}");
                ChangeAttribute(string.Empty, -1);
                OnModeChange(currentEditMode, CurrentSelectedUnit);
            }
        }
        public Action<EditMode, EditableUnit> OnModeChange;

        EditableUnit currentSelectedUnit;
        public EditableUnit CurrentSelectedUnit
        {
            get
            {
                return currentSelectedUnit;
            }
            set
            {
                if (currentSelectedUnit == value) return;
                if (currentSelectedUnit != null)
                    currentSelectedUnit.SetSelect(false);
                currentSelectedUnit = value;
                OnSelectedUnitChange(currentSelectedUnit);
                if (currentSelectedUnit == null)
                    ui.SetTips($"Selected : null");
                else
                {
                    currentSelectedUnit.SetSelect(true);
                    ui.SetTips($"Selected : {currentSelectedUnit.data.id} : {currentSelectedUnit.name}");
                }
                axis.ReSet(currentSelectedUnit ? currentSelectedUnit.attr : null);
            }
        }
        public Action<EditableUnit> OnSelectedUnitChange;

        EditableUnit currentSelectedParentUnit = null;
        string currentSelectedCreate = null;



        public EditUI ui;
        public EditableUnit unit;
        public TransformAxis axis;
        public JointLimitView jointLimitView;
        public ColliderView colldierView;
        public EditAssetsData assetsData;

        private Dictionary<int, EditableUnit> editableUnits = new Dictionary<int, EditableUnit>();

        public static EditMain Instance = null;

        void OnValidate()
        {
            Instance = this;
        }
        string filePath => Application.streamingAssetsPath + "/SceneData";
        void Awake()
        {
            Instance = this;
            Physics.autoSimulation = false;
            Physics.gravity = Vector3.zero;
            jointLimitView.gameObject.SetActive(false);
            colldierView.gameObject.SetActive(false);

            UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<EditAssetsData>("AssetsData").Completed += (handle) =>
            {
                assetsData = handle.Result;
                ui.Init(assetsData.typeData,
                (mode) => CurrentEditMode = mode,
                (s) => currentSelectedCreate = s,
                (index) => SelectUnitIndex(index),
                () => DeleteCurrentUnit(),
                (sa, sp, o, i) => ChangeValue(sa, sp, o, i),
                (v3) => ChangeTransform(v3),
                (s, i) => ChangeAttribute(s, i),
                filePath,
                (s, b) => SelectFile(s, b),
                (b) => ChangeGround(b)
                );
                OnModeChange += ui.ModeChange;
                OnModeChange += axis.ModeChange;
                OnSelectedUnitChange += ui.UnitChange;
                OnModeChange(CurrentEditMode, CurrentSelectedUnit);
                OnSelectedUnitChange(CurrentSelectedUnit);
            };
        }
        void ChangeGround(bool b)
        {
            Ground = b;
            ui.GroundChange(b);
        }
        void ChangeAttribute(string attr, int index)
        {
            if (attr == "Articulations")
                jointLimitView.SetArticulationData((CurrentSelectedUnit.attr as ControllerAttr).ArticulationDatas[index]);
            else
                jointLimitView.SetArticulationData(null);
            if (attr == "Colliders")
                colldierView.SetColliderData((CurrentSelectedUnit.attr as ColliderAttr).ColliderDatas[index]);
            else
                colldierView.SetColliderData(null);
        }
        void ChangeTransform(Vector3 vector3)
        {
            switch (CurrentEditMode)
            {
                case EditMode.Move:
                    CurrentSelectedUnit.attr.transform.localPosition = vector3;
                    axis.ReSet(CurrentSelectedUnit.attr);
                    break;
                case EditMode.Rotate:
                    CurrentSelectedUnit.attr.transform.localEulerAngles = vector3;
                    axis.ReSet(CurrentSelectedUnit.attr);
                    break;
                case EditMode.Scale:
                    CurrentSelectedUnit.attr.transform.localScale = vector3;
                    axis.ReSet(CurrentSelectedUnit.attr);
                    break;
            }
        }
        void ChangeValue(string attrName, string propertyName, object value, int index)
        {
            CurrentSelectedUnit.attr.GetType().GetProperty(propertyName).SetValue(CurrentSelectedUnit.attr, value);
            ChangeAttribute(attrName, index);
        }
        public void SelectParent(string name)
        {
            ui.SetTips($"SetParent : {CurrentSelectedUnit.data.id} : {CurrentSelectedUnit.name} -> {currentSelectedParentUnit.data.id} : {currentSelectedParentUnit.name} : {name}");
            CurrentSelectedUnit.attr.SetParent(currentSelectedParentUnit.attr.ID, name);
        }
        public void SelectUnit(EditableUnit unit)
        {
            if (CurrentEditMode == EditMode.Parent)
            {
                if (currentSelectedParentUnit == unit) return;
                currentSelectedParentUnit = unit;
                if (CurrentSelectedUnit == currentSelectedParentUnit)
                {
                    CurrentSelectedUnit.attr.transform.SetParent(null);
                    ui.RefeshParentList(new List<string>(), SelectParent);
                    return;
                }
                Transform[] transforms = currentSelectedParentUnit.attr.GetComponentsInChildren<Transform>();
                List<string> names = new List<string>();
                foreach (var item in transforms)
                {
                    names.Add(item.name);
                }
                ui.RefeshParentList(names, SelectParent);
            }
            else
                CurrentSelectedUnit = unit;
        }
        public void SelectUnitID(int id)
        {
            if (editableUnits.TryGetValue(id, out EditableUnit selectedUnit))
            {
                CurrentSelectedUnit = selectedUnit;
            }
        }
        void SelectUnitIndex(int index)
        {
            if (CurrentEditMode != EditMode.Select) return;
            if (editableUnits.Count > index)
                CurrentSelectedUnit = editableUnits.Values.ToList()[index];
        }
        public void OnSelectCreate(string path)
        {
            currentSelectedCreate = path;
            Debug.Log("SelectCreate:" + path);
        }

        void CreateUnit(string name, int id, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            if (name == null) return;
            if (id < 0)
            {
                do
                {
                    id = UnityEngine.Random.Range(10000000, 99999999);
                }
                while (editableUnits.ContainsKey(id));
            }
            BaseAttrData baseAttrData = new BaseAttrData()
            {
                name = name,
                id = id,
                position = new float[] { position.x, position.y, position.z },
                rotation = new float[] { rotation.x, rotation.y, rotation.z },
                scale = new float[] { scale.x, scale.y, scale.z },
                parentID = -1
            };
            CreateUnit(baseAttrData, false);
        }
        void CreateUnit(BaseAttrData baseAttrData, bool allSet = true)
        {
            EditableUnit newUnit = Instantiate(unit);
            // newUnit.name = baseAttrData.name;
            // newUnit.id = baseAttrData.id;
            // newUnit.transform.localPosition = new Vector3(baseAttrData.position[0], baseAttrData.position[1], baseAttrData.position[2]);
            // newUnit.transform.localEulerAngles = new Vector3(baseAttrData.rotation[0], baseAttrData.rotation[1], baseAttrData.rotation[2]);
            // newUnit.transform.localScale = new Vector3(baseAttrData.scale[0], baseAttrData.scale[1], baseAttrData.scale[2]);
            newUnit.data = baseAttrData;
            newUnit.Load(allSet);
            editableUnits.Add(newUnit.data.id, newUnit);
        }
        void DeleteAllUnit()
        {
            CurrentSelectedUnit = null;
            foreach (var item in editableUnits.Values)
            {
                DestroyImmediate(item.attr.gameObject);
            }
            editableUnits.Clear();
            BaseAttr.ClearAttr();
            ui.RefeshObjectList(editableUnits.Values.ToList());
        }

        void DeleteUnit(EditableUnit unit)
        {
            if (unit == null) return;
            if (CurrentSelectedUnit == unit)
                CurrentSelectedUnit = null;
            editableUnits.Remove(unit.data.id);
            BaseAttr.RemoveAttr(unit.attr);
            Destroy(unit.attr.gameObject);
            ui.RefeshObjectList(editableUnits.Values.ToList());
        }
        void DeleteCurrentUnit()
        {
            DeleteUnit(CurrentSelectedUnit);
        }
        void SelectFile(string file, bool mode)
        {
            if (mode)
                LoadData(file);
            else
                SaveData(file);
        }
        void LoadData(string file)
        {
            DeleteAllUnit();
            string dataStriing = File.ReadAllText(filePath + "/" + file);
            SceneData data = JsonConvert.DeserializeObject<SceneData>(dataStriing, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            Ground = data.ground;
            mainCamera.transform.position = new Vector3(data.cameraPosition[0], data.cameraPosition[1], data.cameraPosition[2]);
            mainCamera.transform.eulerAngles = new Vector3(data.cameraRotation[0], data.cameraRotation[1], data.cameraRotation[2]);
            ground.transform.position = new Vector3(data.groundPosition[0], data.groundPosition[1], data.groundPosition[2]);
            AssetManager.Instance.PreLoadAssetsAsync(data.assetsData.Select((a) => a.name).ToList(), () =>
            {
                List<int> headID = new List<int>();
                BaseAttrData temp;
                bool dirty = true;
                while (dirty)
                {
                    dirty = false;
                    for (int i = 0; i < data.assetsData.Count; i++)
                    {
                        if (data.assetsData[i].parentID > 0 && !headID.Contains(data.assetsData[i].parentID))
                        {
                            temp = data.assetsData[i];
                            data.assetsData.Remove(temp);
                            data.assetsData.Add(temp);
                            dirty = true;
                            i--;
                        }
                        else
                        {
                            headID.Add(data.assetsData[i].id);
                        }
                    }
                }

                foreach (var item in data.assetsData)
                {
                    CreateUnit(item);
                }
                ui.RefeshObjectList(editableUnits.Values.ToList());
            });
        }
        void SaveData(string file)
        {
            SceneData data = new SceneData();
            data.ground = Ground;
            if (!PlayerMain.Instance.mainCamera)
            {
                UnityEngine.Debug.LogError("No Camera");
                return;
            }
            data.cameraPosition = new float[] { mainCamera.transform.position.x, mainCamera.transform.position.y, mainCamera.transform.position.z };
            data.cameraRotation = new float[] { mainCamera.transform.eulerAngles.x, mainCamera.transform.eulerAngles.y, mainCamera.transform.eulerAngles.z };
            if (data.ground)
                data.groundPosition = new float[] { ground.transform.position.x, ground.transform.position.y, ground.transform.position.z };
            foreach (var item in editableUnits.Values)
            {
                data.assetsData.Add(item.attr.GetAttrData());
            }
            string dataString = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });
            File.WriteAllText(filePath + "/" + file, dataString);
        }
        private void Update()
        {
            switch (CurrentEditMode)
            {
                case EditMode.Create:
                    if (Input.GetMouseButtonDown(0))
                        if (!EventSystem.current.IsPointerOverGameObject())
                        {
                            Plane p = new Plane(Vector3.up, Vector3.zero);
                            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
                            if (p.Raycast(r, out float enter))
                            {
                                CreateUnit(currentSelectedCreate, -1, r.origin + r.direction * enter, Vector3.zero, Vector3.one);
                                ui.RefeshObjectList(editableUnits.Values.ToList());
                            }
                        }
                    break;
            }

        }
    }
}
