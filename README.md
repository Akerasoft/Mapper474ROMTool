# Mapper474ROMTool

(C) Copyright 2023 Akerasoft

(Akeraiotitasoft LLC DBA Akerasoft)

Apache 2.0 License

This program either splits or combines ROM images from .NES rom format to ROM chip format
or to .NES rom format from ROM chip format

To NES rom
Input: 8 KB CHR ROM + 64 KB PRG ROM.  

Output: Only 48KB PRG rom is stored, with some unused/ignored but existing bytes.

From NES rom
Input: .NES ROM with Mapper 474 and submapper 0 or 1

Output: 64KB PRG ROM and 8KB CHR ROM and header file.
