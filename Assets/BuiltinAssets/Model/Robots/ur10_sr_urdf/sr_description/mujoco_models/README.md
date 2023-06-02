# Hand E Mujoco Models

This directory contains files necessary for a Mujoco simulation of the dexterous hand (Hand E).

## Under-Actuation Logic

The simulation correctly models the under-actuation of J1 and J2 of the first, middle, ring and little fingers by defining one tendon for each J1-J2 pair. The model defines two separate joints, but only one actuator for the pair (J0), which drives the tendon. The tendon is abstract, with it's "length" being the combined angles of J1 and J2. [mujoco_ros_control](https://github.com/shadow-robot/mujoco_ros_pkgs/tree/kinetic-devel/mujoco_ros_control) has been modified to report joint angles from the two separate joints, but half of the tendon effort for each joint's effort. Additionally, effort or positions commands received for J1 and J2 are summed and applied to J0. This is a reasonable approximation of the real Hand E, with the exception that the real Hand E tends to actuate J2 before J1 due to internal friction.

## Explanation of Files

* [rh_trajectory_controller.yaml](rh_trajectory_controller.yaml)
    * Contains config for the joint trajectory controller. PID values were based on those used in the pre-existing Gazebo simulation, with anti-windup added when integral windup was observed to be an issue.
* [shared_assets.xml](shared_assets.xml)
    * Contains assets (e.g. mesh and material definitions) likely to be common to dexterous hand models (Hand E, G etc.).
* [shared_options.xml](shared_options.xml)
    * Contains simulation options likely to be common to dexterous hand models (Hand E, G etc.). Separate from shared_assets.xml due to Mujoco inclusion syntax (see sr_hand_e_environment.xml).
* [sr_hand_e_options.xml](sr_hand_e_options.xml)
    * Contains simulation options specific to Hand E (actuator definitions, collision exemptions etc.).
* [sr_hand_e_model.xml](sr_hand_e_model.xml)
    * Contains the main definition of the Mujoco model of Hand E.
* [sr_hand_e_environment.xml](sr_hand_e_environment.xml)
    * Contains an environment definition and include statements to import the above elements.
* [sr_hand_e_environment_underactuation_test.xml](sr_hand_e_environment_underactuation_test.xml)
    * Similar to sr_hand_e_environment.xml, but adds a cylinder to test the under-actuation of FFJ0, MFJ0, RFJ0 and LFJ0.