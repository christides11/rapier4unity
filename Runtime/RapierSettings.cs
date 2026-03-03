using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RapierPhysics
{
    public class RapierSettings : ScriptableObject
    {
        public const string settingsPath = "RapierSettings.asset";

        [SerializeField] public SimulationMode simulationMode = SimulationMode.FixedUpdate;

        private static RapierSettings instance;
        
        public static RapierSettings GetOrCreateSettings()
        {
            if (instance != null) return instance;
            instance = Resources.Load<RapierSettings>(settingsPath);
            if (instance == null)
            {
                instance = ScriptableObject.CreateInstance<RapierSettings>();
                instance.simulationMode = SimulationMode.FixedUpdate;
#if UNITY_EDITOR
                AssetDatabase.CreateAsset(instance, "Assets/Resources/" + settingsPath);
                AssetDatabase.SaveAssets();
#endif
            }
            
            return instance;
        }

        public static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }
}