#!/bin/bash

# Software License Agreement (BSD License)
# Copyright © 2022 belongs to Shadow Robot Company Ltd.
# All rights reserved.
#
# Redistribution and use in source and binary forms, with or without modification,
# are permitted provided that the following conditions are met:
#   1. Redistributions of source code must retain the above copyright notice,
#      this list of conditions and the following disclaimer.
#   2. Redistributions in binary form must reproduce the above copyright notice,
#      this list of conditions and the following disclaimer in the documentation
#      and/or other materials provided with the distribution.
#   3. Neither the name of Shadow Robot Company Ltd nor the names of its contributors
#      may be used to endorse or promote products derived from this software without
#      specific prior written permission.
#
# This software is provided by Shadow Robot Company Ltd "as is" and any express
# or implied warranties, including, but not limited to, the implied warranties of
# merchantability and fitness for a particular purpose are disclaimed. In no event
# shall the copyright holder be liable for any direct, indirect, incidental, special,
# exemplary, or consequential damages (including, but not limited to, procurement of
# substitute goods or services; loss of use, data, or profits; or business interruption)
# however caused and on any theory of liability, whether in contract, strict liability,
# or tort (including negligence or otherwise) arising in any way out of the use of this
# software, even if advised of the possibility of such damage.

# Launch all the existing hand models in RVIZ for visual inspection.

source ./test_models.sh

# Testing default hand - hand_type:="hand_e" hand_version:="E3M5" side:="right" fingers:="all" tip_sensors:="pst"
launch sr_hand

# Testing left side
launch sr_hand side:="left"

# Testing 'bt_sp' tip sensors
launch sr_hand tip_sensors:="bt_sp"

# Testing 'bt_2p' tip sensors
launch sr_hand tip_sensors:="bt_2p"

# Testing custom finger set
launch sr_hand fingers:="th,ff,mf,rf"
launch sr_hand fingers:="th,ff,mf"

# Testing hand lite
launch sr_hand hand_type:="hand_g" hand_version:="G1M5"

# Testing muscle hand
launch sr_hand hand_type:="hand_c" hand_version:="C6M2"
