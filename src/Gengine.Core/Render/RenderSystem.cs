using Gengine.Framework;
using System;
using System.NumericsX;
using System.NumericsX.OpenStack;
using GlIndex = System.Int32;

namespace Gengine.Render
{
    // Contains variables specific to the OpenGL configuration being run right now. These are constant once the OpenGL subsystem is initialized.
    public class Glconfig
    {
        public string renderer_string;
        public string vendor_string;
        public string version_string;
        public string extensions_string;

        public int maxTextureSize;          // queried from GL
        public int maxTextureUnits;
        public float maxTextureAnisotropy;

        public int colorBits, depthBits, stencilBits;

        public bool anisotropicAvailable;

        public bool npotAvailable;

        public bool depthStencilAvailable;

        public int vidWidth, vidHeight; // passed to R_BeginFrame

        public int vidWidthReal, vidHeightReal; // The real resolution of the screen, uses framebuffer if not the same as vidWidth

        public int displayFrequency;

        public bool isFullscreen;

        public bool isInitialized;
    }

    public struct GlyphInfo
    {
        public int height;          // number of scan lines
        public int top;         // top of glyph in buffer
        public int bottom;          // bottom of glyph in buffer
        public int pitch;           // width for copying
        public int xSkip;           // x adjustment
        public int imageWidth;      // width of actual image
        public int imageHeight; // height of actual image
        public float s;             // x offset in image where glyph starts
        public float t;             // y offset in image where glyph starts
        public float s2;
        public float t2;
        public Material glyph;          // shader with the glyph
        public string shaderName;
    }

    public class FontInfo
    {
        public GlyphInfo[] glyphs = new GlyphInfo[R.GLYPHS_PER_FONT];
        public float glyphScale;
        public string name;
    }

    public class FontInfoEx
    {
        public FontInfo fontInfoSmall;
        public FontInfo fontInfoMedium;
        public FontInfo fontInfoLarge;
        public int maxHeight;
        public int maxWidth;
        public int maxHeightSmall;
        public int maxWidthSmall;
        public int maxHeightMedium;
        public int maxWidthMedium;
        public int maxHeightLarge;
        public int maxWidthLarge;
        public string name;
    }

    public static partial class R
    {
        // font support
        public const int GLYPH_START = 0;
        public const int GLYPH_END = 255;
        public const int GLYPH_CHARSTART = 32;
        public const int GLYPH_CHAREND = 127;
        public const int GLYPHS_PER_FONT = GLYPH_END - GLYPH_START + 1;

        public const int SMALLCHAR_WIDTH = 8;
        public const int SMALLCHAR_HEIGHT = 16;
        public const int BIGCHAR_WIDTH = 16;
        public const int BIGCHAR_HEIGHT = 16;

        // all drawing is done to a 640 x 480 virtual screen size and will be automatically scaled to the real resolution
        public const int SCREEN_WIDTH = 640;
        public const int SCREEN_HEIGHT = 480;

        // functions mainly intended for editor and dmap integration

        // returns the frustum planes in world space
        public static void RenderLightFrustum(RenderLight renderLight, Plane[] lightFrustum) => throw new NotImplementedException();

        // for use by dmap to do the carving-on-light-boundaries and for the editor for display
        public static void LightProjectionMatrix(Vector3 origin, Plane rearPlane, Vector4[] mat) => throw new NotImplementedException();

        // used by the view shot taker
        public static void ScreenshotFilename(ref int lastNumber, string base_, out string fileName) => throw new NotImplementedException();

        public static Glconfig glConfig = new();

        public static CVar r_useLightPortalFlow = new("r_useLightPortalFlow", "1", CVAR.RENDERER | CVAR.BOOL, "use a more precise area reference determination");
        public static CVar r_multiSamples = new("r_multiSamples", "0", CVAR.RENDERER | CVAR.ARCHIVE | CVAR.INTEGER, "number of antialiasing samples");
        public static CVar r_mode = new("r_mode", "3", CVAR.ARCHIVE | CVAR.RENDERER | CVAR.INTEGER, "video mode number");
        public static CVar r_displayRefresh = new("r_displayRefresh", "0", CVAR.RENDERER | CVAR.INTEGER | CVAR.NOCHEAT, "optional display refresh rate option for vid mode", 0f, 200f);
        public static CVar r_fullscreen = new("r_fullscreen", "0", CVAR.RENDERER | CVAR.ARCHIVE | CVAR.BOOL | CVAR.ROM, "0 = windowed, 1 = full screen");
        public static CVar r_customWidth = new("r_customWidth", "720", CVAR.RENDERER | CVAR.ARCHIVE | CVAR.INTEGER, "custom screen width. set r_mode to -1 to activate");
        public static CVar r_customHeight = new("r_customHeight", "486", CVAR.RENDERER | CVAR.ARCHIVE | CVAR.INTEGER, "custom screen height. set r_mode to -1 to activate");
        public static CVar r_checkBounds = new("r_checkBounds", "0", CVAR.RENDERER | CVAR.BOOL, "compare all surface bounds with precalculated ones");

