using Cathei.Mathematics;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Numerics;

[Serializable]
public class SerializedInventoryData
{
    public Dictionary<string, Incremental> Inventory = new Dictionary<string, Incremental>();

    public Incremental GetAmount(string key)
    {
        if (Inventory.TryGetValue(key, out Incremental amount))
        {
            return amount;
        }

        return 0;
    }

    public void SetAmount(string key, Incremental value)
    {
        if (value < 0)
        {
            throw new Exception("value is not valid");
        }
        Inventory[key] = value;
    }

    public void AddDeltaAmount(string key, Incremental value)
    {
        if (Inventory.TryGetValue(key, out Incremental existing))
        {
            var newVal = existing + value;
            if (newVal < 0)
            {
                newVal = 0;
            }

            SetAmount(key, newVal);
        }
        else if (value > 0)
        {
            SetAmount(key, value);
        }
    }
}
