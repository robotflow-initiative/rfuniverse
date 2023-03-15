cmake_minimum_required(VERSION 3.0.2)
project(sr_description)

find_package(catkin REQUIRED)

catkin_package(
    DEPENDS
    CATKIN_DEPENDS
    INCLUDE_DIRS
    LIBRARIES
)

if (CATKIN_ENABLE_TESTING)
  find_package(urdf REQUIRED)

  include_directories(${urdf_INCLUDE_DIRS})
  catkin_add_gtest(test_sr_description_urdf test/test_sr_description_urdf.cpp)
  target_link_libraries(test_sr_description_urdf ${urdf_LIBRARIES})

endif()

install(DIRECTORY hand/
   DESTINATION ${CATKIN_PACKAGE_SHARE_DESTINATION}/hand)

install(DIRECTORY meshes/
   DESTINATION ${CATKIN_PACKAGE_SHARE_DESTINATION}/meshes)

install(DIRECTORY loaders/
   DESTINATION ${CATKIN_PACKAGE_SHARE_DESTINATION}/loaders)

install(DIRECTORY robots/
   DESTINATION ${CATKIN_PACKAGE_SHARE_DESTINATION}/robots)

install(FILES other/xacro/materials.urdf.xacro
   DESTINATION ${CATKIN_PACKAGE_SHARE_DESTINATION})
