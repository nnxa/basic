10 A% = 1
20 B% = 1
30 PRINT A%; " ";
40 PRINT B%; " ";
50 FOR I% = 1 TO 90
60 B% = A% + B%
70 A% = B% - A%
80 PRINT B%; " ";
90 NEXT I%
100 PRINT
