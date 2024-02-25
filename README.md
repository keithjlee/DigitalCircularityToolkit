![](Resources/anim.mp4)

# DigitalCircularityToolkit
A Grasshopper plugin to facilitate circular/reuse/inventory-driven design workflows, including:
- Extracting principal axes from arbitrary geometry
- Abstracing objects as primitive lines, planes, boxes, and spheres
- Knolling and alignment
- Shape characterization: LineScore, PlaneScore, BoxScore, SphereScore, RadialSignature
- Fourier shape descriptors (in complex and real coordinates) of radial signatures
- Custom feature vector definitions
- Optimal assignment using the Hungarian matching algorithm
- Visualization tools for assignment

# Development Notes
- This plugin was developed by Keith J. Lee, PhD Candidate @ MIT, in collaboration with the Digital Circularity research collective within the Digital Structures research group in the Department of Architecture, Building Technology.
- [Karl-Johan Soerensen](https://github.com/soerensenkarl) developed I/O functionality for read/write workflows with Google Sheets and local .CSV files. To be released publicly in the near future.
- Initial release was given to students taking the IAP2024 design workshop: *4.181 Digital Circularity: Tooling up for reuse with Odds & Mods*
- Public release coincides with the S2024 design workshop: *4.185 ODDS & MODS Castaways*
- The Digital Circularity team consists of Rachel Blowes (SMArchS BT '25), Celia Chaussabel (SMArchS AD '25), Alex Htet Kyaw (SMArchS Computation '25), Keith J. Lee (PhD BT '25), and Karl-Johan Soerensen (SMArchS Computation '25, SM CEE '25). It is advised by Professor Caitlin Mueller.

# Installation
The initial public build was compiled February 25, 2024 in preparation for registration in the Food4Rhino ecosystem.

1. Find and download the latest build (.zip file) in the [releases](https://github.com/keithjlee/DigitalCircularityToolkit/releases) page.
2. Find your Grasshopper components folder by opening Grasshopper, then `File>Special Folders>Components Folder`
3. **For Windows users:** right click the downloaded .zip file and select `Properties`, and make sure that `Unblock` is checked off.
4. Directly extract the contents of the .zip file to your Components Folder.
5. Restart Rhino/Grasshopper

