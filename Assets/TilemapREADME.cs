using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilemapREADME : MonoBehaviour
{
    /*
     ==== TILEMAPS ====
     There are (so far) two main container GameObjects in the hierarchy:
        - Visuals
        - Colliders
     DO NOT change the order of objects in the hierarchy as they are designed to be in line with the sorting layers.
     Important notes:
     - If you want to draw water that the player can walk on, draw it in both "WaterSurface" and "WaterColliders".
     - If you want to draw buildings, put them in GroundColliders. (I will make a sub-layer to separate buildings and actual collider props like signs)
    */

    /*
     Sorting Layers (back -> front)
     * Background – distant background art, parallax backdrops.
     * Ground – walkable tiles, sidewalks, dirt, floor.
     * WaterSurface – water top/sheen; sits above ground but below characters.
     * Shadows – fake blob/soft shadows painted as tiles/decals.
     * PropsLow – small rocks, grass tufts, stumps that should appear under characters.
     * Characters – player/NPCs (SpriteRenderer sorting layer).
     * PropsHigh – tree trunks upper parts, signs, upper-level building bits that should appear over characters.
     * Buildings - structures of any kind.
     * ForegroundFX – fog, canopy leaves, roof overhangs, screen-edge vignette.
     * UI – canvases.
    */
}
