# Samira
### SamiraMod was created under Riot Games' "Legal Jibber Jabber" policy using assets owned by Riot Games.  Riot Games does not endorse or sponsor this project.

- Adds the champion Samira, from League of Legends, to Risk of Rain 2 as a survivor.
<div style="display: flex; justify-content: space-around;">
  <img src="https://github.com/user-attachments/assets/1daeca5d-d164-40bc-8951-c6ce8ed89453" alt="Example Image 1"  />
</div>

<div style="display: flex; justify-content: center;">
  <img src="https://github.com/user-attachments/assets/10ed63ea-7f51-4544-bcb5-eece9949afbf" alt="Example Image 1" width="400" />
  <img src="https://github.com/user-attachments/assets/535d7204-7109-41e4-8ee0-eb585a95e8c2" alt="Example Image 2" width="400" />
</div>

<div style="display: flex; justify-content: center;">
  <img src="https://github.com/user-attachments/assets/995963c0-23ef-4ab1-89cf-913ab42ac063" alt="Example Image 1" width="400" />
  <img src="https://github.com/user-attachments/assets/6584969c-49c1-4859-8252-565bc434f1a6" alt="Example Image 2" width="400" />
</div>






## Introduction
After her Shuriman home was destroyed as a child, Samira found her true calling in Noxus Crest icon Noxus, where she built a reputation as a stylish daredevil taking on dangerous missions of the highest caliber. Wielding black-powder pistols and a custom-engineered blade, Samira thrives in life-or-death circumstances, eliminating any who stand in her way with flash and flair.

## Samira's Base Stats
- Health: 110
- Health Regen: 1 / s
- Damage: 12
- Speed: 7 m/s
- Armor: 20

## Skills

| Skill             | Image                                                                                                                             | Description                                                                                                                                                                   | Stats                                  |
|:------------------|-----------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------|
| Daredevil Impulse | <img src="https://github.com/user-attachments/assets/0c8abc75-e761-4ccd-88a8-6674e51d8595" alt="Daredevil Impulse" width= "512"/> | Samira's unique attacks generate a stack of Style for 6 seconds. For each stack, Samira gains bonus movement speed. At maximum stacks, Samira can cast Inferno Trigger.       | MS: `4%`/`8%`/`12%`/`16%`/`20%`/`25%`/ |
| Flair             | <img src="https://github.com/user-attachments/assets/c0ba30d5-5de7-4c28-ba4b-c46416b1a790" alt="Flair" width="512"/>              | Samira fires a shot or swings her sword, dealing `130%` damage. Every 5 uses triggers a unique attack, dealing `40%` additional damage. Melee attacks deal `10%` more damage. | Proc: `1.0`                            |
| Blade Whirl       | <img src="https://github.com/user-attachments/assets/935b00d2-909e-4e83-8dc5-f182f37b1242" alt = "Blade Whirl" width = "512"/>    | Samira slashes around her for `0.5 seconds`, damaging enemies twice for `80%` damage, while destroying incoming projectiles.                                                  | Proc: `1.0`, CD: `10 sec`              |
| Wild Rush         | <img src="https://github.com/user-attachments/assets/eb2beb14-2c19-4822-b09d-19aa6a4892b7" alt = "Wild Rush" width="512"/>        | Samira dashes forward slashing through any enemy in her path, dealing `70%` damage. Getting a takedown against an enemy resets Wild Rush's cooldown.                          | CD: `6 sec`                            |
| Inferno Trigger   | <img src="https://github.com/user-attachments/assets/59ce6cec-e999-4947-a2c4-ea3bcf8f8bf3" alt="Inferno Trigger" width="512"/>    | Samira unleashes a torrent of shots for `3 seconds`, dealing `160%` damage. The number of shots fired is scaled with Attack Speed.                                            | CD: `7 sec`                            |

## Custom Skills

