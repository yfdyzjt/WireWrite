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