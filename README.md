# BenchMaestro


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

- v1.0.13 Alpha
    - Fix: Missing Bench Details Expanders handling
    - Fix: Initial Window position, now Bench are center top
    - Fix: Windows cannot be restored outside of Workarea, support for taskbar on top
    - Fix: Main Window restore position, cannot be restored outside of Workarea
    - Fix: Dispose of ZenStates-Core DLL
- v1.0.12 Alpha
    - Fix: Zen CO label not initialized causing crash at start with non AMD CPUs
    - Fix: Missing BenchMaestro version in Window title
    - Fix: Improved Bench Details Expanders handling
    - Fix: Improved countdown in seconds for bench run
- v1.0.11 Alpha
    - Add: CCD temperature monitoring for Zen via ZenCore
    - Add: Better clock stretching display
    - Fix: Zen check for CO improved
    - Fix: Fix for Zen1 temperature
    - Fix: Fix for Zen1/2 coremap
    - Fix: ThreadID for Logical cores load in details
- v1.0.10 Alpha
    - New: Fixed version of cpuminer-opt binaries thx to JayDDee, all binaries now are using same algo
    - Fix: Zen restored missing SMU and PT version
    - Fix: Support for Alder Lake
    - Fix: Fix for Zen1 temperature
- v1.0.8 Alpha
    - New: Zen early support for 1000s CPU 
    - New: Zen improved CO counts display with + for positive
    - Fix: Zen CoreMap display fix
    - Fix: Zen CPU details display fix
    - Fix: Submit unknown Zen PowerTable
    - Fix: Tentative fix for Alder Lake
- v1.0.7 Alpha
    - New: Zen monitoring PBO PPT/TDC/EDC with Limits
    - New: Zen CoreMap display, shows if and which cores are burned
    - New: Zen clock stretching detection slightly improved with C0 state
    - New: Better layout display for Effective/Reference/Stretching clocks
    - Fix: Converion decimal issue which resulted in 10x scores
    - Fix: Zen missing clocks and stats for samples with burned cores/ccds
    - Fix: Fixed again switched VDDG CCD/IOD voltages
    - Fix: Score grid row issues
    - Fix: Wrong settings runtime shown
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