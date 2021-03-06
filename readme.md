# C# battle simulation

This is the source code of pretty accurate simulation of Pokemon Go battles. 
This was intended to be used in canceled commercial project.

Every aspect of battle was considered in this simulation so you can rely on it's results. 
The only problem is accuracy - because it's hard to imitate actions of human player.
I will suggest to implement realistically random behavior of player and defender and take average result of certain number of simulations as a final result.

Current approach to random aspects of battle:
- If defending pokemon get enough energy to use charge move it will use one quick before this (this is statistically average behavior). All defender's moves have fixed +2 seconds delay to cool down.
- Player tries to use charge moves as soon as possible and dodge any attack right after yellow flash with 400 milliseconds delay to react (this constant might be changed).

"data2.1.xlsx" file contains all game data (before type effectiveness change that came with gym rework in Summer 2017).

Licensing is MIT which basically means you can do whatever you want with it.
I will appreciate if you mention my authorship of this code in your projects.

# Debugging

Setting `Battle_simulator.write_battle_log = true;` will make this program write in current folder `battle log.csv` with highly detailed log of simulated battles with each of 3 preset dodge tactics.

# Recursive simulation

I also made alternatime simulation algorithm in different branch. 

The concept is - every time when defender can use charged move simulation split in two timelines (one for used charge, two for not used). 
Every time battle ends in any timeline the result is added to `Battle_simulator.simulation_results`. With this data you can get realistic win chance and exact average result (use `Battle_simulator.show_average_result()`).

It's pretty heavy and probably statistic algorithm will be more accurate.
Just want to share this curious idea.

# PS

This chart might help you to understand how I defined time for dodges:

![https://github.com/InfernumDeus/Csharp-PokemonGO-battle-simulation/blob/master/time%20chart.png](https://raw.githubusercontent.com/InfernumDeus/Csharp-PokemonGO-battle-simulation/master/time%20chart.png)
