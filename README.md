# Steam-Multiplayer-for-Unity
# This library is outdated and no longer maintained.

# If you need Unity - Steam multiplayer,
# See [Mirror](http://markdown-here.com)<br>

[![Issues](https://img.shields.io/github/issues/Asixa/Steam-Multiplayer-for-Unity.svg?style=flat-square)](https://github.com/Asixa/Steam-Multiplayer-for-Unity/issues)  
[![Release](https://img.shields.io/github/release/Asixa/Steam-Multiplayer-for-Unity.svg?style=flat-square)](https://github.com/Asixa/Steam-Multiplayer-for-Unity/releases/latest)

This is a Steam multiplayer online solution based on Steamwork.Net for small P2P multiplayer games.

If you think this project is helpful to you, please give :star: ,

If you want to work with us to improve this project, please feel free to feedback Issue or send a Pull Request, I will invite you to join the collaborators.

# How To Play The Demo
Run the main scene (Main.unity), you will see this.
Make sure the Steam client is open. The Demo scene uses the test Appid 480. This is the official Test Appid for Steam, so you will see many other people's lobbys, but you can't join them.

![Screenshot](https://github.com/Asixa/Steam-Multiplayer-for-Unity/blob/master/GitHub/Resource/Tutorial1.png?raw=true "Screenshot")

Click the Create button to create a lobby where you can make simple settings
![Screenshot](https://github.com/Asixa/Steam-Multiplayer-for-Unity/blob/master/GitHub/Resource/Tutorial2.png?raw=true "Screenshot")

After the creation is complete, you will join the lobby. The lobby contains a chat bar. You can click on Invite to directly invite Steam friends to join the game. Note that this button is only useful in the Standalone state. In the Unity editor state, please open the steam client. , right click friend avatar - invite to join the game, to invite.

![Screenshot](https://github.com/Asixa/Steam-Multiplayer-for-Unity/blob/master/GitHub/Resource/Tutorial3.png?raw=true "Screenshot")
![Screenshot](https://github.com/Asixa/Steam-Multiplayer-for-Unity/blob/master/GitHub/Resource/Tutorial10.png?raw=true "Screenshot")

Then click Play to play the game.
![Screenshot](https://github.com/Asixa/Steam-Multiplayer-for-Unity/blob/master/GitHub/Resource/Tutorial11.jpg?raw=true "Screenshot")

# How To Use
This multiplayer game framework is very similar to UNET.

## Identity
The Identity component saves the ID of the object and automatically adds it when adding other components.

![Screenshot](https://github.com/Asixa/Steam-Multiplayer-for-Unity/blob/master/GitHub/Resource/Tutorial8.png?raw=true "Screenshot")

## Sync
With the Sync component you can directly synchronize the script's Public variable, but currently does not support synchronizing custom structs and classes, only basic types and Unity's Vector3 and Quaternion are supported.

![Screenshot](https://github.com/Asixa/Steam-Multiplayer-for-Unity/blob/master/GitHub/Resource/Tutorial5.png?raw=true "Screenshot")
## RPC
Using the RPC component you can call the function method of the object remotely. You need to add the method to the list inside the component and then call the method ID.

![Screenshot](https://github.com/Asixa/Steam-Multiplayer-for-Unity/blob/master/GitHub/Resource/Tutorial6.png?raw=true "Screenshot")

## SyncTransform
With the SyncTransform component you can [smoothly] synchronize the movement of objects, but the object rotation requires Sync.

![Screenshot](https://github.com/Asixa/Steam-Multiplayer-for-Unity/blob/master/GitHub/Resource/Tutorial7.png?raw=true "Screenshot")

## NetworkManager
You need to make all the objects that will be generated into Prefab and then add them to the SpawnablePrefab array of NetworkManager, just like UNET.
NetworkManager needs to have two components, NetworkControl and NetworkLobbyManager, NetworkLobbyManager is responsible for the lobby, NetworkControl is responsible for in-game logic, this object will not be deleted during the scene loading process.

![Screenshot](https://github.com/Asixa/Steam-Multiplayer-for-Unity/blob/master/GitHub/Resource/Tutorial9.png?raw=true "Screenshot")

