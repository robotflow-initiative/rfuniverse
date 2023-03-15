#!/usr/bin/env python
# -*- coding: utf-8 -*-

######################################################################
# Software License Agreement (BSD License)
#
#  Copyright (c) 2021, Bielefeld University
#  All rights reserved.
#
#  Redistribution and use in source and binary forms, with or without
#  modification, are permitted provided that the following conditions
#  are met:
#
#   * Redistributions of source code must retain the above copyright
#     notice, this list of conditions and the following disclaimer.
#   * Redistributions in binary form must reproduce the above
#     copyright notice, this list of conditions and the following
#     disclaimer in the documentation and/or other materials provided
#     with the distribution.
#   * Neither the name of Bielefeld University nor the names of its
#     contributors may be used to endorse or promote products derived
#     from this software without specific prior written permission.
#
#  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
#  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
#  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
#  FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
#  COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
#  INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
#  BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
#  LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
#  CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
#  LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
#  ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
#  POSSIBILITY OF SUCH DAMAGE.
######################################################################

# Author: Robert Haschke <rhaschke@techfak.uni-bielefeld.de>


from __future__ import absolute_import, print_function

import ast
import re
import os
import unittest
from unittest import subTest  # pylint: disable=C0103
import xml.dom
from xml.dom.minidom import parseString
import xacro

# regex to match whitespace
whitespace = re.compile(r'\s+')


def text_values_match(arg1, arg2):
    # generic comparison
    if whitespace.sub(' ', arg1).strip() == whitespace.sub(' ', arg2).strip():
        return True

    try:  # special handling of dicts: ignore order
        a_dict = ast.literal_eval(arg1)
        b_dict = ast.literal_eval(arg1)
        if (isinstance(a_dict, dict) and isinstance(b_dict, dict) and a_dict == b_dict):
            return True
    except Exception:  # Attribute values aren't dicts
        pass

    # on failure, try to split a and b at whitespace and compare snippets
    def match_splits(arg1, arg2):
        if len(arg1) != len(arg2):
            return False
        el1, el2 = 0, 0
        for el1, el2 in zip(arg1, arg2):
            if el1 == el2:
                continue
            try:  # compare numeric values only up to some accuracy
                if abs(float(el1) - float(el2)) > 1.0e-9:
                    return False
            except ValueError:  # values aren't numeric and not identical
                return False
        return True

    return match_splits(arg1.split(), arg2.split())


def all_attributes_match(arg1, arg2):
    if len(arg1.attributes) != len(arg2.attributes):
        raise AssertionError('Different number of attributes: [{}] != [{}]'.
                             format(', '.join(sorted(arg1.attributes.keys())),
                                    ', '.join(sorted(arg2.attributes.keys()))))
    a_atts = arg1.attributes.items()
    b_atts = arg2.attributes.items()
    a_atts.sort()
    b_atts.sort()

    el1, el2 = 0, 0
    for el1, el2 in zip(a_atts, b_atts):
        if el1[0] != el2[0]:
            raise AssertionError('Different attribute names: %s and %s' % (el1[0], el2[0]))
        if not text_values_match(el1[1], el2[1]):
            raise AssertionError('Different attribute values: {}={} and {}={}'.
                                 format(el1[0], el1[1], el2[0], el2[1]))
    return True


def text_matches(arg1, arg2):
    if text_values_match(arg1, arg2):
        return True
    raise AssertionError("Different text values: '%s' and '%s'" % (arg1, arg2))


