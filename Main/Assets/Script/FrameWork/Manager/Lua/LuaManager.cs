using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class LuaManager : GameMgrBase<LuaManager>
{
    LuaEnv m_Lua = null;
    //[LuaCallCSharp]
    //public Action<float, float> updateWithTime;
    protected override void Init() {
        InitLuaEnv();
    }

    public LuaEnv GetLuaEnv()
    {
        if (m_Lua==null)
        {
            InitLuaEnv();
        }
        return m_Lua;
    }
    private void InitLuaEnv()
    {
        Dispose();
        m_Lua = new LuaEnv();
        ILuaLoader m_Loader = new LuaLoaderEditorLua();
        m_Lua.AddLoader((ref string filepath) =>
        {
            return m_Loader.Load(filepath, ".lua");
        });
        m_Lua.DoString("require 'Main'");

    }
    public  void Dispose()
    {
        
        if (m_Lua != null)
        {
            m_Lua.Dispose();
            m_Lua = null; 
        }
    }

    private void Update()
    {
        if (m_Lua != null)
        {
            m_Lua.Tick();
        }
      //  updateWithTime?.Invoke(Time.time, Time.unscaledTime);
    }
}

