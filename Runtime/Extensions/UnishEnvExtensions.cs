using UnityEngine;

namespace RUtil.Debug.Shell
{
    public static class UnishEnvExtensions
    {
        public static bool TrySet(this IUnishEnv env, string key, string value, UnishCommandArgType expectedType)
        {
            var arg = new UnishCommandArg(key, expectedType, value);
            if (arg.Type != expectedType)
            {
                return false;
            }

            env[key] = arg;
            return true;
        }

        public static void Set(this IUnishEnv env, string key, string value, UnishCommandArg defaultValue)
        {
            if (!env.TrySet(key, value, defaultValue.Type))
            {
                env[key] = defaultValue;
            }
        }

        public static void Set(this IUnishEnv env, string key, string value)
        {
            env[key] = new UnishCommandArg(key, value);
        }

        public static void Set(this IUnishEnv env, string key, bool value)
        {
            env[key] = new UnishCommandArg(key, value);
        }

        public static void Set(this IUnishEnv env, string key, int value)
        {
            env[key] = new UnishCommandArg(key, value);
        }

        public static void Set(this IUnishEnv env, string key, float value)
        {
            env[key] = new UnishCommandArg(key, value);
        }

        public static void Set(this IUnishEnv env, string key, Vector2 value)
        {
            env[key] = new UnishCommandArg(key, value);
        }

        public static void Set(this IUnishEnv env, string key, Vector3 value)
        {
            env[key] = new UnishCommandArg(key, value);
        }

        public static void Set(this IUnishEnv env, string key, Color value)
        {
            env[key] = new UnishCommandArg(key, value);
        }


        public static bool TryGetValue(this IUnishEnv env, string key, UnishCommandArgType type, out UnishCommandArg value)
        {
            return env.TryGetValue(key, out value) && value.Type == type;
        }

        public static bool TryGet(this IUnishEnv env, string key, out string value)
        {
            if (!env.TryGetValue(key, UnishCommandArgType.String, out var tmp))
            {
                value = default;
                return false;
            }

            value = tmp.s;
            return true;
        }

        public static bool TryGet(this IUnishEnv env, string key, out bool value)
        {
            if (!env.TryGetValue(key, UnishCommandArgType.Bool, out var tmp))
            {
                value = default;
                return false;
            }

            value = tmp.b;
            return true;
        }

        public static bool TryGet(this IUnishEnv env, string key, out int value)
        {
            if (!env.TryGetValue(key, UnishCommandArgType.Int, out var tmp))
            {
                value = default;
                return false;
            }

            value = tmp.i;
            return true;
        }

        public static bool TryGet(this IUnishEnv env, string key, out float value)
        {
            if (!env.TryGetValue(key, UnishCommandArgType.Float, out var tmp))
            {
                value = default;
                return false;
            }

            value = tmp.f;
            return true;
        }

        public static bool TryGet(this IUnishEnv env, string key, out Vector2 value)
        {
            if (!env.TryGetValue(key, UnishCommandArgType.Vector2, out var tmp))
            {
                value = default;
                return false;
            }

            value = tmp.v;
            return true;
        }

        public static bool TryGet(this IUnishEnv env, string key, out Vector3 value)
        {
            if (!env.TryGetValue(key, UnishCommandArgType.Vector3, out var tmp))
            {
                value = default;
                return false;
            }

            value = tmp.v3;
            return true;
        }

        public static bool TryGet(this IUnishEnv env, string key, out Color value)
        {
            if (!env.TryGetValue(key, UnishCommandArgType.Color, out var tmp))
            {
                value = default;
                return false;
            }

            value = tmp.c;
            return true;
        }
    }
}
