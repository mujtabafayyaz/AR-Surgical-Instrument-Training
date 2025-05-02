# AR-surgical-instrument-training
## Video Demonstration of Application
https://github.bath.ac.uk/mf809/AR-surgical-instrument-training/assets/5488/bf1b62fb-0ca9-4766-be6d-9b704562e68c

## Introduction

This Unity-based augmented reality application is designed to train nurses in handling and passing surgical instruments correctly, with visual guidance and hand tracking.

The app uses **Vuforia** for AR camera tracking and **MediaPipe HandLandmarker** to detect and track the user's hand in real time. It overlays guidance markers, orientation feedback, and a dynamic visual connection between the instrument and a virtual surgeon’s open hand.

---

## Scenes

There are two main AR training scenarios:

- `Assets/Scenes/Scalpel.unity` – Demonstrates correct orientation and passing logic for a scalpel.
- `Assets/Scenes/Scissors.unity` – Demonstrates similar logic for scissors with vertical orientation detection.

---

## Scripts

All scripts are located under `Assets/Scripts/`:

### `ImageProcessor.cs`
- Accesses the AR camera feed via Vuforia.
- Registers the pixel format and retrieves the raw camera image.
- Displays the image on a `RawImage` UI component.

### `VuforiaHandTracking.cs`
- Uses the `ImageProcessor` output (camera texture) and passes it to MediaPipe’s `HandLandmarker` model.
- Runs asynchronous hand tracking inference.
- Publishes hand landmark results to be accessed by other scripts.

### `Visibility.cs`
- Drives all augmentation logic:
  - Controls when guidance elements (ghost hands, arrows, grip zones) are shown.
  - Checks instrument orientation (e.g., vertically upright for scissors).
  - Responds to hand alignment and switching scenes.
  - Dynamically draws a dashed line from the instrument tip to the open hand.

---

## Materials

All materials used (ghost hands, arrows, line colors, etc.) are located in 'Assets/Materials/'

---

## Hand Tracking Model

The hand tracking model is located in 'Assets/Models/hand_landmarker.bytes'

## Requirements

- **Unity Version:** `6000.0.34f1`
- **Vuforia Engine:** `11.1.3`
- **MediaPipe Unity Plugin:** preconfigured HandLandmarker task (.bytes model)

> **Note**: To run this project, you’ll need a [Vuforia Developer Account](https://developer.vuforia.com/) and a valid license key.
You can set this up in **Unity → Vuforia Configuration** after importing the project.

---

## Testing the application

1. Clone the repository.
2. Open in **Unity 6000.0.34f1**.
3. Add your **Vuforia License Key**.
4. Build to your Android or AR device.

---

## Copyright

Attention is drawn to the fact that the copyright of this Dissertation
rests with its author. The Intellectual Property Rights of the
products produced as part of the project belong to the author
unless otherwise specified below, in accordance with the University
of Bath’s policy on intellectual property (see https://www.bath.ac.uk/publications/universityordinances/attachments/Ordinances 1 October 2020.pdf).
This copy of the Dissertation has been supplied on the condition
that anyone who consults it is understood to recognise that its
copyright rests with its author and that no quotation from the
Dissertation and no information derived from it may be published
without the prior written consent of the author.
