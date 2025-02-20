#!/bin/bash

inkscape logo.svg -w 16 -o logo16.png
inkscape logo.svg -w 24 -o logo24.png
inkscape logo.svg -w 32 -o logo32.png
inkscape logo.svg -w 48 -o logo48.png
inkscape logo.svg -w 256 -o logo256.png

convert logo*.png logo.ico
rm -f logo*.png

