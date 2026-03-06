using System;
using System.Collections.Generic;
using Packages.rapier4unity.Runtime;
using RapierPhysics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace RapierPhysics
{
    public static class RapierPhysics
    {
        public struct LocalRaycastHit
        {
            internal Vector3 m_Point;
            internal Vector3 m_Normal;
            internal uint m_FaceID;
            internal float m_Distance;
            internal Vector2 m_UV;
            internal int m_Collider;
        }
        
        public static bool Raycast(Ray ray, out RaycastHit hit)
        {
            bool did_hit = BindingExtensions.CastRay(ray.origin.x, ray.origin.y, ray.origin.z, ray.direction.x, ray.direction.y, ray.direction.z, out RapierRaycastHit rapierHit);
            if (!did_hit)
            {
                hit = new RaycastHit();
                return false;
            }
            LocalRaycastHit localHit = new LocalRaycastHit
            {
                m_Point = rapierHit.m_Point,
                m_Normal = rapierHit.m_Normal,
                m_FaceID = rapierHit.m_FaceID,
                m_Distance = rapierHit.m_Distance,
                m_UV = rapierHit.m_UV,
                m_Collider = RapierRuntimeData.handleToCollider[rapierHit.m_Collider].GetInstanceID()
            };
            hit = UnsafeUtility.As<LocalRaycastHit, RaycastHit>(ref localHit);
            return true;
        }
        
        public static bool CastCuboid(Vector3 start, Vector3 direction, Vector3 halfExtents, out RaycastHit shapecastHit)
        {
            var options = new RapierShapecastOptions()
            {
                m_MaxTimeOfImpact = float.MaxValue,
                m_TargetDistance = 0.0f,
                m_StopAtPenetration = true,
                m_ComputeImpactGeometryOnPenetration = true
            };
            return CastCuboid(start, direction, halfExtents, options, out shapecastHit);
        }
        
        public static bool CastCuboid(Vector3 start, Vector3 direction, Vector3 halfExtents, RapierShapecastOptions options, out RaycastHit shapecastHit)
        {
            var result = BindingExtensions.CastCuboid(start.x, start.y, start.z, direction.x, direction.y, direction.z, halfExtents.x, halfExtents.y, halfExtents.z, options, out var rapierHit);
            if (!result)
            {
                shapecastHit = default;
                return false;
            }
            var localHit = new LocalRaycastHit
            {
                m_Point = rapierHit.m_Point,
                m_Normal = rapierHit.m_Normal,
                m_FaceID = 0,
                m_Distance = rapierHit.m_Distance,
                m_UV = rapierHit.m_UV,
                m_Collider = RapierRuntimeData.handleToCollider[rapierHit.m_Collider].GetInstanceID()
            };
            shapecastHit = UnsafeUtility.As<LocalRaycastHit, RaycastHit>(ref localHit);
            return true;
        }
    }
}