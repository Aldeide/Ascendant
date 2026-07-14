using UnityEngine;

namespace Ascendant.SystemsExtensions.Celestial
{
    [System.Serializable]
    public struct GridCoordinate : Unity.Netcode.INetworkSerializeByMemcpy
    {
        public int CellX;
        public int CellY;
        public int CellZ;
        public Vector3 Offset;

        public static readonly float CellSize = 1000f;

        public GridCoordinate(int x, int y, int z, Vector3 offset)
        {
            CellX = x;
            CellY = y;
            CellZ = z;
            Offset = offset;
            Normalize();
        }

        public GridCoordinate(Vector3 worldPos)
        {
            CellX = Mathf.FloorToInt((worldPos.x + CellSize / 2f) / CellSize);
            CellY = Mathf.FloorToInt((worldPos.y + CellSize / 2f) / CellSize);
            CellZ = Mathf.FloorToInt((worldPos.z + CellSize / 2f) / CellSize);
            Offset = worldPos - new Vector3(CellX * CellSize, CellY * CellSize, CellZ * CellSize);
        }

        public Vector3 ToWorldPosition()
        {
            return new Vector3(CellX * CellSize, CellY * CellSize, CellZ * CellSize) + Offset;
        }

        public void Normalize()
        {
            float halfCell = CellSize / 2f;

            // Normalize X
            if (Mathf.Abs(Offset.x) > halfCell)
            {
                int deltaX = Mathf.RoundToInt(Offset.x / CellSize);
                CellX += deltaX;
                Offset.x -= deltaX * CellSize;
            }

            // Normalize Y
            if (Mathf.Abs(Offset.y) > halfCell)
            {
                int deltaY = Mathf.RoundToInt(Offset.y / CellSize);
                CellY += deltaY;
                Offset.y -= deltaY * CellSize;
            }

            // Normalize Z
            if (Mathf.Abs(Offset.z) > halfCell)
            {
                int deltaZ = Mathf.RoundToInt(Offset.z / CellSize);
                CellZ += deltaZ;
                Offset.z -= deltaZ * CellSize;
            }
        }

        public static Vector3 GetDifference(GridCoordinate target, GridCoordinate origin)
        {
            Vector3 cellDiff = new Vector3(target.CellX - origin.CellX, target.CellY - origin.CellY, target.CellZ - origin.CellZ) * CellSize;
            return cellDiff + (target.Offset - origin.Offset);
        }

        public static float GetDistance(GridCoordinate a, GridCoordinate b)
        {
            return GetDifference(a, b).magnitude;
        }
    }
}
