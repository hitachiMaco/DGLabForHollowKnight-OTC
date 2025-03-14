# DGLabForHollowKnight-OTC
适配郊狼3.0的空洞骑士HollowKnight模组，使用OTC控制器

视频教程马上到来！

**本项目使用MIT协议开源，欢迎大家写出你喜欢的游戏的mod，上传分享哦**


**我的仓库：https://github.com/hitachiMaco/DGLabForHollowKnight-OTC**

#### 在此感谢：

OTC控制器github仓库：https://github.com/open-toys-controller/open-DGLAB-controller

HK-modding仓库：https://github.com/hk-modding/api

空洞骑士mod教程：BV1mH4y1w7tC

空洞骑士API：https://github.com/hk-modding/api

API文档：https://radiance.synthagen.net/apidocs/_images/Getting-Started.html

#### 非常感谢！


以下教程面对windows版本、steam版游戏。
### 不想改动代码、个性化功能的：
1. 手机、电脑连接同一局域网，手机下载OTC控制器，连接设备，打开娱乐模式。
2. 下载hk-modding给出的.dll 文件，覆盖到\hollow_knight_Data\Managed 文件夹下
3. 下载本mod的.dll文件，放入 \hollow_knight_Data\Managed\Mods\DGLab
     注意Mods文件夹下要新建一个文件夹，将.dll放在里面。不要直接放在Mods文件夹下。
4. 开始游戏。

备注：若没有正常加载mod，或功能不生效，可以通过查看"C:\Users\${你的用户名}\AppData\LocalLow\Team Cherry\Hollow Knight\ModLog.txt" 的输出日志，以判断实际问题。

### 有一点编程基础，可以个性化设置触发条件、强度、时间等：
1. 1、2 步同上
3. 下载visual studio 2022，按照视频教程完成配置。
4. 下载我的github仓库中的全部.cs文件到一个文件夹下，使用visual studio打开，并按照视频配置。
5. 个性化修改代码。
6. 生成解决方案，找到生成的.dll文件，放入\hollow_knight_Data\Managed\Mods\DGLab 中。
7. 进入游戏，测试是否成功。若失败，请参考Log（同上）。

