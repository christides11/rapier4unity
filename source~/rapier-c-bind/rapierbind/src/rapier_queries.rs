use rapier3d::prelude::{nalgebra, Cuboid, Rot3, Vec4};
use rapier3d::geometry::{FeatureId, Ray};
use rapier3d::math::{Pose, Vec3, Vector};
use rapier3d::na::vector;
use rapier3d::parry::query::{ShapeCastOptions};
use rapier3d::pipeline::QueryFilter;
use crate::get_mutable_physics_solver;
use crate::types::{RaycastHit, ShapecastHit, ShapecastOptions};

#[unsafe(no_mangle)]
extern "C" fn cast_ray(
    from_x: f32,
    from_y: f32,
    from_z: f32,
    dir_x: f32,
    dir_y: f32,
    dir_z: f32,
    out_hit: *mut RaycastHit,
) -> bool {
    let psd = get_mutable_physics_solver();
    let ray = Ray::new(Vector::new(from_x, from_y, from_z), Vector::new(dir_x, dir_y, dir_z));
    let pstate = &psd.state;
    if let Some((handle, intersection)) = pstate.broad_phase.as_query_pipeline(pstate.narrow_phase.query_dispatcher(), &pstate.rigid_body_set, &pstate.collider_set, QueryFilter::default()).cast_ray_and_get_normal(
        &ray,
        4.0,
        true
    ) {
        let point = ray.point_at(intersection.time_of_impact);
        let normal = intersection.normal;
        let face_id = match intersection.feature {
            FeatureId::Face(id) => id,
            FeatureId::Vertex(id) => id,
            FeatureId::Edge(id) => id,
            _ => 0,
        };
        let distance = intersection.time_of_impact;
        let uv = vector![0.0, 0.0];
        let hit = RaycastHit {
            m_point: point,
            m_normal: normal,
            m_face_id: face_id,
            m_distance: distance,
            m_uv: uv,
            m_collider: handle.into(),
        };
        unsafe {
            *out_hit = hit;
        }
        true
    } else {
        false
    }
}

#[unsafe(no_mangle)]
extern "C" fn cast_cuboid(
    from_x: f32,
    from_y: f32,
    from_z: f32,
    dir_x: f32,
    dir_y: f32,
    dir_z: f32,
    half_extents_x: f32,
    half_extents_y: f32,
    half_extents_z: f32,
    options : ShapecastOptions,
    out_hit: *mut ShapecastHit,
) -> bool{
    let psd = get_mutable_physics_solver();
    let pstate = &psd.state;

    let shape = Cuboid::new(Vector::new(half_extents_x, half_extents_y, half_extents_z));
    let shape_pos = Pose::from_parts(Vector::new(from_x, from_y, from_z), Rot3::from_vec4(Vec4::new(1.0, 0.0, 0.0, 0.0)));
    let shape_velocity = Vector::new(dir_x, dir_y, dir_z);
    let options = ShapeCastOptions {
        max_time_of_impact: options.max_time_of_impact,
        target_distance: options.target_distance,
        stop_at_penetration: options.stop_at_penetration,
        compute_impact_geometry_on_penetration: options.compute_impact_geometry_on_penetration,
    };

    if let Some((collider_handle, hit)) = pstate.broad_phase
        .as_query_pipeline(pstate.narrow_phase.query_dispatcher(), &pstate.rigid_body_set, &pstate.collider_set, QueryFilter::default())
        .cast_shape(
        &shape_pos,
        shape_velocity,
        &shape,
        options
    ) {
        hit.transform1_by(pstate.collider_set.get(collider_handle).unwrap().position());
        let point : Vec3 = hit.witness1;
        let normal = hit.normal1;
        let distance = shape_velocity.length() * hit.time_of_impact;
        let uv = vector![0.0, 0.0];
        let hit = ShapecastHit {
            m_point: point,
            m_normal: normal,
            m_distance: distance,
            m_uv: uv,
            m_collider: collider_handle.into(),
        };
        unsafe {
            *out_hit = hit;
        }
        true
    } else {
        false
    }
}