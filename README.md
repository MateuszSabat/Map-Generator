# Procedural Map Generator

![](https://github.com/MateuszSabat/Map-Generator/blob/master/Assets/Procedural%20Map/Data/Maps/map.png)


### Features
  * Generation is based on custom [gradient generation](https://github.com/MateuszSabat/Map-Generator/blob/master/Assets/Procedural%20Map/Compute/GradientGenerator.compute) and [perlin-like noise](https://github.com/MateuszSabat/Map-Generator/blob/master/Assets/Procedural%20Map/Compute/FieldGenerator.compute)
  * Heat-humidity biome generation is easy to extend - all you need is to change landSample and waterSample textures, height-, humidity- and heatLevel values in MapGenerator script
  * Rocks are based on slope of terrain
  * Map is saved as png file in [Assets/ProceduralMap/Data/Maps](https://github.com/MateuszSabat/Map-Generator/tree/master/Assets/Procedural%20Map/Data/Maps)
  * Whole procedural generation proceeds on GPU what provides the fastest results
  * There is an additional [script](https://github.com/MateuszSabat/Map-Generator/blob/master/Assets/Procedural%20Map/Scripts/MapDisplay.cs) that creates landscape mesh based on generated height and texture
  * Current version returns map of archipelago that is coverd by ice on the edges to provide natural barrier of the world, you can change it by changing [EditHeat.compute](https://github.com/MateuszSabat/Map-Generator/blob/master/Assets/Procedural%20Map/Compute/EditHeat.compute)
  
### What does that tool really do?
You can generate only png map (when you press button "Generate" in MapGenerator script), or png map and MapDisplay mesh (when you press button "Create" in MapDisplay script)

And here is how it happens
  * Firstly height, heat and humidity arrays are being generated
    * There is FieldGenerator for each array that generates raw 2d random and continous field of floats flatten to a 1d array
    * Than every field is edited to add its characteristic features (eg. heat may decrease with distance from center or with altitude or both)
  * When these are generated map is being created
    * Every point on map gets its color based on height, heat and humidity
      * Firstly ice, that depends only on heat, is set
      * Than water that depends only on height
      * Than rocks that depend on slope of terrain
      * Than beaches that depend only on height
      * And than biomes that depends on biomeSample texture, heat and humidity
    * That texture is saved as png file, if you pressed button "Generate" in MapGenerator generation ends now
    * If not texture of mesh of MapDisplay is being genereted than. It is proceeded in similar way to above texture, but there is no water added, just rocks, sand and biomes;
  * Next MapDisplay mesh is being created - it's simply height-based mesh with previously generated no-water texture
  
### Unity Version
2019.3.4f1
