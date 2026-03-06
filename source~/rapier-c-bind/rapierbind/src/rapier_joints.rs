use rapier3d::dynamics::{FixedJointBuilder, PrismaticJointBuilder, RevoluteJointBuilder, SphericalJointBuilder};
use rapier3d::math::{Pose, Vector};
use crate::get_mutable_physics_solver;
use crate::handles::{SerializableImpulseJointHandle, SerializableRigidBodyHandle};

#[unsafe(no_mangle)]
extern "C" fn add_fixed_joint(
    rb1_handle: SerializableRigidBodyHandle,
    rb2_handle: SerializableRigidBodyHandle,
    local_frame1_x: f32,
    local_frame1_y: f32,
    local_frame1_z: f32,
    local_frame2_x: f32,
    local_frame2_y: f32,
    local_frame2_z: f32,
    self_collision: bool,
) -> SerializableImpulseJointHandle {
    let psd = get_mutable_physics_solver();
    let point1: Vector = Vector::new(local_frame1_x, local_frame1_y, local_frame1_z);
    let point2: Vector = Vector::new(local_frame2_x, local_frame2_y, local_frame2_z);
    let anchor_rb = psd.state.rigid_body_set.get(rb1_handle.into()).unwrap();
    let mover_rb = psd.state.rigid_body_set.get(rb2_handle.into()).unwrap();
    // Construct the local_frame for the first body
    let local_frame1 =
        Pose::from_parts(point1, anchor_rb.position().rotation);
    // Construct the local_frame for the second body
    let local_frame2 = Pose::from_parts(point2, mover_rb.position().rotation);
    let joint = FixedJointBuilder::new()
        .local_frame1(local_frame1)
        .local_frame2(local_frame2)
        .contacts_enabled(self_collision);

    psd.state.impulse_joint_set
        .insert(rb1_handle.into(), rb2_handle.into(), joint, false)
        .into()
}

#[unsafe(no_mangle)]
extern "C" fn add_spherical_joint(
    rb1_handle: SerializableRigidBodyHandle,
    rb2_handle: SerializableRigidBodyHandle,
    local_frame1_x: f32,
    local_frame1_y: f32,
    local_frame1_z: f32,
    local_frame2_x: f32,
    local_frame2_y: f32,
    local_frame2_z: f32,
    self_collision: bool,
) -> SerializableImpulseJointHandle {
    let psd = get_mutable_physics_solver();
    let point1: Vector = Vector::new(local_frame1_x, local_frame1_y, local_frame1_z);
    let point2: Vector = Vector::new(local_frame2_x, local_frame2_y, local_frame2_z);
    let anchor_rb = psd.state.rigid_body_set.get(rb1_handle.into()).unwrap();
    let mover_rb = psd.state.rigid_body_set.get(rb2_handle.into()).unwrap();
    // Construct the local_frame for the first body
    let local_frame1 =
        Pose::from_parts(point1, anchor_rb.position().rotation);
    // Construct the local_frame for the second body
    let local_frame2 =
        Pose::from_parts(point2, mover_rb.position().rotation);
    let joint = SphericalJointBuilder::new()
        // .local_anchor1(point1)
        // .local_anchor2(point2)
        .local_frame1(local_frame1)
        .local_frame2(local_frame2)
        .contacts_enabled(self_collision);

    psd.state.impulse_joint_set
        .insert(rb1_handle.into(), rb2_handle.into(), joint, false)
        .into()
}

#[unsafe(no_mangle)]
extern "C" fn add_revolute_joint(
    rb1_handle: SerializableRigidBodyHandle,
    rb2_handle: SerializableRigidBodyHandle,
    axis_x: f32,
    axis_y: f32,
    axis_z: f32,
    local_frame1_x: f32,
    local_frame1_y: f32,
    local_frame1_z: f32,
    local_frame2_x: f32,
    local_frame2_y: f32,
    local_frame2_z: f32,
    self_collision: bool,
) -> SerializableImpulseJointHandle {
    let psd = get_mutable_physics_solver();
    let point1: Vector = Vector::new(local_frame1_x, local_frame1_y, local_frame1_z);
    let point2: Vector = Vector::new(local_frame2_x, local_frame2_y, local_frame2_z);
    let axis: Vector = Vector::new(axis_x, axis_y, axis_z).normalize();
    let joint = RevoluteJointBuilder::new(axis)
        .local_anchor1(point1)
        .local_anchor2(point2)
        .contacts_enabled(self_collision);

    psd.state.impulse_joint_set
        .insert(rb1_handle.into(), rb2_handle.into(), joint, false)
        .into()
}

#[unsafe(no_mangle)]
extern "C" fn add_prismatic_joint(
    rb1_handle: SerializableRigidBodyHandle,
    rb2_handle: SerializableRigidBodyHandle,
    axis_x: f32,
    axis_y: f32,
    axis_z: f32,
    local_frame1_x: f32,
    local_frame1_y: f32,
    local_frame1_z: f32,
    local_frame2_x: f32,
    local_frame2_y: f32,
    local_frame2_z: f32,
    limit_min: f32,
    limit_max: f32,
    self_collision: bool,
) -> SerializableImpulseJointHandle {
    let psd = get_mutable_physics_solver();
    let point1: Vector = Vector::new(local_frame1_x, local_frame1_y, local_frame1_z);
    let point2: Vector = Vector::new(local_frame2_x, local_frame2_y, local_frame2_z);
    let axis: Vector = Vector::new(axis_x, axis_y, axis_z).normalize();
    let joint = PrismaticJointBuilder::new(axis)
        .local_anchor1(point1)
        .local_anchor2(point2)
        .limits([limit_min, limit_max])
        // If the anchor is kinematic, then don't collide with the other body
        .contacts_enabled(self_collision);

    psd.state.impulse_joint_set
        .insert(rb1_handle.into(), rb2_handle.into(), joint, false)
        .into()
}

#[unsafe(no_mangle)]
extern "C" fn remove_joint(handle: SerializableImpulseJointHandle) {
    let psd = get_mutable_physics_solver();
    psd.state.impulse_joint_set.remove(handle.into(), true);
}
