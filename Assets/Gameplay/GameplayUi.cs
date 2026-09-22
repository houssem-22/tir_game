namespace Cipher.Gameplay
{
    /// <summary>
    /// Lightweight modal flag so movement / shooting pause while a UI (code pad) is open.
    /// </summary>
    public static class GameplayUi
    {
        static int _modalCount;

        public static bool IsModal => _modalCount > 0;

        public static void PushModal()
        {
            _modalCount++;
        }

        public static void PopModal()
        {
            if (_modalCount > 0) _modalCount--;
        }

        public static void Reset()
        {
            _modalCount = 0;
        }
    }
}
