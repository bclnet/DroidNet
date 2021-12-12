using Gengine.CM;
using Gengine.Framework;
using Gengine.Framework.Async;
using Gengine.Render;
using Gengine.Sound;
using Gengine.UI;
using System;
using System.Runtime.CompilerServices;
//using GL_INDEX_TYPE = System.UInt32; // GL_UNSIGNED_INT
//using GlIndex = System.Int32;
[assembly: InternalsVisibleTo("Gengine.Framework")]
[assembly: InternalsVisibleTo("Gengine.Render")]
[assembly: InternalsVisibleTo("Gengine.Sound")]

namespace Gengine
{
    // https://github.com/WaveEngine/OpenGL.NET
    public unsafe static class Lib
    {
        public const string ENGINE_VERSION = "Doom3Quest 1.1.6";	// printed in console
        public const int BUILD_NUMBER = 1304;

        public static IUserInterfaceManager uiManager;
        public static ISoundSystem soundSystem;
        public static IRenderSystem renderSystem; // public static RenderSystemLocal tr; 
        public static IRenderModelManager renderModelManager;
        public static ImageManager globalImages = new();     // pointer to global list for the rest of the system
        public static DeclManager declManager;
        public static VertexCacheX vertexCache = new();
        public static ISession session;
        public static EventLoop eventLoop = new();
        public static ICollisionModelManager collisionModelManager;
        public static INetworkSystem networkSystem;


        public static IGame game;
        public static IGameEdit gameEdit;

        //: TODO-MOVE
        public static readonly BackEndState backEnd;

        public static string R_GetVidModeListString(bool addCustom) => throw new NotImplementedException();
        public static string R_GetVidModeValsString(bool addCustom) => throw new NotImplementedException();

        //: TODO-SET
        public static Action GL_CheckErrors;
        public static Action<int> GL_SelectTexture;
        public static void* R_StaticAlloc(int bytes) => throw new NotImplementedException();
        public static void R_StaticFree(byte* value) => throw new NotImplementedException();
        public static void R_WriteTGA(string name, byte* data, int width, int height, bool flag) => throw new NotImplementedException();
    }
}