        public static CVar r_usePhong = new("r_usePhong", "1", CVAR.RENDERER | CVAR.BOOL, "use phong instead of blinn-phong shader for interactions");
        public static CVar r_specularExponent = new("r_specularExponent", "3", CVAR.RENDERER | CVAR.FLOAT, "specular exponent, to be used in GLSL shaders");
        public static CVar r_useConstantMaterials = new("r_useConstantMaterials", "1", CVAR.RENDERER | CVAR.BOOL, "use pre-calculated material registers if possible");
        public static CVar r_useSilRemap = new("r_useSilRemap", "1", CVAR.RENDERER | CVAR.BOOL, "consider verts with the same XYZ, but different ST the same for shadows");
        public static CVar r_useNodeCommonChildren = new("r_useNodeCommonChildren", "1", CVAR.RENDERER | CVAR.BOOL, "stop pushing reference bounds early when possible");
        public static CVar r_useShadowProjectedCull = new("r_useShadowProjectedCull", "1", CVAR.RENDERER | CVAR.BOOL, "discard triangles outside light volume before shadowing");
        public static CVar r_useShadowSurfaceScissor = new("r_useShadowSurfaceScissor", "1", CVAR.RENDERER | CVAR.BOOL, "scissor shadows by the scissor rect of the interaction surfaces");
        public static CVar r_useInteractionTable = new("r_useInteractionTable", "1", CVAR.RENDERER | CVAR.BOOL, "create a full entityDefs * lightDefs table to make finding interactions faster");
        public static CVar r_useTurboShadow = new("r_useTurboShadow", "1", CVAR.RENDERER | CVAR.BOOL, "use the infinite projection with W technique for dynamic shadows");
        public static CVar r_useDeferredTangents = new("r_useDeferredTangents", "1", CVAR.RENDERER | CVAR.BOOL, "defer tangents calculations after deform");
        public static CVar r_useCachedDynamicModels = new("r_useCachedDynamicModels", "1", CVAR.RENDERER | CVAR.BOOL, "cache snapshots of dynamic models");
        public static CVar r_useStateCaching = new("r_useStateCaching", "1", CVAR.RENDERER | CVAR.BOOL, "avoid redundant state changes in GL_*() calls");
        public static CVar r_useInfiniteFarZ = new("r_useInfiniteFarZ", "1", CVAR.RENDERER | CVAR.BOOL, "use the no-far-clip-plane trick");

        public static CVar r_znear = new("r_znear", "3", CVAR.RENDERER | CVAR.FLOAT, "near Z clip plane distance", 0.001f, 200f);

        public static CVar r_ignoreGLErrors = new("r_ignoreGLErrors", "1", CVAR.RENDERER | CVAR.BOOL, "ignore GL errors");
        public static CVar r_finish = new("r_finish", "0", CVAR.RENDERER | CVAR.BOOL, "force a call to glFinish() every frame");
        public static CVar r_swapInterval = new("r_swapInterval", "1", CVAR.RENDERER | CVAR.ARCHIVE | CVAR.INTEGER, "changes the GL swap interval");

        public static CVar r_gamma = new("r_gamma", "1", CVAR.RENDERER | CVAR.ARCHIVE | CVAR.FLOAT, "changes gamma tables", 0.5f, 3f);
        public static CVar r_brightness = new("r_brightness", "1", CVAR.RENDERER | CVAR.ARCHIVE | CVAR.FLOAT, "changes gamma tables", 0.5f, 3f);

        public static CVar r_jitter = new("r_jitter", "0", CVAR.RENDERER | CVAR.BOOL, "randomly subpixel jitter the projection matrix");

