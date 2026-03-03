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
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty(nameof(RapierSettings.PhysicsTicksPerSecond)));
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty(nameof(RapierSettings.NumSolverIterations)));
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty(nameof(RapierSettings.NumInternalPgsIterations)));
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty(nameof(RapierSettings.NumAdditionalFrictionIterations)));
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty(nameof(RapierSettings.NumInternalStabilizationIterations)));
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty(nameof(RapierSettings.MaxCcdSubsteps)));
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty(nameof(RapierSettings.ContactDampingRatio)));
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty(nameof(RapierSettings.ContactNaturalFrequency)));
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty(nameof(RapierSettings.JointNaturalFrequency)));
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty(nameof(RapierSettings.JointDampingRatio)));
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty(nameof(RapierSettings.LengthUnit)));
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty(nameof(RapierSettings.NormalizedPredictionDistance)));
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty(nameof(RapierSettings.NormalizedMaxCorrectiveVelocity)));
            m_CustomSettings.ApplyModifiedPropertiesWithoutUndo();
        }

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            return new RapierSettingsProvider("Project/Physics/Rapier", SettingsScope.Project);
        }
    }
}