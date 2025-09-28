#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MalgarHotel.EditorTools
{
    public static class HierarchyCleanup
    {
        [MenuItem("Tools/Scene/Clean & Name")] 
        public static void CleanAndName()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogWarning("No active scene to clean.");
                return;
            }

            foreach (var rootObject in scene.GetRootGameObjects())
            {
                ProcessGameObject(rootObject);
            }

            EditorUtility.DisplayDialog("Hierarchy Cleanup", $"Processed scene '{scene.name}'.", "OK");
        }

        private static void ProcessGameObject(GameObject go)
        {
            if (go == null)
            {
                return;
            }

            if (go.GetComponent<CharacterController>() != null || go.GetComponent<MalgarHotel.Player.PlayerController>() != null)
            {
                go.name = "Player";
                foreach (var camera in go.GetComponentsInChildren<Camera>(true))
                {
                    camera.gameObject.name = camera.transform.parent == go.transform ? "PlayerCamera" : camera.gameObject.name;
                }

                foreach (var light in go.GetComponentsInChildren<Light>(true))
                {
                    if (light.type == LightType.Spot || light.type == LightType.Point)
                    {
                        light.gameObject.name = "Flashlight";
                    }
                }
            }
            else if (go.GetComponent<Canvas>() != null)
            {
                if (go.GetComponent<MalgarHotel.Core.HudController>() != null)
                {
                    go.name = "Canvas (HUD)";
                }
                else if (go.GetComponent<MalgarHotel.Core.PauseMenuController>() != null)
                {
                    go.name = "Canvas (PauseMenu)";
                }
            }
            else if (go.GetComponent<MalgarHotel.World.FusePanelMiniGame>() != null)
            {
                go.name = "FusePanel";
            }
            else if (go.GetComponent<MalgarHotel.World.BatteryPickup>() != null)
            {
                go.name = "BatteryPickup";
            }
            else if (go.GetComponent<MalgarHotel.World.ElevatorTrigger>() != null)
            {
                go.name = "ElevatorExit";
            }
            else if (go.GetComponent<MalgarHotel.World.ProceduralCorridorGenerator>() != null)
            {
                go.name = "ProceduralCorridorGenerator";
            }
            else if (go.GetComponent<AudioSource>() != null && go.name.Contains("Audio"))
            {
                go.name = "ProceduralAmbience";
            }

            foreach (Transform child in go.transform)
            {
                if (child == null)
                {
                    continue;
                }

                if (child.name.StartsWith("Socket_"))
                {
                    child.name = child.name.ToUpperInvariant();
                }

                ProcessGameObject(child.gameObject);
            }

            SerializedObject so = new SerializedObject(go);
            SerializedProperty components = so.FindProperty("m_Component");
            bool missingScriptFound = false;
            for (int i = components.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty componentProp = components.GetArrayElementAtIndex(i);
                SerializedProperty componentRef = componentProp.FindPropertyRelative("component");
                if (componentRef != null && componentRef.objectReferenceValue == null)
                {
                    missingScriptFound = true;
                    components.DeleteArrayElementAtIndex(i);
                }
            }

            if (missingScriptFound)
            {
                so.ApplyModifiedProperties();
            }
        }
    }
}
#endif
