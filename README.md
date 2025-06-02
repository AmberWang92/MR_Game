# Mixed Reality Game for Oculus Quest — Ghost Gym
## Game Idea
The player stands and fights a ghost boss, holds two controllers to punch and lower the boss’s health bar.
When the ghost shoots laser light to attack. Every laser light last 3 seconds. 
The player must squat to dodge the attack. 
When the laser light is over, the player stands up again and keeps punching the ghost or grab the ray gun to shoot the ghost.
The goal is to beat the ghost, or the player loses.

## Installation
Download the Building.apk and install it through SideQuest Desktop App.

## AI system
The Boss AI system uses a combination of State Machine + Fuzzy Logic + NavMesh path navigation to control the Boss's behavior.

The behavior of the boss AI is determined by the following five **States**:
Idle
Attack
RunAway
TakeDamage
Defeat

The current state is managed by currentState, and every frame Update() will:
Update the timer
Execute the current state logic (such as running or attacking)
Every 1 second, use EvaluateNextAction() to evaluate the next state using a fuzzy logic system.
