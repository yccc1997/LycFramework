

public static class ConfigInfo 
{
    public const string ASSETS_DIR = "Assets/";
    // Lua文件存放目录
    public const string luaDir = "Script/Lua";
    public static string editorLuaDir
    {
        get { return ASSETS_DIR + luaDir; }
    }
}
