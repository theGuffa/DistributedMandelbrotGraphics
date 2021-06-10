# DistributedMandelbrotGraphics
A Windows desktop application that lets you explore mandelbrot graphics, and lets you use additional computers in the network to help with the calculations.

Fetaures:
- Mouse and keyboard shortcuts to quickly explore the mandelbrot set.
- Responsive interface that lets you zoom, pan and choose colors while the image is calculated.
- Ability to use a worker application running on other computers in the network.
- A set of common screen and image sizes to use for the target image.
- Many color sets to choose from.
- A color offset to adjust how color sets are used.
- Different calculation precision settings to use for different depths.
- Automatic precision change depending on depth.
- Two different smoothing modes.
- Save image settings in a JSON format.
- Image export to JPEG/PNG/TIFF/BMP.

Current version:
0.9

Known problems:
- During panning the updates of the image doesn't work very well, so movement is jerky.
- When panning there is a bug that sometimes keeps calculated data from being written to the internal array, leaving squares and bands of missing data.
- Auto precision doesn't work perfectly in the entire set, at some places the precision limitations get very visible before the precision is increased.
