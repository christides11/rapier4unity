using System;
using Packages.rapier4unity.Runtime;
using UnityEngine;

namespace RapierPhysics
{
    public static class RapierOverrides
    {
	    public static bool Raycast(Ray ray, out RaycastHit hit)
	    {
		    return RapierPhysics.Raycast(ray, out hit);
	    }
	    
		public static void AddForceWithMode(Rigidbody rigidbody, Vector3 force, ForceMode mode)
		{
			RigidBodyHandle handle = RapierRuntimeData.rigidbodyToHandle[rigidbody];
			RapierBindings.AddForce(handle, force.x, force.y, force.z, mode);
		}

		public static void AddForce(Rigidbody rigidbody, Vector3 force)
		{
			RigidBodyHandle handle = RapierRuntimeData.rigidbodyToHandle[rigidbody];
			RapierBindings.AddForce(handle, force.x, force.y, force.z, ForceMode.Force);
		}

		public static void AddTorque(Rigidbody rigidbody, Vector3 torque)
		{
			RigidBodyHandle handle = RapierRuntimeData.rigidbodyToHandle[rigidbody];
			RapierBindings.AddTorque(handle, torque.x, torque.y, torque.z, ForceMode.Force);
		}

		public static void AddTorqueWithMode(Rigidbody rigidbody, Vector3 torque, ForceMode mode)
		{
			RigidBodyHandle handle = RapierRuntimeData.rigidbodyToHandle[rigidbody];
			RapierBindings.AddTorque(handle, torque.x, torque.y, torque.z, mode);
		}

		public static void MovePosition(Rigidbody rigidbody, Vector3 position)
		{
			if (!rigidbody.isKinematic)
			{
				Debug.LogError(
					"MovePosition is not supported for non-kinematic rigidbodies. Apply forces instead or set the rigidbody to kinematic.");
				return;
			}

			RigidBodyHandle handle = RapierRuntimeData.rigidbodyToHandle[rigidbody];
			RapierBindings.SetTransformPosition(handle, position.x, position.y, position.z);
		}

		public static void MoveRotation(Rigidbody rigidbody, Quaternion rotation)
		{
			if (!rigidbody.isKinematic)
			{
				Debug.LogError(
					"MovePosition is not supported for non-kinematic rigidbodies. Apply forces instead or set the rigidbody to kinematic.");
				return;
			}

			RigidBodyHandle handle = RapierRuntimeData.rigidbodyToHandle[rigidbody];
			RapierBindings.SetTransformRotation(handle, rotation.x, rotation.y, rotation.z, rotation.w);
		}

		public static void Move(Rigidbody rigidbody, Vector3 position, Quaternion rotation)
		{
			if (!rigidbody.isKinematic)
			{
				Debug.LogError(
					"MovePosition is not supported for non-kinematic rigidbodies. Apply forces instead or set the rigidbody to kinematic.");
				return;
			}

			RigidBodyHandle handle = RapierRuntimeData.rigidbodyToHandle[rigidbody];
			RapierBindings.SetTransform(handle, position.x, position.y, position.z, rotation.x, rotation.y, rotation.z,
				rotation.w);
		}

		public static void AddRelativeForce(Rigidbody rigidbody, Vector3 force)
		{
			throw new NotImplementedException(
				"AddRelativeForce is not supported in Rapier4Unity. Use AddForce instead.");
		}

		public static void AddRelativeForceWithMode(Rigidbody rigidbody, Vector3 force, ForceMode mode)
		{
			throw new NotImplementedException(
				"AddRelativeForce is not supported in Rapier4Unity. Use AddForce instead.");
		}

		public static void AddRelativeTorque(Rigidbody rigidbody, Vector3 torque)
		{
			throw new NotImplementedException(
				"AddRelativeTorque is not supported in Rapier4Unity. Use AddTorque instead.");
		}

		public static void AddRelativeTorqueWithMode(Rigidbody rigidbody, Vector3 torque, ForceMode mode)
		{
			throw new NotImplementedException(
				"AddRelativeTorque is not supported in Rapier4Unity. Use AddTorque instead.");
		}
    }
}