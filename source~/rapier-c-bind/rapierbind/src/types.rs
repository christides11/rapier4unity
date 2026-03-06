use rapier3d::math::Vector;
use rapier3d::na::{Vector2, Vector4};
use unitybridge::IUnityLog;
use crate::handles::SerializableColliderHandle;

#[repr(C)]
pub struct FunctionsToCallFromRust {
    pub unity_log_ptr: *const IUnityLog,
}

#[repr(C)]
pub struct RawArray<T> {
    pub ptr: *mut T,
    pub len: usize,
    pub capacity: usize,
}

#[repr(C)]
#[derive(Debug, Clone, Copy)]
pub enum ForceMode {
    Force = 0,
    Impulse = 1,
    VelocityChange = 2,
    Acceleration = 5,
}

#[repr(C)]
#[derive(Debug, Clone, Copy)]
pub struct RapierTransform {
    pub rotation: Vector4<f32>,
    pub position: Vector,
}

#[repr(C)]
#[derive(Debug, Clone, Copy)]
pub struct SerializableCollisionEvent {
    pub collider1: SerializableColliderHandle,
    pub collider2: SerializableColliderHandle,
    pub is_started: bool,
}

#[repr(C)]
#[derive(Debug, Clone, Copy)]
pub struct RaycastHit {
    pub m_point: Vector,
    pub m_normal: Vector,
    pub m_face_id: u32,
    pub m_distance: f32,
    pub m_uv: Vector2<f32>,
    pub m_collider: SerializableColliderHandle,
}

#[repr(C)]
#[derive(Debug, Clone, Copy)]
pub struct ShapecastHit {
    pub m_point: Vector,
    pub m_normal: Vector,
    pub m_distance: f32,
    pub m_uv: Vector2<f32>,
    pub m_collider: SerializableColliderHandle,
}

#[repr(C)]
#[derive(Debug, Clone, Copy)]
pub struct ShapecastOptions {
    pub max_time_of_impact: f32,
    pub target_distance: f32,
    pub stop_at_penetration: bool,
    pub compute_impact_geometry_on_penetration: bool,
}