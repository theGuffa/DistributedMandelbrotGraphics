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
- v0.9

Requirement:
- .NET 5.0

Known problems:
- When panning while calculations are running there is a bug that keeps some calculated data from being written to the internal array, leaving squares and bands of missing data. Workaround to recalculate: toggle sharpening (Ctrl+Q) on and off to force recalculation of the data.
- Auto precision doesn't work perfectly in the entire set, at some places the precision limitations get very visible before the precision is increased.

Quick start:
- Use the mouse wheel to zoom in/out in the image.
- Use Alt+Right to increase the number of iterations.
- Use Ctrl+(Up/Down/Left/Right) to adjust colors.
- Use F1 to bring up the Quick Guide.
- Use Ctrl+N to restart with the entire set.
