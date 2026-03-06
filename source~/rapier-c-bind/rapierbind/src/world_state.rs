use rapier3d::dynamics::{CCDSolver, ImpulseJointSet, IntegrationParameters, IslandManager, MultibodyJointSet, RigidBodySet, SpringCoefficients};
use rapier3d::geometry::{ColliderSet, DefaultBroadPhase, NarrowPhase};
use rapier3d::math::Vector;
use rapier3d::pipeline::{ChannelEventCollector, EventHandler, PhysicsHooks, PhysicsPipeline};
use serde::{Deserialize, Serialize};
use xxhash_rust::xxh64::xxh64;
use crate::{get_mutable_physics_solver, SerializableCollisionEvent};

// PhysicsSolverData is a struct that holds all the data needed to solve physics.
pub struct PhysicsWorld<'a> {
    pub physics_pipeline: PhysicsPipeline,
    pub physics_hooks: &'a dyn PhysicsHooks,
    pub event_handler: &'a dyn EventHandler,
    pub state : PhysicsWorldState
}

#[derive(Serialize, Deserialize)]
pub struct PhysicsWorldState {
    pub gravity: Vector,
    pub integration_parameters: IntegrationParameters,
    pub island_manager: IslandManager,
    pub broad_phase: DefaultBroadPhase,
    pub narrow_phase: NarrowPhase,
    pub impulse_joint_set: ImpulseJointSet,
    pub multibody_joint_set: MultibodyJointSet,
    pub ccd_solver: CCDSolver,
    pub rigid_body_set: RigidBodySet,
    pub collider_set: ColliderSet,
}

impl Default for PhysicsWorldState {
    fn default() -> Self{
        let mut integration_parameters = IntegrationParameters::default();
        integration_parameters.dt = 1.0 / 50.0;
        integration_parameters.min_ccd_dt = 1.0 / 50.0 / 100.0;
        PhysicsWorldState {
            gravity: Vector::new(0.0, -9.81, 0.0),
            integration_parameters,
            island_manager: IslandManager::new(),
            broad_phase: DefaultBroadPhase::new(),
            narrow_phase: NarrowPhase::new(),
            impulse_joint_set: ImpulseJointSet::new(),
            multibody_joint_set: MultibodyJointSet::new(),
            ccd_solver: CCDSolver::new(),
            rigid_body_set: RigidBodySet::new(),
            collider_set: ColliderSet::new(),
        }
    }
}

impl Default for PhysicsWorld<'_> {
    fn default() -> Self {
        PhysicsWorld {
            physics_pipeline: PhysicsPipeline::new(),
            physics_hooks: &(),
            event_handler: &(),
            state: PhysicsWorldState::default()
        }
    }
}

impl PhysicsWorld<'_> {
    pub fn hash(&mut self) -> u64{
        let ss = bincode::serde::encode_to_vec(&self.state, bincode::config::standard()).unwrap();
        xxh64(&ss, 0)
    }

    pub fn solve(&mut self) -> Vec<SerializableCollisionEvent> {
        let (collision_send, collision_recv) = std::sync::mpsc::channel();
        let (contact_force_send, _contact_force_recv) = std::sync::mpsc::channel();
        let event_handler = ChannelEventCollector::new(collision_send, contact_force_send);

        self.physics_pipeline.step(
            self.state.gravity,
            &self.state.integration_parameters,
            &mut self.state.island_manager,
            &mut self.state.broad_phase,
            &mut self.state.narrow_phase,
            &mut self.state.rigid_body_set,
            &mut self.state.collider_set,
            &mut self.state.impulse_joint_set,
            &mut self.state.multibody_joint_set,
            &mut self.state.ccd_solver,
            &(),
            &event_handler,
        );

        let mut collision_events = Vec::new();
        while let Ok(collision_event) = collision_recv.try_recv() {
            if collision_event.started() {
                collision_events.push(SerializableCollisionEvent {
                    collider1: collision_event.collider1().into(),
                    collider2: collision_event.collider2().into(),
                    is_started: true,
                });
            } else if collision_event.stopped() {
                collision_events.push(SerializableCollisionEvent {
                    collider1: collision_event.collider1().into(),
                    collider2: collision_event.collider2().into(),
                    is_started: false,
                });
            } else {
                log::warn!("Unknown collision event: {:?}", collision_event);
            }
        }

        collision_events
    }
}

#[unsafe(no_mangle)]
extern "C" fn set_gravity(x: f32, y: f32, z: f32) {
    get_mutable_physics_solver().state.gravity = Vector::new(x, y, z);
}

#[unsafe(no_mangle)]
extern "C" fn set_time_step(dt: f32) {
    get_mutable_physics_solver().state.integration_parameters.dt = dt;
    get_mutable_physics_solver()
        .state
        .integration_parameters
        .min_ccd_dt = dt / 100.0;
}

#[unsafe(no_mangle)]
pub extern "C" fn set_integration_parameters(
    // Time step
    dt: f32,
    // Solver parameters
    solver_iterations: usize,
    solver_pgs_iterations: usize,
    solver_stabilization_iterations: usize,
    ccd_substeps: usize,
    // Damping parameters
    contact_damping_ratio: f32,
    // Frequency parameters
    contact_frequency: f32,
    // Prediction parameters
    prediction_distance: f32,
    max_corrective_velocity: f32,
    // Length unit
    length_unit: f32,
) {
    let psd = get_mutable_physics_solver();
    psd.state.integration_parameters.dt = dt;
    psd.state.integration_parameters.min_ccd_dt = dt / 100.0;
    psd.state.integration_parameters.num_solver_iterations = solver_iterations;
    psd.state.integration_parameters.num_internal_pgs_iterations = solver_pgs_iterations;
    //psd.integration_parameters.friction_model = ;
    psd.state.integration_parameters
        .num_internal_stabilization_iterations = solver_stabilization_iterations;
    psd.state.integration_parameters.max_ccd_substeps = ccd_substeps;
    psd.state.integration_parameters.contact_softness = SpringCoefficients::new(contact_frequency, contact_damping_ratio);
    psd.state.integration_parameters.normalized_prediction_distance = prediction_distance;
    psd.state.integration_parameters
        .normalized_max_corrective_velocity = max_corrective_velocity;
    psd.state.integration_parameters.length_unit = length_unit;
}

#[unsafe(no_mangle)]
extern "C" fn get_physics_world_hash() -> u64 {
    get_mutable_physics_solver().hash()
}