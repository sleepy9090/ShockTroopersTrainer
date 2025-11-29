Game Trainer for Shock Troopers (GOG Version)
Setup Version: setup_shock_troopers_gog-3_(12274)

Values:

P1 Invincible
Set to 255
"shocktro.exe"+000E3A08  126
"shocktro.exe"+000E3A0C  126

P2 Invincible
Set to 255
"shocktro.exe"+000E3A08  2126
"shocktro.exe"+000E3A0C  2126

P1 Health
byte, 0-128 (129-255 garbled)
"shocktro.exe"+000E3A08  1B4
"shocktro.exe"+000E3A0C  1B4

P2 Health
byte, 0-128 (129-255 garbled)
"shocktro.exe"+000E3A08  21B4
"shocktro.exe"+000E3A0C  21B4

Level Timer
byte, stored in hex but only chars 0-9?
"shocktro.exe"+000E3A08 8D00
"shocktro.exe"+000E3A0C 8D00

P1 Score
byte, hex
example score: 34127856
0D53064A = 12
0D53064B = 34
0D53064C = 56
0D53064D = 78
"shocktro.exe"+000E3A08 8DB2
"shocktro.exe"+000E3A08 8DB3
"shocktro.exe"+000E3A08 8DB4
"shocktro.exe"+000E3A08 8DB5

"shocktro.exe"+000E3A0C 8DB2
"shocktro.exe"+000E3A0C 8DB3
"shocktro.exe"+000E3A0C 8DB4
"shocktro.exe"+000E3A0C 8DB5

P2 Score
byte, hex
example score: 34127856
0D53064A = 12
0D53064B = 34
0D53064C = 56
0D53064D = 78
"shocktro.exe"+000E3A08 8FB2
"shocktro.exe"+000E3A08 8FB3
"shocktro.exe"+000E3A08 8FB4
"shocktro.exe"+000E3A08 8FB5

"shocktro.exe"+000E3A0C 8FB2
"shocktro.exe"+000E3A0C 8FB3
"shocktro.exe"+000E3A0C 8FB4
"shocktro.exe"+000E3A0C 8FB5

P1 Weapon Type
byte, 0 = Normal, 4 = Heavy, 8 = Vulcan, 12 = 3-Way, 16 = Buster, 20 = Flame, 24 = Rocket, 28 = Missile, 32 = Hyper
Any other value results in game lockup.
"shocktro.exe"+000E3A08 8DB8
"shocktro.exe"+000E3A0C 8DB8

P2 Weapon Type
byte, 0 = Normal, 4 = Heavy, 8 = Vulcan, 12 = 3-Way, 16 = Buster, 20 = Flame, 24 = Rocket, 28 = Missile, 32 = Hyper
Any other value results in game lockup.
"shocktro.exe"+000E3A08 8FB8
"shocktro.exe"+000E3A0C 8FB8

P1 Ammo Count
2 bytes (0 - 209 displayable, rest is garbled, max 65535)
"shocktro.exe"+000E3A08 8DBA
"shocktro.exe"+000E3A0C 8DBA

P2 Ammo Count
2 bytes (0 - 209 displayable, rest is garbled, max 65535)
"shocktro.exe"+000E3A08 8FBA
"shocktro.exe"+000E3A0C 8FBA

P1 Bomb Count (Team Battle Character 1)
byte, 0-128 (129-255 grenade not thrown)
"shocktro.exe"+000E3A08 8DC2
"shocktro.exe"+000E3A0C 8DC2

P1 Bomb Count (Team Battle Character 2)
byte, 0-128 (129-255 grenade not thrown)
"shocktro.exe"+000E3A08 8DC6
"shocktro.exe"+000E3A0C 8DC6

P1 Bomb Count (Team Battle Character 3)
byte, 0-128 (129-255 grenade not thrown)
"shocktro.exe"+000E3A08 8DCA
"shocktro.exe"+000E3A0C 8DCA

P2 Bomb Count (Team Battle Character 1)
byte, 0-128 (129-255 grenade not thrown)
"shocktro.exe"+000E3A08 8FC2
"shocktro.exe"+000E3A0C 8FC2

P2 Bomb Count (Team Battle Character 2)
byte, 0-128 (129-255 grenade not thrown)
"shocktro.exe"+000E3A08 8FC6
"shocktro.exe"+000E3A0C 8FC6

P2 Bomb Count (Team Battle Character 3)
byte, 0-128 (129-255 grenade not thrown)
"shocktro.exe"+000E3A08 8FCA
"shocktro.exe"+000E3A0C 8FCA

P1 Credits
byte, hex, 0-99h but no A-F, 0-153d (99h), rest is garbled, max 255d (FFh)
16d = 10 Credits
"shocktro.exe"+000E3A10 34

P2 Credits
byte, hex, 0-99h but no A-F, 0-153d (99h), rest is garbled, max 255d (FFh)
16d = 10 Credits
"shocktro.exe"+000E3A10 35



