# Blender Model of the Dexterous Hand

## Origin

This blender model was created by Shadow Robot Company in 2007-11-14.
Most meshes used in the URDF derive from this Blender model.

## Description

The model is a multi-mesh model, fully rigged, including IK.

## Usage

When editing this file, note the distinction between the base meshes in layers that can be modified and their instantiation as group in the armature that should not be modified.

For animations, move the *dest bones to benefit from IK.

![Tutorial](BlenderHandTutoComments.png)

## Changelog

* 2012-07-22
    - Made meshes show only envelops
    - Made all objects Z-aligned for easy URDF insertion
    - Made model decimate ready
* 2011 to 2012
    - Strongly changed with mesh reduction, group and armature rework
* 2007-11-14
    - Initial model

## Authors

* Shadow Robot Company
* UPMC (Guillaume Walck)

## LICENSE

CC-BY-4.0
