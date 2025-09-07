using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Indicator : MonoBehaviour
{
    public enum Type{ bsp, l_system, terrain };
    
    public enum BSP_type { width, height, minroom, maxroom,tile_size, wall_height,floor_size };
    public enum terrain_type { size, height_scale, roughness, cell_size, base_height};
    public enum L_System_type { axiom, turning_angle, length, depth, thickness};

    public Type type;
    public BSP_type bsp;
    public terrain_type Terrain_Type;
    public L_System_type L_System_;
    public TextMeshProUGUI textparent;
    public BSPDungeonMesh bsp_mesh;
    public L_System l_System;
    public DiamondSquareTerrain Diamond_terrain;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (type)
        {
            case Type.bsp:
                BSP_SWITCH();
                break;
            case Type.terrain:
                TERRAIN_SWITCH();
                break;
            case Type.l_system:
                L_SYSTEM_SWITCH();
                break;
            default:
                Debug.Log("Problem with assignation of text");
                break;
        }
    }
    public void L_SYSTEM_SWITCH()
    {
        switch (L_System_)
        {
            case L_System_type.turning_angle: textparent.text = l_System.Turning_Angle.ToString(); break;
            case L_System_type.length: textparent.text = l_System.Length.ToString(); break;
            case L_System_type.depth: textparent.text = l_System.Depth.ToString(); break;
            case L_System_type.thickness: textparent.text = l_System.Thickness.ToString(); break;
        }
    }
    public void BSP_SWITCH()
    {
        switch (bsp) { 
            case BSP_type.width: textparent.text = bsp_mesh.DungeonWidth.ToString(); break;
            case BSP_type.height: textparent.text = bsp_mesh.DungeonHeight.ToString(); break;
            case BSP_type.minroom: textparent.text = bsp_mesh.MinRoomSize.ToString(); break;
            case BSP_type.maxroom: textparent.text = bsp_mesh.MaxRoomSize.ToString(); break;
            case BSP_type.tile_size: textparent.text = bsp_mesh.TileSize.ToString(); break;  
            case BSP_type.wall_height: textparent.text = bsp_mesh.WallHeight.ToString(); break;
            case BSP_type.floor_size: textparent.text = bsp_mesh.FloorHeight.ToString(); break;
            default: Debug.Log("Problem with assignation of BSP"); break;
        }
    }
    public void TERRAIN_SWITCH()
    {
        switch (Terrain_Type)
        {
            case terrain_type.size: textparent.text = Diamond_terrain.Size.ToString(); break;
            case terrain_type.height_scale: textparent.text = Diamond_terrain.HeightScale.ToString(); break;
            case terrain_type.roughness: textparent.text = Diamond_terrain.Roughness.ToString(); break;
            case terrain_type.cell_size: textparent.text = Diamond_terrain.CellSize.ToString(); break;
            case terrain_type.base_height: textparent.text = Diamond_terrain.BaseHeight.ToString(); break;
            default: Debug.Log("Problem with assignation of Terrain"); break;
        }
    }
}
