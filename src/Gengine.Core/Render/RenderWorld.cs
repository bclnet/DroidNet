using Gengine.Framework;
using Gengine.Sound;
using Gengine.UI;
using System;
using System.NumericsX;
using System.NumericsX.Core;

namespace Gengine.Render
{
    static partial class RenderWorldX
    {
        public const string PROC_FILE_EXT = "proc";
        public const string PROC_FILE_ID = "mapProcFile003";

        // shader parms
        public const int MAX_GLOBAL_SHADER_PARMS = 12;

        public const int SHADERPARM_RED = 0;
        public const int SHADERPARM_GREEN = 1;
        public const int SHADERPARM_BLUE = 2;
        public const int SHADERPARM_ALPHA = 3;
        public const int SHADERPARM_TIMESCALE = 3;
        public const int SHADERPARM_TIMEOFFSET = 4;
        public const int SHADERPARM_DIVERSITY = 5; // random between 0.0 and 1.0 for some effects (muzzle flashes, etc)
        public const int SHADERPARM_MODE = 7;  // for selecting which shader passes to enable
        public const int SHADERPARM_TIME_OF_DEATH = 7; // for the monster skin-burn-away effect enable and time offset

        // model parms
        public const int SHADERPARM_MD5_SKINSCALE = 8; // for scaling vertex offsets on md5 models (jack skellington effect)

        public const int SHADERPARM_MD3_FRAME = 8;
        public const int SHADERPARM_MD3_LASTFRAME = 9;
        public const int SHADERPARM_MD3_BACKLERP = 10;

        public const int SHADERPARM_BEAM_END_X = 8;    // for _beam models
        public const int SHADERPARM_BEAM_END_Y = 9;
        public const int SHADERPARM_BEAM_END_Z = 10;
        public const int SHADERPARM_BEAM_WIDTH = 11;

        public const int SHADERPARM_SPRITE_WIDTH = 8;
        public const int SHADERPARM_SPRITE_HEIGHT = 9;

        public const int SHADERPARM_PARTICLE_STOPTIME = 8; // don't spawn any more particles after this time

        // guis
        public const int MAX_RENDERENTITY_GUI = 3;

        public static int SIMD_ROUND_JOINTS(int numJoints) => (numJoints + 1) & ~1;
        public static void SIMD_INIT_LAST_JOINT(JointMat[] joints, int numJoints)
        {
            if ((numJoints & 1) != 0)
                joints[numJoints] = joints[numJoints - 1];
        }
    }

    public delegate bool DeferredEntityCallback(RenderEntity e, RenderView v);

    public class RenderEntity
    {
        public IRenderModel hModel;              // this can only be null if callback is set

        public int entityNum;
        public int bodyId;

        // Entities that are expensive to generate, like skeletal models, can be deferred until their bounds are found to be in view, in the frustum
        // of a shadowing light that is in view, or contacted by a trace / overlay test. This is also used to do visual cueing on items in the view
        // The renderView may be NULL if the callback is being issued for a non-view related source.
        // The callback function should clear renderEntity->callback if it doesn't want to be called again next time the entity is referenced (ie, if the
        // callback has now made the entity valid until the next updateEntity)
        public Bounds bounds;                    // only needs to be set for deferred models and md5s
        public DeferredEntityCallback callback;

        public byte[] callbackData;         // used for whatever the callback wants

        // player bodies and possibly player shadows should be suppressed in views from that player's eyes, but will show up in mirrors and other subviews
        // security cameras could suppress their model in their subviews if we add a way of specifying a view number for a remoteRenderMap view
        public int suppressSurfaceInViewID;
        public int suppressShadowInViewID;

        // world models for the player and weapons will not cast shadows from view weapon muzzle flashes
        public int suppressShadowInLightID;

        // if non-zero, the surface and shadow (if it casts one) will only show up in the specific view, ie: player weapons
        public int allowSurfaceInViewID;

        // positioning
        // axis rotation vectors must be unit length for many
        // R_LocalToGlobal functions to work, so don't scale models!
        // axis vectors are [0] = forward, [1] = left, [2] = up
        public Vector3 origin;
        public Matrix3x3 axis;

        // texturing
        public Material customShader;         // if non-0, all surfaces will use this
        public Material referenceShader;      // used so flares can reference the proper light shader
        public DeclSkin customSkin;               // 0 for no remappings
        public ISoundEmitter referenceSound;         // for shader sound tables, allowing effects to vary with sounds
        public float[] shaderParms = new float[Material.MAX_ENTITY_SHADER_PARMS]; // can be used in any way by shader or model generation

