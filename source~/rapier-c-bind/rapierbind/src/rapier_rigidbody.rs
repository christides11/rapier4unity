use rapier3d::dynamics::{RigidBody, RigidBodyBuilder, RigidBodyType};
use rapier3d::glamx::{Rot3, Vec4};
use rapier3d::math::{AngVector, Pose, Rotation, Vector};
use rapier3d::na::{Quaternion, UnitQuaternion, Vector3, Vector4};
use crate::{get_mutable_physics_solver, ForceMode, RapierTransform};
use crate::handles::{SerializableColliderHandle, SerializableRigidBodyHandle, SerializableRigidBodyType};
use crate::utils::{cancel_axis_velocity, locked_axes_to_unity_constraints, unity_constraints_to_locked_axes};

#[unsafe(no_mangle)]
extern "C" fn add_rigid_body(
    collider: SerializableColliderHandle,
    rb_type: SerializableRigidBodyType,
    position_x: f32,
    position_y: f32,
    position_z: f32,
    rotation_x: f32,
    rotation_y: f32,
    rotation_z: f32,
    rotation_w: f32,
) -> SerializableRigidBodyHandle {
    let psd = get_mutable_physics_solver();
    let quat = Quaternion::new(rotation_w, rotation_x, rotation_y, rotation_z);

    // Convert to unit quaternion
    let unit_quat = UnitQuaternion::from_quaternion(quat);

    // Extract the rotation angle and axis
    let angle = unit_quat.angle();
    let axis = unit_quat.axis().unwrap_or(Vector3::z_axis());

    // Create the AngVector (axis-angle representation)
    let ang_vector = axis.into_inner();

    // Build with the AngVector
    let rigid_body = RigidBodyBuilder::new(rb_type.into())
        .translation(Vector::new(position_x, position_y, position_z))
        .rotation(AngVector::new(ang_vector.x * angle, ang_vector.y * angle, ang_vector.z * angle))
        .build();

    let rb_handle = psd.state.rigid_body_set.insert(rigid_body);
    psd.state.collider_set
        .set_parent(collider.into(), Some(rb_handle), &mut psd.state.rigid_body_set);
    rb_handle.into()
}

#[unsafe(no_mangle)]
extern "C" fn remove_rigid_body(rb_handle: SerializableRigidBodyHandle) {
    let psd = get_mutable_physics_solver();
    psd.state.rigid_body_set.remove(
        rb_handle.into(),
        &mut psd.state.island_manager,
        &mut psd.state.collider_set,
        &mut psd.state.impulse_joint_set,
        &mut psd.state.multibody_joint_set,
        true,
    );
}

#[unsafe(no_mangle)]
extern "C" fn update_rigid_body_properties(
    rb_handle: SerializableRigidBodyHandle,
    rb_type: SerializableRigidBodyType,
    enable_ccd: bool,
    constraints: u32,
    linear_drag: f32,
    angular_drag: f32,
) {
    let psd = get_mutable_physics_solver();
    let rb = psd.state.rigid_body_set.get_mut(rb_handle.into()).unwrap();

    // Update body type if different
    let rb_type_enum: RigidBodyType = rb_type.into();
    if rb.body_type() != rb_type_enum {
        rb.set_body_type(rb_type_enum, true);
    }

    // Set CCD
    rb.enable_ccd(enable_ccd);

    // Update constraints if different
    if locked_axes_to_unity_constraints(rb.locked_axes()) != constraints {
        let locks = unity_constraints_to_locked_axes(constraints);
        rb.set_locked_axes(locks, false);
        cancel_axis_velocity(locks, rb);
    }

    // Set drag values
    rb.set_linear_damping(linear_drag);
    rb.set_angular_damping(angular_drag);
}

#[unsafe(no_mangle)]
extern "C" fn get_transform(rb_handle: SerializableRigidBodyHandle) -> RapierTransform {
    let psd = get_mutable_physics_solver();
    let rb = psd.state.rigid_body_set.get(rb_handle.into()).unwrap();
    let pos = rb.position();
    RapierTransform {
        rotation: Vector4::new(pos.rotation.x, pos.rotation.y, pos.rotation.z, pos.rotation.w),
        position: pos.translation,
    }
}

#[unsafe(no_mangle)]
extern "C" fn set_transform_position(
    rb_handle: SerializableRigidBodyHandle,
    position_x: f32,
    position_y: f32,
    position_z: f32,
) {
    let psd = get_mutable_physics_solver();
    let rb = psd.state.rigid_body_set.get_mut(rb_handle.into()).unwrap();
    let iso = Pose::from_parts(
        Vector::new(position_x, position_y, position_z),
        // This is a trick to make sure the order of the position versus rotation will work in either order
        rb.next_position().rotation,
    );
    rb.set_next_kinematic_position(iso);
}

