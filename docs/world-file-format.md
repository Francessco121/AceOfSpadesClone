# AoS Clone World File Format (.aosw)

This file documents the third version of the world file format used by this game. Example world files can be found under the game's content folder (files with the `.aosw` extension).

> Note: All v3 world files are fully compressed using GZIP.

World files contain (in order) the following data sections:
1. [File header](#file-header) - Contains the version of the file format. 
2. [World objects](#world-objects) - Game objects serialized in the world.
3. [Chunks](#chunks) - The terrain data (split into chunks).

## File header
| Name    | Type    | Notes                                    |
| ------- | ------- | ---------------------------------------- |
| Version | float32 | The file format version, in this case: 3 |

## World objects

The world objects section contains all of the game objects which are serialized in the world file (e.g. command posts and intel).

This section is made up of the following sub-sections:
1. A [header](#world-objects-header)
2. Zero or more [World object description](#world-object-description)s. See the header for the number of objects. Each world object section appears right after one another.

### World objects header
| Name      | Type      | Notes                                    |
| --------- | --------- | ---------------------------------------- |
| # Objects | int32     | The number of objects in this world      |

### World object description
A world object is described by its identifier tag and a variable number of data fields. Data fields for example may contain the XYZ coordinates of the game object. These descriptions are further deserialized into actual game objects based on their tag.

World object descriptions are made up of the following sub-sections:
1. A [header](#world-object-description-header).
2. Zero or more [fields](#world-object-description-field). See the header for the number of fields. Each field appears right after one another.

#### World object description header
| Name      | Type                   | Notes                                       |
| --------- | ---------------------- | ------------------------------------------- |
| Tag       | Null terminated string | A string identifying the object             |
| # Fields  | uint16                 | The number of fields describing this object |

#### World object description field
| Name      | Type                                         | Notes                      |
| --------- | -------------------------------------------- | -------------------------- |
| Key       | Null terminated string                       | The field's identifier     |
| Type      | A [primitive type](#primitive-type) (byte)   | The data-type of the field |
| Value     | The primitive specified by the 'Type' field. | See above row              |

#### Primtive Type
| Name     | Value  |
| -------- | -------|
| None     | 0      |
| Byte     | 1      |
| SByte    | 2      |
| Char     | 3      |
| Boolean  | 4      |
| Int16    | 5      |
| UInt16   | 6      |
| Int32    | 7      |
| UInt32   | 8      |
| Int64    | 9      |
| UInt64   | 10     |
| Single   | 11     |
| Double   | 12     |
| ByteFlag | 13     |

## Chunks

The chunks section contains the actual terrain data, split into 32x32x32 chunks of block data.

This section is made up of the following sub-sections:
1. A [header](#chunks-header).
2. Zero or more [chunk](#chunk)s.

### Chunks header
| Name      | Type      | Notes                                    |
| --------- | --------- | ---------------------------------------- |
| # Chunks  | uint16    | The number of chunks in this world       |

### Chunk

Chunks are made of a header section and a variable number of block sections. A single header section will always be at the beginning of a chunk definition. Chunk sections should be continuously read until the end of the file.

#### Chunk section
| Name         | Type             | Notes                                      |
| ------------ | ---------------- | ------------------------------------------ |
| Section Type | byte             | 0 for start of new chunk, 1 for block data |
| Section | [header](#chunk-header) or [blocks](#chunk-block-section) | See above row |

#### Chunk header
| Name         | Type             | Notes                                      |
| ------------ | ---------------- | ------------------------------------------ |
| X            | int32            | X coordinate of chunk                      |
| Y            | int32            | Y coordinate of chunk                      |
| Z            | int32            | Z coordinate of chunk                      |

#### Chunk block section
Block sections of each chunk represent a variable number of blocks in a row which all contain the same data and are of the same type.

This section is made up of the following sub-sections:
1. A [header](#chunk-block-section-header).
2. Optionally, a [color section](#chunk-block-color-section). Existence is determined by the header.

A block can be constructed using the data field from the header and the color section. This block should be repeated the number of times specified by the header. 

The actual positions (inside of the chunk) of each block can be calculated by keeping track of how many blocks have been added to the chunk, using the following formula:
```csharp
int x = index / (32 * 32);
int y = (index / 32) % 32;
int z = index % 32;
```

##### Chunk block section header
The data field of this header stores two pieces of information, the material of each block and the health of each block. The material can be determined by extracting the lower 4-bits of the data byte, and the health can be determined by extracting the higher 4-bits.

| Name         | Type             | Notes                                      |
| ------------ | ---------------- | ------------------------------------------ |
| # Blocks     | uint16           | Number of blocks encoded in this section.  |
| Data         | byte             | Data of each block (material and health).  |

##### Chunk block color section
This optional section appears after the block section header only when the material specified by the data is not air. The material can be retrieved by extracting the lower 4-bits of the data byte. If the material is not equal to 0, then this section will be present.

| Name         | Type             | Notes                                      |
| ------------ | ---------------- | ------------------------------------------ |
| R            | byte             | Red color component                        |
| G            | byte             | Green color component                      |
| B            | byte             | Blue color component                       |