        public static CVar r_skipSuppress = new("r_skipSuppress", "0", CVAR.RENDERER | CVAR.BOOL, "ignore the per-view suppressions");
        public static CVar r_skipPostProcess = new("r_skipPostProcess", "0", CVAR.RENDERER | CVAR.BOOL, "skip all post-process renderings");
        public static CVar r_skipInteractions = new("r_skipInteractions", "0", CVAR.RENDERER | CVAR.BOOL, "skip all light/surface interaction drawing");
        public static CVar r_skipDynamicTextures = new("r_skipDynamicTextures", "0", CVAR.RENDERER | CVAR.BOOL, "don't dynamically create textures");
        public static CVar r_skipCopyTexture = new("r_skipCopyTexture", "0", CVAR.RENDERER | CVAR.BOOL, "do all rendering, but don't actually copyTexSubImage2D");
        public static CVar r_skipBackEnd = new("r_skipBackEnd", "0", CVAR.RENDERER | CVAR.BOOL, "don't draw anything");
        public static CVar r_skipRender = new("r_skipRender", "0", CVAR.RENDERER | CVAR.BOOL, "skip 3D rendering, but pass 2D");
        public static CVar r_skipTranslucent = new("r_skipTranslucent", "0", CVAR.RENDERER | CVAR.BOOL, "skip the translucent interaction rendering");
        public static CVar r_skipAmbient = new("r_skipAmbient", "0", CVAR.RENDERER | CVAR.BOOL, "bypasses all non-interaction drawing");
        public static CVar r_skipNewAmbient = new("r_skipNewAmbient", "0", CVAR.RENDERER | CVAR.BOOL | CVAR.ARCHIVE, "bypasses all vertex/fragment program ambient drawing");
        public static CVar r_skipBlendLights = new("r_skipBlendLights", "0", CVAR.RENDERER | CVAR.BOOL, "skip all blend lights");
        public static CVar r_skipFogLights = new("r_skipFogLights", "0", CVAR.RENDERER | CVAR.BOOL, "skip all fog lights");
        public static CVar r_skipDeforms = new("r_skipDeforms", "0", CVAR.RENDERER | CVAR.BOOL, "leave all deform materials in their original state");
        public static CVar r_skipFrontEnd = new("r_skipFrontEnd", "0", CVAR.RENDERER | CVAR.BOOL, "bypasses all front end work, but 2D gui rendering still draws");
        public static CVar r_skipUpdates = new("r_skipUpdates", "0", CVAR.RENDERER | CVAR.BOOL, "1 = don't accept any entity or light updates, making everything static");
        public static CVar r_skipOverlays = new("r_skipOverlays", "0", CVAR.RENDERER | CVAR.BOOL, "skip overlay surfaces");
        public static CVar r_skipSpecular = new("r_skipSpecular", "0", CVAR.RENDERER | CVAR.BOOL | CVAR.CHEAT | CVAR.ARCHIVE, "use black for specular1");
        public static CVar r_skipBump = new("r_skipBump", "0", CVAR.RENDERER | CVAR.BOOL | CVAR.ARCHIVE, "uses a flat surface instead of the bump map");
        public static CVar r_skipDiffuse = new("r_skipDiffuse", "0", CVAR.RENDERER | CVAR.BOOL, "use black for diffuse");
        public static CVar r_skipROQ = new("r_skipROQ", "0", CVAR.RENDERER | CVAR.BOOL, "skip ROQ decoding");

