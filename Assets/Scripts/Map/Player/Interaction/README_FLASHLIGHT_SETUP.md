# Flashlight Pickup System Setup

This document explains how to set up the flashlight pickup system in your game.

## Setup Instructions

### 1. Player Flashlight Setup
1. Find your player character object in the scene.
2. Make sure it has the `PlayerFlashlight` component attached.
3. Assign a spotlight to the `flashlight` field in the Inspector.
4. Set `hasFlashlight` to `false` to ensure the player starts without a flashlight.
5. Ensure your player has the "Player" tag.

### 2. Pickup Flashlight Setup
1. Find the `Flashlight.fbx` model: `Assets/Sci-Fi Styled Modular Pack/Prefabs/Lights/Flashlight.fbx`
2. Create a prefab instance in your scene where you want the flashlight to appear.
3. Add the `InteractableFlashlight` component to the flashlight object.
4. Configure the following settings:
   - Set `interaction prompt` text as desired (e.g., "Нажмите E чтобы подобрать фонарик").
   - Set `player layer` to match your player's layer.
   - Optionally add a pickup sound to `pickup sound`.
   - Adjust the `interaction distance` as needed.
5. Add a Collider component if not already present (the script will add a SphereCollider automatically if none exists).

### 3. UI Prompt Setup
1. Create a UI Canvas in your scene (if not already present).
2. Create a panel for interaction prompts with a TextMeshPro text component inside.
3. Create an empty GameObject and add the `UIPromptManager` component to it.
4. Assign the panel to `prompt panel` and the text component to `prompt text` in the UIPromptManager.
5. Ensure the panel is initially disabled.

## How It Works
- When the player enters the trigger area around the flashlight, an interaction prompt appears.
- Pressing the E key when near the flashlight will pick it up.
- After pickup, the player can toggle the flashlight on/off using the F key.
- The physical flashlight model will disappear after being picked up. 