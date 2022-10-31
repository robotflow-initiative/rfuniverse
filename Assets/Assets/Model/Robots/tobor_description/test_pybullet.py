import pybullet as p
import pybullet_data
import time

physicsClient = p.connect(p.GUI)
p.setAdditionalSearchPath(pybullet_data.getDataPath()) #optionally
p.setGravity(0,0,-10)
planeId = p.loadURDF("plane.urdf")
startPos = [0,0,1]
startOrientation = p.getQuaternionFromEuler([0,0,0])

# robotId = p.loadURDF("tobor.urdf", startPos, startOrientation)
robotId = p.loadURDF("tobor_arm.urdf", startPos, startOrientation)

joint_num = p.getNumJoints(robotId)
print("joint_number:", joint_num)

#for i in range(joint_num):
#    print("joint ", i, " info:", p.getJointInfo(robotId, i))

camTargetPos = [0, 0, 0]
cameraUp = [0, 0, 1]
cameraPos = [1, 1, 1]
yaw=90
pitch = -10.0
roll = 0
upAxisIndex = 2
camDistance = 4
pixelWidth = 320
pixelHeight = 200
nearPlane = 0.01
farPlane = 100

fov = 60



for i in range (10000):
    p.stepSimulation()
    viewMatrix = p.computeViewMatrixFromYawPitchRoll(camTargetPos, camDistance, yaw, pitch,
                                                            roll, upAxisIndex)
    aspect = pixelWidth / pixelHeight
    projectionMatrix = p.computeProjectionMatrixFOV(fov, aspect, nearPlane, farPlane)
    img_arr = p.getCameraImage(pixelWidth,
                                        pixelHeight,
                                        viewMatrix,
                                        projectionMatrix,
                                        shadow=1,
                                        lightDirection=[1, 1, 1],
                                        renderer=p.ER_BULLET_HARDWARE_OPENGL)
    time.sleep(1./240.)

p.disconnect()