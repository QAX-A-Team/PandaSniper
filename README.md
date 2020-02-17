# PandaSniper
Linux C2 框架demo，为期2周的”黑客编程马拉松“，从学习编程语言开始到实现一个demo的产物。

特别注意：此程序仅仅是demo，请勿用于实际项目。

但是我们会不定时更新，随着我们对这次新学的编程语言的更深入了解和运用，我们会不断更新和优化代码以及架构，让PandaSniper随着我们一起成长，作为我们小组技术提升的体现。

为什么叫”PandaSniper“，没有为什么，觉得熊猫狙击手比较好听，就选它了。

# 起因

我相信每一个入行（信息安全）的人都有一个木马梦，向往有一天能够骑着自己的”马儿“浪迹天涯。从红狼、上行、大灰狼到现在的MSF、Cobaltstrike等，名字在不停的变化，功能在不停的优化，架构在不停的更新，异或慢慢消失在了信息安全这个小行业的历史长河里，但是”马儿“永远是”搞站“（渗透测试、红队评估等等）活动中的不可或缺的一大利器。

作为安全圈一名入行十余年的老鸟和带领的几个入行几年的新鸟但都是编码圈的菜鸟的我们，一起达成了一个共识，我们要实现我们的“木马梦”，虽然我们2周前还不会写GO、C#、C/C++。我们不做”如果有这么一款马儿就好了的“美梦，我们自己上，成不成看天意，做不做看自己。

# 架构简述

PandaSniper使用不同编程语言，编写3个独立的组件，分为Master、Agent、Implant这3个部分。

- Master（主控端）：使用C#和WPF编写，作为主要功能的展示和操作部分。

- Agent（代理端）：使用GO编写，顾名思义作为Master端的代理人，接收和发出Master的各种指令以及数据。

- Implant（植入端）：使用c/c++编写，用于目标主机数据获取的植入程序，并发送数据到Agent端。

  

  数据流和协议：

​                        Master       <----(tcp/ssl)----->    Agent    <----(http)---->    Implant



如果你打开我们的Master界面会发现，及其像Cobaltstrike。我们整个架构和界面都是模仿的Cobaltstrike，Cobaltstrike是我目前用过的最好用的和扩展性最好的以及最稳定的C2工具。我也希望我们的PandaSniper能像Cobaltstrike一样好用，但目前相差太远（及其遥远），但我们相信未来。

#  功能

目前只有一个可能还存在bug的命令执行功能。对一些需要交互式的命令基本没有支持，功能还需大力完善。

# 依赖

- Master：.NET Framework 4.6 （Visual Studio 2019）
- Agent: 无
- Implant：libcurl4

#### 编译和安装：

- Master端使用了MaterialDesignColors和MaterialDesignThemes.Wpf（项目网址：http://materialdesigninxaml.net/），Newtonsoft.Json项目。使用Visual Studio 2019直接编译。

- Agent端依赖包
  - github.com/bitly/go-simplejson
  - github.com/dgrijalva/jwt-go
  -  github.com/urfave/cli
- Implant端
  - 安装依赖：apt-get install libcurl4-gnutls-dev
  - 编译：gcc main.cpp -lcurl -lstdc++ -lpthread

# 截图

![sc_20200217185834](/Users/pentest/Desktop/sc_20200217185834.png)