# PixelLab

A comprehensive image processing and color space visualization tool built with C# and Emgu CV.  
Explore the hidden structure of color by transforming images into 3D geometric spaces.

## Features

### Image Operations
- Load images via file dialog or drag & drop (supports .jpg, .png, .bmp).
- Rotate the processed image 90 degrees clockwise.
- Save the processed image in JPEG, PNG, or BMP format.
- View image info (filename, format, size on disk, dimensions, total pixels, full path).

### Color Manipulation
- Choose a color space: RGB, HSV, LAB, YCbCr, CMYK, or YUV.
- Display individual channels as grayscale images (e.g., Red/Green/Blue, Hue/Saturation/Value, etc.).
- Toggle channels on/off to see their contribution.
- Adjust channel intensity via three trackbars (-100 to +100).
- Quantize colors: reduce the palette to 2, 4, 8, 16, or 256 (original) levels.

### Interactive Color Information
- Click on the original or processed image to get the pixel's color values in all supported spaces simultaneously:
  - RGB: (R, G, B)
  - HSV: (Hue, Saturation%, Value%)
  - LAB: (L, a, b)
  - YCbCr: (Y, Cr, Cb)
  - CMYK: (C%, M%, Y%, K%)
  - YUV: (Y, U, V)

### 3D Chart-based Visualization
- Click "3D Chart Spaces" to see a bubble-chart representation of the image's colors in each space.
- Rotate / Zoom using the mouse.
- Click a bubble to see its full color information.
- For the three classic spaces, the chart even draws the geometric wireframes:
  - RGB: Cube (R,G,B axes)
  - HSV: Cone (Hue, Saturation, Value)
  - LAB: Sphere (axes L, a, b)

### Real 3D Custom Rendering
- Click "Real 3D Spaces" for a high-performance, fully interactive 3D view rendered with GDI+.
- Drag to rotate freely; mouse wheel to zoom.
- Each space is visually enclosed in its mathematical boundary:
  - RGB / YCbCr / YUV / CMYK: Cube
  - HSV: Cylinder
  - LAB: Axes (Cartesian)
- Axis labels are displayed in the view.
- Click a point to instantly see its color broken down into all six spaces.

## Requirements

- Operating System: Windows (the application uses WinForms and native Emgu CV binaries)
- .NET Framework: 4.7.2 or higher / .NET 6/8 with Windows compatibility
- GPU: Not required (all rendering is software-based)

## Dependencies

| Package | Purpose |
|--------|---------|
| Emgu.CV | OpenCV wrapper for .NET (image processing and color space conversions) |
| Emgu.CV.Bitmap | Extension methods for Mat to Bitmap conversion |
| Emgu.CV.runtime.windows | Native OpenCV DLLs for Windows |
| System.Windows.Forms.DataVisualization | Chart control used for the bubble-chart 3D views |

## Getting Started

### Build from Source
1. Clone the repository:
   git clone https://github.com/majddakhoul/PixelLab.git
2. Open PixelLab.sln in Visual Studio.
3. Restore NuGet packages (right-click solution, select Restore NuGet Packages).
4. Build the solution (Ctrl+Shift+B).
5. Run the project (F5).

### Installation (Release)
Copy the contents of the bin\Release folder (including all DLLs) to any folder and run PixelLab.exe. No installer required.

## Usage

1. Launch the application.
2. Click "Load Image" or drag an image onto the left pane.
3. Select a color space from the dropdown and use the trackbars to adjust channels.
4. Click "3D Chart Spaces" to view an interactive bubble chart, or "Real 3D Spaces" for the immersive wireframe view.
5. In any view, click a color point to see its full multi-space representation.

## Project Structure

PixelLab/
  Form1.cs           - Main application logic
  Program.cs         - Entry point
  PixelLab.csproj    - Project file with NuGet references
  README.md
  (other resources)

## How It Works

### Color Processing
Emgu CV converts the image to the selected color space. The three channels are split, modified, merged back, and (if necessary) re-converted to BGR for display. Quantization is applied directly on the final 8-bit per-channel bitmap.

### Chart-based 3D
A Bubble series is created for each color space. The three coordinates are mapped to (X, Y, bubbleSize), where bubble size encodes the third dimension. Standard 3D chart manipulation (rotation, zoom) is provided by the control. Wireframes are drawn using additional Line series.

### Real 3D Rendering
Points are projected from the color-space coordinates into screen space using standard 3D rotation matrices. Depth sorting ensures proper occlusion. The geometric boundaries (cube, cylinder, axes) are drawn as line segments.

## Known Limitations

- Very large images (over 10 megapixels) may slow down the real-time 3D views because every pixel is plotted as a circle.
- The CMYK space visualization uses a CMY projection (no K channel) for 3D; the K value is still shown in the info panel.
- The "sphere" label for LAB is nominal; the actual visualisation shows Cartesian axes rather than a true sphere boundary.

## Contributing

Pull requests are welcome! For major changes, please open an issue first to discuss what you would like to change.

## License

This project is licensed under the MIT License. See the LICENSE file for details.

## Acknowledgments

- Emgu CV for making OpenCV accessible in .NET.
- Microsoft Chart Controls for the 3D charting foundation.
- All the open-source contributors whose code snippets and ideas helped shape this tool.
