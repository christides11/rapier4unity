using RapierPhysics;
using UnityEngine;
using UnityEngine.LowLevel;

namespace Packages.rapier4unity.Runtime
{
	/// <summary>
	/// This class is used to wrap the physics event system.
	/// Everything from OnTriggerEnter to OnJointBreak is wrapped here.
	/// </summary>
#if UNITY_EDITOR
	[UnityEditor.InitializeOnLoad]
#endif
	public static class PhysicsEventWrapper
	{
#if UNITY_EDITOR
		static PhysicsEventWrapper()
		{
			// Ensure that we teardown the physics bindings when exiting play mode
			UnityEditor.EditorApplication.playModeStateChanged += state =>
			{
				if (state == UnityEditor.PlayModeStateChange.EnteredEditMode)
				{
					if (RapierBindings.IsAvailable)
					{
						RapierBindings.Teardown();
						RapierBindings.UnloadCalls();
					}
				}
			};
		}
#endif

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void SetupPlayerLoop()
		{
			RapierBindings.LoadCalls();
			RapierBindings.Version();
			var rapierSettings = RapierSettings.GetOrCreateSettings();
			rapierSettings.ApplySettings();

			// Get the current player loop
			PlayerLoopSystem loop = PlayerLoop.GetCurrentPlayerLoop();
			for (int i = 0; i < loop.subSystemList.Length; i++)
			{
				PlayerLoopSystem subSystem = loop.subSystemList[i];
				if (subSystem.type == typeof(UnityEngine.PlayerLoop.FixedUpdate))
				{
					// Append FixedUpdate to subsytems
					PlayerLoopSystem[] newSubSystems = new PlayerLoopSystem[subSystem.subSystemList.Length + 1];
					for (int j = 0; j < subSystem.subSystemList.Length; j++)
						newSubSystems[j] = subSystem.subSystemList[j];
					newSubSystems[subSystem.subSystemList.Length] = new PlayerLoopSystem
					{
						type = typeof(RapierLoop),
						updateDelegate = RapierLoop.FixedUpdate
					};
					subSystem.subSystemList = newSubSystems;
					loop.subSystemList[i] = subSystem;
				}

				if (subSystem.type == typeof(UnityEngine.PlayerLoop.Initialization))
				{
					// Append Rapier initialization to subsytems
					PlayerLoopSystem[] newSubSystems = new PlayerLoopSystem[subSystem.subSystemList.Length + 1];
					for (int j = 0; j < subSystem.subSystemList.Length; j++)
						newSubSystems[j] = subSystem.subSystemList[j];
					newSubSystems[subSystem.subSystemList.Length] = new PlayerLoopSystem
					{
						type = typeof(RapierLoop),
						updateDelegate = RapierLoop.Initialization
					};
					subSystem.subSystemList = newSubSystems;
					loop.subSystemList[i] = subSystem;
				}
			}

			// Set the modified player loop
			PlayerLoop.SetPlayerLoop(loop);
		}
	}
}