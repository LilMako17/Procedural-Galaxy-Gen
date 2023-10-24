using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GalaxyStar : MonoBehaviour
{
    public int DataId { get; private set; }

    public void Init(int dataId)
    {
        DataId = dataId;
    }

    public StarNode GetData()
    {
        return GalaxyMap.Instance.StarData.StarMap[DataId];
    }

    public string GetStarTypeDisplayName(StarNode data)
    {
        if (data.AssetIndex == 0)
        {
            return "Yellow Star";
        }
        else if (data.AssetIndex == 1)
        {
            return "Blue Star";
        }
        else if (data.AssetIndex == 2)
        {
            return "Red Star";
        }

        return "";
    }

    private void OnMouseUpAsButton()
    {
        GalaxyMap.Instance.OnSelectStar(this);
    }
}
