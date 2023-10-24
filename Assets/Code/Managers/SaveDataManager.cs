using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SaveDataManager
{
    public const string SAVE_NAME = "savedata";

    public static SerializedUserData LoadData(string dataName)
    {
        var path = Path.Join(UnityEngine.Application.persistentDataPath, dataName + ".json");
        if (File.Exists(path))
        {
            var bytes = File.ReadAllBytes(path);
            var data = SerializationUtility.DeserializeValue<SerializedUserData>(bytes, DataFormat.JSON);
            UnityEngine.Debug.Log("loaded " + path);
            return data;
        }

        UnityEngine.Debug.LogError("no file at " + path);
        return null;
    }

    public static string SaveData(string dataName, SerializedUserData userData)
    {
        var bytes = SerializationUtility.SerializeValue(userData, DataFormat.JSON);
        var path = Path.Join(UnityEngine.Application.persistentDataPath, dataName + ".json");
        File.WriteAllBytes(path, bytes);

        UnityEngine.Debug.Log("Saved " + path);
        return path;
    }

    public static bool DoesDataExist(string dataName)
    {
        var path = Path.Join(UnityEngine.Application.persistentDataPath, dataName + ".json");
        if (File.Exists(path))
        {
            return true;
        }

        return false;
    }
}
