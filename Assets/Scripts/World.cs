using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public int seed;
    public BiomeAttributes biome;

    public Transform player;
    public Vector3 spawnPosition;

    public Material material;
    public BlockType[] blockTypes;

    private Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    private List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    private ChunkCoord playerLastChunkCoord;

    private void Start()
    {
        Random.InitState(seed);

        spawnPosition = new Vector3((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f,
            VoxelData.ChunkHeight - 50f,
            (VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f);
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
    }

    void Update()
    {
        /*
        if (GetChunkCoordFromVector3(player.position).Equals(playerLastChunkCoord) == false)
        {
            playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
            CheckViewDistance();
        }
        */
    }

    void GenerateWorld()
    {
        for (int x = VoxelData.WorldSizeInChunks / 2 - VoxelData.viewDistanceInChunks;
             x < VoxelData.WorldSizeInChunks / 2 + VoxelData.viewDistanceInChunks;
             x++)
        {
            for (int z = VoxelData.WorldSizeInChunks / 2 - VoxelData.viewDistanceInChunks;
                 z < VoxelData.WorldSizeInChunks / 2 + VoxelData.viewDistanceInChunks;
                 z++)
            {
                CreateNewChunk(x, z);
            }
        }

        player.transform.position = spawnPosition;
    }

    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, z);
    }

    void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        for (int x = coord.x - VoxelData.viewDistanceInChunks; x < coord.x + VoxelData.viewDistanceInChunks; x++)
        {
            for (int z = coord.z - VoxelData.viewDistanceInChunks; z < coord.z + VoxelData.viewDistanceInChunks; z++)
            {
                if (IsChunkInWorld(new ChunkCoord(x, z)))
                {
                    if (chunks[x, z] == null)
                    {
                        CreateNewChunk(x, z);
                    }
                    else if (chunks[x, z].isActive == false)
                    {
                        chunks[x, z].isActive = true;
                        activeChunks.Add(new ChunkCoord(x, z));
                    }
                }

                for (int i = 0; i < previouslyActiveChunks.Count; i++)
                {
                    if (previouslyActiveChunks[i].Equals(new ChunkCoord(x, z)) == true)
                    {
                        previouslyActiveChunks.RemoveAt(i);
                    }
                }
            }
        }

        foreach (ChunkCoord c in previouslyActiveChunks)
        {
            chunks[c.x, c.z].isActive = false;
        }
    }

    public bool CheckForVoxel(float _x, float _y, float _z)
    {
        int xCheck = Mathf.FloorToInt(_x);
        int yCheck = Mathf.FloorToInt(_y);
        int zCheck = Mathf.FloorToInt(_z);

        int xChunk = xCheck / VoxelData.ChunkWidth;
        int zChunk = zCheck / VoxelData.ChunkWidth;

        xCheck -= (xChunk * VoxelData.ChunkWidth);
        zCheck -= (zChunk * VoxelData.ChunkWidth);

        if (xChunk < 0 || xChunk >= VoxelData.WorldSizeInChunks || zChunk < 0 || zChunk >= VoxelData.WorldSizeInChunks)
            return false;

        return blockTypes[chunks[xChunk, zChunk].voxelMap[xCheck, yCheck, zCheck]].isSolid;
    }

    public byte GetVoxel(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);

        // 월드 밖이면 공기        
        if (!IsVoxelInWorld(pos))
            return 0;

        // 청크의 제일 밑바닥이면 기반암
        if (yPos == 0)
            return 1;

        // 기본 지형 패스 

        int terrainHeight =
            Mathf.FloorToInt(
                biome.terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.terrainScale) +
                biome.solidGroundHeight);

        byte voxelValue = 0;

        if (yPos == terrainHeight)
            voxelValue = 3;
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
            voxelValue = 5;
        else if (yPos > terrainHeight)
            return 0;
        else
            voxelValue = 2;

        // 두번째 패스

        if (voxelValue == 2)
        {
            foreach (Lode lode in biome.lodes)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                        voxelValue = lode.blockID;
            }
        }

        return voxelValue;
    }

    void CreateNewChunk(int x, int z)
    {
        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this);
        activeChunks.Add(new ChunkCoord(x, z));
    }

    bool IsChunkInWorld(ChunkCoord coord)
    {
        if (coord.x >= 0 && coord.x < VoxelData.WorldSizeInChunks && coord.z >= 0 &&
            coord.z < VoxelData.WorldSizeInChunks)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.ChunkHeight
            && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;

    [Header("Texture Values")] public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    // Back, Front, Top, Bottom, Left, Right

    public int GetTextureID(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("GetTexture ID // 텍스쳐 인덱스 잘못됨");
                return 0;
        }
    }
}