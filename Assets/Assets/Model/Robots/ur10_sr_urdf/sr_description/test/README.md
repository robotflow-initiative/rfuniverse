# Visual Tests

In addition to the gtest test_sr_description_urdf which tests xacro expansion and resulting urdf validity, 
visual tests have been added to check if the generated urdf produce the desired hand with the correct joints.

The visual tests are scripts that require user interaction:

1. Start the test
  ./tests_hand.sh

2. User is requested to press enter the first time to start
3. To finish an inspection, press CTRL-C in the terminal or close rviz