        // networking: see WriteGUIToSnapshot / ReadGUIFromSnapshot
        public IUserInterface[] gui = new IUserInterface[RenderWorldX.MAX_RENDERENTITY_GUI];

        public RenderView remoteRenderView;     // any remote camera surfaces will use this

        public int numJoints;
        public JointMat joints;                 // array of joints that will modify vertices.
                                                // NULL if non-deformable model.  NOT freed by renderer

        public float modelDepthHack;           // squash depth range so particle effects don't clip into walls

        // options to override surface shader flags (replace with material parameters?)
        public bool noSelfShadow;          // cast shadows onto other objects,but not self
        public bool noShadow;              // no shadow at all

        public bool noDynamicInteractions; // don't create any light / shadow interactions after the level load is completed.  This is a performance hack
                                           // for the gigantic outdoor meshes in the monorail map, so all the lights in the moving monorail don't touch the meshes

        public bool weaponDepthHack;       // squash depth range so view weapons don't poke into walls this automatically implies noShadow
        public int forceUpdate;            // force an update (NOTE: not a bool to keep this struct a multiple of 4 bytes)
        public int timeGroup;
        public int xrayIndex;
    }

    public class RenderLight
    {
        public Matrix3x3 axis;                // rotation vectors, must be unit length
        public Vector3 origin;

        // if non-zero, the light will not show up in the specific view, which may be used if we want to have slightly different muzzle
        // flash lights for the player and other views
        public int suppressLightInViewID;

        // if non-zero, the light will only show up in the specific view which can allow player gun gui lights and such to not effect everyone
        public int allowLightInViewID;

        // I am sticking the four bools together so there are no unused gaps in the padded structure, which could confuse the memcmp that checks for redundant updates
        public bool noShadows;         // (should we replace this with material parameters on the shader?)
        public bool noSpecular;            // (should we replace this with material parameters on the shader?)

        public bool pointLight;            // otherwise a projection light (should probably invert the sense of this, because points are way more common)
        public bool parallel;          // lightCenter gives the direction to the light at infinity
        public Vector3 lightRadius;     // xyz radius for point lights
        public Vector3 lightCenter;     // offset the lighting direction for shading and shadows, relative to origin

        // frustum definition for projected lights, all reletive to origin
        // FIXME: we should probably have real plane equations here, and offer  a helper function for conversion from this format
        public Vector3 target;
        public Vector3 right;
        public Vector3 up;
        public Vector3 start;
        public Vector3 end;

        // Dmap will generate an optimized shadow volume named _prelight_<lightName> for the light against all the _area* models in the map.  The renderer will
        // ignore this value if the light has been moved after initial creation
        public IRenderModel prelightModel;

        // muzzle flash lights will not cast shadows from player and weapon world models
        public int lightId;

        public Material shader;               // NULL = either lights/defaultPointLight or lights/defaultProjectedLight
        public float[] shaderParms = new float[Material.MAX_ENTITY_SHADER_PARMS];     // can be used in any way by shader
        public ISoundEmitter referenceSound;     // for shader sound tables, allowing effects to vary with sounds
    }

    public class RenderView
    {
        // player views will set this to a non-zero integer for model suppress / allow subviews (mirrors, cameras, etc) will always clear it to zero
        public int viewID;

        // sized from 0 to SCREEN_WIDTH / SCREEN_HEIGHT (640/480), not actual resolution
        public int x, y, width, height;

        float fov_x, fov_y;
        public Vector3 vieworg;
        public Matrix3x3 viewaxis;            // transformation matrix, view looks down the positive X axis

        public bool cramZNear;         // for cinematics, we want to set ZNear much lower
        public bool forceUpdate;       // for an update
        public bool forceMono;             // force mono

        // time in milliseconds for shader effects and other time dependent rendering issues
        public int time;
        public float[] shaderParms = new float[RenderWorldX.MAX_GLOBAL_SHADER_PARMS];     // can be used in any way by shader
        public Material globalMaterial;                           // used to override everything draw
    }

    // exitPortal_t is returned by idRenderWorld::GetPortal()
    public struct ExitPortal
    {
        public int[] areas;       // areas connected by this portal
        public Winding w;             // winding points have counter clockwise ordering seen from areas[0]
        public int blockingBits;   // PS_BLOCK_VIEW, PS_BLOCK_AIR, etc
        public IntPtr portalHandle;
    }

