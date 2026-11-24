# Builds the Discover Tunisia monument models and exports one FBX per monument for Unity.
# Usage: blender --background --python Tools/Blender/monuments.py -- --out Assets/DiscoverTunisia/Art/Models [Name ...]

import math
import os
import random
import sys

import bmesh
import bpy
from mathutils import Matrix, Vector

# sRGB colors, named after the Unity materials in Assets/DiscoverTunisia/Art/Materials.
PALETTE = {
    "M_Sand": (0.75, 0.68, 0.55),
    "M_Stone": (0.86, 0.80, 0.66),
    "M_StoneDark": (0.66, 0.58, 0.45),
    "M_Sandstone": (0.80, 0.62, 0.40),
    "M_Brick": (0.78, 0.55, 0.38),
    "M_Whitewash": (0.92, 0.92, 0.90),
    "M_Tile": (0.20, 0.55, 0.80),
    "M_TileGreen": (0.18, 0.52, 0.42),
    "M_Terracotta": (0.72, 0.38, 0.24),
    "M_PaintPink": (0.91, 0.64, 0.62),
    "M_PaintYellow": (0.94, 0.80, 0.46),
    "M_DoorwayDark": (0.16, 0.12, 0.09),
    "M_Wood": (0.25, 0.16, 0.10),
    "M_Water": (0.10, 0.35, 0.55),
    "M_Hull": (0.85, 0.85, 0.80),
    "M_Sail": (1.00, 1.00, 1.00),
    "M_Mast": (0.50, 0.40, 0.30),
    "M_Gold": (1.00, 0.80, 0.30),
    "M_Foliage": (0.22, 0.34, 0.20),
}

BEVEL = 0.012
SMOOTH_ANGLE = math.radians(40)
# Builders face their front to Blender -Y. Unity's AR and preview cameras see Blender +Y, so every model turns half a circle.
FACING_TURN = 180.0


def srgb_to_linear(c):
    return c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4


def material(name):
    mat = bpy.data.materials.get(name)
    if mat is None:
        mat = bpy.data.materials.new(name)
        r, g, b = (srgb_to_linear(c) for c in PALETTE[name])
        mat.diffuse_color = (r, g, b, 1.0)
        mat.use_nodes = True
        bsdf = next(n for n in mat.node_tree.nodes if n.type == "BSDF_PRINCIPLED")
        bsdf.inputs["Base Color"].default_value = (r, g, b, 1.0)
        bsdf.inputs["Roughness"].default_value = 0.85
    return mat


