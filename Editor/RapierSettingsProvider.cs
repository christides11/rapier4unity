using UnityEditor;
using UnityEngine.UIElements;

namespace RapierPhysics
{
    class RapierSettingsProvider : SettingsProvider
    {
        private SerializedObject m_CustomSettings;

        public const string customSettingsPath = "Assets/Resources/RapierSettings.asset";

        public RapierSettingsProvider(string path, SettingsScope scope = SettingsScope.Project)
            : base(path, scope)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            m_CustomSettings = RapierSettings.GetSerializedSettings();
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("simulationMode"));
            m_CustomSettings.ApplyModifiedPropertiesWithoutUndo();
        }

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            return new RapierSettingsProvider("Project/Physics/Rapier", SettingsScope.Project);
        }
    }
}