10 C% = 1000
20 DIM X%(C%): P% = 2
30 PRINT P%; " ";
40 FOR M% = P% * 2 TO C% STEP P%
50 X%(M%) = 1
60 NEXT M%
70 P% = P% + 1
80 IF P% > C% THEN PRINT : END
90 IF X%(P%) = 0 THEN GOTO 30
100 GOTO 70