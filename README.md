# Rapier for Unity
This package lets you simulate physics in unity using Rapier. 

This is useful for when you want to run physics in non-unity clients for validation. 
E.g. Server Authoritive setup with client side prediction. An example of excellent use is running Rapier on a SpaceTimeDB server, and when syncing it with a client also running Rapier.

# Overview
Quick showcase:
<video src='https://github.com/user-attachments/assets/7429283b-71d8-4e6a-a1fb-1256f48382cc' width=180/>

Note how it acts just like normal Unity! and it even supports raycasting! 
<video src='https://github.com/user-attachments/assets/5d6ed46d-ddd2-47f2-9d2e-7d43020aa74f' width=180/>

# Installation
## Embedded Package
Currently, this package only supports being a embedded package, being placed directly in the packages folder.
You can either clone the repo into the packages folder or use it as a submodule in your own repos.
<img width="622" height="209" alt="image" src="https://github.com/user-attachments/assets/c1cbaac2-b6d4-41de-b10f-b9c895b2c97f" />

## Changing Physics Engine
Once installed, you'll have to set your Gameobject Physics SDK to "None", then restart the project.
<img width="456" height="320" alt="image" src="https://github.com/user-attachments/assets/12e69798-23ab-4fe0-836d-9646031c2050" />

# References
* https://www.naps62.com/posts/unity-meets-rust
* https://rjgameiro.medium.com/let-fun-rust-unity-f7f62609ba49
* https://www.forrestthewoods.com/blog/how-to-reload-native-plugins-in-unity/
* https://github.com/appsinacup/godot-rapier-physics
