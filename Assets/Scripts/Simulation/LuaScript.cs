using System.Reflection;
using NLua;
using UnityEngine;

namespace DLS.Simulation {
    public class LuaScript {
        static NLua.Lua env;
        public object[] Return {get; private set;}
        private readonly string code;
        public LuaScript(string code, bool Execute, SimChip chip) {
            if (Execute) Return = env.DoString(code);
            this.code = code;
            env["Chip"] = new Lua.Chip(chip);
        }

        public static void init() {
            env = new();
            env.LoadCLRPackage();
            env["PinState"] = new Lua.PinState() /* Why not */;
            env.RegisterFunction("print", typeof(Debug).GetMethod(
                "Log",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new[] {typeof(object)},
                null
            ));
            env.RegisterFunction("RegisterSimFunc", typeof(LuaScript).GetMethod(
                "RegSimFunc",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new[] {typeof(LuaFunction)},
                null
            ));
            env.RegisterFunction("error", typeof(Debug).GetMethod(
                "LogError",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new[] {typeof(object)},
                null
            ));
        }
        public void Execute() => Return = env.DoString(code);

        public static void RegSimFunc(LuaFunction func) => Simulator.queuedFuncs.Add(func);
    }
}