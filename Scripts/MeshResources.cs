using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshResources
{
    public static Mesh GenerateTri()
    {
        Mesh triMesh = new Mesh();

        Vector3[] positions = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0.0f),
            new Vector3( 0.0f, 0.5f, 0.0f),
            new Vector3( 0.5f, -0.5f, 0.0f)
        };

        Vector3[] normals = new Vector3[]
        {
            new Vector3(0.0f, 0.0f, -1.0f),
            new Vector3(0.0f, 0.0f, -1.0f),
            new Vector3(0.0f, 0.0f, -1.0f)
        };

        Vector2[] texcroods = new Vector2[]
        {
            new Vector2(0.0f, 0.0f),
            new Vector2(0.5f, 1.0f),
            new Vector2(1.0f, 0.0f)
        };

        int[] indices = new int[]
        {
            0, 1, 2
        };

        triMesh.vertices = positions;
        triMesh.uv = texcroods;
        triMesh.normals = normals;
        triMesh.triangles = indices;

        return triMesh;

    }
    public static Mesh generateQuadBounds(Vector3[] points, float marginX, float marginY, float stroke, Vector3 normal, Vector3 hudCenter, Vector3 tangent, Vector3 bitangent)
    {
        Vector3 bl = points[0];
        Vector3 br = points[1];
        Vector3 tl = points[2];
        Vector3 tr = points[3];

        Vector3 center = hudCenter;

        bl = bl + (tangent * marginX) + (bitangent * marginY);
        br = br - (tangent * marginX) + (bitangent * marginY);
        tl = tl + (tangent * marginX) - (bitangent * marginY);
        tr = tr - (tangent * marginX) - (bitangent * marginY);

        Vector3 blExtrude = bl + (tangent * stroke) + (bitangent * stroke);
        Vector3 brExtrude = br - (tangent * stroke) + (bitangent * stroke);
        Vector3 tlExtrude = tl + (tangent * stroke) - (bitangent * stroke);
        Vector3 trExtrude = tr - (tangent * stroke) - (bitangent * stroke);

        Mesh quadMesh = new Mesh();
        Vector3[] positions = new Vector3[]{
            bl, br, tr, tl,
            blExtrude, brExtrude, trExtrude, tlExtrude
        };

        Vector3[] normals = new Vector3[]
        {
            normal,
            normal,
            normal,
            normal,
            normal,
            normal,
            normal,
            normal
        };

        int[] indices = new int[]
        {
            0, 4, 1,
            1, 4, 5,
            1, 5, 2,
            2, 5, 6,
            2, 6, 3,
            3, 6, 7,
            3, 7, 0,
            0, 7, 4
        };

        quadMesh.vertices = positions;
        quadMesh.normals = normals;
        quadMesh.triangles = indices;
        return quadMesh;

    }
    public static Mesh generateQuad4p(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal)
    {
        Mesh quadMesh = new Mesh();
        Vector3[] positions = new Vector3[]
        {
            p0,
            p1,
            p2,
            p3
        };

        Vector3[] normals = new Vector3[]
        {
            normal,
            normal,
            normal,
            normal
        };

        Vector2[] uvs = new Vector2[]
        {
            new Vector2(0.0f,  0.0f),
            new Vector2(0.0f,  1.0f),
            new Vector2(1.0f,  1.0f),
            new Vector2(1.0f,  0.0f),
        };

        int[] indices = new int[]
        {
            0,  1,  2,
            3,  0,  2
        };

        quadMesh.vertices = positions;
        quadMesh.uv = uvs;
        quadMesh.normals = normals;
        quadMesh.triangles = indices;
        return quadMesh;

    }
    public static Mesh generateScreenQuad()
    {
        Mesh quadMesh = new Mesh();
        Vector3[] positions = new Vector3[]
        {
            new Vector3(-1.0f, -1.0f,  0.0f),
            new Vector3(-1.0f,  1.0f,  0.0f),
            new Vector3( 1.0f, -1.0f,  0.0f),
            new Vector3( 1.0f,  1.0f,  0.0f)
        };

        Vector3[] normals = new Vector3[]
        {
            new Vector3(0.0f, 0.0f, 1.0f),
            new Vector3(0.0f, 0.0f, 1.0f),
            new Vector3(0.0f, 0.0f, 1.0f),
            new Vector3(0.0f, 0.0f, 1.0f)
        };

        Vector2[] uvs = new Vector2[]
        {
            new Vector2(0.0f,  0.0f),
            new Vector2(0.0f,  1.0f),
            new Vector2(1.0f,  0.0f),
            new Vector2(1.0f,  1.0f),
        };


        int[] indices = new int[]
        {
            2,  0,  1,
            2,  1,  3
        };

        quadMesh.vertices = positions;
        quadMesh.uv = uvs;
        quadMesh.normals = normals;
        quadMesh.triangles = indices;
        return quadMesh;

    }
    public static Mesh generateVertSlider(Vector3 anchor, float height, float width, Vector3 normal, Vector3 tangent, Vector3 bitangent)
    {
        Vector3 bl = anchor;
        Vector3 br = bl + width * tangent;
        Vector3 tl = anchor + height * bitangent;
        Vector3 tr = tl + width * tangent;

        Mesh quadMesh = new Mesh();
        Vector3[] positions = new Vector3[]{
            bl, tl, tr, br
        };

        Vector3[] normals = new Vector3[]
        {
            normal,
            normal,
            normal,
            normal
        };

        Vector2[] uvs = new Vector2[]
        {
            new Vector2(0.0f,  0.0f),
            new Vector2(0.0f,  1.0f),
            new Vector2(1.0f,  1.0f),
            new Vector2(1.0f,  0.0f),
        };

        int[] indices = new int[]
        {
            0, 1, 3,
            3, 1, 2
        };

        quadMesh.vertices = positions;
        quadMesh.normals = normals;
        quadMesh.triangles = indices;
        quadMesh.uv = uvs;
        return quadMesh;
    }

}
