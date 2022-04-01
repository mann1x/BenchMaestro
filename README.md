﻿# BenchMaestro


BenchMaestro is a benchmarking and tools uility for CPU & GPU


## **USE AT YOUR OWN RISK**


## Installation

It's a standalone application; the only software pre-requisite is the Desktop Runtime for .NET Core 5.0 (https://versionsof.net/core/5.0/5.0.15/).

Move it into a permanent directory, you can create a shortcut to launch it or use the drop-down menu option to create it on the desktop.

An installer will be available at some point.


## Usage

## **BENCHMARKS WITH MINERS WILL NEED WHITELISTING OF BENCHMAESTRO DIRECTORY IN AV OR MS DEFENDER**

OCN Support thread: https://www.overclock.net/threads/benchmaestro-cpu-gpu-benchmarking-and-tools-utility.1797775/

"CPPC Custom:" You can define your custom CPPC order for thread scheduling by moving with drag & drop the cores in the desired order and checking "Enable"

**Please use the Issues tab on GitHub if you find issues or have a request**


## Compilation

You can compile with Visual Studio 2019 and .NET Core 5.


## Changelog:

- v1.0.6 Alpha
    - Fix: Switched VDDG CCD/IOD voltages
    - Fix: Reading CPPC tags from localized Windows
- v1.0.5 Alpha
    - Fix: Autoupdater bug
- v1.0.4 Alpha
    - Fix: more retries for Zen PT
    - Fix: Codepage issue with Win11
- v1.0.3 Alpha
    - New: First public Alpha
    - Known issue: details box resize with window is messed up
    - Known issue: fix for logical threads assignment on Alder Lake P/E cores