# Insert Part Number QR Code From SQL Data

This VBA macro inserts a QR code by reading the data from SQL table.

Macro reads the name of the file from the drawing view and queries the part index and category from the SQL table. Resulting data is concatenated and inserted in the QR code into the left bottom corner.

| Id | FileName  | PartIndex | PartCategory |
|:--:|:---------:|:---------:|:------------:|
| 0  | FileName1 |     1     |     CAT1     |
| 1  | FileName2 |     2     |     CAT2     |