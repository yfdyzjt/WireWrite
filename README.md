# Wire Write
Wire Write 是一个可以向泰拉瑞亚储存电路中写入数据的独立程序。  
  
**WireWrite 已经弃用，不会继续更新。它的最新版本是可用的。该项目已迁移至[TMake](https://github.com/yfdyzjt/TMake "TMake")**

## 使用

*注：Wire Write 使用了[.NET 8.0](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0 ".NET 8.0")，如果没有安装，请点击蓝色的字安装*

你可以使用下面的指令打开 Wire Write：

```shell
wirewrite <地图文件名> <数据文件名>
```

也可以直接打开 Wire Write ，然后再输入地图文件和数据文件名。  

文件名可以是完整的路径，也可以只有文件名。程序会按照下面的顺序搜索文件：

> 程序运行目录  
> 泰拉瑞亚原版地图目录  
> 泰拉瑞亚模组地图目录

之后 Wire Write 会执行地图文件内的 Lua 脚本，将数据文件里的数据写入到脚本指定的储存电路中。

## 示例

你可以在`Example`文件夹中找到`Bunny.wld`和`Bunny.bin`，将`Bunny.wld`放到泰拉瑞亚的地图目录，将`Bunny.bin`放到程序运行目录。  

之后使用下面的指令（或直接打开 Wire Write ，然后手动输入地图文件名和数据文件名）：

```shell
wirewrite Bunny.wld Bunny.bin
```

这条指令打开会地图文件`Bunny.wld`，执行里面的脚本，将数据文件`Bunny.bin`里面的数据写入到脚本指定的只读储存器中。

![Bunny.wld](./Image/Bunny.wld.png "Bunny.wld")  
*Bunny.wld内的ROM，脚本在左上角的告示牌内*
***
之后再进入地图，激活拉杆，你应该会看到类似下面的效果:

<img src="./Image/Bunny_Run.gif" width="10%"> 

**兔子动了起来！**  

实际上，将视频写入只读储存器只是 Wire Write 功能的一部分。它完全可以将任何计算机可以处理和表示的数据（文字、图片、视频、程序等等）写入到绝大部分的储存电路中（寄存器、只读储存器、可读写储存器、顺序储存器、随机储存器等等）。  
只需要改变储存电路左上角的告示牌内的`Lua`脚本即可。 

*注：`Lua`语言是与`C`语言类似的脚本语言，如果你会`C`语言的话，可以直接编写`Lua`脚本，只需要注意一点点与C不同的地方*

***
但是当你打开左上角的告示牌，会看见一大堆像乱码的东西：  

<details>
<summary>点击展开 告示牌内容</summary>

![Script On Sign](./Image/Script_On_Sign.png "Script On Sign")  
</details>

######

别担心，告示牌的内容与`Bunny.lua`的内容完全相同，只是将所有换行符`/r/n`替换为`//r//n`而已。

<details>
<summary>点击展开 Bunny.lua</summary>

```lua
WIRE_WRITE BUNNY

-- 储存电路起始坐标相对于告示牌左上角坐标的偏移
Offset_X = 6
Offset_Y = 7

-- 储存电路最大行数，每行有四种颜色
Max_Line = 4
-- 屏幕像素大小
Max_Pixel_X = 16
Max_Pixel_Y = 14

-- 判断 data 的第 bit 位是否是1
function IsOne(data, bit)
	if(((1 << bit) & data) ~= 0) then
		return true
	else
		return false
	end
end

-- 遍历储存电路的所有行
for line = 0, Max_Line - 1, 1 do
	-- 遍历储存电路每行中所有颜色
	for color = 0, 3, 1 do
		-- 遍历屏幕的所有行
		for p_line = 0, Max_Pixel_Y - 1, 1 do
			-- 从数据文件中读取数据
			data = bin.ReadUInt16()
			-- 遍历屏幕每行中所有列
			for p_row = 0, Max_Pixel_X - 1, 1 do
				-- 计算出当前数据写入位置坐标
				-- 水平坐标为：告示牌水平坐标、水平偏移、每行最大像素倍屏幕行数、屏幕列数 之和 
				x = sign.X + Offset_X + p_line * Max_Pixel_X + p_row
				-- 竖直坐标为：告示牌竖直坐标、竖直偏移、三倍行数（每行有三格高） 之和 
				y = sign.Y + Offset_Y + line * 3
				-- 判断当前电线颜色，将电线设为数据当前位对应的值
				if(color == 0) then
					tiles[x][y].WireRed = IsOne(data, p_row)
				elseif(color == 1)then
					tiles[x][y].WireBlue = IsOne(data, p_row)
				elseif(color == 2)then
					tiles[x][y].WireGreen = IsOne(data, p_row)
				elseif(color == 3)then
					tiles[x][y].WireYellow = IsOne(data, p_row)
				end
			end
		end
	end
end
```
</details>  
  
######

*注：为什么要转义换行符？因为泰拉瑞亚的告示牌会限制换行符的数量，但是每行的长度没有限制。所以需要将转义后再写入告示牌。*

## 脚本

**查看如何编写脚本请点击[这里](https://github.com/yfdyzjt/WireWrite/wiki/%E8%84%9A%E6%9C%AC "脚本")**
