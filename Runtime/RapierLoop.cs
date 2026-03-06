using System;
using System.Collections.Generic;
using Packages.rapier4unity.Runtime;
using RapierPhysics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace RapierPhysics
{
	public class RapierLoop
	{
		public static void RegisterJoint(RapierJoint joint)
		{
			if (RapierRuntimeData.joints.ContainsKey(joint))
				return;

			// If the mover is kinematic, we cannot continue
			if (joint.Mover.isKinematic)
			{
				Debug.LogError("Mover must not be kinematic for the joint to work properly.");
				return;
			}

			// Make sure the rigidbodies are registered
			if (!RapierRuntimeData.rigidbodyToHandle.ContainsKey(joint.Anchor))
			{
				RigidBodyHandle handle = CreateOrGetRigidBodyHandle(joint.Anchor);
				UpdateRigidBody(joint.Anchor, handle);
			}

			if (!RapierRuntimeData.rigidbodyToHandle.ContainsKey(joint.Mover))
			{
				RigidBodyHandle handle = CreateOrGetRigidBodyHandle(joint.Mover);
				UpdateRigidBody(joint.Mover, handle);
			}

			RigidBodyHandle anchorHandle = RapierRuntimeData.rigidbodyToHandle[joint.Anchor];
			RigidBodyHandle moverHandle = RapierRuntimeData.rigidbodyToHandle[joint.Mover];
			ImpulseJointHandle jointHandle;
			if (joint is RapierFixedJoint fixedJoint)
			{

				Vector3 anchor1 = Vector3.Scale(fixedJoint.Anchor1, fixedJoint.Anchor.transform.lossyScale);
				Vector3 anchor2 = Vector3.Scale(fixedJoint.Anchor2, fixedJoint.Mover.transform.lossyScale);
				jointHandle = RapierBindings.AddFixedJoint(
					anchorHandle,
					moverHandle,
					anchor1.x,
					anchor1.y,
					anchor1.z,
					anchor2.x,
					anchor2.y,
					anchor2.z,
					fixedJoint.SelfCollision);
			}
			else if (joint is RapierSphericalJoint sphericalJoint)
			{
				Vector3 anchor1 = Vector3.Scale(sphericalJoint.Anchor1, sphericalJoint.Anchor.transform.lossyScale);
				Vector3 anchor2 = Vector3.Scale(sphericalJoint.Anchor2, sphericalJoint.Mover.transform.lossyScale);
				jointHandle = RapierBindings.AddSphericalJoint(
					anchorHandle,
					moverHandle,
					anchor1.x,
					anchor1.y,
					anchor1.z,
					anchor2.x,
					anchor2.y,
					anchor2.z,
					sphericalJoint.SelfCollision);
			}
			// Check which kind of joint we need to create
			else if (joint is RapierRevoluteJoint revoluteJoint)
			{
				Vector3 anchor1 = Vector3.Scale(revoluteJoint.Anchor1, revoluteJoint.Anchor.transform.lossyScale);
				Vector3 anchor2 = Vector3.Scale(revoluteJoint.Anchor2, revoluteJoint.Mover.transform.lossyScale);
				jointHandle = RapierBindings.AddRevoluteJoint(
					anchorHandle,
					moverHandle,
					revoluteJoint.Axis.x,
					revoluteJoint.Axis.y,
					revoluteJoint.Axis.z,
					anchor1.x,
					anchor1.y,
					anchor1.z,
					anchor2.x,
					anchor2.y,
					anchor2.z,
					revoluteJoint.SelfCollision);
			}
			else if (joint is RapierPrismaticJoint prismaticJoint)
			{
				Vector3 anchor1 = Vector3.Scale(prismaticJoint.Anchor1, prismaticJoint.Anchor.transform.lossyScale);
				Vector3 anchor2 = Vector3.Scale(prismaticJoint.Anchor2, prismaticJoint.Mover.transform.lossyScale);
				jointHandle = RapierBindings.AddPrismaticJoint(
					anchorHandle,
					moverHandle,
					prismaticJoint.Axis.x,
					prismaticJoint.Axis.y,
					prismaticJoint.Axis.z,
					anchor1.x,
					anchor1.y,
					anchor1.z,
					anchor2.x,
					anchor2.y,
					anchor2.z,
					prismaticJoint.Limits.x,
					prismaticJoint.Limits.y,
					prismaticJoint.SelfCollision);
			}
			else
			{
				Debug.LogError($"Unsupported joint type: {joint.GetType()}");
				return;
			}

			RapierRuntimeData.joints[joint] = jointHandle;
		}

		public static void UnregisterJoint(RapierJoint joint)
		{
			if (!RapierRuntimeData.joints.ContainsKey(joint))
				return;

			// Remove joint handle
			ImpulseJointHandle jointHandle = RapierRuntimeData.joints[joint];
			RapierBindings.RemoveJoint(jointHandle);
			RapierRuntimeData.joints.Remove(joint);
		}

		public static void EnqueueCollider(Collider collider)
		{
			RapierRuntimeData.collidersToRegister.Add(collider);
		}

		public static void EnqueueRigidbody(Rigidbody rigidbody)
		{
			RapierRuntimeData.rigidbodiesToRegister.Add(rigidbody);
		}

		// Called in the beginning of every frame, this ensures that all colliders and rigidbodies are initialized
		public static unsafe void Initialization()
		{
			if (!Application.isPlaying || !RapierBindings.IsAvailable)
				return;

			foreach (Collider collider in RapierRuntimeData.collidersToRegister)
				AddCollider(collider);

			foreach (Rigidbody rigidbody in RapierRuntimeData.rigidbodiesToRegister)
			{
				TryCreateRigidBodyHandle(rigidbody);
			}

			foreach (var rth in RapierRuntimeData.rigidbodyToHandle)
			{
				UpdateRigidBody(rth.Key, rth.Value);
			}
		}

		private unsafe static void AddCollider(Collider collider)
		{
			// Add Rapier Collider if it doesn't exist
			if (!RapierRuntimeData.colliderToHandle.ContainsKey(collider))
			{
				Rigidbody potentialRigidbody = collider.GetComponent<Rigidbody>();
				Vector3 transformScale = collider.transform.localScale;

				switch (collider)
				{
					case BoxCollider boxCollider:
					{
						ColliderHandle newColliderHandle = RapierBindings.AddCuboidCollider(
							transformScale.x * boxCollider.size.x / 2,
							transformScale.y * boxCollider.size.y / 2,
							transformScale.z * boxCollider.size.z / 2,
							potentialRigidbody == null ? 0 : potentialRigidbody.mass,
							boxCollider.isTrigger);

						RapierRuntimeData.colliderToHandle[collider] = newColliderHandle;
						RapierRuntimeData.handleToCollider[newColliderHandle] = collider;
						break;
					}
					case SphereCollider sphereCollider:
					{
						ColliderHandle newColliderHandle = RapierBindings.AddSphereCollider(
							transformScale.x * sphereCollider.radius,
							potentialRigidbody == null ? 0 : potentialRigidbody.mass,
							sphereCollider.isTrigger);
						RapierRuntimeData.colliderToHandle[collider] = newColliderHandle;
						RapierRuntimeData.handleToCollider[newColliderHandle] = collider;
						break;
					}
					case CapsuleCollider capsuleCollider:
					{
						ColliderHandle newColliderHandle = RapierBindings.AddCapsuleCollider(
							transformScale.x * capsuleCollider.radius,
							transformScale.y * (capsuleCollider.height / 2) -
							(transformScale.x * capsuleCollider.radius),
							potentialRigidbody == null ? 0 : potentialRigidbody.mass,
							capsuleCollider.isTrigger);
						RapierRuntimeData.colliderToHandle[collider] = newColliderHandle;
						RapierRuntimeData.handleToCollider[newColliderHandle] = collider;
						break;
					}
					case MeshCollider meshCollider:
					{
						// Make sure we have a valid mesh
						Mesh mesh = meshCollider.sharedMesh;
						if (mesh == null)
						{
							Debug.LogError($"MeshCollider on {collider.gameObject.name} has no mesh assigned!");
							return;
						}

						// Get vertex data
						Vector3[] vertices = mesh.vertices;
						Vector3 scale = collider.transform.localScale;
						NativeArray<float3> verticesFlat = new NativeArray<float3>(vertices.Length, Allocator.Temp);

						// Apply local scale to vertices
						for (int i = 0; i < vertices.Length; i++)
							verticesFlat[i] = (float3)vertices[i] * scale;

						ColliderHandle newColliderHandle;

						if (meshCollider.convex)
						{
							// Use convex hull for convex meshes (better performance)
							newColliderHandle = RapierBindings.AddConvexMeshCollider(
								(float*)verticesFlat.GetUnsafeReadOnlyPtr(), (UIntPtr)vertices.Length,
								potentialRigidbody == null ? 0 : potentialRigidbody.mass,
								meshCollider.isTrigger);
						}
						else
						{
							// Use trimesh for concave meshes
							int[] triangles = mesh.triangles;
							var indicesFlat = new NativeArray<uint>(triangles.Length, Allocator.Temp);
							for (int i = 0; i < triangles.Length; i++)
								indicesFlat[i] = (uint)triangles[i];


							newColliderHandle = RapierBindings.AddMeshCollider(
								(float*)verticesFlat.GetUnsafeReadOnlyPtr(), (UIntPtr)vertices.Length,
								(uint*)indicesFlat.GetUnsafeReadOnlyPtr(), (UIntPtr)(triangles.Length / 3),
								potentialRigidbody == null ? 0 : potentialRigidbody.mass,
								meshCollider.isTrigger);
						}

						RapierRuntimeData.colliderToHandle[collider] = newColliderHandle;
						RapierRuntimeData.handleToCollider[newColliderHandle] = collider;
						break;
					}
				}

				// Add events for collider
				RapierRuntimeData.EventsForCollider eventsForCollider = new RapierRuntimeData.EventsForCollider();

				eventsForCollider.onTriggerEnter = new List<RapierRuntimeData.BasicEvent<Collider>>();
				eventsForCollider.onTriggerExit = new List<RapierRuntimeData.BasicEvent<Collider>>();
				eventsForCollider.onTriggerStay = new List<RapierRuntimeData.BasicEvent<Collider>>();

				eventsForCollider.onCollisionEnter = new List<RapierRuntimeData.BasicEvent<Collision>>();
				eventsForCollider.onCollisionExit = new List<RapierRuntimeData.BasicEvent<Collision>>();
				eventsForCollider.onCollisionStay = new List<RapierRuntimeData.BasicEvent<Collision>>();

				MonoBehaviour[] components = collider.gameObject.GetComponents<MonoBehaviour>();
				foreach (MonoBehaviour component in components)
				{
					// Lookup events for MonoBehaviour, create if they don't exist
					if (!RapierRuntimeData.monoBehaviourEvents.TryGetValue(component, out RapierRuntimeData.EventsForMonoBehaviour eventsForMonoBehaviour))
					{
						eventsForMonoBehaviour.onTriggerEnter = new RapierRuntimeData.BasicEvent<Collider>(component, "OnTriggerEnter");
						eventsForMonoBehaviour.onTriggerExit = new RapierRuntimeData.BasicEvent<Collider>(component, "OnTriggerExit");
						eventsForMonoBehaviour.onTriggerStay = new RapierRuntimeData.BasicEvent<Collider>(component, "OnTriggerStay");

						eventsForMonoBehaviour.onCollisionEnter =
							new RapierRuntimeData.BasicEvent<Collision>(component, "OnCollisionEnter");
						eventsForMonoBehaviour.onCollisionExit =
							new RapierRuntimeData.BasicEvent<Collision>(component, "OnCollisionExit");
						eventsForMonoBehaviour.onCollisionStay =
							new RapierRuntimeData.BasicEvent<Collision>(component, "OnCollisionStay");

						RapierRuntimeData.monoBehaviourEvents[component] = eventsForMonoBehaviour;
					}

					// Add events for collider
					if (eventsForMonoBehaviour.onTriggerEnter.IsValid)
						eventsForCollider.onTriggerEnter.Add(eventsForMonoBehaviour.onTriggerEnter);
					if (eventsForMonoBehaviour.onTriggerExit.IsValid)
						eventsForCollider.onTriggerExit.Add(eventsForMonoBehaviour.onTriggerExit);
					if (eventsForMonoBehaviour.onTriggerStay.IsValid)
						eventsForCollider.onTriggerStay.Add(eventsForMonoBehaviour.onTriggerStay);

					if (eventsForMonoBehaviour.onCollisionEnter.IsValid)
						eventsForCollider.onCollisionEnter.Add(eventsForMonoBehaviour.onCollisionEnter);
					if (eventsForMonoBehaviour.onCollisionExit.IsValid)
						eventsForCollider.onCollisionExit.Add(eventsForMonoBehaviour.onCollisionExit);
					if (eventsForMonoBehaviour.onCollisionStay.IsValid)
						eventsForCollider.onCollisionStay.Add(eventsForMonoBehaviour.onCollisionStay);
				}

				RapierRuntimeData.physicsEvents[collider] = eventsForCollider;

				// In Unity if an object doesn't have a rigidbody, it's considered static
				// In Rapier, we need to add a fixed rigidbody, so we can simulate dynamic object interacting with static objects
				if (potentialRigidbody == null &&
				    RapierRuntimeData.colliderToHandle.TryGetValue(collider, out ColliderHandle colliderHandle))
				{
					RapierRuntimeData.fixedRigidbodies[collider] = RapierBindings.AddRigidBody(
						colliderHandle,
						RigidBodyType.Fixed,
						collider.transform.position.x,
						collider.transform.position.y,
						collider.transform.position.z,
						collider.transform.rotation.x,
						collider.transform.rotation.y,
						collider.transform.rotation.z,
						collider.transform.rotation.w);
				}
			}
		}

		private static RigidBodyHandle CreateOrGetRigidBodyHandle(Rigidbody rigidbody,
			RigidBodyType type = RigidBodyType.Dynamic)
		{
			// Try to get the handle from the dictionary first
			if (RapierRuntimeData.rigidbodyToHandle.TryGetValue(rigidbody, out RigidBodyHandle handle))
			{
				return handle;
			}

			// Handle doesn't exist, create a new one
			Collider[] colliders = rigidbody.GetComponents<Collider>();
			Assert.AreEqual(colliders.Length, 1, "Rigidbody must have exactly one collider for the moment");

			// Try to add a collider in case we don't have one yet
			AddCollider(colliders[0]);
			ColliderHandle colliderHandle = RapierRuntimeData.colliderToHandle[colliders[0]];
			Transform trs = rigidbody.transform;
			RigidBodyHandle rigidBodyHandle = RapierBindings.AddRigidBody(
				colliderHandle,
				type,
				trs.position.x,
				trs.position.y,
				trs.position.z,
				trs.rotation.x,
				trs.rotation.y,
				trs.rotation.z,
				trs.rotation.w);

			RapierRuntimeData.rigidbodyToHandle[rigidbody] = rigidBodyHandle;
			return rigidBodyHandle;
		}

		private static void TryCreateRigidBodyHandle(Rigidbody rigidbody, RigidBodyType type = RigidBodyType.Dynamic)
		{
			// Try to get the handle from the dictionary first
			if (RapierRuntimeData.rigidbodyToHandle.ContainsKey(rigidbody)) return;

			// Handle doesn't exist, create a new one
			Collider[] colliders = rigidbody.GetComponents<Collider>();
			Assert.AreEqual(colliders.Length, 1, "Rigidbody must have exactly one collider for the moment");

			// Try to add a collider in case we don't have one yet
			AddCollider(colliders[0]);
			ColliderHandle colliderHandle = RapierRuntimeData.colliderToHandle[colliders[0]];
			Transform trs = rigidbody.transform;
			RigidBodyHandle rigidBodyHandle = RapierBindings.AddRigidBody(
				colliderHandle,
				type,
				trs.position.x,
				trs.position.y,
				trs.position.z,
				trs.rotation.x,
				trs.rotation.y,
				trs.rotation.z,
				trs.rotation.w);

			RapierRuntimeData.rigidbodyToHandle[rigidbody] = rigidBodyHandle;
		}

		private static void UpdateRigidBody(Rigidbody rigidbody, RigidBodyHandle handle)
		{
			// TODO Determine what other kinds of properties might need to update per frame. 
			RapierBindings.UpdateRigidBodyProperties(handle,
				rigidbody.isKinematic ? RigidBodyType.KinematicPositionBased : RigidBodyType.Dynamic,
				rigidbody.collisionDetectionMode == CollisionDetectionMode.Continuous,
				(uint)rigidbody.constraints,
				rigidbody.linearDamping,
				rigidbody.angularDamping);
		}

		// Called at the end of the FixedUpdate loop
		// This is where we solve physics and get collision events
		// After that we update the GameObject positions of GameObjects with RigidBody component
		public static void FixedUpdate()
		{
			// Only run in play mode
			if (!Application.isPlaying || !RapierBindings.IsAvailable)
				return;

			if (Physics.simulationMode != SimulationMode.FixedUpdate) return;

			Tick();
		}

		public static void Tick()
		{
			unsafe
			{
				// Solve physics and get collision events
				RawArray<CollisionEvent>* eventsPtrToArray = RapierBindings.Solve();
				if (eventsPtrToArray == null)
					return;

				// Handle collision events
				for (int i = 0; i < eventsPtrToArray->length; i++)
				{
					CollisionEvent @event = (*eventsPtrToArray)[i];
					Collider collider1 = RapierRuntimeData.handleToCollider[@event.collider1];
					Collider collider2 = RapierRuntimeData.handleToCollider[@event.collider2];
					RapierRuntimeData.EventsForCollider eventsForCollider1 = RapierRuntimeData.physicsEvents[collider1];
					RapierRuntimeData.EventsForCollider eventsForCollider2 = RapierRuntimeData.physicsEvents[collider2];
					if (@event.is_started)
					{
						if (collider1.isTrigger || collider2.isTrigger)
						{
							foreach (RapierRuntimeData.BasicEvent<Collider> enter in eventsForCollider1.onTriggerEnter)
								enter.Invoke(collider2);
							foreach (RapierRuntimeData.BasicEvent<Collider> enter in eventsForCollider2.onTriggerEnter)
								enter.Invoke(collider1);
						}

						RapierRuntimeData.activeTriggerPairs.Add((collider1, collider2));
					}
					else
					{
						if (collider1.isTrigger || collider2.isTrigger)
						{
							foreach (RapierRuntimeData.BasicEvent<Collider> exit in eventsForCollider1.onTriggerExit)
								exit.Invoke(collider2);
							foreach (RapierRuntimeData.BasicEvent<Collider> exit in eventsForCollider2.onTriggerExit)
								exit.Invoke(collider1);
						}

						RapierRuntimeData.activeTriggerPairs.Remove((collider1, collider2));
					}
				}

				// Handle trigger stay
				foreach ((Collider, Collider) activeTriggerPair in RapierRuntimeData.activeTriggerPairs)
				{
					(Collider collider1, Collider collider2) = activeTriggerPair;
					RapierRuntimeData.EventsForCollider eventsForCollider1 = RapierRuntimeData.physicsEvents[collider1];
					RapierRuntimeData.EventsForCollider eventsForCollider2 = RapierRuntimeData.physicsEvents[collider2];
					if (collider1.isTrigger || collider2.isTrigger)
					{
						foreach (RapierRuntimeData.BasicEvent<Collider> stay in eventsForCollider1.onTriggerStay)
							stay.Invoke(collider2);
						foreach (RapierRuntimeData.BasicEvent<Collider> stay in eventsForCollider2.onTriggerStay)
							stay.Invoke(collider1);
					}
				}

				RapierBindings.FreeCollisionEvents(eventsPtrToArray);
			}

			// Update GameObject positions of GameObjects with RigidBody component
			foreach (var rigidbodyAndHandle in RapierRuntimeData.rigidbodyToHandle)
			{
				RigidBodyHandle handle = RapierRuntimeData.rigidbodyToHandle[rigidbodyAndHandle.Key];
				RapierTransform position = RapierBindings.GetTransform(handle);
				rigidbodyAndHandle.Key.transform.SetPositionAndRotation(position.position, position.rotation);
			}

			// Update fixed rigidbodies
			foreach ((Collider collider, RigidBodyHandle rbhandle) in RapierRuntimeData.fixedRigidbodies)
			{
			}
		}
	}
}