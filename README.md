# Ninja-Pinball-Game
My second Unity project - a simple 2D pinball game. It is part of **GAME PROGRAMMING** graduate coursework at **Western University**.

# Game Overview
**Ninja Pinball** is a 2D physics-based pinball game where the player launches and controls pinballs. The goal is to hit ememies with pinballs and avoiding hazards.

# Tech
- Engine: Unity **2022.3.46f1**
- Language: C#

# How to Play
- **Main Goal**: Control the Ninja, shoot pinballs to eliminate all monsters
- At the start of each turn, the player chooses one of three random buffs
- The player can then move and fire all available pinballs. Once all pinballs are launched, movement is disabled
- When all active pinballs are destroyed, monsters move downward.
    - If a monster collides with the player, it will knock them back.
    - If the player is pushed to the bottom boundary, they die immediately.
    - If a monster reaches the bottom boundary, it charges toward the player, deal damage equal to its remaining HP.
    - If the player has no pinballs left to shoot at the beginning of turn, they die.
- As all monsters are eliminated, next level will be loaded, all buffs on the player will reset

# Game Control
| Key | Action |
|-----|---------|
| WASD | Move the player |
| Left Mouse Button | Launch the pinball |
| Esc | Open the menu / pause the game |

# Assets
- Monsters: https://beowulf.itch.io/pixel-rpg-dungeons-monsters-16x16
- Tiles & Decorations: https://assetstore.unity.com/packages/2d/environments/2d-topdown-tilesets-and-sprites-hd-edition-249881
- Ninja: https://pixel-boy.itch.io/ninja-adventure-asset-pack
- Lava: https://imgur.com/a/ow1J6F5
- Fire: https://tenor.com/en-GB/view/fire-fireball-8bit-gif-14681886
<br>
<br>
By Tianyue Fang<br>
[LinkedIn] https://www.linkedin.com/in/tianyuefang/