        public static CVar r_ignore = new("r_ignore", "0", CVAR.RENDERER, "used for random debugging without defining new vars");
        public static CVar r_ignore2 = new("r_ignore2", "0", CVAR.RENDERER, "used for random debugging without defining new vars");
        public static CVar r_usePreciseTriangleInteractions = new("r_usePreciseTriangleInteractions", "0", CVAR.RENDERER | CVAR.BOOL, "1 = do winding clipping to determine if each ambiguous tri should be lit");
        public static CVar r_useCulling = new("r_useCulling", "2", CVAR.RENDERER | CVAR.INTEGER, "0 = none, 1 = sphere, 2 = sphere + box", 0, 2, CmdArgs.ArgCompletion_Integer(0, 2));
        public static CVar r_useLightCulling = new("r_useLightCulling", "3", CVAR.RENDERER | CVAR.INTEGER, "0 = none, 1 = box, 2 = exact clip of polyhedron faces, 3 = also areas", 0, 3, CmdArgs.ArgCompletion_Integer(0, 3));
        public static CVar r_useLightScissors = new("r_useLightScissors", "1", CVAR.RENDERER | CVAR.BOOL, "1 = use custom scissor rectangle for each light");
        public static CVar r_useClippedLightScissors = new("r_useClippedLightScissors", "1", CVAR.RENDERER | CVAR.INTEGER, "0 = full screen when near clipped, 1 = exact when near clipped, 2 = exact always", 0, 2, CmdArgs.ArgCompletion_Integer(0, 2));
        public static CVar r_useEntityCulling = new("r_useEntityCulling", "1", CVAR.RENDERER | CVAR.BOOL, "0 = none, 1 = box");
        public static CVar r_useEntityScissors = new("r_useEntityScissors", "0", CVAR.RENDERER | CVAR.BOOL, "1 = use custom scissor rectangle for each entity");
        public static CVar r_useInteractionCulling = new("r_useInteractionCulling", "1", CVAR.RENDERER | CVAR.BOOL, "1 = cull interactions");
        public static CVar r_useInteractionScissors = new("r_useInteractionScissors", "2", CVAR.RENDERER | CVAR.INTEGER, "1 = use a custom scissor rectangle for each shadow interaction, 2 = also crop using portal scissors", -2, 2, CmdArgs.ArgCompletion_Integer(-2, 2));
        public static CVar r_useShadowCulling = new("r_useShadowCulling", "1", CVAR.RENDERER | CVAR.BOOL, "try to cull shadows from partially visible lights");
        public static CVar r_useFrustumFarDistance = new("r_useFrustumFarDistance", "0", CVAR.RENDERER | CVAR.FLOAT, "if != 0 force the view frustum far distance to this distance");
        public static CVar r_clear = new("r_clear", "2", CVAR.RENDERER, "force screen clear every frame, 1 = purple, 2 = black, 'r g b' = custom");
        public static CVar r_offsetFactor = new("r_offsetfactor", "0", CVAR.RENDERER | CVAR.FLOAT, "polygon offset parameter");
        public static CVar r_offsetUnits = new("r_offsetunits", "-600", CVAR.RENDERER | CVAR.FLOAT, "polygon offset parameter");
        public static CVar r_shadowPolygonOffset = new("r_shadowPolygonOffset", "-1", CVAR.RENDERER | CVAR.FLOAT, "bias value added to depth test for stencil shadow drawing");
        public static CVar r_shadowPolygonFactor = new("r_shadowPolygonFactor", "0", CVAR.RENDERER | CVAR.FLOAT, "scale value for stencil shadow drawing");
        public static CVar r_skipSubviews = new("r_skipSubviews", "0", CVAR.RENDERER | CVAR.INTEGER, "1 = don't render any gui elements on surfaces");
        public static CVar r_skipGuiShaders = new("r_skipGuiShaders", "0", CVAR.RENDERER | CVAR.INTEGER, "1 = skip all gui elements on surfaces, 2 = skip drawing but still handle events, 3 = draw but skip events", 0, 3, CmdArgs.ArgCompletion_Integer(0, 3));
        public static CVar r_skipParticles = new("r_skipParticles", "0", CVAR.RENDERER | CVAR.INTEGER, "1 = skip all particle systems", 0, 1, CmdArgs.ArgCompletion_Integer(0, 1));
        public static CVar r_subviewOnly = new("r_subviewOnly", "0", CVAR.RENDERER | CVAR.BOOL, "1 = don't render main view, allowing subviews to be debugged");
        public static CVar r_shadows = new("r_shadows", "0", CVAR.RENDERER | CVAR.BOOL | CVAR.ARCHIVE, "enable shadows");
        public static CVar r_testGamma = new("r_testGamma", "0", CVAR.RENDERER | CVAR.FLOAT, "if > 0 draw a grid pattern to test gamma levels", 0, 195);
        public static CVar r_testGammaBias = new("r_testGammaBias", "0", CVAR.RENDERER | CVAR.FLOAT, "if > 0 draw a grid pattern to test gamma levels");
        public static CVar r_testStepGamma = new("r_testStepGamma", "0", CVAR.RENDERER | CVAR.FLOAT, "if > 0 draw a grid pattern to test gamma levels");
        public static CVar r_lightScale = new("r_lightScale", "2", CVAR.RENDERER | CVAR.FLOAT, "all light intensities are multiplied by this");
        public static CVar r_lightSourceRadius = new("r_lightSourceRadius", "0", CVAR.RENDERER | CVAR.FLOAT, "for soft-shadow sampling");
        public static CVar r_flareSize = new("r_flareSize", "1", CVAR.RENDERER | CVAR.FLOAT, "scale the flare deforms from the material def");

