
using System.IO;

public class LuaLoaderEditorLua  : ILuaLoader
{
    string m_PathFormat;

    public LuaLoaderEditorLua()
    {
        m_PathFormat = ConfigInfo.editorLuaDir + "/{0}{1}";
    }
    public bool Exists(string path, string extension)
    {
        var luaPath = string.Format(m_PathFormat, path, extension);
        return File.Exists(luaPath);
    }

    public byte[] Load(string path, string extension)
    {
        var luaPath = string.Format(m_PathFormat, path, extension);
        try
        {
            return File.ReadAllBytes(luaPath);
        }
        catch
        {
            return null;
        }
    }

    public string LoadText(string path, string extension)
    {
        var luaPath = string.Format(m_PathFormat, path, extension);
        try
        {
            return File.ReadAllText(luaPath);
        }
        catch
        {
            return null;
        }
    }

}
