## README ##

## RasterPaint ##

RasterPaint is a simple program approaching raster graphics. 

## It allows user to: ##
* Add points, lines and polygons (of different width and color),
* List all the objects from the scene, edit and delete them,
* Show grid (with given size) and different color dynamically,
* Clip polygons, fill polygons (using solid color or bitmap),
* Show or hide background on the scene (grid with given size),
* (De-)serialize the entire scene to XML, save scene to .png,
* Reduce Color Palettes using Color Quantization Algorithms.

## Technology: ##
* Logic: C# (elements of C# 6.0)
* Design: WPF (XAML)

## Algorithms: ##
* [Bresenham's Line Algorithm](https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm)
* [Scan-Line Fill Algorithm](http://www.techfak.uni-bielefeld.de/ags/wbski/lehre/digiSA/WS0607/3DVRCG/Vorlesung/13.RT3DCGVR-vertex-2-fragment.pdf)
* [Cohen-Sutherland Algorithm](https://en.wikipedia.org/wiki/Cohen–Sutherland_algorithm)
* [Sutherland-Hodgman Algorithm](https://en.wikipedia.org/wiki/Sutherland–Hodgman_algorithm)
* [Uniform Color Quantization Algorithm](http://web.cs.wpi.edu/~matt/courses/cs563/talks/color_quant/CQindex.html)
* [Popularity Color Quantization Algorithm](http://web.cs.wpi.edu/~matt/courses/cs563/talks/color_quant/CQindex.html)
* [Octree Color Quantization Algorithm](http://www.cubic.org/docs/octree.htm)

## Packages used: ##
* [WriteableBitmapEx](https://writeablebitmapex.codeplex.com/)
* [WpfExtendedToolkit](http://wpftoolkit.codeplex.com/)

## Licence: ##
[MIT Licence](https://opensource.org/licenses/MIT)
