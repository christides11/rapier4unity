using System.Collections.Generic;
using Packages.rapier4unity.Runtime;
using UnityEngine;
using UnityEngine.Events;

namespace RapierPhysics
{
    public static class RapierRuntimeData
    {
        public struct BasicEvent<T>
        {
            //public delegate* <T, void> callback;
            delegate void Callback(T collider);
            Callback m_Callback;
            public void Invoke(T collider) => m_Callback?.Invoke(collider);
            public bool IsValid => m_Callback != null;
            public BasicEvent(MonoBehaviour component, string methodName)
            {
                System.Reflection.MethodInfo methodInfo = UnityEventBase.GetValidMethodInfo(component.GetType(), methodName, new[] { typeof(T) });
                m_Callback = (Callback)methodInfo?.CreateDelegate(typeof(Callback), component);
            }
        }
        
        public struct EventsForCollider
        {
            public List<BasicEvent<Collider>> onTriggerEnter;
            public List<BasicEvent<Collider>> onTriggerExit;
            public List<BasicEvent<Collider>> onTriggerStay;

            public List<BasicEvent<Collision>> onCollisionEnter;
            public List<BasicEvent<Collision>> onCollisionExit;
            public List<BasicEvent<Collision>> onCollisionStay;
        }

        public struct EventsForMonoBehaviour
        {
            public BasicEvent<Collider> onTriggerEnter;
            public BasicEvent<Collider> onTriggerExit;
            public BasicEvent<Collider> onTriggerStay;

            public BasicEvent<Collision> onCollisionEnter;
            public BasicEvent<Collision> onCollisionExit;
            public BasicEvent<Collision> onCollisionStay;
        }
        
        public static List<Collider> collidersToRegister = new List<Collider>();
        public static List<Rigidbody> rigidbodiesToRegister = new List<Rigidbody>();
        public static Dictionary<Rigidbody, RigidBodyHandle> rigidbodyToHandle = new();
        public static Dictionary<Collider, ColliderHandle> colliderToHandle = new();
        public static Dictionary<ColliderHandle, Collider> handleToCollider = new();
        public static Dictionary<Collider, RigidBodyHandle> fixedRigidbodies = new();
        public static Dictionary<Collider, EventsForCollider> physicsEvents = new();
        public static Dictionary<RapierJoint, ImpulseJointHandle> joints = new();
        public static Dictionary<MonoBehaviour, EventsForMonoBehaviour> monoBehaviourEvents = new();
        public static HashSet<(Collider, Collider)> activeTriggerPairs = new();
    }
}