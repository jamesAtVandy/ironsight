# HoloLens 2 UI/Overlay System - Integration Instructions

## 📁 Folder Structure

```
Assets/
├── Scripts/
│   ├── UI/
│   │   ├── UITheme.cs
│   │   ├── UIController.cs
│   │   ├── UIObjectListBuilder.cs
│   │   ├── UIControlsHandler.cs
│   │   ├── FloatingTagController.cs
│   │   └── MiniMapController.cs
│   ├── Networking/
│   │   └── HTTPCommandPoller.cs
│   ├── JSON/
│   │   └── JSONRoomLoader.cs
│   ├── Core/
│   │   └── SceneObjectRegistry.cs
│   └── ObjectFactory/
│       └── ObjectFactory.cs
├── Prefabs/
│   ├── UI/
│   │   ├── MainUICanvas.prefab
│   │   ├── LeftPanel_ObjectHierarchy.prefab
│   │   ├── RightPanel_Controls.prefab
│   │   ├── BottomPanel_Logs.prefab
│   │   ├── TopCenter_Title.prefab
│   │   ├── TopRight_Minimap.prefab
│   │   └── FloatingObjectTag.prefab
│   ├── Markers/
│   └── Outlines/
├── Resources/
│   └── UITheme.asset
└── Scenes/
    └── MainScene.unity
```

## 🔧 Setup Steps

### 1. MRTK3 Setup

1. **Import MRTK3 Foundation Package**
   - Open Unity Package Manager
   - Add package from git URL or import MRTK3 Foundation
   - Ensure TextMeshPro is imported (Window > TextMeshPro > Import TMP Essential Resources)

2. **Configure MRTK3 Scene**
   - Add MRTK Scene Content prefab to your scene
   - Configure XR Rig for HoloLens 2

### 2. Create UI Canvas

1. **Create Main Canvas**
   - Right-click in Hierarchy > UI > Canvas
   - Set Canvas component:
     - Render Mode: **World Space**
     - Event Camera: Main Camera
   - Add **CanvasScaler** component:
     - UI Scale Mode: Scale With Screen Size
     - Reference Resolution: 1920 x 1080
   - Add **GraphicRaycaster** component

2. **Configure for MRTK3**
   - Add **CanvasXR** component (MRTK3) if available
   - Or use MRTK's **CanvasHelper** for proper world space rendering

### 3. Build UI Panels

#### Left Panel - Object Hierarchy
1. Create empty GameObject: `LeftPanel_ObjectHierarchy`
2. Add RectTransform:
   - Anchor: Top-Left
   - Position: X=200, Y=-100
   - Size: 400 x 600
3. Add Image component (background)
4. Add Vertical Layout Group
5. Add ScrollRect for scrolling
6. Add `UIObjectListBuilder` script
7. Create child: `ListContainer` (Content for ScrollRect)

#### Right Panel - Controls
1. Create empty GameObject: `RightPanel_Controls`
2. Add RectTransform:
   - Anchor: Top-Right
   - Position: X=-200, Y=-100
   - Size: 400 x 600
3. Add Image component (background)
4. Add Vertical Layout Group
5. Add `UIControlsHandler` script
6. Add MRTK3 Button prefabs for:
   - Reload JSON
   - Toggle Outlines
   - Toggle Markers
   - Toggle Objects
   - Clear Highlights
7. Add TMP_InputField for manual commands

#### Bottom Panel - Logs
1. Create empty GameObject: `BottomPanel_Logs`
2. Add RectTransform:
   - Anchor: Bottom-Center
   - Position: Y=100
   - Size: 1200 x 200
3. Add Image component (background)
4. Add TextMeshProUGUI for transcription
5. Add TextMeshProUGUI for agent status
6. Add ScrollRect with TextMeshProUGUI for log entries

#### Top Center - Title
1. Create empty GameObject: `TopCenter_Title`
2. Add RectTransform:
   - Anchor: Top-Center
   - Position: Y=-50
   - Size: 800 x 100
3. Add TextMeshProUGUI for title
4. Add TextMeshProUGUI for room ID

#### Top Right - Minimap
1. Create empty GameObject: `TopRight_Minimap`
2. Add RectTransform:
   - Anchor: Top-Right
   - Position: X=-150, Y=-150
   - Size: 200 x 200
3. Add Image component (background)
4. Add `MiniMapController` script
5. Create child: `CompassContainer` for compass overlay

### 4. Configure UIController

1. Create empty GameObject: `UIController`
2. Add `UIController` script
3. Assign all panel references
4. Assign UITheme asset from Resources
5. Link component references:
   - UIObjectListBuilder
   - UIControlsHandler
   - MiniMapController
   - FloatingTagController

### 5. Setup Scene Object Registry

