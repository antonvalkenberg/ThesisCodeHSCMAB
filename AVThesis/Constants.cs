/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
/// </summary>
namespace AVThesis {

    /// <summary>
    /// Collection of constants used throughout the AVThesis namespace.
    /// </summary>
    public static class Constants {

        /// <summary>
        /// The base prime number we use for calculating hash codes.
        /// See: http://www.isthe.com/chongo/tech/comp/fnv/#FNV-1
        /// </summary>
        public const uint HASH_OFFSET_BASIS = 2166136261;

        /// <summary>
        /// The prime number we use for calculating hash codes.
        /// See: http://www.isthe.com/chongo/tech/comp/fnv/#FNV-1
        /// </summary>
        public const int HASH_FNV_PRIME = 16777619;

        /// <summary>
        /// The tolerance to allow when checking floating-point equality.
        /// </summary>
        public const double DOUBLE_EQUALITY_TOLERANCE = 0.0000001;

        /// <summary>
        /// The name of `Player1' in the SabberStone game config.
        /// </summary>
        public const string SABBERSTONE_GAMECONFIG_PLAYER1_NAME = "Player1";

        /// <summary>
        /// The name of `Player2' in the SabberStone game config.
        /// </summary>
        public const string SABBERSTONE_GAMECONFIG_PLAYER2_NAME = "Player2";

    }
}
