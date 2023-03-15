/*********************************************************************
* Software License Agreement (BSD License)
*
*  Copyright (c) 2021, Bielefeld University
*  All rights reserved.
*
*  Redistribution and use in source and binary forms, with or without
*  modification, are permitted provided that the following conditions
*  are met:
*
*   * Redistributions of source code must retain the above copyright
*     notice, this list of conditions and the following disclaimer.
*   * Redistributions in binary form must reproduce the above
*     copyright notice, this list of conditions and the following
*     disclaimer in the documentation and/or other materials provided
*     with the distribution.
*   * Neither the name of Bielefeld University nor the names of its
*     contributors may be used to endorse or promote products derived
*     from this software without specific prior written permission.
*
*  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
*  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
*  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
*  FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
*  COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
*  INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
*  BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
*  LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
*  CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
*  LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
*  ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
*  POSSIBILITY OF SUCH DAMAGE.
*********************************************************************/

/** \author Robert Haschke */

#include <gtest/gtest.h>
#include <dirent.h>
#include <cstdio>
#include <string>
#include <iostream>

#include <ros/package.h>

int runExternalProcess(const std::string &executable, const std::string &args)
{
  return system((executable + " " + args).c_str());
}

/* Search sr_description/sub_folder for *.urdf.xacro files, run xacro on them and check the result with check_urdf */
void walker(const std::string& sub_folder)
{
  std::string root_path = ros::package::getPath("sr_description") + "/" + sub_folder;
  std::string tmp_name = std::tmpnam(nullptr);

  DIR           *d;
  struct dirent *dir;
  d = opendir(root_path.c_str());
  ASSERT_TRUE(d != NULL) << "Path does not exist: " << root_path;
  while ((dir = readdir(d)))
  {
    if (dir->d_type != DT_DIR)
    {
      std::string file_name = dir->d_name;
      if (file_name.find(std::string(".urdf.xacro")) == file_name.size()-11)
      {
        std::string name = root_path + "/" + file_name;
        printf("Processing: %s\n", name.c_str());
        EXPECT_EQ(runExternalProcess("xacro", name + " > " + tmp_name), 0) << "xacro failed on " << name;
        EXPECT_EQ(runExternalProcess("check_urdf", tmp_name), 0) << "check_urdf failed for " << name;
      }
    }
  }
  closedir(d);
}

TEST(URDF, CorrectFormat)
{
  walker("robots");
}

int main(int argc, char **argv)
{
  testing::InitGoogleTest(&argc, argv);
  return RUN_ALL_TESTS();
}