| Skill           | Slot      | Description                                                                                                                                       | Stats                                          |
|:----------------|-----------|---------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------|
| Explosive Shot  | Primary   | Samira only fires bullets, additionally shooting an explosive bullet every 4 attacks.                                                             | DMG: `400%`                                    |
| Slashing Maniac | Primary   | Samira only swings her sword, gaining an attack speed and movement speed buff on-hit. She additionally has a cleave, giving an extra style point. | AS: `1%` <br/>MS: `2%`                         |
| Exposing Whirl  | Secondary | Samira fires bullets at nearby enemies, inflicting armor penetration.                                                                             | Proc: `1.0`, CD: `10 sec`                      |
| Quick Steps     | Utility   | Samira dashes forward slashing through any enemy in her path. On press, Samira gains attack speed.                                                | CD: `8 seconds`<br/>Stocks:`2`<br/> AS : `30%` |
| Infinite Rain   | Special   | Samira unleashes a torrent of shots for `3 seconds`, dealing `160%` damage. The number of shots fired is scaled with Attack Speed.                | CD: `7 sec`<br/> +Duration: `0.25 sec`         |

## Skins
| Skin  | Image                                                                                                                             | Description                                                                                                                                                                   | Requirement           |
|:------|-----------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-----------------------|
| Dante | <img src="https://github.com/user-attachments/assets/0c8abc75-e761-4ccd-88a8-6674e51d8595" alt="Daredevil Impulse" width= "512"/> | Samira's unique attacks generate a stack of Style for 6 seconds. For each stack, Samira gains bonus movement speed. At maximum stacks, Samira can cast Inferno Trigger.       | Avaliable on startup! |


## Planned
- Better Multiplayer
- Skins
- Passive Implementation

## Special Thanks
- TheTimeSweeper - mod was built off of HenryTutorial
- Lemonlust - learned more about Risk of Rain 2 Modding through SettMod and JinxMod
- Risk of Rain 2 Modding Discord - lots of answers found here
- Family & friends - helped test the mod out
- Feedback from the community

---

# Patch Notes

Patch 1.2.0
- Added 'Dante' from the Devil May Cry Series. Models were from DMC3, while sounds were from DMC5.
- Note: When shooting while moving, Dante and Samira's guns are not in the correct position.

Patch 1.1.1
- Credit to anartoast for feedback again!
- **Wild Rush/Quick Steps**: Added back the invincibility because players take damage if colliding with the environment
- **Wild Rush**: Fixed a bug where cooldown was not being reset when enemies were killed.

Patch 1.1.0
- Credits to Dangobalang0 and anartoast for feedback to the mod! Again, any feedback is always appreciated.

### Nerfs
- **Wild Rush**: Nerfed Damage, Removed Attack Speed Buff 
- **Blade Whirl**: Nerfed Damage 
- **Inferno Trigger**: Nerfed Damage 
- **Blade Whirl & Wild Rush**: Removed hidden buffs (invincibility/armor buffs)

### Bugfixes and QOL
- Wild Rush can now be canceled into Secondary and Special skills (Blade Whirl & Inferno Trigger)
- Blade Whirl can now be canceled into Special skills (Inferno Trigger)
- Fixed a bug where bullet attacks did not grant on-hit effects such as: inferno trigger's lifesteal, style points from m1 and dash m1 abilities)
- Made the transition from Wild Rush to Flair feel much less janky; the ability's duration is additionally scaled with attack speed now. 
- Changed text colors in loadout because skill text and background were the same.

### New Content
- Lots of new skills!

### Primary:
- **Explosive Shot**: Samira may only shoot her guns now, but every 4 attacks she fires an explosive shot which deals a high amount of damage. This unique attack does not grant a style point like Flair.
- **Slashing Maniac**: Samira may only swing her sword now, but every time she hits an enemy, she will gain a movement speed and attack speed buff. This buff may infinitely stack, but has a duration. Additionally, this ability inherits Flair's unique attack, and does grant its style point.

### Secondary:
- **Exposing Whirl**: Just like Blade Whirl, but Samira will only damage her enemies once (in the beginning). Instead, Samira fires armor-penetrating bullets at nearby enemies.

### Utility:
- **Quick Steps**: Samira gains two charges of dash. On press, Samira gains attack speed for a duration.

### Special:
- **Infinite Rain**: Samira unleashes a volley of bullets at nearby enemies. When enemies are killed, the duration of the ability is extended.

Patch 1.0.0
- Initial Release