        public static CVar r_useExternalShadows = new("r_useExternalShadows", "1", CVAR.RENDERER | CVAR.INTEGER, "1 = skip drawing caps when outside the light volume, 2 = force to no caps for testing", 0, 2, CmdArgs.ArgCompletion_Integer(0, 2));
        public static CVar r_useOptimizedShadows = new("r_useOptimizedShadows", "1", CVAR.RENDERER | CVAR.BOOL, "use the dmap generated static shadow volumes");
        public static CVar r_useScissor = new("r_useScissor", "1", CVAR.RENDERER | CVAR.BOOL, "scissor clip as portals and lights are processed");

        public static CVar r_screenFraction = new("r_screenFraction", "100", CVAR.RENDERER | CVAR.INTEGER, "for testing fill rate, the resolution of the entire screen can be changed");
        public static CVar r_demonstrateBug = new("r_demonstrateBug", "0", CVAR.RENDERER | CVAR.BOOL, "used during development to show IHV's their problems");
        public static CVar r_usePortals = new("r_usePortals", "1", CVAR.RENDERER | CVAR.BOOL, " 1 = use portals to perform area culling, otherwise draw everything");
        public static CVar r_singleLight = new("r_singleLight", "-1", CVAR.RENDERER | CVAR.INTEGER, "suppress all but one light");
        public static CVar r_singleEntity = new("r_singleEntity", "-1", CVAR.RENDERER | CVAR.INTEGER, "suppress all but one entity");
        public static CVar r_singleSurface = new("r_singleSurface", "-1", CVAR.RENDERER | CVAR.INTEGER, "suppress all but one surface on each entity");
        public static CVar r_singleArea = new("r_singleArea", "0", CVAR.RENDERER | CVAR.BOOL, "only draw the portal area the view is actually in");
        public static CVar r_forceLoadImages = new("r_forceLoadImages", "0", CVAR.RENDERER | CVAR.ARCHIVE | CVAR.BOOL, "draw all images to screen after registration");
        public static CVar r_orderIndexes = new("r_orderIndexes", "1", CVAR.RENDERER | CVAR.BOOL, "perform index reorganization to optimize vertex use");
        public static CVar r_lightAllBackFaces = new("r_lightAllBackFaces", "0", CVAR.RENDERER | CVAR.BOOL, "light all the back faces, even when they would be shadowed");