class Builder:
    """Collects parts of one monument and joins them into a single mesh object."""

    def __init__(self, name):
        self.name = name
        self.parts = []
        self.rng = random.Random(name)
        self.turn = 0.0

    # --- primitives -----------------------------------------------------

    def _add(self, bm, mat_name, loc, rot, scale=(1, 1, 1)):
        """Rot is a Z angle in degrees, or an (x, y, z) tuple of degrees."""
        mesh = bpy.data.meshes.new(self.name + "_part")
        bm.to_mesh(mesh)
        bm.free()
        obj = bpy.data.objects.new(mesh.name, mesh)
        bpy.context.scene.collection.objects.link(obj)
        mesh.materials.append(material(mat_name))
        obj.location = loc
        rx, ry, rz = rot if isinstance(rot, tuple) else (0.0, 0.0, rot)
        obj.rotation_euler = (math.radians(rx), math.radians(ry), math.radians(rz))
        obj.scale = scale
        self.parts.append(obj)
        return obj

    def box(self, size, loc, mat, rot=0.0, bevel=BEVEL, taper=1.0):
        """Box with its base centered on loc. Taper scales the top face."""
        sx, sy, sz = size
        bm = bmesh.new()
        bmesh.ops.create_cube(bm, size=1.0)
        for v in bm.verts:
            t = taper if v.co.z > 0 else 1.0
            v.co = Vector((v.co.x * sx * t, v.co.y * sy * t, (v.co.z + 0.5) * sz))
        if bevel > 0:
            b = min(bevel, sx * 0.3, sy * 0.3, sz * 0.3)
            bmesh.ops.bevel(bm, geom=list(bm.edges), offset=b, segments=1, affect="EDGES", profile=0.5)
        return self._add(bm, mat, loc, rot)

    def cylinder(self, radius, height, loc, mat, segments=16, top_radius=None, rot=0.0):
        bm = bmesh.new()
        bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=segments,
                              radius1=radius, radius2=radius if top_radius is None else top_radius,
                              depth=height, matrix=Matrix.Translation((0, 0, height / 2)))
        return self._add(bm, mat, loc, rot)

    def cone(self, radius, height, loc, mat, segments=12, rot=0.0):
        return self.cylinder(radius, height, loc, mat, segments, top_radius=0.0, rot=rot)

    def dome(self, radius, loc, mat, ribs=0, height=1.0, segments=24, rings=6):
        """Half sphere on loc. Ribs pleat it like the Kairouan domes. Height squashes or stretches it."""
        bm = bmesh.new()
        segments = max(segments, ribs * 2)
        bmesh.ops.create_uvsphere(bm, u_segments=segments, v_segments=rings * 2, radius=radius)
        bmesh.ops.delete(bm, geom=[v for v in bm.verts if v.co.z < -1e-4], context="VERTS")
        for v in bm.verts:
            elevation = math.asin(max(-1.0, min(1.0, v.co.z / radius)))
            if ribs and v.co.xy.length > 1e-6:
                azimuth = math.atan2(v.co.y, v.co.x)
                pleat = 1.0 + 0.06 * math.cos(ribs * azimuth) * math.cos(elevation)
                v.co.x *= pleat
                v.co.y *= pleat
            v.co.z *= height
        rim = [e for e in bm.edges if e.is_boundary]
        if rim:
            bmesh.ops.holes_fill(bm, edges=rim, sides=0)
        return self._add(bm, mat, loc, 0.0)

    def hull(self, points, loc, mat, rot=0.0):
        """Convex shape around the given points."""
        bm = bmesh.new()
        verts = [bm.verts.new(p) for p in points]
        bmesh.ops.convex_hull(bm, input=verts)
        bmesh.ops.delete(bm, geom=[v for v in bm.verts if not v.link_faces], context="VERTS")
        return self._add(bm, mat, loc, rot)

    def vault(self, length, radius, thickness, loc, mat, rot=0.0, arc=180.0, steps=12):
        """Barrel vault along local X, springing at loc's height. Arc below 180 leaves it broken."""
        bm = bmesh.new()
        ri = radius - thickness
        a0 = math.radians(0.0)
        profile = []
        for i in range(steps + 1):
            a = a0 + math.radians(arc) * i / steps
            profile.append((radius * math.cos(a), radius * math.sin(a)))
        for i in range(steps, -1, -1):
            a = a0 + math.radians(arc) * i / steps
            profile.append((ri * math.cos(a), ri * math.sin(a)))
        left = [bm.verts.new((-length / 2, y, z)) for y, z in profile]
        right = [bm.verts.new((length / 2, y, z)) for y, z in profile]
        bm.faces.new(left)
        bm.faces.new(list(reversed(right)))
        n = len(profile)
        for i in range(n):
            j = (i + 1) % n
            bm.faces.new((left[i], left[j], right[j], right[i]))
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
        return self._add(bm, mat, loc, rot)

    def boat(self, length, width, loc, rot=0.0, hull="M_Hull", band="M_Tile"):
        """Small fishing boat: a pointed hull with a colored lower band."""
        h = width * 0.45
        def ring(z, w, l):
            return [(-l / 2, -w / 2, z), (-l / 2, w / 2, z), (l * 0.15, -w / 2, z), (l * 0.15, w / 2, z), (l / 2, 0, z)]
        self.hull(ring(0, width * 0.55, length * 0.8) + ring(h * 0.45, width * 0.9, length * 0.95), loc, band, rot)
        top = Vector(loc) + Vector((0, 0, h * 0.45))
        self.hull(ring(0, width * 0.9, length * 0.95) + ring(h * 0.55, width, length), top, hull, rot)
        deck = top + Vector((0, 0, h * 0.55))
        self.hull(ring(0, width * 0.72, length * 0.78) + ring(0.004, width * 0.72, length * 0.78), deck, band, rot)

    def mosaic(self, nx, ny, cell, loc, colors, rot=0.0):
        """Floor of square tesserae. Colors maps (i, j) to a material name."""
        ox = -nx * cell / 2 + cell / 2
        oy = -ny * cell / 2 + cell / 2
        c, s = math.cos(math.radians(rot)), math.sin(math.radians(rot))
        for i in range(nx):
            for j in range(ny):
                x, y = ox + i * cell, oy + j * cell
                p = Vector(loc) + Vector((x * c - y * s, x * s + y * c, 0))
                self.box((cell * 0.94, cell * 0.94, 0.012), p, colors(i, j), rot=rot, bevel=0)

    def rubble_wall(self, length, height, loc, mat, rot=0.0, thickness=0.09, piece=0.12):
        """Excavated wall stub whose top wanders up and down like broken masonry."""
        count = max(1, round(length / piece))
        step = length / count
        c, s = math.cos(math.radians(rot)), math.sin(math.radians(rot))
        other = "M_Stone" if mat == "M_StoneDark" else "M_StoneDark"
        h = height * self.rng.uniform(0.7, 1.0)
        for i in range(count):
            h = min(height * 1.15, max(height * 0.3, h + height * self.rng.uniform(-0.28, 0.28)))
            off = -length / 2 + step * (i + 0.5)
            p = Vector(loc) + Vector((off * c, off * s, 0))
            stone = other if self.rng.random() < 0.25 else mat
            self.box((step * 1.02, thickness * self.rng.uniform(0.9, 1.08), h), p, stone, rot=rot, bevel=0.008)

    def cypress(self, height, loc):
        """Mediterranean cypress: stacked cones on a short trunk."""
        trunk = height * 0.08
        self.cylinder(height * 0.025, trunk, loc, "M_Wood", segments=6)
        radius = height * 0.12
        for z, r, h in ((0.06, 1.0, 0.55), (0.32, 0.8, 0.5), (0.58, 0.55, 0.42)):
            p = Vector(loc) + Vector((0, 0, trunk + height * z))
            self.cone(radius * r, height * h, p, "M_Foliage", segments=8)

    def jar(self, radius, height, loc, mat, rot=0.0):
        """Pottery jar: a lathe profile with a neck."""
        profile = [(0.0, 0.0), (0.55, 0.0), (0.95, 0.35), (1.0, 0.55), (0.7, 0.85), (0.45, 0.92), (0.5, 1.0), (0.0, 1.0)]
        bm = bmesh.new()
        segments = 12
        rings = []
        for px, pz in profile:
            ring = []
            for i in range(segments):
                a = 2 * math.pi * i / segments
                ring.append(bm.verts.new((math.cos(a) * px * radius, math.sin(a) * px * radius, pz * height)))
            rings.append(ring)
        for r0, r1 in zip(rings, rings[1:]):
            for i in range(segments):
                j = (i + 1) % segments
                bm.faces.new((r0[i], r0[j], r1[j], r1[i]))
        bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=1e-5)
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
        return self._add(bm, mat, loc, rot)

    # --- architecture ---------------------------------------------------

    @staticmethod
    def arch_profile(width, spring, horseshoe=20.0, steps=10):
        """Outline of an arched opening in the XZ plane, from the right foot round to the left foot."""
        theta = math.radians(horseshoe)
        r = (width / 2) / math.cos(theta)
        zc = spring + r * math.sin(theta)
        pts = [(width / 2, 0.0)]
        for i in range(steps + 1):
            a = -theta + (math.pi + 2 * theta) * i / steps
            pts.append((r * math.cos(a), zc + r * math.sin(a)))
        pts.append((-width / 2, 0.0))
        return pts

    def cutter(self, width, spring, depth, loc, rot=0.0, horseshoe=20.0):
        """Prism with an arch outline, used to cut doors and arcades."""
        bm = bmesh.new()
        pts = self.arch_profile(width, spring, horseshoe)
        front = [bm.verts.new((x, -depth / 2, z)) for x, z in pts]
        back = [bm.verts.new((x, depth / 2, z)) for x, z in pts]
        bm.faces.new(front)
        bm.faces.new(list(reversed(back)))
        n = len(pts)
        for i in range(n):
            j = (i + 1) % n
            bm.faces.new((front[i], back[i], back[j], front[j]))
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
        obj = self._add(bm, "M_DoorwayDark", loc, rot)
        self.parts.remove(obj)
        return obj

    def cut(self, target, cutters):
        """Subtracts the cutters from the target and deletes them."""
        for c in cutters:
            mod = target.modifiers.new("cut", "BOOLEAN")
            mod.operation = "DIFFERENCE"
            mod.solver = "EXACT"
            mod.object = c
        depsgraph = bpy.context.evaluated_depsgraph_get()
        evaluated = target.evaluated_get(depsgraph)
        mesh = bpy.data.meshes.new_from_object(evaluated)
        old = target.data
        target.modifiers.clear()
        target.data = mesh
        bpy.data.meshes.remove(old)
        for c in cutters:
            bpy.data.meshes.remove(c.data)
        return target

    def arcade(self, length, height, depth, loc, mat, count, rot=0.0, spring=0.55, horseshoe=20.0, open_ratio=0.72):
        """Wall pierced by a row of arches along its local X."""
        wall = self.box((length, depth, height), loc, mat, rot=rot)
        pitch = length / count
        width = pitch * open_ratio
        c, s = math.cos(math.radians(rot)), math.sin(math.radians(rot))
        cutters = []
        for i in range(count):
            off = -length / 2 + pitch * (i + 0.5)
            p = Vector(loc) + Vector((off * c, off * s, -0.001))
            cutters.append(self.cutter(width, height * spring, depth * 3, p, rot, horseshoe))
        return self.cut(wall, cutters)

    def merlons(self, length, loc, mat, rot=0.0, size=0.07, gap=0.05, height=None, depth=None):
        """Row of crenellations along local X, centered on loc."""
        height = height or size * 1.2
        depth = depth or size * 0.8
        count = max(1, int((length + gap) // (size + gap)))
        span = count * size + (count - 1) * gap
        c, s = math.cos(math.radians(rot)), math.sin(math.radians(rot))
        for i in range(count):
            off = -span / 2 + size / 2 + i * (size + gap)
            p = Vector(loc) + Vector((off * c, off * s, 0))
            self.box((size, depth, height), p, mat, rot=rot, bevel=0.006)

    def ring_merlons(self, sx, sy, z, mat, size=0.07, gap=0.05, inset=0.0, height=None, depth=None, center=(0, 0)):
        """Crenellations around the top of a rectangle of size sx by sy."""
        cx, cy = center
        hx, hy = sx / 2 - inset, sy / 2 - inset
        d = depth or size * 0.8
        self.merlons(sx - 2 * inset, (cx, cy - hy + d / 2, z), mat, 0, size, gap, height, depth)
        self.merlons(sx - 2 * inset, (cx, cy + hy - d / 2, z), mat, 0, size, gap, height, depth)
        self.merlons(sy - 2 * inset - 2 * d, (cx - hx + d / 2, cy, z), mat, 90, size, gap, height, depth)
        self.merlons(sy - 2 * inset - 2 * d, (cx + hx - d / 2, cy, z), mat, 90, size, gap, height, depth)

    def base(self, sx, sy, mat="M_Sand", height=0.08):
        """Diorama slab every monument stands on. Its top is at z = height."""
        self.box((sx, sy, height), (0, 0, 0), mat, bevel=0.03)
        return height

    # --- output ---------------------------------------------------------

    def finish(self):
        """Joins every part into one mesh object named after the monument, base centered on the origin."""
        names = []
        bm = bmesh.new()
        turn = Matrix.Rotation(math.radians(self.turn + FACING_TURN), 4, "Z")
        for obj in self.parts:
            mesh = obj.data.copy()
            mesh.transform(turn @ Matrix.LocRotScale(obj.location, obj.rotation_euler, obj.scale))
            for poly in mesh.polygons:
                mat_name = obj.data.materials[poly.material_index].name
                if mat_name not in names:
                    names.append(mat_name)
                poly.material_index = names.index(mat_name)
            bm.from_mesh(mesh)
            bpy.data.meshes.remove(mesh)
            old = obj.data
            bpy.data.objects.remove(obj)
            bpy.data.meshes.remove(old)
        bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=1e-5)
        mesh = bpy.data.meshes.new(self.name)
        bm.to_mesh(mesh)
        bm.free()
        for n in names:
            mesh.materials.append(material(n))
        mesh.shade_smooth()
        mesh.set_sharp_from_angle(angle=SMOOTH_ANGLE)
        obj = bpy.data.objects.new(self.name, mesh)
        bpy.context.scene.collection.objects.link(obj)
        self.parts = []
        return obj


# --- monuments --------------------------------------------------------------


def great_mosque(b):
    """Great Mosque of Kairouan: buttressed enclosure, three-tier minaret, ribbed domes over the prayer hall."""
    b.turn = 180.0
    z = b.base(2.9, 4.1)
    wx, wy, wh, wt = 2.4, 3.6, 0.36, 0.07
    stone, dark = "M_Sandstone", "M_StoneDark"
    b.box((wx, wy, 0.02), (0, 0, z), "M_Stone", bevel=0)
    # enclosure walls with buttresses and crenellations
    b.box((wx, wt, wh), (0, -wy / 2 + wt / 2, z), stone)
    b.box((wx, wt, wh), (0, wy / 2 - wt / 2, z), stone)
    b.box((wt, wy, wh), (-wx / 2 + wt / 2, 0, z), stone)
    b.box((wt, wy, wh), (wx / 2 - wt / 2, 0, z), stone)
    for i in range(9):
        y = -wy / 2 + 0.25 + i * (wy - 0.5) / 8
        for side in (-1, 1):
            b.box((0.09, 0.11, wh * 0.92), (side * (wx / 2 + 0.035), y, z), stone, taper=0.8)
    for i in range(6):
        x = -wx / 2 + 0.25 + i * (wx - 0.5) / 5
        if abs(x) < 0.35:
            continue
        for side in (-1, 1):
            b.box((0.11, 0.09, wh * 0.92), (x, side * (wy / 2 + 0.035), z), stone, taper=0.8)
    b.ring_merlons(wx, wy, z + wh, stone, size=0.06, gap=0.06, height=0.05, depth=0.05)
    # prayer hall at the south end, roof and arcaded courtyard front
    hall_y0, hall_y1 = -wy / 2 + wt, -0.25
    hall_depth = hall_y1 - hall_y0
    b.box((wx - 2 * wt, hall_depth, wh * 1.05), (0, (hall_y0 + hall_y1) / 2, z), "M_Stone")
    b.arcade(wx - 2 * wt, wh * 0.95, 0.06, (0, hall_y1 + 0.03, z), "M_Whitewash", 13)
    for i in range(1, 13):
        x = -(wx - 2 * wt) / 2 + i * (wx - 2 * wt) / 13
        b.box((0.012, hall_depth * 0.92, 0.02), (x, (hall_y0 + hall_y1) / 2, z + wh * 1.05), dark, bevel=0)
    # central nave raised, with the two ribbed domes
    b.box((0.36, hall_depth, wh * 1.2), (0, (hall_y0 + hall_y1) / 2, z), "M_Stone")
    for y in (hall_y0 + 0.22, hall_y1 - 0.18):
        b.box((0.34, 0.34, 0.08), (0, y, z + wh * 1.2), stone)
        b.cylinder(0.15, 0.07, (0, y, z + wh * 1.2 + 0.08), stone, segments=8)
        b.dome(0.15, (0, y, z + wh * 1.2 + 0.15), "M_Stone", ribs=12, height=1.05)
    # side porticoes around the courtyard
    court_y0, court_y1 = hall_y1 + 0.06, wy / 2 - wt
    court_len = court_y1 - court_y0
    for side in (-1, 1):
        x = side * (wx / 2 - wt - 0.1)
        b.arcade(court_len, wh * 0.8, 0.05, (x, (court_y0 + court_y1) / 2, z), "M_Whitewash", 11, rot=90)
        b.box((0.2, court_len, 0.03), (side * (wx / 2 - wt - 0.1 + side * 0.07), (court_y0 + court_y1) / 2, z + wh * 0.8), "M_Stone", bevel=0.005)
    b.arcade(wx - 2 * wt - 0.4, wh * 0.8, 0.05, (0, court_y1 - 0.1, z), "M_Whitewash", 9)
    # courtyard paving and the well curbs
    b.box((wx - 0.5, court_len - 0.3, 0.015), (0, (court_y0 + court_y1) / 2 - 0.05, z + 0.02), "M_Whitewash", bevel=0)
    b.cylinder(0.05, 0.06, (0.3, 0.6, z), "M_Stone", segments=10)
    b.cylinder(0.05, 0.06, (-0.3, 0.6, z), "M_Stone", segments=10)
    # minaret on the north wall: three tiers, crenellated, topped with a ribbed dome
    my = wy / 2 - 0.12
    t1, t2, t3 = 0.95, 0.32, 0.22
    b.box((0.5, 0.5, t1), (0, my, z), stone, taper=0.88)
    b.ring_merlons(0.44, 0.44, z + t1, stone, size=0.05, gap=0.04, height=0.05, depth=0.04, center=(0, my))
    b.box((0.33, 0.33, t2), (0, my, z + t1), stone, taper=0.95)
    b.ring_merlons(0.33, 0.33, z + t1 + t2, stone, size=0.04, gap=0.035, height=0.04, depth=0.035, center=(0, my))
    b.box((0.22, 0.22, t3), (0, my, z + t1 + t2), stone)
    b.dome(0.11, (0, my, z + t1 + t2 + t3), "M_Stone", ribs=10, height=1.2)
    b.cylinder(0.008, 0.08, (0, my, z + t1 + t2 + t3 + 0.12), "M_Gold", segments=6)
    for level in (0.3, 0.6):
        b.box((0.05, 0.02, 0.1), (0, my - 0.25 * (1 - 0.06 * level / 0.3), z + t1 * level), "M_DoorwayDark", bevel=0)
    b.box((0.04, 0.02, 0.08), (0, my - 0.17, z + t1 + 0.1), "M_DoorwayDark", bevel=0)


def three_doors(b):
    """Mosque of the Three Doors: carved sandstone facade with three horseshoe arches and a corner minaret."""
    z = b.base(3.2, 2.0)
    stone, dark = "M_Sandstone", "M_StoneDark"
    b.box((3.0, 0.5, 0.01), (0, -0.62, z), "M_Stone", bevel=0)
    # prayer hall block and its facade
    cx, fy, w, d, h = 0.15, -0.35, 1.7, 1.1, 1.25
    hall = b.box((w, d, h), (cx, fy + d / 2, z), stone)
    pitch = 0.52
    cutters = [b.cutter(0.36, 0.42, 0.16, (cx + (i - 1) * pitch, fy, z - 0.001)) for i in range(3)]
    b.cut(hall, cutters)
    for i in range(3):
        x = cx + (i - 1) * pitch
        b.box((0.26, 0.02, 0.5), (x, fy + 0.07, z), "M_Wood", bevel=0.004)
        b.box((0.03, 0.025, 0.5), (x, fy + 0.065, z), "M_DoorwayDark", bevel=0)
    for i in range(4):
        x = cx + (i - 1.5) * pitch
        b.cylinder(0.032, 0.5, (x, fy - 0.03, z), "M_Whitewash", segments=10)
        b.box((0.09, 0.09, 0.05), (x, fy - 0.03, z + 0.5), "M_Stone", bevel=0.006)
        b.box((0.08, 0.08, 0.03), (x, fy - 0.03, z), "M_Stone", bevel=0.006)
    # carved bands with a Kufic-like rhythm, then the corbelled cornice
    for k, bz in enumerate((0.86, 0.98, 1.1)):
        b.box((w + 0.02, 0.03, 0.075), (cx, fy - 0.012, z + bz), dark, bevel=0.004)
        x = cx - w / 2 + 0.04
        while x < cx + w / 2 - 0.04:
            gh = b.rng.choice((0.03, 0.045, 0.06)) if k != 1 else 0.035
            b.box((0.014, 0.012, gh), (x, fy - 0.03, z + bz + 0.008), stone, bevel=0)
            x += b.rng.choice((0.03, 0.04, 0.05))
    b.box((w + 0.12, 0.12, 0.045), (cx, fy - 0.03, z + h), dark, bevel=0.006)
    for i in range(22):
        x = cx - w / 2 + 0.04 + i * (w - 0.08) / 21
        b.box((0.03, 0.06, 0.04), (x, fy - 0.02, z + h - 0.04), dark, bevel=0.004)
    b.merlons(w + 0.08, (cx, fy + 0.0, z + h + 0.045), stone, size=0.07, gap=0.05, height=0.06, depth=0.06)
    # corner minaret with a tiled twin-arch window and stepped crenellations
    mx, my, ms, mh = cx - w / 2 - 0.15, fy + 0.45, 0.55, 2.15
    tower = b.box((ms, ms, mh), (mx, my, z), stone, taper=0.97)
    def window():
        return [b.cutter(0.1, 0.12, 0.2, (mx + dx, my - ms / 2 * 0.97, z + mh - 0.55)) for dx in (-0.065, 0.065)]
    b.cut(tower, window())
    panel = b.box((0.32, 0.02, 0.36), (mx, my - ms / 2 * 0.985 - 0.006, z + mh - 0.63), "M_Tile", bevel=0.004)
    b.cut(panel, window())
    b.box((0.012, 0.03, 0.2), (mx, my - ms / 2 * 0.97, z + mh - 0.55), "M_Whitewash", bevel=0)
    b.ring_merlons(ms * 0.97, ms * 0.97, z + mh, stone, size=0.08, gap=0.045, height=0.09, depth=0.06, center=(mx, my))
    b.box((0.2, 0.2, 0.18), (mx, my, z + mh), stone)
    b.dome(0.1, (mx, my, z + mh + 0.18), "M_Stone", ribs=8)
    # whitewashed neighbours with blue doors
    b.box((0.62, 1.0, 0.85), (1.33, 0.15, z), "M_Whitewash")
    b.box((0.22, 0.02, 0.42), (1.25, -0.36, z), "M_Tile", bevel=0.004)
    b.box((0.55, 1.1, 0.75), (-1.28, 0.25, z), "M_Whitewash")
    b.box((0.12, 0.02, 0.12), (-1.2, -0.31, z + 0.45), "M_DoorwayDark", bevel=0)


def mausoleum(b):
    """Barber's Mausoleum: white arcaded courtyard, a large ribbed dome and a green-roofed minaret."""
    z = b.base(3.2, 3.2)
    white = "M_Whitewash"
    cy, cs, wh = -0.35, 1.9, 0.72
    # outer walls around the courtyard galleries, low enough to show the arcades
    oh = wh * 0.82 + 0.05
    for sy in (-1, 1):
        b.box((cs, 0.08, oh), (0, cy + sy * (cs / 2 - 0.04), z), white)
        b.box((cs + 0.02, 0.1, 0.025), (0, cy + sy * (cs / 2 - 0.04), z + oh), "M_Stone", bevel=0.004)
    for sx in (-1, 1):
        b.box((0.08, cs - 0.16, oh), (sx * (cs / 2 - 0.04), cy, z), white)
        b.box((0.1, cs - 0.16, 0.025), (sx * (cs / 2 - 0.04), cy, z + oh), "M_Stone", bevel=0.004)
    # inner arcades on four sides and the gallery roofs
    inner = cs - 0.62
    for sy in (-1, 1):
        b.arcade(inner + 0.06, wh * 0.82, 0.06, (0, cy + sy * inner / 2, z), white, 5, horseshoe=25)
    for sx in (-1, 1):
        b.arcade(inner - 0.06, wh * 0.82, 0.06, (sx * inner / 2, cy, z), white, 5, rot=90, horseshoe=25)
    ring = (cs - inner) / 2
    for sy in (-1, 1):
        b.box((cs - 0.04, ring, 0.04), (0, cy + sy * (inner / 2 + ring / 2 - 0.03), z + wh * 0.82), white, bevel=0.006)
    for sx in (-1, 1):
        b.box((ring, inner, 0.04), (sx * (inner / 2 + ring / 2 - 0.03), cy, z + wh * 0.82), white, bevel=0.006)
    # courtyard floor: marble with a tiled star
    b.box((inner - 0.04, inner - 0.04, 0.012), (0, cy, z), white, bevel=0)
    b.mosaic(9, 9, 0.09, (0, cy, z + 0.012),
             lambda i, j: "M_TileGreen" if (i + j) % 2 == 0 and 1 <= i <= 7 and 1 <= j <= 7 else "M_Tile" if abs(i - 4) + abs(j - 4) <= 1 else "M_Whitewash")
    # entrance gate with a tiled frame
    gate = b.box((0.6, 0.14, 0.95), (0, cy - cs / 2 - 0.02, z), white)
    b.cut(gate, [b.cutter(0.28, 0.42, 0.4, (0, cy - cs / 2 - 0.02, z - 0.001), horseshoe=25)])
    b.box((0.42, 0.02, 0.62), (0, cy - cs / 2 - 0.095, z + 0.02), "M_TileGreen", bevel=0.004)
    b.box((0.24, 0.025, 0.42), (0, cy - cs / 2 - 0.1, z + 0.03), "M_Wood", bevel=0.004)
    # tomb chamber behind the courtyard: cube, octagonal drum, large ribbed dome
    ty = cy + cs / 2 + 0.5
    b.box((1.0, 1.0, 1.0), (0, ty, z), white)
    b.box((1.04, 1.04, 0.05), (0, ty, z + 1.0), "M_TileGreen", bevel=0.006)
    b.cylinder(0.43, 0.14, (0, ty, z + 1.05), white, segments=8)
    b.dome(0.43, (0, ty, z + 1.19), white, ribs=16, height=1.1)
    b.cylinder(0.012, 0.12, (0, ty, z + 1.19 + 0.47), "M_Gold", segments=6)
    b.dome(0.03, (0, ty, z + 1.19 + 0.59), "M_Gold", segments=8, rings=3)
    # minaret at the front corner with a tiled band and a green pyramid roof
    mx, my, ms, mh = cs / 2 + 0.25, cy - cs / 2 + 0.3, 0.36, 1.55
    b.box((ms, ms, mh), (mx, my, z), white)
    for k, bz in enumerate((0.95, 1.3)):
        b.box((ms + 0.02, ms + 0.02, 0.06 if k else 0.12), (mx, my, z + bz), "M_TileGreen", bevel=0.004)
    b.ring_merlons(ms, ms, z + mh, white, size=0.05, gap=0.035, height=0.05, depth=0.04, center=(mx, my))
    b.box((0.2, 0.2, 0.22), (mx, my, z + mh), white)
    b.cone(0.17, 0.2, (mx, my, z + mh + 0.22), "M_TileGreen", segments=4, rot=45)
    b.cylinder(0.008, 0.07, (mx, my, z + mh + 0.4), "M_Gold", segments=6)


def medina(b):
    """Nabeul medina: a vaulted souk lane between whitewashed shops, a sandstone gate and the potters' stall."""
    b.turn = -90.0
    z = b.base(3.3, 2.7)
    white, blue = "M_Whitewash", "M_Tile"
    lane, length = 0.5, 2.6
    widths = (0.5, 0.62, 0.45, 0.55, 0.48)
    for side in (-1, 1):
        x = -length / 2 - 0.15
        y = side * (lane / 2 + 0.3)
        for i, wdt in enumerate(widths):
            h = 0.62 + 0.18 * ((i + (side > 0)) % 2) + b.rng.uniform(-0.04, 0.04)
            px = x + wdt / 2
            b.box((wdt, 0.6, h), (px, y, z), white)
            b.box((wdt + 0.02, 0.62, 0.03), (px, y, z + h), "M_Stone", bevel=0.005)
            face = y + side * 0.3
            b.box((0.18, 0.02, 0.34), (px, face + side * 0.008, z), blue, bevel=0.004)
            if side < 0:
                b.box((0.13, 0.02, 0.1), (px + 0.11 * (1 if i % 2 else -1), face - 0.008, z + h - 0.25), blue, bevel=0.003)
                b.box((0.09, 0.025, 0.07), (px + 0.11 * (1 if i % 2 else -1), face - 0.01, z + h - 0.235), "M_DoorwayDark", bevel=0)
            b.box((0.16, 0.02, 0.3), (px, y - side * 0.3 - side * 0.008, z), blue, bevel=0.004)
            x += wdt + 0.01
    # the vaulted roof over the lane, ribs at regular steps
    vault_h = 0.62
    b.box((length + 0.1, lane + 0.04, 0.02), (-0.15, 0, z), "M_Stone", bevel=0)
    b.vault(length, lane / 2 + 0.06, 0.06, (-0.15, 0, z + vault_h), white)
    for i in range(6):
        b.vault(0.05, lane / 2 + 0.08, 0.04, (-0.15 - length / 2 + 0.05 + i * (length - 0.1) / 5, 0, z + vault_h), "M_Stone")
    # medina gate across the east end of the lane
    gx = length / 2 - 0.15 + 0.12
    gate = b.box((0.24, 1.8, 1.08), (gx, 0, z), "M_Sandstone")
    b.cut(gate, [b.cutter(0.42, 0.55, 0.6, (gx, 0, z - 0.001), rot=90, horseshoe=22)])
    b.merlons(1.8, (gx, 0, z + 1.08), "M_Sandstone", rot=90, size=0.08, gap=0.06, height=0.08, depth=0.2)
    b.box((0.27, 0.6, 0.06), (gx, 0, z + 0.92), "M_StoneDark", bevel=0.005)
    # potters' stall in front: table, jars, plates
    sy = -lane / 2 - 0.6 - 0.32
    b.box((1.1, 0.26, 0.04), (-0.4, sy, z + 0.2), "M_Wood", bevel=0.006)
    for dx in (-0.48, 0.48):
        b.box((0.04, 0.22, 0.2), (-0.4 + dx, sy, z), "M_Wood", bevel=0.004)
    for dx, r, hgt in ((-0.75, 0.07, 0.22), (-0.6, 0.05, 0.16), (-0.25, 0.045, 0.12), (-0.05, 0.06, 0.18), (0.25, 0.08, 0.26)):
        on_ground = dx < -0.5 or dx > 0.2
        b.jar(r, hgt, (-0.4 + dx, sy - 0.12, z) if on_ground else (-0.4 + dx, sy, z + 0.24), "M_Terracotta")
    for i, dx in enumerate((-0.32, -0.14)):
        b.cylinder(0.06, 0.015, (-0.4 + dx + 0.5, sy, z + 0.24), "M_Tile" if i else "M_PaintYellow", segments=12)


def neapolis(b):
    """Neapolis: excavated Roman rooms with a mosaic floor, the fish-salting vats and a broken rubble vault."""
    z = b.base(3.2, 2.6)
    stone, dark = "M_Stone", "M_StoneDark"
    # rooms: low walls with broken tops
    b.rubble_wall(2.6, 0.26, (0, -0.95, z), dark)
    b.rubble_wall(2.6, 0.3, (0, 0.25, z), dark)
    b.rubble_wall(1.2, 0.28, (-1.3, -0.35, z), dark, rot=90)
    b.rubble_wall(1.2, 0.22, (-0.05, -0.35, z), dark, rot=90)
    b.rubble_wall(1.2, 0.3, (1.3, -0.35, z), dark, rot=90)
    # mosaic floor of the main room: border, checker field, central rosette
    def tesserae(i, j):
        if i in (0, 11) or j in (0, 8):
            return "M_Terracotta"
        if abs(i - 5.5) + abs(j - 4) <= 2:
            return "M_PaintYellow" if (i + j) % 2 else "M_Tile"
        return "M_Whitewash" if (i + j) % 2 else "M_StoneDark"
    b.mosaic(12, 9, 0.095, (-0.68, -0.35, z), tesserae)
    # garum vats with a reddish lining
    for i in range(4):
        vx = 0.25 + i * 0.26
        b.box((0.24, 0.42, 0.18), (vx, -0.35, z), stone, bevel=0.006)
        b.box((0.17, 0.34, 0.02), (vx, -0.35, z + 0.165), "M_Terracotta", bevel=0)
    # back terrace: the rubble vault from the photo, broken at one end
    b.box((2.2, 0.75, 0.12), (-0.2, 0.75, z), "M_Sand", bevel=0.02)
    b.rubble_wall(0.7, 0.36, (-0.85, 0.75, z + 0.12), dark, rot=90, thickness=0.14)
    b.vault(0.62, 0.34, 0.11, (-0.5, 0.75, z + 0.12 + 0.18), "M_Stone", rot=90, arc=125, steps=10)
    b.rubble_wall(0.62, 0.2, (-0.5 + 0.0, 0.75 - 0.31, z + 0.12), dark, thickness=0.12)
    # amphorae and shrubs
    b.jar(0.06, 0.24, (0.95, 0.6, z + 0.12), "M_Terracotta")
    b.jar(0.055, 0.22, (0.6, 0.78, z + 0.18), "M_Terracotta", rot=(80, 0, 30))
    b.jar(0.05, 0.2, (1.12, 0.85, z + 0.12), "M_Terracotta")
    for x, y, r in ((1.35, 0.95, 0.12), (-1.35, 0.95, 0.1), (1.4, -1.1, 0.09)):
        b.dome(r, (x, y, z), "M_Foliage", segments=10, rings=3, height=0.8)


def old_port(b):
    """Bizerte old port: colorful quay houses along the canal, the octagonal minaret, the kasbah wall and boats."""
    z = b.base(3.4, 3.0, mat="M_StoneDark")
    # quays and the canal
    b.box((3.3, 1.55, 0.08), (0, 0.7, z), "M_Stone", bevel=0.008)
    b.box((3.3, 0.3, 0.08), (0, -1.33, z), "M_Stone", bevel=0.008)
    b.box((3.3, 1.08, 0.03), (0, -0.64, z), "M_Water", bevel=0)
    q = z + 0.08
    # houses facing the water, flat roofs with parapets
    colors = ("M_Whitewash", "M_Tile", "M_PaintPink", "M_Whitewash", "M_PaintYellow", "M_Whitewash", "M_Tile")
    x = -1.55
    for i, color in enumerate(colors):
        wdt = (0.42, 0.5, 0.38, 0.46, 0.4, 0.5, 0.36)[i]
        h = (0.75, 0.95, 0.68, 1.05, 0.8, 0.9, 0.7)[i]
        px = x + wdt / 2
        b.box((wdt, 0.7, h), (px, 0.35, q), color)
        b.box((wdt + 0.01, 0.71, 0.04), (px, 0.35, q + h), "M_Stone" if color != "M_Whitewash" else "M_Whitewash", bevel=0.004)
        shutter = "M_Whitewash" if color == "M_Tile" else "M_Tile"
        for row in range(1, int(h / 0.32) + 1):
            for dx in ((-0.1, 0.1) if wdt > 0.4 else (0.0,)):
                b.box((0.08, 0.02, 0.12), (px + dx, 0.0 - 0.005, q + row * 0.3 - 0.1), shutter, bevel=0.003)
        b.box((0.12, 0.02, 0.22), (px, -0.005, q), "M_Wood", bevel=0.003)
        x += wdt + 0.005
    # octagonal minaret of the mosque behind the houses
    mx, my = -0.35, 0.95
    b.cylinder(0.17, 1.45, (mx, my, q), "M_Stone", segments=8)
    b.cylinder(0.21, 0.04, (mx, my, q + 1.2), "M_StoneDark", segments=8)
    b.cylinder(0.12, 0.22, (mx, my, q + 1.45), "M_Stone", segments=8)
    b.dome(0.12, (mx, my, q + 1.67), "M_TileGreen", segments=8, rings=3)
    b.cylinder(0.006, 0.07, (mx, my, q + 1.78), "M_Gold", segments=6)
    # kasbah wall along the east end, with a round bastion on the water
    kx = 1.55
    b.box((0.18, 1.5, 0.75), (kx, 0.72, q), "M_Sandstone")
    b.merlons(1.5, (kx, 0.72, q + 0.75), "M_Sandstone", rot=90, size=0.08, gap=0.06, height=0.08, depth=0.18)
    b.cylinder(0.26, 0.82, (kx - 0.05, -0.05, z), "M_Sandstone", segments=14)
    for i in range(8):
        a = 2 * math.pi * i / 8
        b.box((0.07, 0.07, 0.08), (kx - 0.05 + math.cos(a) * 0.22, -0.05 + math.sin(a) * 0.22, z + 0.82), "M_Sandstone", rot=math.degrees(a), bevel=0.005)
    # boats in the canal and bollards on the quay
    wz = z + 0.03
    b.boat(0.62, 0.22, (-0.9, -0.62, wz), rot=8)
    b.boat(0.55, 0.2, (0.05, -0.75, wz), rot=-12, band="M_PaintYellow")
    b.boat(0.7, 0.24, (0.85, -0.5, wz), rot=175)
    b.cylinder(0.004, 0.3, (0.85, -0.5, wz + 0.05), "M_Mast", segments=4)
    for bx in (-1.2, -0.4, 0.4, 1.0):
        b.cylinder(0.025, 0.05, (bx, -0.02 - 0.03, q), "M_StoneDark", segments=8)


def utique(b):
    """Utique: excavated Roman houses on a street grid, standing marble columns and cypress trees."""
    z = b.base(3.2, 3.2)
    stone, dark = "M_Stone", "M_StoneDark"
    # paved street crossing the site
    b.box((3.0, 0.32, 0.015), (0, -0.15, z), dark, bevel=0)
    for i in range(14):
        b.box((0.2, 0.3, 0.012), (-1.4 + i * 0.215, -0.15, z + 0.015), stone, bevel=0.004)
    # house foundations: rooms on both sides of the street
    for bx, by, sx, sy in ((-0.85, 0.65, 1.1, 0.9), (0.45, 0.7, 1.3, 1.0), (-0.75, -0.95, 1.2, 0.9), (0.55, -0.95, 1.0, 0.9)):
        b.rubble_wall(sx, 0.22, (bx, by - sy / 2, z), stone)
        b.rubble_wall(sx, 0.22, (bx, by + sy / 2, z), stone)
        b.rubble_wall(sy, 0.2, (bx - sx / 2, by, z), stone, rot=90)
        b.rubble_wall(sy, 0.18, (bx + sx / 2, by, z), stone, rot=90)
        b.rubble_wall(sy * 0.6, 0.15, (bx + sx * 0.1, by + sy * 0.2, z), dark, rot=90, thickness=0.07)
    b.mosaic(7, 5, 0.1, (0.45, 0.7, z), lambda i, j: "M_Terracotta" if i in (0, 6) or j in (0, 4) else ("M_Whitewash" if (i + j) % 2 else "M_StoneDark"))
    # standing columns of a peristyle, one broken, drums lying around
    for i, h in enumerate((0.62, 0.62, 0.34, 0.62)):
        x = -1.2 + i * 0.28
        b.box((0.11, 0.11, 0.04), (x, 0.3, z), stone, bevel=0.006)
        b.cylinder(0.04, h, (x, 0.3, z + 0.04), "M_Whitewash", segments=12)
        if h > 0.5:
            b.box((0.11, 0.11, 0.05), (x, 0.3, z + 0.04 + h), "M_Whitewash", bevel=0.006)
    b.box((0.4, 0.1, 0.05), (-1.06, 0.3, z + 0.04 + 0.62 + 0.05), stone, bevel=0.006)
    b.cylinder(0.04, 0.22, (-0.5, -0.45, z + 0.04), "M_Whitewash", segments=12, rot=(90, 0, 70))
    b.cylinder(0.04, 0.16, (0.2, -0.5, z + 0.04), "M_Whitewash", segments=12, rot=(90, 0, -20))
    # cypresses
    for x, y, h in ((-1.35, 1.3, 1.25), (-1.05, 1.35, 0.95), (1.3, 1.3, 1.35), (1.35, -1.3, 1.05)):
        b.cypress(h, (x, y, z))


MONUMENTS = {
    "Monument_GreatMosque": great_mosque,
    "Monument_ThreeDoors": three_doors,
    "Monument_Mausoleum": mausoleum,
    "Monument_Medina": medina,
    "Monument_Neapolis": neapolis,
    "Monument_OldPort": old_port,
    "Monument_Utique": utique,
}


def clear_scene():
    for obj in list(bpy.data.objects):
        bpy.data.objects.remove(obj)
    for mesh in list(bpy.data.meshes):
        if mesh.users == 0:
            bpy.data.meshes.remove(mesh)


def build(name):
    """Builds one monument in the open scene and returns its object."""
    existing = bpy.data.objects.get(name)
    if existing is not None:
        bpy.data.objects.remove(existing)
    b = Builder(name)
    MONUMENTS[name](b)
    return b.finish()


def export(obj, out_dir):
    os.makedirs(out_dir, exist_ok=True)
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.export_scene.fbx(
        filepath=os.path.join(out_dir, obj.name + ".fbx"),
        use_selection=True,
        object_types={"MESH"},
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        bake_space_transform=True,
        use_mesh_modifiers=True,
        mesh_smooth_type="FACE",
        colors_type="NONE",
        use_tspace=False,
        add_leaf_bones=False,
        bake_anim=False,
        embed_textures=False,
        path_mode="STRIP",
    )


def main(argv):
    args = argv[argv.index("--") + 1:] if "--" in argv else []
    out_dir = "Assets/DiscoverTunisia/Art/Models"
    if "--out" in args:
        i = args.index("--out")
        out_dir = args[i + 1]
        del args[i:i + 2]
    names = args or list(MONUMENTS)
    clear_scene()
    for name in names:
        export(build(name), out_dir)
        clear_scene()
    print("Exported", len(names), "monuments to", out_dir)


if __name__ == "__main__" and bpy.app.background:
    main(sys.argv)
