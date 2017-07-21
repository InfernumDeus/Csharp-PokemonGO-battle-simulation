# C# battle simulation

Source code of pretty accurate simulation of Pokemon Go battles. 
This was intended to be used in conceled commercial project.

Every aspect of battle was cosidered in this simulation so you can rely on it's results. 
The only problem is accuracy - because it's hard to imitate actions of human player.

Current approach to random aspects of battle:
- If defending pokemon get enough energy to use charged move he use one quick before this (this is statistically average behavior). All defender's moves have fixed +2 seconds to cool down.
- Player tries to use charged moves as soon as possible and dodge any attack right after yellow flash with 400 milliseconds to react (this constant might be changed).

"data2.1.xlsx" file contain all gamedata before type effectiveness chadge that came with gym rework in Summer 2017.

Licensing is MIT which basically means you can do whatever you want with it.
I will appreciate if you mention my authorship of this code in your projects.