#[unsafe(no_mangle)]
extern "C" fn set_transform_rotation(
    rb_handle: SerializableRigidBodyHandle,
    rotation_x: f32,
    rotation_y: f32,
    rotation_z: f32,
    rotation_w: f32,
) {
    let psd = get_mutable_physics_solver();
    let rb: &mut RigidBody = psd.state.rigid_body_set.get_mut(rb_handle.into()).unwrap();
    rb.set_next_kinematic_rotation(Rotation::from_vec4(Vec4::new(rotation_w, rotation_x, rotation_y, rotation_z)).normalize());
}

#[unsafe(no_mangle)]
extern "C" fn set_transform(
    rb_handle: SerializableRigidBodyHandle,
    position_x: f32,
    position_y: f32,
    position_z: f32,
    rotation_x: f32,
    rotation_y: f32,
    rotation_z: f32,
    rotation_w: f32,
) {
    let psd = get_mutable_physics_solver();
    let rb = psd.state.rigid_body_set.get_mut(rb_handle.into()).unwrap();
    let iso = Pose::from_parts(Vector::new(position_x, position_y, position_z), Rot3::from_vec4(Vec4::new(rotation_w, rotation_x, rotation_y, rotation_z)).normalize());
    rb.set_next_kinematic_position(iso);
}

#[unsafe(no_mangle)]
extern "C" fn set_linear_velocity(
    rb_handle: SerializableRigidBodyHandle,
    velocity_x: f32,
    velocity_y: f32,
    velocity_z: f32,
) {
    let psd = get_mutable_physics_solver();
    let rb = psd.state.rigid_body_set.get_mut(rb_handle.into()).unwrap();
    rb.set_linvel(Vector::new(velocity_x, velocity_y, velocity_z), true);
}

#[unsafe(no_mangle)]
extern "C" fn set_angular_velocity(
    rb_handle: SerializableRigidBodyHandle,
    velocity_x: f32,
    velocity_y: f32,
    velocity_z: f32,
) {
    let psd = get_mutable_physics_solver();
    let rb = psd.state.rigid_body_set.get_mut(rb_handle.into()).unwrap();
    rb.set_angvel(Vector::new(velocity_x, velocity_y, velocity_z), true);
}

#[unsafe(no_mangle)]
extern "C" fn get_linear_velocity(rb_handle: SerializableRigidBodyHandle) -> Vector {
    let psd = get_mutable_physics_solver();
    let rb = psd.state.rigid_body_set.get(rb_handle.into()).unwrap();
    rb.linvel().clone()
}

#[unsafe(no_mangle)]
extern "C" fn get_angular_velocity(rb_handle: SerializableRigidBodyHandle) -> Vector {
    let psd = get_mutable_physics_solver();
    let rb = psd.state.rigid_body_set.get(rb_handle.into()).unwrap();
    rb.angvel().clone()
}

// Add Force
#[unsafe(no_mangle)]
extern "C" fn add_force(
    rb_handle: SerializableRigidBodyHandle,
    force_x: f32,
    force_y: f32,
    force_z: f32,
    mode: ForceMode,
) {
    let psd = get_mutable_physics_solver();
    let rb = psd.state.rigid_body_set.get_mut(rb_handle.into()).unwrap();
    let mut linvel = rb.linvel().clone();
    match mode {
        ForceMode::Force => {
            linvel +=
                Vector::new(force_x, force_y, force_z) * psd.state.integration_parameters.dt / rb.mass();
        }
        ForceMode::Impulse => {
            linvel += Vector::new(force_x, force_y, force_z) / rb.mass();
        }
        ForceMode::VelocityChange => {
            linvel += Vector::new(force_x, force_y, force_z);
        }
        ForceMode::Acceleration => {
            linvel += Vector::new(force_x, force_y, force_z) * psd.state.integration_parameters.dt;
        }
    }
    // log::info!("linvel: {:?}, mode: {:?}", linvel, mode);
    rb.set_linvel(linvel, true);
}

#[unsafe(no_mangle)]
extern "C" fn add_torque(
    rb_handle: SerializableRigidBodyHandle,
    torque_x: f32,
    torque_y: f32,
    torque_z: f32,
    mode: ForceMode,
) {
    let psd = get_mutable_physics_solver();
    let rb = psd.state.rigid_body_set.get_mut(rb_handle.into()).unwrap();
    let mut angvel = rb.angvel().clone();
    match mode {
        ForceMode::Force => {
            angvel +=
                Vector::new(torque_x, torque_y, torque_z) * psd.state.integration_parameters.dt / rb.mass();
        }
        ForceMode::Impulse => {
            angvel += Vector::new(torque_x, torque_y, torque_z) / rb.mass();
        }
        ForceMode::VelocityChange => {
            angvel += Vector::new(torque_x, torque_y, torque_z);
        }
        ForceMode::Acceleration => {
            angvel += Vector::new(torque_x, torque_y, torque_z) * psd.state.integration_parameters.dt;
        }
    }
    rb.set_angvel(angvel, true);
}