using UnityEngine;

namespace RapierPhysics
{
    public class RapierBody : MonoBehaviour
    {
        public bool autoRegister = true;
        public bool autoDeregister = true;

        public Collider[] colliders = new Collider[0];
        public Rigidbody[] rigidbodies = new Rigidbody[0];

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
            for(int i = 0; i < colliders.Length; i++) RapierLoop.EnqueueCollider(colliders[i]);
            for(int i = 0; i < rigidbodies.Length; i++) RapierLoop.EnqueueRigidbody(rigidbodies[i]);
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