    // guiPoint_t is returned by idRenderWorld::GuiTrace()
    public struct GuiPoint
    {
        public float x, y;         // 0.0 to 1.0 range if trace hit a gui, otherwise -1
        public int guiId;          // id of gui ( 0, 1, or 2 ) that the trace happened against
        public float fraction;      // Koz added fraction of trace completed for touch screens
    }

    // modelTrace_t is for tracing vs. visual geometry
    public struct ModelTrace
    {
        public float fraction;         // fraction of trace completed
        public Vector3 point;               // end point of trace in global space
        public Vector3 normal;              // hit triangle normal vector in global space
        public Material material;         // material of hit surface
        public RenderEntity entity;               // render entity that was hit
        public int jointNumber;     // md5 joint nearest to the hit triangle
    }

    static partial class RenderWorldX
    {
        public const int NUM_PORTAL_ATTRIBUTES = 3;
    }

    public enum PortalConnection
    {
        PS_BLOCK_NONE = 0,

        PS_BLOCK_VIEW = 1,
        PS_BLOCK_LOCATION = 2,      // game map location strings often stop in hallways
        PS_BLOCK_AIR = 4,           // windows between pressurized and unpresurized areas

        PS_BLOCK_ALL = (1 << RenderWorldX.NUM_PORTAL_ATTRIBUTES) - 1
    }

    public interface IRenderWorld
    {
        // The same render world can be reinitialized as often as desired a NULL or empty mapName will create an empty, single area world
        bool InitFromMap(string mapName);

        //-------------- Entity and Light Defs -----------------

        // entityDefs and lightDefs are added to a given world to determine what will be drawn for a rendered scene.  Most update work is defered
        // until it is determined that it is actually needed for a given view.
        IntPtr AddEntityDef(RenderEntity re);
        void UpdateEntityDef(IntPtr entityHandle, RenderEntity re);
        void FreeEntityDef(IntPtr entityHandle);
        RenderEntity GetRenderEntity(IntPtr entityHandle);

        IntPtr AddLightDef(RenderLight rlight);
        void UpdateLightDef(IntPtr lightHandle, RenderLight rlight);
        void FreeLightDef(IntPtr lightHandle);
        RenderLight GetRenderLight(IntPtr lightHandle);

        // Force the generation of all light / surface interactions at the start of a level If this isn't called, they will all be dynamically generated
        void GenerateAllInteractions();

        // returns true if this area model needs portal sky to draw
        bool CheckAreaForPortalSky(int areaNum);

        //-------------- Decals and Overlays  -----------------

        // Creates decals on all world surfaces that the winding projects onto. The projection origin should be infront of the winding plane.
        // The decals are projected onto world geometry between the winding plane and the projection origin. The decals are depth faded from the winding plane to a certain distance infront of the
        // winding plane and the same distance from the projection origin towards the winding.
        void ProjectDecalOntoWorld(out FixedWinding winding, out Vector3 projectionOrigin, bool parallel, float fadeDepth, Material material, int startTime);

        // Creates decals on static models.
        void ProjectDecal(IntPtr entityHandle, out FixedWinding winding, out Vector3 projectionOrigin, bool parallel, float fadeDepth, Material material, int startTime);

        // Creates overlays on dynamic models.
        void ProjectOverlay(IntPtr entityHandle, Plane[] localTextureAxis, Material material);

        // Removes all decals and overlays from the given entity def.
        void RemoveDecals(IntPtr entityHandle);

        //-------------- Scene Rendering -----------------

        // some calls to material functions use the current renderview time when servicing cinematics.  this function ensures that any parms accessed (such as time) are properly set.
        void SetRenderView(RenderView renderView);

        // rendering a scene may actually render multiple subviews for mirrors and portals, and may render composite textures for gui console screens and light projections
        // It would also be acceptable to render a scene multiple times, for "rear view mirrors", etc
        void RenderScene(RenderView renderView);

        //-------------- Portal Area Information -----------------

        // returns the number of portals
        int NumPortals();

        // returns 0 if no portal contacts the bounds
        // This is used by the game to identify portals that are contained inside doors, so the connection between areas can be topologically
        // terminated when the door shuts.
        IntPtr FindPortal(out Bounds b);

        // doors explicitly close off portals when shut
        // multiple bits can be set to block multiple things, ie: ( PS_VIEW | PS_LOCATION | PS_AIR )
        void SetPortalState(IntPtr portal, int blockingBits);
        int GetPortalState(IntPtr portal);

