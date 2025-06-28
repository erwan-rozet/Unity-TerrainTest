using UnityEngine;

[ExecuteAlways]
public class TerrainAutoPainter : MonoBehaviour
{
    [Header("Références")]
    public Terrain terrain;
    public TerrainLayer[] terrainLayers;

    [Header("Critères")]
    [Tooltip("Hauteur sous laquelle on applique la texture de terre")]
    public float lowHeight = 5f;

    [Tooltip("Hauteur sous laquelle on applique la texture d'herbe")]
    public float midHeight = 30f;

    [Tooltip("Pente au-dessus de laquelle on applique la texture de roche")]
    [Range(0f, 90f)]
    public float slopeThreshold = 65f;

    [Header("Blending (transition douce entre textures)")]
    [Tooltip("Largeur de la zone de transition en hauteur (mètres)")]
    [Range(0f, 20f)]
    public float heightBlendRange = 5f;

    [Tooltip("Largeur de la zone de transition de pente (degrés)")]
    [Range(0f, 30f)]
    public float slopeBlendRange = 10f;

    [Header("Options")]
    public bool autoUpdate = false;

    public void ApplyAutoTexture()
    {
        if (terrain == null || terrainLayers == null || terrainLayers.Length < 3)
        {
            Debug.LogWarning("Terrain ou layers insuffisants !");
            return;
        }

        TerrainData data = terrain.terrainData;
        int width = data.alphamapWidth;
        int height = data.alphamapHeight;
        float[,,] splatmap = new float[width, height, terrainLayers.Length];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float normX = (float)x / (width - 1);
                float normY = (float)y / (height - 1);

                float worldHeight = data.GetInterpolatedHeight(normX, normY) + terrain.transform.position.y;
                float steepness = data.GetSteepness(normX, normY);

                float[] weights = new float[terrainLayers.Length];

                // Pente forte = roche
                float slopeWeight = Mathf.InverseLerp(slopeThreshold - slopeBlendRange, slopeThreshold + slopeBlendRange, steepness);

                // Terre = base du terrain
                float dirtWeight = Mathf.InverseLerp(0f, lowHeight + heightBlendRange, worldHeight);

                // Herbe = altitude intermédiaire
                float height01 = Mathf.InverseLerp(lowHeight - heightBlendRange, midHeight + heightBlendRange, worldHeight);
                float grassWeight = Mathf.Clamp01(1f - Mathf.Abs(height01 - 0.5f) * 2f); // maximum au centre

                // Roche par hauteur
                float rockByHeightWeight = Mathf.InverseLerp(midHeight - heightBlendRange, midHeight + heightBlendRange, worldHeight);

                // Final blending
                weights[0] = dirtWeight * (1f - slopeWeight);             // terre
                weights[1] = grassWeight * (1f - slopeWeight);           // herbe
                weights[2] = Mathf.Max(slopeWeight, rockByHeightWeight); // roche

                // Normalisation
                float total = 0f;
                foreach (var w in weights) total += w;
                for (int i = 0; i < terrainLayers.Length; i++)
                    splatmap[x, y, i] = weights[i] / total;
            }
        }

        data.terrainLayers = terrainLayers;
        data.SetAlphamaps(0, 0, splatmap);

        Debug.Log("✅ Auto texture appliquée.");
    }

    private void OnValidate()
    {
        if (autoUpdate)
        {
            ApplyAutoTexture();
        }
    }
}
