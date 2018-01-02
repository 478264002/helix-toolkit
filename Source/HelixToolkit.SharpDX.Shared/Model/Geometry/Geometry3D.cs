﻿/*
The MIT License (MIT)
Copyright (c) 2018 Helix Toolkit contributors
*/
#if NETFX_CORE
namespace HelixToolkit.UWP
#else
namespace HelixToolkit.Wpf.SharpDX
#endif
{
    using System;
    using global::SharpDX;
    using Core;
    using Model;
    using System.Runtime.Serialization;

#if !NETFX_CORE
    [Serializable]
#endif
    [DataContract]
    public abstract class Geometry3D : ObservableObject, IGUID
    {
        public const string VertexBuffer = "VertexBuffer";
        public const string TriangleBuffer = "TriangleBuffer";
        [DataMember]
        public Guid GUID { set; get; } = Guid.NewGuid();
        
        private IntCollection indices = null;

        /// <summary>
        /// Indices, can be triangle list, line list, etc.
        /// </summary>
        [DataMember]
        public IntCollection Indices
        {
            get
            {
                return indices;
            }
            set
            {
                if (Set(ref indices, value))
                {
                    Octree = null;
                }
            }
        }

        private Vector3Collection position = null;

        /// <summary>
        /// Vertex Positions
        /// </summary>
        [DataMember]
        public Vector3Collection Positions
        {
            get
            {
                return position;
            }
            set
            {
                if (Set(ref position, value))
                {                 
                    Octree = null;
                    UpdateBounds();
                }
            }
        }

        private BoundingBox bound;
        /// <summary>
        /// Geometry AABB
        /// </summary>
        [IgnoreDataMember]
        public BoundingBox Bound
        {
            set
            {
                Set(ref bound, value);
            }
            get
            {
                return bound;
            }
        }

        private BoundingSphere boundingSphere;
        /// <summary>
        /// Geometry Bounding Sphere
        /// </summary>
        [IgnoreDataMember]
        public BoundingSphere BoundingSphere
        {
            set
            {
                Set(ref boundingSphere, value);
            }
            get
            {
                return boundingSphere;
            }
        }

        private Color4Collection colors = null;
        /// <summary>
        /// Vertex Color
        /// </summary>
        [DataMember]
        public Color4Collection Colors
        {
            get
            {
                return colors;
            }
            set
            {
                Set(ref colors, value);
            }
        }

        /// <summary>
        /// TO use Octree during hit test to improve hit performance, please call UpdateOctree after model created.
        /// </summary>
        public IOctree<GeometryModel3D> Octree { private set; get; }

        public OctreeBuildParameter OctreeParameter { private set; get; } = new OctreeBuildParameter();
        /// <summary>
        /// Call to manually update vertex buffer. Use with <see cref="ObservableObject.DisablePropertyChangedEvent"/>
        /// </summary>
        public void UpdateVertices()
        {
            RaisePropertyChanged(VertexBuffer);
        }
        /// <summary>
        /// Call to manually update triangle buffer. Use with <see cref="ObservableObject.DisablePropertyChangedEvent"/>
        /// </summary>
        public void UpdateTriangles()
        {
            RaisePropertyChanged(TriangleBuffer);
        }


        /// <summary>
        /// Create Octree for current model.
        /// </summary>
        public void UpdateOctree(float minSize = 1f, bool autoDeleteIfEmpty = true)
        {
            if (CanCreateOctree())
            {
                OctreeParameter.MinimumOctantSize = minSize;
                OctreeParameter.AutoDeleteIfEmpty = autoDeleteIfEmpty;
                this.Octree = CreateOctree(this.OctreeParameter);
                this.Octree?.BuildTree();
            }
            else
            {
                this.Octree = null;
            }
        }
        
        protected virtual bool CanCreateOctree()
        {
            return Positions != null && Indices != null && Positions.Count > 0 && Indices.Count > 0;
        }


        /// <summary>
        /// Override to create different octree in subclasses.
        /// </summary>
        /// <returns></returns>
        protected virtual IOctree<GeometryModel3D> CreateOctree(OctreeBuildParameter parameter)
        {
            return null;
        }


        /// <summary>
        /// Set octree to null
        /// </summary>
        public void ClearOctree()
        {
            Octree = null;
        }
        /// <summary>
        /// Manually call this function to update AABB and Bounding Sphere
        /// </summary>
        public virtual void UpdateBounds()
        {
            if (position == null || position.Count == 0)
            {
                Bound = new BoundingBox();
                BoundingSphere = new BoundingSphere();
            }
            else
            {
                Bound = BoundingBoxExtensions.FromPoints(Positions);
                BoundingSphere = BoundingSphereExtensions.FromPoints(Positions);
            }
            if(Bound.Maximum.IsUndefined() || Bound.Minimum.IsUndefined() || BoundingSphere.Center.IsUndefined())
            {
                throw new Exception("Position vertex contains invalid value(Example: Float.NaN).");
            }
        }


        public struct Triangle
        {
            public Vector3 P0, P1, P2;
        }

        public struct Line
        {
            public Vector3 P0, P1;
        }

        public struct Point
        {
            public Vector3 P0;
        }
    }
}
