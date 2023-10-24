# Procedural-Galaxy-Gen
A data driven procedural galaxy generator (a la Stellaris) made in Unity.

A variable set of stars 3d space connected  by "hyperlanes". Generated bassed on Poisson Disc Sampling algorithm, parameters loaded from Scriptable Objects.
Each star has a solar system of procedurally generated planets and moons. These planetoids are run-time generated meshes using a custom shader to render color based on vertex height and fake lighting.
Each solar system is fully serializable and deterministicly created, so it can saved and loaded to and from disc.
![Image Sequence_003_0033](https://github.com/LilMako17/Procedural-Galaxy-Gen/assets/6510984/a5fdf46b-7085-4913-b19a-a74ee1bebb00)
![Image Sequence_003_0015](https://github.com/LilMako17/Procedural-Galaxy-Gen/assets/6510984/791809a9-2b14-4525-bcdd-903e9778b5d2)
![Image Sequence_004_0008](https://github.com/LilMako17/Procedural-Galaxy-Gen/assets/6510984/0ed9902d-a919-45c9-86b4-6c25f6f81e4d)
