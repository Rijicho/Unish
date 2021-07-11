using UnityEngine;

namespace RUtil.Debug.Shell
{
    public static class UnishEnvExtensions
    {
        public static bool TrySet(this IUnishEnv env, string key, string value, UnishVariableType expectedType)
        {
            var arg = new UnishVariable(key, expectedType, value);
            if (arg.Type != expectedType)
            {
                return false;
            }

            env[key] = arg;
            return true;
        }

        public static void Set(this IUnishEnv env, string key, string value, UnishVariable defaultValue)
        {
            if (!env.TrySet(key, value, defaultValue.Type))
            {
                env[key] = defaultValue;
            }
        }

        public static void Set(this IUnishEnv env, string key, string value)
        {
            env[key] = new UnishVariable(key, value);
        }

        public static void Set(this IUnishEnv env, string key, bool value)
        {
            env[key] = new UnishVariable(key, value);
        }

        public static void Set(this IUnishEnv env, string key, int value)
        {
            env[key] = new UnishVariable(key, value);
        }

        public static void Set(this IUnishEnv env, string key, float value)
        {
            env[key] = new UnishVariable(key, value);
        }

        public static void Set(this IUnishEnv env, string key, Vector2 value)
        {
            env[key] = new UnishVariable(key, value);
        }

        public static void Set(this IUnishEnv env, string key, Vector3 value)
        {
            env[key] = new UnishVariable(key, value);
        }

        public static void Set(this IUnishEnv env, string key, Color value)
        {
            env[key] = new UnishVariable(key, value);
        }


        public static bool TryGetValue(this IUnishEnv env, string key, UnishVariableType type, out UnishVariable value)
        {
            return env.TryGetValue(key, out value) && value.Type == type;
        }

        public static bool TryGet(this IUnishEnv env, string key, out string value)
        {
            if (!env.TryGetValue(key, UnishVariableType.String, out var tmp))
            {
                value = default;
                return false;
            }

            value = tmp.S;
            return true;
        }

        public static bool TryGet(this IUnishEnv env, string key, out bool value)
        {
            if (!env.TryGetValue(key, UnishVariableType.Bool, out var tmp))
            {
                value = default;
                return false;
            }

            value = tmp.B;
            return true;
        }

        public static bool TryGet(this IUnishEnv env, string key, out int value)
        {
            if (!env.TryGetValue(key, UnishVariableType.Int, out var tmp))
            {
                value = default;
                return false;
            }

            value = tmp.I;
            return true;
        }

        public static bool TryGet(this IUnishEnv env, string key, out float value)
        {
            if (!env.TryGetValue(key, UnishVariableType.Float, out var tmp))
            {
                value = default;
                return false;
            }

            value = tmp.F;
            return true;
        }

        public static bool TryGet(this IUnishEnv env, string key, out Vector2 value)
        {
            if (!env.TryGetValue(key, UnishVariableType.Vector2, out var tmp))
            {
                value = default;
                return false;
            }

            value = tmp.V2;
            return true;
        }

        public static bool TryGet(this IUnishEnv env, string key, out Vector3 value)
        {
            if (!env.TryGetValue(key, UnishVariableType.Vector3, out var tmp))
            {
                value = default;
                return false;
            }

            value = tmp.V3;
            return true;
        }

        public static bool TryGet(this IUnishEnv env, string key, out Color value)
        {
            if (!env.TryGetValue(key, UnishVariableType.Color, out var tmp))
            {
                value = default;
                return false;
            }

            value = tmp.C;
            return true;
        }
    }
}
