use rapier3d::prelude::*;
use crate::get_mutable_physics_solver;
use crate::handles::SerializableColliderHandle;

#[unsafe(no_mangle)]
extern "C" fn add_cuboid_collider(
    half_extents_x: f32,
    half_extents_y: f32,
    half_extents_z: f32,
    mass: f32,
    is_sensor: bool,
) -> SerializableColliderHandle {
    let psd = get_mutable_physics_solver();
    let collider = ColliderBuilder::cuboid(half_extents_x, half_extents_y, half_extents_z)
        .active_events(ActiveEvents::COLLISION_EVENTS)
        .density(mass)
        .sensor(is_sensor)
        .build();
    psd.state.collider_set.insert(collider).into()
}

#[unsafe(no_mangle)]
extern "C" fn add_sphere_collider(
    radius: f32,
    mass: f32,
    is_sensor: bool,
) -> SerializableColliderHandle {
    let psd = get_mutable_physics_solver();
    let collider = ColliderBuilder::ball(radius)
        .density(mass)
        .active_events(ActiveEvents::COLLISION_EVENTS)
        .sensor(is_sensor)
        .build();
    psd.state.collider_set.insert(collider).into()
}

#[unsafe(no_mangle)]
extern "C" fn add_capsule_collider(
    half_height: f32,
    radius: f32,
    mass: f32,
    is_sensor: bool,
) -> SerializableColliderHandle {
    let psd = get_mutable_physics_solver();
    let collider = ColliderBuilder::capsule_y(half_height, radius)
        .density(mass)
        .active_events(ActiveEvents::COLLISION_EVENTS)
        .sensor(is_sensor)
        .build();
    psd.state.collider_set.insert(collider).into()
}

// TODO Investigate optimizing this a bit
#[unsafe(no_mangle)]
extern "C" fn add_mesh_collider(
    vertices_ptr: *const f32,
    vertices_count: usize,
    indices_ptr: *const u32,
    indices_count: usize,
    mass: f32,
    is_sensor: bool,
) -> SerializableColliderHandle {
    let psd = get_mutable_physics_solver();

    // Convert C arrays to Rust slices
    let vertices_flat = unsafe { std::slice::from_raw_parts(vertices_ptr, vertices_count * 3) };
    let indices_flat = unsafe { std::slice::from_raw_parts(indices_ptr, indices_count * 3) };

    // Convert flat arrays to points
    let mut vertices = Vec::with_capacity(vertices_count);
    for i in 0..vertices_count {
        vertices.push(Vector::new(
            vertices_flat[i * 3],
            vertices_flat[i * 3 + 1],
            vertices_flat[i * 3 + 2]
        ));
    }

    // Convert flat indices to triangle indices
    let mut indices = Vec::with_capacity(indices_count);
    for i in 0..indices_count {
        indices.push([
            indices_flat[i * 3],
            indices_flat[i * 3 + 1],
            indices_flat[i * 3 + 2],
        ]);
    }

    // Build the trimesh collider
    if let Ok(collider_builder) = ColliderBuilder::trimesh(vertices, indices) {
        let collider = collider_builder
            .active_events(ActiveEvents::COLLISION_EVENTS)
            .density(mass)
            .sensor(is_sensor)
            .build();
        psd.state.collider_set.insert(collider).into()
    } else {
        log::warn!("Failed to create mesh collider");
        ColliderHandle::invalid().into()
    }
}

// TODO Investigate optimizing this a bit
// In practice we may only want to use this option as opposed to full mesh collision
#[unsafe(no_mangle)]
extern "C" fn add_convex_mesh_collider(
    vertices_ptr: *const f32,
    vertices_count: usize,
    mass: f32,
    is_sensor: bool,
) -> SerializableColliderHandle {
    let psd = get_mutable_physics_solver();

    // Convert C arrays to Rust slices
    let vertices_flat = unsafe { std::slice::from_raw_parts(vertices_ptr, vertices_count * 3) };

    // Convert flat arrays to points
    let mut points = Vec::with_capacity(vertices_count);
    for i in 0..vertices_count {
        points.push(Vector::new(
            vertices_flat[i * 3],
            vertices_flat[i * 3 + 1],
            vertices_flat[i * 3 + 2]
        ));
    }

    // Build the convex hull collider
    if let Some(collider_builder) = ColliderBuilder::convex_hull(&points) {
        let collider = collider_builder
            .active_events(ActiveEvents::COLLISION_EVENTS)
            .density(mass)
            .sensor(is_sensor)
            .build();
        psd.state.collider_set.insert(collider).into()
    } else {
        log::warn!("Failed to create convex hull collider");
        ColliderHandle::invalid().into()
    }
}

#[unsafe(no_mangle)]
extern "C" fn remove_collider(handle: SerializableColliderHandle){
    let psd = get_mutable_physics_solver();
    (psd.state.collider_set).remove(
        handle.into(),
        &mut psd.state.island_manager,
        &mut psd.state.rigid_body_set,
        true);
}