        // visual debugging info
        public static CVar r_showPortals = new("r_showPortals", "0", CVAR.RENDERER | CVAR.BOOL, "draw portal outlines in color based on passed / not passed");
        public static CVar r_showUnsmoothedTangents = new("r_showUnsmoothedTangents", "0", CVAR.RENDERER | CVAR.BOOL, "if 1, put all nvidia register combiner programming in display lists");
        public static CVar r_showSilhouette = new("r_showSilhouette", "0", CVAR.RENDERER | CVAR.BOOL, "highlight edges that are casting shadow planes");
        public static CVar r_showVertexColor = new("r_showVertexColor", "0", CVAR.RENDERER | CVAR.BOOL, "draws all triangles with the solid vertex color");
        public static CVar r_showUpdates = new("r_showUpdates", "0", CVAR.RENDERER | CVAR.BOOL, "report entity and light updates and ref counts");
        public static CVar r_showDemo = new("r_showDemo", "0", CVAR.RENDERER | CVAR.BOOL, "report reads and writes to the demo file");
        public static CVar r_showDynamic = new("r_showDynamic", "0", CVAR.RENDERER | CVAR.BOOL, "report stats on dynamic surface generation");
        public static CVar r_showDefs = new("r_showDefs", "0", CVAR.RENDERER | CVAR.BOOL, "report the number of modeDefs and lightDefs in view");
        public static CVar r_showIntensity = new("r_showIntensity", "0", CVAR.RENDERER | CVAR.BOOL, "draw the screen colors based on intensity, red = 0, green = 128, blue = 255");
        public static CVar r_showLights = new("r_showLights", "0", CVAR.RENDERER | CVAR.INTEGER, "1 = just print volumes numbers, highlighting ones covering the view, 2 = also draw planes of each volume, 3 = also draw edges of each volume", 0, 3, CmdArgs.ArgCompletion_Integer(0, 3));
        public static CVar r_showShadowCount = new("r_showShadowCount", "0", CVAR.RENDERER | CVAR.INTEGER, "colors screen based on shadow volume depth complexity, >= 2 = print overdraw count based on stencil index values, 3 = only show turboshadows, 4 = only show static shadows", 0, 4, CmdArgs.ArgCompletion_Integer(0, 4));
        public static CVar r_showLightScissors = new("r_showLightScissors", "0", CVAR.RENDERER | CVAR.BOOL, "show light scissor rectangles");
        public static CVar r_showEntityScissors = new("r_showEntityScissors", "0", CVAR.RENDERER | CVAR.BOOL, "show entity scissor rectangles");
        public static CVar r_showInteractionFrustums = new("r_showInteractionFrustums", "0", CVAR.RENDERER | CVAR.INTEGER, "1 = show a frustum for each interaction, 2 = also draw lines to light origin, 3 = also draw entity bbox", 0, 3, CmdArgs.ArgCompletion_Integer(0, 3));
        public static CVar r_showInteractionScissors = new("r_showInteractionScissors", "0", CVAR.RENDERER | CVAR.INTEGER, "1 = show screen rectangle which contains the interaction frustum, 2 = also draw construction lines", 0, 2, CmdArgs.ArgCompletion_Integer(0, 2));
        public static CVar r_showLightCount = new("r_showLightCount", "0", CVAR.RENDERER | CVAR.INTEGER, "1 = colors surfaces based on light count, 2 = also count everything through walls, 3 = also print overdraw", 0, 3, CmdArgs.ArgCompletion_Integer(0, 3));
        public static CVar r_showViewEntitys = new("r_showViewEntitys", "0", CVAR.RENDERER | CVAR.INTEGER, "1 = displays the bounding boxes of all view models, 2 = print index numbers");
        public static CVar r_showTris = new("r_showTris", "0", CVAR.RENDERER | CVAR.INTEGER, "enables wireframe rendering of the world, 1 = only draw visible ones, 2 = draw all front facing, 3 = draw all", 0, 3, CmdArgs.ArgCompletion_Integer(0, 3));
        public static CVar r_showSurfaceInfo = new("r_showSurfaceInfo", "0", CVAR.RENDERER | CVAR.BOOL, "show surface material name under crosshair");
        public static CVar r_showNormals = new("r_showNormals", "0", CVAR.RENDERER | CVAR.FLOAT, "draws wireframe normals");
        public static CVar r_showMemory = new("r_showMemory", "0", CVAR.RENDERER | CVAR.BOOL, "print frame memory utilization");
        public static CVar r_showCull = new("r_showCull", "0", CVAR.RENDERER | CVAR.BOOL, "report sphere and box culling stats");
        public static CVar r_showInteractions = new("r_showInteractions", "0", CVAR.RENDERER | CVAR.BOOL, "report interaction generation activity");
        public static CVar r_showDepth = new("r_showDepth", "0", CVAR.RENDERER | CVAR.BOOL, "display the contents of the depth buffer and the depth range");
        public static CVar r_showSurfaces = new("r_showSurfaces", "0", CVAR.RENDERER | CVAR.BOOL, "report surface/light/shadow counts");
        public static CVar r_showPrimitives = new("r_showPrimitives", "0", CVAR.RENDERER | CVAR.INTEGER, "report drawsurf/index/vertex counts");
        public static CVar r_showEdges = new("r_showEdges", "0", CVAR.RENDERER | CVAR.BOOL, "draw the sil edges");
        public static CVar r_showTexturePolarity = new("r_showTexturePolarity", "0", CVAR.RENDERER | CVAR.BOOL, "shade triangles by texture area polarity");
        public static CVar r_showTangentSpace = new("r_showTangentSpace", "0", CVAR.RENDERER | CVAR.INTEGER, "shade triangles by tangent space, 1 = use 1st tangent vector, 2 = use 2nd tangent vector, 3 = use normal vector", 0, 3, CmdArgs.ArgCompletion_Integer(0, 3));
        public static CVar r_showDominantTri = new("r_showDominantTri", "0", CVAR.RENDERER | CVAR.BOOL, "draw lines from vertexes to center of dominant triangles");
        public static CVar r_showAlloc = new("r_showAlloc", "0", CVAR.RENDERER | CVAR.BOOL, "report alloc/free counts");
        public static CVar r_showTextureVectors = new("r_showTextureVectors", "0", CVAR.RENDERER | CVAR.FLOAT, " if > 0 draw each triangles texture (tangent) vectors");

