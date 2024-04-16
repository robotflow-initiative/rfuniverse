/*
© Siemens AG, 2018
Author: Suzannah Smith (suzannah.smith@siemens.com)
Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
<http://www.apache.org/licenses/LICENSE-2.0>.
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using UnityEditor;
using UnityEngine;

namespace Unity.Robotics.UrdfImporter.Editor
{
    [CustomEditor(typeof(UrdfLink))]
    public class UrdfLinkEditor : UnityEditor.Editor
    {
        private UrdfLink urdfLink;
        private UrdfJoint.JointTypes jointType = UrdfJoint.JointTypes.Fixed;

        public override void OnInspectorGUI()
        {
            urdfLink = (UrdfLink)target;

            GUILayout.Space(5);
            urdfLink.IsBaseLink = EditorGUILayout.Toggle("Is Base Link", urdfLink.IsBaseLink);
            GUILayout.Space(5);

            EditorGUILayout.BeginVertical("HelpBox");
            jointType = (UrdfJoint.JointTypes)EditorGUILayout.EnumPopup(
                "Child Joint Type", jointType);

            if (GUILayout.Button("Add child link (with joint)"))
            {
                UrdfLink childLink = UrdfLinkExtensions.Create(urdfLink.transform).GetComponent<UrdfLink>();
                UrdfJoint.Create(childLink.gameObject, jointType);
            }

            if (GUILayout.Button("Generation Visuals and Collisions"))
            {

                GameObject vis = GameObject.Instantiate(urdfLink.gameObject);
                vis.transform.DetachChildren();
                DestroyImmediate(vis.GetComponent<UrdfJointRevolute>());
                DestroyImmediate(vis.GetComponent<UrdfLink>());
                DestroyImmediate(vis.GetComponent<ArticulationBody>());

                MeshFilter mf = urdfLink.GetComponent<MeshFilter>();

                UrdfVisuals uvs = UrdfVisualsExtensions.Create(urdfLink.transform);
                UrdfCollisions ucs = UrdfCollisionsExtensions.Create(urdfLink.transform);

                UrdfVisual uv = UrdfVisualExtensions.Create(uvs.transform, GeometryTypes.Mesh);
                UrdfCollision uc = UrdfCollisionExtensions.Create(ucs.transform, GeometryTypes.Mesh);

                vis.transform.parent = uv.transform;
                vis.transform.localPosition = Vector3.zero;
                vis.transform.localRotation = Quaternion.identity;
                uc.GetComponentInChildren<MeshCollider>().sharedMesh = mf.sharedMesh;

                DestroyImmediate(urdfLink.GetComponent<Renderer>());
                DestroyImmediate(urdfLink.GetComponent<MeshFilter>());
            }

            EditorGUILayout.EndVertical();
        }
    }
}
