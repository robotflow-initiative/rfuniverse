# Copyright (c) 2022 Mateus Menezes

# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:

# The above copyright notice and this permission notice shall be included in all
# copies or substantial portions of the Software.

# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
# SOFTWARE.

import os
from ament_index_python.packages import get_package_share_directory
from launch import LaunchDescription
from launch.actions import DeclareLaunchArgument
from launch_ros.actions import Node
from launch.substitutions import Command, FindExecutable, LaunchConfiguration, PathJoinSubstitution
from launch_ros.substitutions import FindPackageShare
from launch.conditions import IfCondition, UnlessCondition
from launch.logging import LaunchLogger
import xacro

def generate_launch_description():

    declared_arguments = DeclareLaunchArgument('model', default_value='x7_lite',
                              description='Robot model name.')
    model = LaunchConfiguration('model')

    pkg_name = 'daxbot_description'

    # xacro_file = os.path.join(
    #     get_package_share_directory(pkg_name),
    #     'urdf',
    #     'BASE0902.urdf'
    # )
    xacro_file = PathJoinSubstitution([
        get_package_share_directory(pkg_name),
        'urdf', model,
        'daxbot.urdf.xacro'
    ])

    rviz_robot_config = os.path.join(
      get_package_share_directory(pkg_name),
      'config',
      'daxbot.rviz'
    )

    # robot_description_urdf = xacro.process_file(xacro_file).toxml()
    robot_description_urdf = Command([
        PathJoinSubstitution([FindExecutable(name='xacro')]),
        ' ',xacro_file
    ])

    return LaunchDescription([
        declared_arguments,
        Node(
            package="rviz2",
            executable="rviz2",
            name="rviz2",
            output="log",
            arguments=['-d', rviz_robot_config]
        ),
        Node(
            package='robot_state_publisher',
            executable='robot_state_publisher',
            output='screen',
            parameters=[{'robot_description': robot_description_urdf}],
        ),
        Node(
            package="joint_state_publisher_gui",
            executable="joint_state_publisher_gui",
        )
    ])
