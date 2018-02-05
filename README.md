# Steam-Multiplayer-for-Unity
恩这个是一个基于Steamwork.Net 的steam多人联机解决方案，适用于小型P2P多人游戏。

如果你觉得这个项目对你有帮助，请给个 :star: XD，

如果你想和我们一起完善这个项目，请尽情的反馈Issue或者发送Pull Request，我会邀请你加入协作者。
# How To Play The Demo
打开主场景（Main.unity）,运行，你会看到这个。
请确保已经打开了Steam客户端，Demo场景采用测试Appid 480，这个是Steam官方的测试Appid，所以你会看到许多其他人的大厅，但是你是无法加入进去的。
![Screenshot](https://github.com/Asixa/Steam-Multiplayer-for-Unity/blob/master/GitHub/Resource/Tutorial1.png?raw=true "Screenshot")

点击Create按钮即可创建大厅，在其中你可以进行简单设置
![Screenshot](https://github.com/Asixa/Steam-Multiplayer-for-Unity/blob/master/GitHub/Resource/Tutorial2.png?raw=true "Screenshot")

创建完成后你会加入大厅，大厅中包含一个聊天栏，你可以点击Invite来直接邀请Steam好友加入游戏，注意，这个按钮只有在Standalone状态下才有用，Unity编辑器状态下，请打开steam客户端，右键好友头像-邀请加入游戏，来邀请。

![Screenshot](https://github.com/Asixa/Steam-Multiplayer-for-Unity/blob/master/GitHub/Resource/Tutorial3.png?raw=true "Screenshot")
![Screenshot](https://github.com/Asixa/Steam-Multiplayer-for-Unity/blob/master/GitHub/Resource/Tutorial10.png?raw=true "Screenshot")

然后点击Play 就可以进行游戏了
![Screenshot](https://github.com/Asixa/Steam-Multiplayer-for-Unity/blob/master/GitHub/Resource/Tutorial11.jpg?raw=true "Screenshot")

# How To Use
这个多人游戏框架与UNET十分类似。

## Identity组件
Identity组件保存物体的ID，会在添加其他组件的时候自动添加

![Screenshot](https://github.com/Asixa/Steam-Multiplayer-for-Unity/blob/master/GitHub/Resource/Tutorial8.png?raw=true "Screenshot")

## Sync组件
使用Sync组件你可以直接同步脚本的Public变量，但是目前不支持同步自定义的结构体和类，只支持基本类型和Unity的Vector3和Quaternion

![Screenshot](https://github.com/Asixa/Steam-Multiplayer-for-Unity/blob/master/GitHub/Resource/Tutorial5.png?raw=true "Screenshot")
## RPC组件
使用RPC组件你可以远程调用物体的函数方法，你需要先将方法添加到组件内的列表，然后调用方法ID。

![Screenshot](https://github.com/Asixa/Steam-Multiplayer-for-Unity/blob/master/GitHub/Resource/Tutorial6.png?raw=true "Screenshot")

## SyncTransform组件
使用SyncTransform组件你可以 [平滑] 地同步物体的移动，但是物体旋转需要使用Sync。

![Screenshot](https://github.com/Asixa/Steam-Multiplayer-for-Unity/blob/master/GitHub/Resource/Tutorial7.png?raw=true "Screenshot")

## NetworkManager
你需要将所有会生成的物体制作成Prefab，然后添加到NetworkManager的SpawnableObjects数组里，这个与UNET一样。
NetworkManager需要有两个组件，NetworkControl和NetworkLobbyManager，NetworkLobbyManager负责大厅，NetworkControl负责游戏内逻辑，这个物体不会在场景加载的过程中删除。
![Screenshot](https://github.com/Asixa/Steam-Multiplayer-for-Unity/blob/master/GitHub/Resource/Tutorial9.png?raw=true "Screenshot")

