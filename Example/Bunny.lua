WIRE_WRITE BUNNY

Offset_X=6
Offset_Y=7

Max_Line=4
Max_Pixel_X=16
Max_Pixel_Y=14

function IsOne(data,bit)
	if(((1<<bit)&data)~=0)then
		return true
	else
		return false
	end
end

for line=0,Max_Line-1,1 do
	for color=0,3,1 do
		for p_line=0,Max_Pixel_Y-1,1 do
			data=bin.ReadUInt16()
			for p_row=0,Max_Pixel_X-1,1 do
				x=sign.X+Offset_X+p_line*16+p_row
				y=sign.Y+Offset_Y+line*3
				if(color==0)then
					tiles[x][y].WireRed=IsOne(data,p_row)
				elseif(color==1)then
					tiles[x][y].WireBlue=IsOne(data,p_row)
				elseif(color==2)then
					tiles[x][y].WireGreen=IsOne(data,p_row)
				elseif(color==3)then
					tiles[x][y].WireYellow=IsOne(data,p_row)
				end
			end
		end
	end
end