1. Create empty GameObject: `SceneObjectRegistry`
2. Add `SceneObjectRegistry` script
3. This will auto-instantiate as singleton

### 6. Setup HTTP Command Poller

1. Create empty GameObject: `HTTPCommandPoller`
2. Add `HTTPCommandPoller` script
3. Configure:
   - Server URL: `http://localhost:5000`
   - Poll Interval: 0.5 seconds
   - Auto Start: true

### 7. Setup JSON Room Loader

1. Create empty GameObject: `JSONRoomLoader`
2. Add `JSONRoomLoader` script
3. Assign prefabs (optional - will create primitives if not assigned)
4. Create parent Transform: `Room` (or leave null to auto-create)

### 8. Create Floating Tag System

1. Create empty GameObject: `FloatingTagController`
2. Add `FloatingTagController` script
3. Optionally create a tag prefab with:
   - Canvas (World Space)
   - Background Image
   - TextMeshProUGUI

### 9. Configure UI Theme

1. Create UITheme ScriptableObject:
   - Right-click in Project > Create > Ironsite > UI Theme
   - Adjust colors to match "techy blue" aesthetic
   - Save to `Assets/Resources/UITheme.asset`

2. Assign theme to UIController

### 10. JSON Configuration (Optional)

If you're using JSONRoomLoader, place your JSON configuration file in `Assets/StreamingAssets/` folder (create if needed). The default filename is `room_config.json` but can be configured in the JSONRoomLoader component.

## 🔌 Python Server Integration

### Python Server Setup (Flask Example)

```python
from flask import Flask, jsonify, request
from flask_cors import CORS

app = Flask(__name__)
CORS(app)

current_command = {}

@app.route('/get_command', methods=['GET'])
def get_command():
    return jsonify(current_command)

@app.route('/send_response', methods=['POST'])
def send_response():
    response = request.form.get('response')
    print(f"Unity response: {response}")
    return jsonify({"status": "received"})

@app.route('/send_command', methods=['POST'])
def send_command():
    global current_command
    current_command = request.json
    return jsonify({"status": "sent"})

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=True)
```

### Command Format

```json
{
  "action": "highlight",
  "objectId": "window_01",
  "parameters": {
    "duration": 5.0
  },
  "transcription": "Show me the window",
  "agentStatus": "processing"
}
```

## 🎨 Styling & Aesthetics

### Color Scheme
- Primary Blue: `#0099FF` (RGB: 0, 153, 255)
- Accent Blue: `#00CCFF` (RGB: 0, 204, 255)
- Dark Background: `#1A1A26` (RGB: 26, 26, 38)
- Highlight Green: `#00FF80` (RGB: 0, 255, 128)

### Font Settings
- Title: 32pt, Bold, White
- Body: 18pt, Regular, White
- Small: 14pt, Regular, Light Gray

### Visual Effects
- Pulsing grid overlay (use shader or animated material)
- Glowing outlines (emission material)
- Fade-in animations for log entries
- Blinking minimap markers on highlight

## 🧪 Testing in HoloLens Emulator

1. **Build Settings**
   - Platform: Universal Windows Platform
   - Target Device: HoloLens
   - Build Type: D3D
   - SDK: Latest installed

2. **XR Settings**
   - Enable XR Plug-in Management
   - Select Windows Mixed Reality

3. **Run in Emulator**
   - File > Build Settings > Build and Run
   - Or use Visual Studio to deploy to emulator

## 📝 Additional Notes

### MRTK3 Button Setup
- Use `PressableButton` component from MRTK3
- Configure interaction profiles (hand tracking, gaze)
- Set up button sounds and haptics

### Hand Tracking
- Ensure hand tracking is enabled in MRTK profile
- UI elements should respond to hand rays
- Use MRTK's `PointerHandler` for interactions

### Performance Optimization
- Use object pooling for frequently created/destroyed UI elements
- Limit log entries to prevent memory issues
- Optimize minimap updates (update only on change)

### Debugging
- Use Unity's Remote Debugging for HoloLens
- Enable MRTK diagnostics overlay
- Check console logs via Device Portal

## 🚀 Next Steps

1. Test JSON loading with sample file
2. Verify HTTP polling connection
3. Test object highlighting system
4. Validate minimap positioning
5. Test all UI toggles and controls
6. Optimize for HoloLens 2 performance
7. Add voice command integration (if needed)
8. Implement blueprint grid shader effect

## ⚠️ Dependencies

Required packages:
- MRTK3 Foundation
- TextMeshPro
- Newtonsoft.Json (JSON.NET) - Add via Package Manager > Add package from git URL: `https://github.com/jilleJr/Newtonsoft.Json-for-Unity.git`

Optional:
- MRTK3 Examples (for reference implementations)
- Unity XR Interaction Toolkit (if not using MRTK3 components)

