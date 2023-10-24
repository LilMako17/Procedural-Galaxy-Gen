using UnityEngine;

public class UserDataManager
{
    public static UserDataManager Instance 
    { 
        get
        {
            if (instance == null)
            {
                instance = new UserDataManager();
            }
            return instance;
        }
    }

    private static UserDataManager instance;

    public SerializedUserData Current { get; private set; }

    private bool _isDirty;
    private string _currentSaveFileName = SaveDataManager.SAVE_NAME;

    public StarNode GetStarNodeById(int id)
    {
        if (Current?.GalaxyData != null)
        {
            return Current.GalaxyData.StarMap[id];
        }

        return null;
    }

    public void CreateNew()
    {
        Current = new SerializedUserData();
    }

    public void SetSaveFileName(string saveFileName)
    {
        _currentSaveFileName = saveFileName;
    }

    public void Save()
    {
        _isDirty = false;
        SaveDataManager.SaveData(_currentSaveFileName, Current);
    }

    public void Load()
    {
        if (SaveDataManager.DoesDataExist(_currentSaveFileName))
        {
            Current = SaveDataManager.LoadData(_currentSaveFileName);
        }
        else
        {
            Current = null;
        }
    }

    public void SetDirty()
    {
        _isDirty = true;
    }

    public void Flush()
    {
        if (_isDirty)
        {
            Save();
        }
    }
}