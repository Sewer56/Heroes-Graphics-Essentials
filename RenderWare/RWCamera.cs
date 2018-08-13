using System;
using System.Runtime.InteropServices;

namespace Reloaded_Mod_Template.RenderWare
{
    /// <summary>
    /// Partial reimplementation of the RenderWare Camera structure from known SDK leaks.
    /// </summary>
    public unsafe struct RWCamera
    {
        public RwObjectHasFrame objectHasFrame;

        /* Parallel or perspective projection */
        public RwCameraProjection projectionType;

        /* Start/end update functions */
        public void* beginUpdate;  // RwCameraBeginUpdateFunc
        public void* endUpdate;    // RwCameraEndUpdateFunc

        /* The view matrix */
        public RwMatrixTag viewMatrix;

        /* The cameras image buffer */
        public void* frameBuffer;

        /* The Z buffer */
        public void* zBuffer;

        /* Cameras mathmatical characteristics */
        public Vector2 viewWindow;
        public Vector2 recipViewWindow;
        public Vector2 viewOffset;
        public float nearPlane;
        public float farPlane;
        public float fogPlane;

        /* Transformation to turn camera z or 1/z into a Z buffer z */
        public float zScale;
        public float zShift;

        /* The clip-planes making up the viewing frustum */
        public RwFrustumPlane frustumPlane1;
        public RwFrustumPlane frustumPlane2;
        public RwFrustumPlane frustumPlane3;
        public RwFrustumPlane frustumPlane4;
        public RwFrustumPlane frustumPlane5;
        public RwFrustumPlane frustumPlane6;
        public RwBBox frustumBoundBox;

        /* Points on the tips of the view frustum */
        public Vector3 frustumCorner1;
        public Vector3 frustumCorner2;
        public Vector3 frustumCorner3;
        public Vector3 frustumCorner4;
        public Vector3 frustumCorner5;
        public Vector3 frustumCorner6;
        public Vector3 frustumCorner7;
        public Vector3 frustumCorner8;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RwObject
    {
        public byte type;
        public byte subType;
        public byte flags;
        public byte privateFlags;
        public void* parent;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RwLLLink
    {
        public RwLLLink* next;
        public RwLLLink* prev;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2
    {
        public float x;
        public float y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RwMatrixTag
    {
        /* These are padded to be 16 byte quantities per line */
        Vector3 right;
        uint flags;
        Vector3 up;
        uint pad1;
        Vector3 at;
        uint pad2;
        Vector3 pos;
        uint pad3;
    };

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RwObjectHasFrame
    {
        public RwObject rwObject;
        public RwLLLink lFrame;
        public void* sync;
    }

    public enum RwCameraProjection : int
    {
        rwNACAMERAPROJECTION = 0,   /**<Invalid projection */
        rwPERSPECTIVE = 1,          /**<Perspective projection */
        rwPARALLEL = 2,             /**<Parallel projection */
        rwCAMERAPROJECTIONFORCEENUMSIZEINT = Int32.MaxValue
    };

    /*
        * Structure describing a frustrum plane.
    */
    [StructLayout(LayoutKind.Sequential)]
    public struct RwFrustumPlane
    {
        RwPlane plane;
        byte closestX;
        byte closestY;
        byte closestZ;
        byte pad;
    };

    /*
        * This type represents a plane
    */
    [StructLayout(LayoutKind.Sequential)]
    public struct RwPlane
    {
        Vector3 normal;     /**< Normal to the plane */
        float distance;     /**< Distance to plane from origin in normal direction*/
    };

    /**
    * \ingroup rwbbox
    * \struct RwBBox
    * This type represents a 3D axis-aligned bounding-box
    * specified by the positions of two corners which lie on a diagonal.
    * Typically used to specify a world bounding-box when the world is created
    * 
    * \param sup Supremum vertex (contains largest values)
    * \param inf Infimum vertex (contains smallest values)
    * 
    * \see RpWorldCreate
    */
    public struct RwBBox
    {
        /* Must be in this order */
        Vector3 sup;   /**< Supremum vertex. */
        Vector3 inf;   /**< Infimum vertex. */
    };
}
