# Word De-password Tool
An attempt to play with decryption of older MS Word Documents. This first pass is going to be brute-force using existing MS Word libraries.
Future versions will look to break the 40 bit encyption that is in place directly.

## Next Steps
Microsft has documented the MS-Doc file: [https://docs.microsoft.com/en-us/openspecs/office_file_formats/ms-doc/ccd7b486-7881-484c-a137-51170af7cc22].
Note that this references MS-CFB: [https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-cfb/53989ce4-7b05-4f8d-829b-d08d6148375b],
and MS-OFFCRYPTO: [https://docs.microsoft.com/en-us/openspecs/office_file_formats/ms-offcrypto/3c34d72a-1a61-4b52-a893-196f9157f083]

### Verification
Based on MS-CFB, the file should start with 512 bytes with a CFB header that is little endian. That header should start with:
```
SIGNATURE      0D CF 11 E0 A1 B1 1A E1
CLSID          0000 0000 0000 0000 0000 0000 0000 0000
MINOR VERSION  003E
MAJOR VERSION  000X
    (3 or 4)
BOM            FFFE
SECTOR SHIFT   000X
    (9 or C)
MINI SHIFT     0006
RESERVED       XXXX XXXX XXXX
DIRECTORIES    XXXX XXXX
FAT SECTORS    XXXX XXXX
FIRST DIR      XXXX XXXX
TRANS ID       XXXX XXXX
MINI CUTOFF    0000 1000
...
```
From there, it looks like the word doc starts at offset 0x0200 (512). Based on MS-DOC, the section should start with a FIB (File Information Block) Base of 32 bytes as:
```
SIGNATURE      A5EC
VERSION        00C1
(unused)       XXXX
LID            XXXX
NEXT           XXXX
FLAGS1         XXXX
    A  B     DOT
    B  B     AutoText
    C  B     Incremental Save
    D  B     Has Pics
    E  BBBB  Saves
    F  B     Encrypted
    G  B     Which Table Stream
    H  B     Read Only
    I  B     Write Limited
    J  1     Extended
    K  B     Load Override
    L  B     Far East
    M  B     Obfuscated
FIB BACK       00XX
    BF or C1
KEY            XXXX XXXX
...
```

I have yet to figure out how to find out where it is, but the encryption header is stored in the front of the table stream. In the example document, that is offset 0x1200 (512 + 4096). Based on MS-OFFCRYPTO, the encryption is then (using RC4 CryptoAPI as per example doc):
```
MAJOR VERSION  000X
    (2, 3, or 4)
MINOR VERSION  0002
FLAGS          XXX0 0000
    A  0     B
    B  0     B
    C  B     Crypto API
    D  B     Doc Props Unencrypted
    E  B     External
    F  B     AES
HEADER SIZE    XXXX XXXX
HEADER         XXXX{n}
```
Breaking the header down, that is:
```
FLAGS          XXX0 0000
    A  0     B
    B  0     B
    C  B     Crypto API
    D  B     Doc Props Unencrypted
    E  B     External
    F  B     AES
SIZE           0000 0000
ALGO ID        0000 XXXX
    6801  RC4
    66XX  AES
ALGO HASH      0000 XXXX
    8004  SHA1
KEY SIZE       XXXX XXXX
PROVIDER TYPE  0000 00XX
    01  RC4
    18  AES
RESERVED       XXXX XXXX XXXX XXXX
CSP NAME       XXXX{n}
    (Unicode Null Terminated)
```
Following the header is the Verifier:
```
SALT SIZE      0000 0010
SALT           XXXX XXXX XXXX XXXX
VERIFIER       XXXX XXXX XXXX XXXX
HASH SIZE      0000 00XX
    14   RC4 (20 bytes)
    20   AES (32 bytes)
HASH           XXXX{n}

H0 = H(salt + password)
H_Final = H(H0 + block)
block = 0000 0000 0000 0000 (2.2.6.3)
```