        // returns true only if a chain of portals without the given connection bits set exists between the two areas (a door doesn't separate them, etc)
        bool AreasAreConnected(int areaNum1, int areaNum2, PortalConnection connection);

        // returns the number of portal areas in a map, so game code can build information tables for the different areas
        int NumAreas();

        // Will return -1 if the point is not in an area, otherwise it will return 0 <= value < NumAreas()
        int PointInArea(out Vector3 point);

        // fills the *areas array with the numbers of the areas the bounds cover
        // returns the total number of areas the bounds cover
        int BoundsInAreas(out Bounds bounds, int[] areas, int maxAreas);

        // Used by the sound system to do area flowing
        int NumPortalsInArea(int areaNum);

        // returns one portal from an area
        ExitPortal GetPortal(int areaNum, int portalNum);

        //-------------- Tracing  -----------------

        // Checks a ray trace against any gui surfaces in an entity, returning the fraction location of the trace on the gui surface, or -1,-1 if no hit.
        // This doesn't do any occlusion testing, simply ignoring non-gui surfaces. start / end are in global world coordinates.
        GuiPoint GuiTrace(IntPtr entityHandle, object animator, Vector3 start, Vector3 end); // Koz added animator

        // Traces vs the render model, possibly instantiating a dynamic version, and returns true if something was hit
        bool ModelTrace(out ModelTrace trace, IntPtr entityHandle, out Vector3 start, out Vector3 end, float radius);

        // Traces vs the whole rendered world. FIXME: we need some kind of material flags.
        bool Trace(out ModelTrace trace, out Vector3 start, out Vector3 end, float radius, bool skipDynamic = true, bool skipPlayer = false);

        // Traces vs the world model bsp tree.
        bool FastWorldTrace(out ModelTrace trace, out Vector3 start, out Vector3 end);

        //-------------- Demo Control  -----------------

        // Writes a loadmap command to the demo, and clears archive counters.
        void StartWritingDemo(VFileDemo demo);
        void StopWritingDemo();

        // Returns true when demoRenderView has been filled in.
        // adds/updates/frees entityDefs and lightDefs based on the current demo file and returns the renderView to be used to render this frame.
        // a demo file may need to be advanced multiple times if the framerate is less than 30hz
        // demoTimeOffset will be set if a new map load command was processed before the next renderScene
        bool ProcessDemoCommand(VFileDemo readDemo, RenderView demoRenderView, out int demoTimeOffset);

        // this is used to regenerate all interactions ( which is currently only done during influences ), there may be a less expensive way to do it
        void RegenerateWorld();

        //-------------- Debug Visualization  -----------------

        // Line drawing for debug visualization
        void DebugClearLines(int time);     // a time of 0 will clear all lines and text
        void DebugLine(Vector4 color, Vector3 start, Vector3 end, int lifetime = 0, bool depthTest = false);
        void DebugArrow(Vector4 color, Vector3 start, Vector3 end, int size, int lifetime = 0);
        void DebugWinding(Vector4 color, Winding w, Vector3 origin, Matrix3x3 axis, int lifetime = 0, bool depthTest = false);
        void DebugCircle(Vector4 color, Vector3 origin, Vector3 dir, float radius, int numSteps, int lifetime = 0, bool depthTest = false);
        void DebugSphere(Vector4 color, Sphere sphere, int lifetime = 0, bool depthTest = false);
        void DebugBounds(Vector4 color, Bounds bounds, Vector3 org = default, int lifetime = 0);
        void DebugBox(Vector4 color, Box box, int lifetime = 0);
        void DebugFrustum(Vector4 color, Frustum frustum, bool showFromOrigin = false, int lifetime = 0);
        void DebugCone(Vector4 color, Vector3 apex, Vector3 dir, float radius1, float radius2, int lifetime = 0);
        void DebugAxis(Vector3 origin, Matrix3x3 axis);

        // Polygon drawing for debug visualization.
        void DebugClearPolygons(int time);      // a time of 0 will clear all polygons
        void DebugPolygon(Vector4 color, Winding winding, int lifeTime = 0, bool depthTest = false);

        // Text drawing for debug visualization.
        void DrawText(string text, Vector3 origin, float scale, Vector4 color, Matrix3x3 viewAxis, int align = 1, int lifetime = 0, bool depthTest = false);
    }
}