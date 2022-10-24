interface ILuaLoader
{
    byte[] Load(string path, string extension);
    string LoadText(string path, string extension);
    bool Exists(string path, string extension);
}