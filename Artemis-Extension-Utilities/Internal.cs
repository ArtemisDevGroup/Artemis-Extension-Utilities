namespace Artemis.Extension.Utilities
{
    internal static class Helpers
    {
        public static void Initialize(byte[] buffer, byte value)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = value;
            }
        }

        public static bool IsEmpty(byte[] array)
        {
            foreach (byte b in array)
                if (b != 0)
                    return false;
            return true;
        }
    }
}
