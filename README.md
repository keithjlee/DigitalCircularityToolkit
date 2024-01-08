![](Resources/axo.png)
# DigitalCircularityToolkit
A Grasshopper plugin to facilitate circular/reuse/inventory-driven design workflows, including:
- Extracting principal axes from arbitrary geometry
- Abstractions as primitive lines, planes, boxes, and spheres
- Knolling and alignment
- Shape characterization: LineScore, PlaneScore, BoxScore, SphereScore, RadialSignature
- Fourier shape descriptors (in complex and real coordinates) of radial signatures
- Custom feature vector definitions
- Optimal assignment using the Hungarian matching algorithm
- Visualization tools for assignment

Released privately to students taking the MIT IAP2024 workshop 4.181 Digital Circularity: Tooling Up for Reuse with Odds & Mods. Will be released to public after initial feedback.

# Components
35 Components are split into seven categories:
1. Objects
2. Sets
3. Utilities
4. Characterization
5. Matching
6. Orientation
7. Distance
   
![](Resources/Complete.png)

<!-- # Objects
![](Resources/)
All analysis in DigitalCircularityToolkit revolves around `Object`s. In its most generic form, you can input a curve, BRep, or mesh as an object: -->
