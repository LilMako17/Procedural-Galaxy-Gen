# Procedural-Galaxy-Gen
A data driven procedural galaxy generator (a la Stellaris) made in Unity.

A variable set of stars 3d space connected  by "hyperlanes". Generated bassed on Poisson Disc Sampling algorithm, parameters loaded from Scriptable Objects.
Each star has a solar system of procedurally generated planets and moons. These planetoids are run-time generated meshes using a custom shader to render color based on vertex height and fake lighting.
Each solar system is fully serializable and deterministicly created, so it can saved and loaded to and from disc.
Generated output is intentially non-realisticm there are no orbital mechanics or physics simulations involved.
![Image Sequence_003_0033](https://github.com/LilMako17/Procedural-Galaxy-Gen/assets/6510984/a5fdf46b-7085-4913-b19a-a74ee1bebb00)
![Image Sequence_006_0004](https://github.com/LilMako17/Procedural-Galaxy-Gen/assets/6510984/8019125b-2093-40c4-9746-12ca9d320076)
![Image Sequence_005_0015](https://github.com/LilMako17/Procedural-Galaxy-Gen/assets/6510984/36668b00-7505-4efb-8c8e-e1494d2ec8b3)
![Image Sequence_003_0015](https://github.com/LilMako17/Procedural-Galaxy-Gen/assets/6510984/791809a9-2b14-4525-bcdd-903e9778b5d2)
![Image Sequence_004_0008](https://github.com/LilMako17/Procedural-Galaxy-Gen/assets/6510984/0ed9902d-a919-45c9-86b4-6c25f6f81e4d)

Authored in Unity 2022.3.4f1
To Run: start from "Assets/Scenes/Galaxy.unity"

Third Party assets:
<br>Space Graphics Toolkit - for rendering stars
<br>Odin Serializer - for serializing data
<br>Unity HFSM - basic state machine library
<br>Kenny UI - UI assets
