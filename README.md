# Soulbound Demo Development

## Description

This project is a demo of a 2D platformer, the focus of this demo was on the development of a movement system which included several of the conventions of a modern platformer (variable jump, coyote jump, wall slide, dash) and make that system feel polished. The game is in an unfinished state, including a movement system and basic hazards. If I had more time to work on it, I would finish importing the sprites, effects and animations to replace the placeholders, make a more fulfilling map to navigate and implement the character switch mechanic from my development proposal.

### Story and Objective

While the story wasn't properly implemented, it was written originally with the "Games for Change" assessment criteria in mind, centering around two astranged sisters who reunite in the underworld and make their way back to the land of the living by making up for each others weaknesses and rekindling their broken bond. The objective of the game at present is to navigate through the environment using your jump, wall climb and dash while avoiding hazards.

### Movement System

## User Documentation

| Action        | Output                              |
| ------------- | ----------------------------------- |
| **A**         | Move Left                           |
| **D**         | Move Right                          |
| **Left Shift**| Dash                                |
| **Space**     | Jump                                |

**Jump**

The jump feature in Soulbound has variable length, increasing the height to a maximum based on how long the player holds the input. Jumps can also be performed while wall sliding.
![Gif of neutral jump and walljump](Assets/ReadMeGifs/Wallclimb.gif)

**Coyote Jump**

A "Coyote Jump" (A feature named after the cartoon Coyote of roadrunner fame which lets the player execute a jump during a small window after they walk off a platform) can also be performed shortly after the player loses collision with the ground.\
![Gif of coyote jump](Assets/ReadMeGifs/Coyote%20Jump.gif)

**Wallslide**

The wallslide mechanic allows the player to hold on to or slide down (by letting go of the directional key) vertical surfaces with the "wall" tag by holding the appropriate directional key into them.
![Gif of wall slide](Assets/ReadMeGifs/Wall%20Slide.gif)

**Dash**

The dash mechanic gives the player a large boost of horizontal speed following their last or current directional input.\
![Gif of wall slide](Assets/ReadMeGifs/Dash.gif)

### Dependencies

* Describe any prerequisites, libraries, OS version, etc., needed before installing program.
* ex. Windows 10

### Installing

* How/where to download your program
* Any modifications needed to be made to files/folders

### Executing program

* How to run the program
* Step-by-step bullets
```
code blocks for commands
```

## Help

Any advise for common problems or issues.
```
command to run if program contains helper info
```

## Authors

Contributors names and contact info

ex. Mr Jones
ex. [@benpaddlejones](https://github.com/benpaddlejones)

## Version History

* 0.2
    * Various bug fixes and optimizations
    * See [commit change]() or See [release history]() or see [branch]()
* 0.1
    * Initial Release

## License

This project is licensed under the [NAME HERE] License - see the LICENSE.md file for details

## Acknowledgments

Inspiration, code snippets, etc.
* [Github md syntax](https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax)
* [TempeHS Unity template](https://github.com/TempeHS/TempeHS_Unity_DevContainer)
