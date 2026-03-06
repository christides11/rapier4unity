mod handles;
mod utils;
mod world_state;
mod rapier_math;
mod rapier_rigidbody;
mod rapier_joints;
mod types;
mod rapier_queries;
pub mod rapier_colliders;

use std::mem;
use unitybridge::AssignUnityLogger;
use crate::types::*;
use crate::world_state::PhysicsWorld;

static mut PHYSIC_SOLVER_DATA: Option<PhysicsWorld> = None;

#[allow(static_mut_refs)]
fn get_mutable_physics_solver() -> &'static mut PhysicsWorld<'static> {
    unsafe { PHYSIC_SOLVER_DATA.as_mut().unwrap() }
}

#[unsafe(no_mangle)]
extern "C" fn init(funcs: *const FunctionsToCallFromRust) {
    unsafe {
        PHYSIC_SOLVER_DATA = Some(PhysicsWorld::default());
        AssignUnityLogger((*funcs).unity_log_ptr);
    }
}

#[unsafe(no_mangle)]
extern "C" fn hello_world() {
    log::info!("Hello, cake!");
}

#[unsafe(no_mangle)]
extern "C" fn version() {
    log::info!("Rapier Version {}", rapier3d::VERSION);
}

// teardown
#[unsafe(no_mangle)]
extern "C" fn teardown() {
    unsafe {
        PHYSIC_SOLVER_DATA = None;
    }
}

#[unsafe(no_mangle)]
#[allow(static_mut_refs)]
extern "C" fn solve() -> *const RawArray<SerializableCollisionEvent> {
    unsafe {
        if PHYSIC_SOLVER_DATA.is_none() {
            log::warn!("Physics solver data is not initialized");
            return std::ptr::null();
        }
    }

    let mut collision_events = get_mutable_physics_solver().solve();
    // box the vector to prevent it from being deallocated
    let ptr = collision_events.as_mut_ptr();
    let len = collision_events.len();
    let capacity = collision_events.capacity();
    let val = Box::new(RawArray { ptr, len, capacity });
    mem::forget(collision_events);
    Box::into_raw(val)
}

#[unsafe(no_mangle)]
extern "C" fn free_collision_events(ptr: *mut RawArray<SerializableCollisionEvent>) {
    unsafe {
        let info = Box::from_raw(ptr);
        let _ = Vec::from_raw_parts(info.ptr, info.len, info.capacity);
    }
}
