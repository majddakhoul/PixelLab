<div align="center">

<br/>

<h1>PixelLab</h1>

<p><strong>A comprehensive image processing and color space visualization tool built with C# and Emgu CV</strong><br/>
Explore the hidden structure of color by transforming images into 3D geometric spaces</p>

<p>
  <img src="https://img.shields.io/badge/C%23-512BD4?style=flat-square&logo=csharp&logoColor=white" alt="C#"/>
  <img src="https://img.shields.io/badge/.NET-4.7.2%2B-512BD4?style=flat-square&logo=dotnet&logoColor=white" alt=".NET"/>
  <img src="https://img.shields.io/badge/Emgu%20CV-OpenCV-5C3EE8?style=flat-square" alt="Emgu CV"/>
  <img src="https://img.shields.io/badge/WinForms-GUI-0078d4?style=flat-square" alt="WinForms"/>
  <img src="https://img.shields.io/badge/6%20color%20spaces-RGB%20HSV%20LAB%20YCbCr%20CMYK%20YUV-0d9488?style=flat-square" alt="6 color spaces"/>
  <img src="https://img.shields.io/badge/GDI%2B-3D%20rendering-7c3aed?style=flat-square" alt="GDI+"/>
</p>

<br/>

</div>

---

## What this is

PixelLab lets you open any image and inspect it across six color spaces simultaneously — RGB, HSV, LAB, YCbCr, CMYK and YUV. Beyond pixel-level inspection it offers two distinct 3D visualization modes: a chart-based bubble view and a fully interactive GDI+ renderer that encloses each space in its mathematical boundary (cube, cylinder or Cartesian axes). Click any point in either view to get the full six-space breakdown instantly.

| Capability | Detail |
|---|---|
| Color spaces | RGB · HSV · LAB · YCbCr · CMYK · YUV |
| Channel control | Toggle on/off, adjust intensity −100 to +100, quantize to 2/4/8/16/256 levels |
| Pixel inspector | Click anywhere on the image — all six space values shown simultaneously |
| 3D Chart view | Bubble chart per space with wireframe geometry (cube / cone / sphere) |
| Real 3D view | GDI+ renderer — drag to rotate, scroll to zoom, click to inspect |
| Geometric boundaries | RGB/YCbCr/YUV/CMYK: Cube · HSV: Cylinder · LAB: Cartesian axes |

---

## Screenshots

| Main Window | 3D Chart View | Real 3D View |
|:---:|:---:|:---:|
| `docs/screenshot-main.png` | `docs/screenshot-chart.png` | `docs/screenshot-3d.png` |

---

## Getting Started

### Requirements

- Windows (WinForms + native Emgu CV binaries)
- .NET Framework 4.7.2 or later, or .NET 6/8 with Windows compatibility
- Visual Studio 2019 or later

### Build from source

```bash
git clone https://github.com/majddakhoul/PixelLab.git
```

1. Open `PixelLab.sln` in Visual Studio.
2. Right-click the solution → **Restore NuGet Packages**.
3. Build: `Ctrl+Shift+B`
4. Run: `F5`

### Run a release build

Copy the contents of `bin\Release\` (including all DLLs) to any folder and run `PixelLab.exe`. No installer required.

---

## Usage

1. Launch the application.
2. Click **Load Image** or drag an image onto the left pane (`.jpg`, `.png`, `.bmp`).
3. Pick a color space from the dropdown and adjust channels with the trackbars.
4. Click any pixel on either pane to read its values across all six spaces.
5. Click **3D Chart Spaces** for the bubble-chart view, or **Real 3D Spaces** for the immersive wireframe renderer.
6. In any 3D view: drag to rotate, scroll to zoom, click a point to inspect its color.

---

## Features

### Image operations

- Load via file dialog or drag & drop.
- Rotate 90° clockwise.
- Save as JPEG, PNG or BMP.
- View file info: name, format, disk size, dimensions, pixel count, full path.

### Color manipulation

- Six color spaces: RGB, HSV, LAB, YCbCr, CMYK, YUV.
- Display any channel as a grayscale image.
- Toggle channels on/off independently.
- Intensity trackbars: −100 to +100 per channel.
- Color quantization: 2, 4, 8, 16 or 256 (original) levels.

### Pixel inspector

Click anywhere on the original or processed image to read the pixel in all six spaces at once:

```
RGB    →  (R, G, B)
HSV    →  (Hue°, Saturation%, Value%)
LAB    →  (L, a, b)
YCbCr  →  (Y, Cr, Cb)
CMYK   →  (C%, M%, Y%, K%)
YUV    →  (Y, U, V)
```

### 3D Chart view

Each color space is rendered as a bubble chart — three coordinates mapped to (X, Y, bubble size). Classic spaces also draw geometric wireframes:

| Space | Wireframe |
|---|---|
| RGB | Cube (R, G, B axes) |
| HSV | Cone (Hue, Saturation, Value) |
| LAB | Sphere (L, a, b axes) |

### Real 3D view

A high-performance GDI+ renderer projects color-space coordinates into screen space using standard rotation matrices with depth sorting. Each space is enclosed in its mathematical boundary:

| Space | Boundary |
|---|---|
| RGB · YCbCr · YUV · CMYK | Cube |
| HSV | Cylinder |
| LAB | Cartesian axes |

---

## Project Structure

```
PixelLab/
├── Form1.cs          Main application logic
├── Program.cs        Entry point
├── PixelLab.csproj   Project file with NuGet references
└── README.md
```

---

## Dependencies

| Package | Purpose |
|---|---|
| `Emgu.CV` | OpenCV wrapper for .NET — image processing and color space conversions |
| `Emgu.CV.Bitmap` | `Mat` → `Bitmap` extension methods |
| `Emgu.CV.runtime.windows` | Native OpenCV DLLs for Windows |
| `System.Windows.Forms.DataVisualization` | Chart control for the bubble-chart 3D views |

---

## How it works

**Color processing.** Emgu CV converts the image to the selected color space. Channels are split, modified by the trackbar values, merged back and re-converted to BGR for display. Quantization is applied directly on the final 8-bit per-channel bitmap.

**Chart 3D.** A Bubble series is created per color space with (X, Y, bubbleSize) encoding the three coordinates. Wireframes are additional Line series. Standard chart controls handle rotation and zoom.

**Real 3D rendering.** Points are projected from color-space coordinates to screen space via 3D rotation matrices. Depth sorting provides correct occlusion. Geometric boundaries are drawn as line segments.

---

## Known Limitations

- Images over ~10 megapixels may slow down the Real 3D view — every pixel is plotted as a circle.
- The CMYK 3D view uses a CMY projection; the K value is still shown in the info panel.
- The LAB boundary is labelled "sphere" but renders as Cartesian axes rather than a true sphere surface.

---

## Contributing

Pull requests are welcome. For major changes please open an issue first to discuss what you would like to change.

---

## License

MIT License. See [`LICENSE`](LICENSE) for the full text.

---

## Acknowledgments

- [Emgu CV](https://www.emgu.com/) for making OpenCV accessible in .NET.
- Microsoft Chart Controls for the 3D charting foundation.