        public static CVar r_lockSurfaces = new("r_lockSurfaces", "0", CVAR.RENDERER | CVAR.BOOL, "allow moving the view point without changing the composition of the scene, including culling");
        public static CVar r_useEntityCallbacks = new("r_useEntityCallbacks", "1", CVAR.RENDERER | CVAR.BOOL, "if 0, issue the callback immediately at update time, rather than defering");

        public static CVar r_showSkel = new("r_showSkel", "0", CVAR.RENDERER | CVAR.INTEGER, "draw the skeleton when model animates, 1 = draw model with skeleton, 2 = draw skeleton only", 0, 2, CmdArgs.ArgCompletion_Integer(0, 2));
        public static CVar r_jointNameScale = new("r_jointNameScale", "0.02", CVAR.RENDERER | CVAR.FLOAT, "size of joint names when r_showskel is set to 1");
        public static CVar r_jointNameOffset = new("r_jointNameOffset", "0.5", CVAR.RENDERER | CVAR.FLOAT, "offset of joint names when r_showskel is set to 1");

        public static CVar r_debugLineDepthTest = new("r_debugLineDepthTest", "0", CVAR.RENDERER | CVAR.ARCHIVE | CVAR.BOOL, "perform depth test on debug lines");
        public static CVar r_debugLineWidth = new("r_debugLineWidth", "1", CVAR.RENDERER | CVAR.ARCHIVE | CVAR.BOOL, "width of debug lines");
        public static CVar r_debugArrowStep = new("r_debugArrowStep", "120", CVAR.RENDERER | CVAR.ARCHIVE | CVAR.INTEGER, "step size of arrow cone line rotation in degrees", 0, 120);
        public static CVar r_debugPolygonFilled = new("r_debugPolygonFilled", "1", CVAR.RENDERER | CVAR.BOOL, "draw a filled polygon");

        //public static CVar r_materialOverride = new("r_materialOverride", "", CVAR.RENDERER, "overrides all materials", CmdArgs.ArgCompletion_Decl<DECL_MATERIAL>);

        public static CVar r_debugRenderToTexture = new("r_debugRenderToTexture", "0", CVAR.RENDERER | CVAR.INTEGER, "");

        // DG: let users disable the "scale menus to 4:3" hack
        public static CVar r_scaleMenusTo43 = new("r_scaleMenusTo43", "1", CVAR.RENDERER | CVAR.ARCHIVE | CVAR.BOOL, "Scale menus, fullscreen videos and PDA to 4:3 aspect ratio");

        public static CVar r_multithread = new("r_multithread", "1", CVAR.RENDERER | CVAR.BOOL, "Multithread backend");

        public static CVar r_noLight = new("r_noLight", "0", CVAR.RENDERER | CVAR.BOOL, "lighting disable hack");
        public static CVar r_useETC1 = new("r_useETC1", "0", CVAR.RENDERER | CVAR.BOOL, "use ETC1 compression");
        public static CVar r_useETC1Cache = new("r_useETC1cache", "0", CVAR.RENDERER | CVAR.BOOL, "cache ETC1 data");
        public static CVar r_useIndexVBO = new("r_useIndexVBO", "0", CVAR.RENDERER | CVAR.BOOL, "Upload Index data to VBO");
        public static CVar r_useVertexVBO = new("r_useVertexVBO", "1", CVAR.RENDERER | CVAR.BOOL, "Upload Vertex data to VBO");

        public static CVar r_maxFps = new("r_maxFps", "0", CVAR.RENDERER | CVAR.INTEGER, "Limit maximum FPS. 0 = unlimited");
    }

