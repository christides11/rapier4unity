using System;
using UnityEngine;

namespace RapierPhysics
{
    public partial class RapierBody : MonoBehaviour
    {
        public bool autoRegister = true;
        public bool autoDeregister = true;

        public Collider[] colliders = Array.Empty<Collider>();
        public Rigidbody body;

        public RapierBody(bool autoRegister = true, bool autoDeregister = true)
        {
            this.autoRegister = autoRegister;
            this.autoDeregister = autoDeregister;
        }
        
        public virtual void Awake()
        {
            if (autoRegister)
            {
                RegisterBody();
            }
        }

        public virtual void RegisterBody()
        {
            if (body != null)
            {
                RapierLoop.EnqueueRigidbody(body);
            }
            else
            {
                for (int i = 0; i < colliders.Length; i++) RapierLoop.EnqueueCollider(colliders[i]);
            }
        }
        
        public virtual void DeregisterBody()
        {
            
        }

        private void OnDestroy()
        {
            if (autoDeregister)
            {
                DeregisterBody();
            }
        }
    }
}