def nodes_match(arg1, arg2, ignore_nodes):
    if not arg1 and not arg2:
        return True
    if not arg1 or not arg2:
        return False

    if arg1.nodeType != arg2.nodeType:
        raise AssertionError('Different node types: %s and %s' % (arg1, arg2))

    # compare text-valued nodes
    if arg1.nodeType in [xml.dom.Node.TEXT_NODE,
                         xml.dom.Node.CDATA_SECTION_NODE,
                         xml.dom.Node.COMMENT_NODE]:
        return text_matches(arg1.data, arg2.data)

    # ignore all other nodes except ELEMENTs
    if arg1.nodeType != xml.dom.Node.ELEMENT_NODE:
        return True

    # compare ELEMENT nodes
    if arg1.nodeName != arg2.nodeName:
        raise AssertionError('Different element names: %s and %s' % (arg1.nodeName, arg2.nodeName))

    try:
        all_attributes_match(arg1, arg2)
    except AssertionError as error:
        raise AssertionError('{err} in node <{node}>'.format(err=str(error), node=arg1.nodeName)) from error

    arg1 = arg1.firstChild
    arg2 = arg2.firstChild
    while arg1 or arg2:
        # ignore whitespace-only text nodes
        # we could have several text nodes in a row, due to replacements
        while (arg1 and
               ((arg1.nodeType in ignore_nodes) or
                (arg1.nodeType == xml.dom.Node.TEXT_NODE and whitespace.sub('', arg1.data) == ""))):
            arg1 = arg1.nextSibling
        while (arg2 and
               ((arg2.nodeType in ignore_nodes) or
                (arg2.nodeType == xml.dom.Node.TEXT_NODE and whitespace.sub('', arg2.data) == ""))):
            arg2 = arg2.nextSibling

        nodes_match(arg1, arg2, ignore_nodes)

        if arg1:
            arg1 = arg1.nextSibling
        if arg2:
            arg2 = arg2.nextSibling

    return True


def xml_matches(arg1, arg2, ignore_nodes=None):
    if ignore_nodes is None:
        ignore_nodes = []
    if isinstance(arg1, str):
        return xml_matches(parseString(arg1).documentElement, arg2, ignore_nodes)
    if isinstance(arg2, str):
        return xml_matches(arg1, parseString(arg2).documentElement, ignore_nodes)
    if arg1.nodeType == xml.dom.Node.DOCUMENT_NODE:
        return xml_matches(arg1.documentElement, arg2, ignore_nodes)
    if arg2.nodeType == xml.dom.Node.DOCUMENT_NODE:
        return xml_matches(arg1, arg2.documentElement, ignore_nodes)

    return nodes_match(arg1, arg2, ignore_nodes)


class TestEquality(unittest.TestCase):
    @staticmethod
    def generate_test_params():
        path = os.path.dirname(__file__)
        old_path = os.path.join(path, 'robots.old')
        new_path = os.path.join(path, 'robots.new')

        for name in os.listdir(old_path):
            old_file = os.path.join(old_path, name)
            new_file = os.path.join(new_path, name)

            if name.endswith('.urdf.xacro') and os.path.isfile(old_file) and os.path.isfile(new_file):
                yield name, old_file, new_file

    @staticmethod
    def save_results(name, doc):
        with open(name, 'w', encoding="utf-8") as result_file:
            result_file.write(doc.toprettyxml(indent='  '))

    def test_files(self):
        def process(filename):
            return xacro.process_file(filename)

        results_dir = None
        for name, old_file, new_file in self.generate_test_params():
            with subTest(msg='Checking {}'.format(name)):
                try:
                    old_doc = process(old_file)
                    new_doc = process(new_file)
                    xml_matches(old_doc, new_doc, ignore_nodes=[xml.dom.Node.COMMENT_NODE])
                except AssertionError:
                    if results_dir is None:
                        import tempfile
                        results_dir = tempfile.mkdtemp(prefix='sr_compat')
                        print('Saving mismatching URDFs to:', results_dir)

                    for suffix, doc in zip(['.old', '.new'], [old_doc, new_doc]):
                        self.save_results(os.path.join(results_dir, name + suffix), doc)

                    raise
                except Exception as error:
                    msg = str(error) or repr(error)
                    xacro.error(msg)
                    xacro.print_location()
                    raise


if __name__ == '__main__':
    unittest.main()
