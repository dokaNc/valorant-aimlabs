# 🎯 Valorant Aim Lab

A personal aim trainer built with Unity, using authentic Valorant 3D models (agents & maps) for realistic practice.

![Unity](https://img.shields.io/badge/Unity-2022.3%20LTS-black?logo=unity)
![License](https://img.shields.io/badge/License-MIT-green)
![Status](https://img.shields.io/badge/Status-In%20Development-orange)

## ✨ Features

- **Authentic Valorant Models** — Train on real agent hitboxes (ISO, Jett, Phoenix...)
- **Real Maps** — Practice in actual Valorant environments (The Range, Ascent...)
- **4 Training Modes** — Flick, Tracking, Speed, Headshot
- **Valorant Crosshair System** — Full customization + import/export codes
- **Realistic Hit Feedback** — Particles + iconic headshot "dink" sound
- **Detailed Stats** — Accuracy, reaction time, KPM, headshot %

## 🎮 Training Modes

| Mode | Description |
|------|-------------|
| **Flick** | Static targets, pure reflexes |
| **Tracking** | Moving targets, smooth pursuit |
| **Speed** | Fast spawns, reaction training |
| **Headshot** | Precision focus on head hitbox |

## 🚀 Getting Started

### Prerequisites

- Unity 2022.3 LTS
- [glTFast package](https://docs.unity3d.com/Packages/com.unity.cloud.gltfast@6.0/manual/index.html) (for GLB import)

### Installation

```bash
# Clone the repo
git clone https://github.com/YOUR_USERNAME/valorant-aim-lab.git

# Open in Unity Hub
# Select Unity 2022.3.x
```

### Adding Models

Models are **not included** due to copyright. You need to provide your own:

1. Place agent models in `Assets/_Project/Models/Characters/`
2. Place map models in `Assets/_Project/Models/Maps/`
3. Supported formats: `.glb`, `.fbx`

## 📁 Project Structure

```
Assets/_Project/
├── Scenes/          # Bootstrap, MainMenu, TrainingRange
├── Scripts/         # All C# scripts
├── Prefabs/         # Targets, Player, UI, Effects
├── Models/          # Your imported 3D models
├── Audio/           # SFX and music
└── ScriptableObjects/  # Training mode configs
```

## 🛠️ Tech Stack

- **Engine**: Unity 2022.3 LTS
- **Language**: C#
- **3D Import**: glTFast 6.0
- **UI**: Unity UI + TextMeshPro

## 📊 Stats Tracked

- Accuracy (%)
- Average Reaction Time (ms)
- Best Reaction Time (ms)
- Kills Per Minute (KPM)
- Headshot Percentage (%)

## 🤝 Contributing

Contributions welcome! Here's how:

1. Fork the repo
2. Create your branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Ideas for Contribution

- [ ] New training modes
- [ ] UI improvements
- [ ] Performance optimizations
- [ ] Additional crosshair presets
- [ ] Localization

## ⚠️ Disclaimer

This is a **personal project** for educational purposes only. 

- Valorant™ is a trademark of Riot Games, Inc.
- 3D models are property of Riot Games
- This project is not affiliated with or endorsed by Riot Games
- **Do not use for commercial purposes**

## 📝 License

MIT License — see [LICENSE](LICENSE) for details.

## 🙏 Acknowledgments

- Riot Games for Valorant
- Unity Technologies
- The aim training community

---

<p align="center">
  <b>Train hard, rank up! 💪</b>
</p>