    public interface IRenderSystem
    {
        // set up cvars and basic data structures, but don't init OpenGL, so it can also be used for dedicated servers
        void Init();

        // only called before quitting
        void Shutdown();

        void InitOpenGL();

        void ShutdownOpenGL();

        bool IsOpenGLRunning { get; }

        bool IsFullScreen { get; }
        int ScreenWidth { get; }
        int ScreenHeight { get; }
        float FOV { get; }
        int Refresh { get; }

        // allocate a renderWorld to be used for drawing
        IRenderWorld AllocRenderWorld();
        void FreeRenderWorld(IRenderWorld rw);

        // All data that will be used in a level should be registered before rendering any frames to prevent disk hits,
        // but they can still be registered at a later time if necessary.
        void BeginLevelLoad();
        void EndLevelLoad();

        // font support
        bool RegisterFont(string fontName, FontInfoEx font);

        // GUI drawing just involves shader parameter setting and axial image subsections
        void SetColor(Vector4 rgba);
        void SetColor4(float r, float g, float b, float a);
        void SetHudOpacity(float opacity);

        void DrawStretchPic(DrawVert[] verts, GlIndex[] indexes, int vertCount, int indexCount, Material material, bool clip = true, float min_x = 0f, float min_y = 0f, float max_x = 640f, float max_y = 480f);
        void DrawStretchPic(float x, float y, float w, float h, float s1, float t1, float s2, float t2, Material material);

        void DrawStretchTri(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 t1, Vector2 t2, Vector2 t3, Material material);
        void GlobalToNormalizedDeviceCoordinates(Vector3 global, Vector3 ndc);
        void GetGLSettings(out int width, out int height);
        void PrintMemInfo(MemInfo mi);

        void DrawSmallChar(int x, int y, int ch, Material material);
        void DrawSmallStringExt(int x, int y, string s, Vector4 setColor, bool forceColor, Material material);
        void DrawBigChar(int x, int y, int ch, Material material);
        void DrawBigStringExt(int x, int y, string s, Vector4 setColor, bool forceColor, Material material);

        // dump all 2D drawing so far this frame to the demo file
        void WriteDemoPics();

        // draw the 2D pics that were saved out with the current demo frame
        void DrawDemoPics();

        // FIXME: add an interface for arbitrary point/texcoord drawing

        // a frame cam consist of 2D drawing and potentially multiple 3D scenes window sizes are needed to convert SCREEN_WIDTH / SCREEN_HEIGHT values
        void BeginFrame(int windowWidth, int windowHeight);

        // if the pointers are not NULL, timing info will be returned
        void EndFrame(out int frontEndMsec, out int backEndMsec);

        // Will automatically tile render large screen shots if necessary
        // Samples is the number of jittered frames for anti-aliasing
        // If ref == NULL, session->updateScreen will be used
        // This will perform swapbuffers, so it is NOT an approppriate way to generate image files that happen during gameplay, as for savegame
        // markers.  Use WriteRender() instead.
        void TakeScreenshot(int width, int height, string fileName, int samples, RenderView ref_);

        // the render output can be cropped down to a subset of the real screen, as for save-game reviews and split-screen multiplayer.  Users of the renderer
        // will not know the actual pixel size of the area they are rendering to

        // the x,y,width,height values are in virtual SCREEN_WIDTH / SCREEN_HEIGHT coordinates

        // to render to a texture, first set the crop size with makePowerOfTwo = true, then perform all desired rendering, then capture to an image
        // if the specified physical dimensions are larger than the current cropped region, they will be cut down to fit
        void CropRenderSize(int width, int height, bool makePowerOfTwo = false, bool forceDimensions = false);
        void CaptureRenderToImage(string imageName);
        // fixAlpha will set all the alpha channel values to 0xff, which allows screen captures
        // to use the default tga loading code without having dimmed down areas in many places
        void CaptureRenderToFile(string fileName, bool fixAlpha = false);
        void UnCrop();

        // the image has to be already loaded ( most straightforward way would be through a FindMaterial )
        // texture filter / mipmapping / repeat won't be modified by the upload
        // returns false if the image wasn't found
        bool UploadImage(string imageName, byte[] data, int width, int height);

        void DirectFrameBufferStart();
        void DirectFrameBufferEnd();
    }
}