# HoloLens 2 UI/Overlay System

Complete UI and overlay interaction system for HoloLens 2 Unity/MRTK3 project with JSON-driven AR object generation and real-time LLM→Python→Unity command pipeline.

## 🎯 Features

- **Complete HUD Overlay System** with multiple panels (Left, Right, Bottom, Top)
- **JSON-Driven Object Generation** for rooms, objects, markers, and outlines
- **Real-Time Command Pipeline** via HTTP polling to Python/LLM server
- **Interactive Object Hierarchy** with tap-to-highlight functionality
- **Minimap/Compass** with top-down room view and orientation indicator
- **Floating Tags** that appear above highlighted objects
- **Techy Blue Aesthetic** matching construction site engineering UI
- **MRTK3 Integration** with hand tracking and gaze support

## 📁 Project Structure

```
Assets/
├── Scripts/
│   ├── UI/              # UI controllers and builders
│   ├── Networking/      # HTTP command polling
│   ├── JSON/            # JSON room loader
│   ├── Core/            # Scene object registry
│   └── ObjectFactory/   # Object creation utilities
├── Prefabs/             # UI and object prefabs
├── Resources/           # JSON configs and themes
└── Scenes/              # Unity scenes
```

## 🚀 Quick Start

1. **Import MRTK3** into your Unity project
2. **Import TextMeshPro** (Window > TextMeshPro > Import TMP Essential Resources)
3. **Add Newtonsoft.Json** via Package Manager
4. **Create UI Canvas** following `INTEGRATION_INSTRUCTIONS.md`
5. **Start Python server** on `localhost:5000` (optional)
6. **Run scene** in HoloLens emulator

## 📖 Documentation

- **INTEGRATION_INSTRUCTIONS.md** - Complete setup and integration guide
- **PREFAB_DEFINITIONS.md** - Detailed prefab specifications
- **PROJECTED_OBJECTS_GUIDE.md** - Guide for working with projected objects

## 🎨 UI Components

### Left Panel - Scene Hierarchy
- Dynamically generated object list
- Expandable groups by type
- Tap to highlight objects
- Visibility toggles

### Right Panel - Controls
- Reload JSON button
- Toggle outlines/markers/objects
- Clear highlights
- Manual command input

### Bottom Panel - Logs
- Voice transcription display
- Agent status indicator
- Command log with timestamps

### Top Center - Title
- Project title
- Room identification

### Top Right - Minimap
- Top-down room view
- Object position markers
- Compass with N/E/S/W labels
- Blinking highlight on selection

## 🔌 Python Server Integration

The system polls `http://localhost:5000/get_command` for LLM commands.

**Command Format:**
```json
{
  "action": "highlight",
  "objectId": "window_01",
  "parameters": {},
  "transcription": "Show me the window",
  "agentStatus": "processing"
}
```

See `INTEGRATION_INSTRUCTIONS.md` for Python server setup.

## 🎨 Theme

The UI uses a "techy blue" construction site aesthetic:
- Primary Blue: `#0099FF`
- Accent Blue: `#00CCFF`
- Dark Background: `#1A1A26`
- Highlight Green: `#00FF80`

Configure via `UITheme` ScriptableObject in Resources.

## 📝 License

This project is part of the Ironsite Hackathon UI system.

