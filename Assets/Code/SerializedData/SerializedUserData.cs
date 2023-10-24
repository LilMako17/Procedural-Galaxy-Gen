using Cathei.Mathematics;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

[Serializable]
public class SerializedUserData
{
    public int Version = 1;
    public SerializedGalaxyData GalaxyData;
    public SerializedInventoryData GlobalInventory = new SerializedInventoryData();
}
