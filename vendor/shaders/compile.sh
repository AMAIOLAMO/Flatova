#!/bin/sh

GLSLC_COMPILER=glslc

for vert in ./*.vert; do
    $GLSLC_COMPILER "$vert" -o "$vert.spv"
done

for frag in ./*.frag; do
    $GLSLC_COMPILER "$frag" -o "$frag.spv"
done
