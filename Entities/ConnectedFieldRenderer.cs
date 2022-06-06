// Copyright (c) Shane Woolcock. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.GravityHelper.Entities
{
    public class ConnectedFieldRenderer<TEntity> : Entity
        where TEntity : Entity, IConnectableField
    {
        public string InitialRoom { get; private set; }

        protected ConnectedFieldRenderer()
        {
            Tag = Tags.TransitionUpdate;
            Depth = 1;

            Add(new CustomBloom(onRenderBloom));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            InitialRoom = SceneAs<Level>().Session.LevelData.Name;
        }

        public void Track(TEntity entity)
        {
            var fieldColor = entity.FieldColor;
            var fieldGroup = Components.GetAll<FieldGroupRenderer>().FirstOrDefault(f => f.Color == fieldColor);

            if (fieldGroup == null)
                Add(fieldGroup = new FieldGroupRenderer(fieldColor));

            fieldGroup.Track(entity, Scene);
        }

        public bool Untrack(TEntity entity, bool removeIfEmpty = false)
        {
            var fieldColor = entity.FieldColor;
            var found = false;

            foreach (var renderer in Components.GetAll<FieldGroupRenderer>().Where(f => f.Color == fieldColor))
            {
                if (renderer.Untrack(entity))
                {
                    found = true;
                    break;
                }
            }

            if (removeIfEmpty && !Components.GetAll<FieldGroupRenderer>().Any())
                RemoveSelf();

            return found;
        }

        private void onRenderBloom()
        {
            foreach (var fieldGroup in Components.GetAll<FieldGroupRenderer>())
                fieldGroup.OnRenderBloom();
        }

        private class FieldGroupRenderer : Component
        {
            public Color Color { get; }

            private readonly List<TEntity> _list = new List<TEntity>();
            private readonly List<Edge> _edges = new List<Edge>();
            private VirtualMap<bool> _tiles;
            private Rectangle _levelTileBounds;
            private bool _dirty;

            public FieldGroupRenderer(Color color) : base(true, true)
            {
                Color = color;
            }

            public void Track(TEntity entity, Scene scene)
            {
                _list.Add(entity);

                if (ensureTiles(scene))
                {
                    for (int x = (int) entity.X / 8; x < entity.Right / 8.0; ++x)
                    for (int y = (int) entity.Y / 8; y < entity.Bottom / 8.0; ++y)
                        _tiles[x - _levelTileBounds.X, y - _levelTileBounds.Y] = true;
                }

                _dirty = true;
            }

            public bool Untrack(TEntity entity, bool removeIfEmpty = true)
            {
                if (!_list.Remove(entity)) return false;

                _dirty = true;

                if (_list.Any())
                {
                    for (int x = (int) entity.X / 8; x < entity.Right / 8.0; ++x)
                    for (int y = (int) entity.Y / 8; y < entity.Bottom / 8.0; ++y)
                        _tiles[x - _levelTileBounds.X, y - _levelTileBounds.Y] = false;
                }
                else
                {
                    _tiles = null;
                    if (removeIfEmpty)
                        RemoveSelf();
                }

                return true;
            }

            private bool ensureTiles(Scene scene)
            {
                if (_tiles == null && scene is Level level)
                {
                    _levelTileBounds = level.TileBounds;
                    _tiles = new VirtualMap<bool>(_levelTileBounds.Width, _levelTileBounds.Height);

                    foreach (var entity in _list)
                    {
                        for (int x = (int) entity.X / 8; x < entity.Right / 8.0; ++x)
                        for (int y = (int) entity.Y / 8; y < entity.Bottom / 8.0; ++y)
                            _tiles[x - _levelTileBounds.X, y - _levelTileBounds.Y] = true;
                    }

                    _dirty = true;
                }

                return _tiles != null;
            }

            public override void EntityAdded(Scene scene)
            {
                base.EntityAdded(scene);
                ensureTiles(scene);
            }

            public override void Update()
            {
                if (_dirty)
                    rebuildEdges();
                updateEdges();
            }

            private void updateEdges()
            {
                Camera camera = SceneAs<Level>().Camera;
                Rectangle view = new Rectangle((int) camera.Left - 4, (int) camera.Top - 4,
                    (int) (camera.Right - (double) camera.Left) + 8,
                    (int) (camera.Bottom - (double) camera.Top) + 8);
                for (int index = 0; index < _edges.Count; ++index)
                {
                    if (_edges[index].Visible)
                    {
                        if (Scene.OnInterval(0.25f, index * 0.01f) && !_edges[index].InView(ref view))
                            _edges[index].Visible = false;
                    }
                    else if (Scene.OnInterval(0.05f, index * 0.01f) && _edges[index].InView(ref view))
                        _edges[index].Visible = true;

                    if (_edges[index].Visible &&
                        (Scene.OnInterval(0.05f, index * 0.01f) || _edges[index].Wave == null))
                        _edges[index].UpdateWave(Scene.TimeActive * 3f);
                }
            }

            private void rebuildEdges()
            {
                _dirty = false;
                _edges.Clear();
                if (_list.Count == 0 || _tiles == null)
                    return;

                Point[] pointArray =
                {
                    new Point(0, -1),
                    new Point(0, 1),
                    new Point(-1, 0),
                    new Point(1, 0),
                };

                foreach (var parent in _list)
                {
                    for (int x = (int) parent.X / 8; x < parent.Right / 8.0; ++x)
                    {
                        for (int y = (int) parent.Y / 8; y < parent.Bottom / 8.0; ++y)
                        {
                            foreach (Point point1 in pointArray)
                            {
                                Point point2 = new Point(-point1.Y, point1.X);
                                if (!inside(x + point1.X, y + point1.Y) &&
                                    (!inside(x - point2.X, y - point2.Y) || inside(x + point1.X - point2.X,
                                        y + point1.Y - point2.Y)))
                                {
                                    Point point3 = new Point(x, y);
                                    Point point4 = new Point(x + point2.X, y + point2.Y);
                                    Vector2 vector2 = new Vector2(4f) + new Vector2(point1.X - point2.X,
                                        point1.Y - point2.Y) * 4f;
                                    for (;
                                        inside(point4.X, point4.Y) && !inside(point4.X + point1.X, point4.Y + point1.Y);
                                        point4.Y += point2.Y)
                                        point4.X += point2.X;
                                    Vector2 a = new Vector2(point3.X, point3.Y) * 8f + vector2 - parent.Position;
                                    Vector2 b = new Vector2(point4.X, point4.Y) * 8f + vector2 - parent.Position;
                                    _edges.Add(new Edge(parent, a, b));
                                }
                            }
                        }
                    }
                }
            }

            private bool inside(int tx, int ty) => _tiles[tx - _levelTileBounds.X, ty - _levelTileBounds.Y];

            public void OnRenderBloom()
            {
                if (_list.Any(e => !e.Visible))
                    return;

                foreach (var entity in _list)
                    Draw.Rect(entity.X, entity.Y, entity.Width, entity.Height, Color.White);

                foreach (var edge in _edges)
                {
                    if (edge.Visible)
                    {
                        Vector2 vector2_1 = edge.Parent.Position + edge.A;
                        for (int index = 0; index <= (double) edge.Length; ++index)
                        {
                            Vector2 start = vector2_1 + edge.Normal * index;
                            Draw.Line(start, start + edge.Perpendicular * edge.Wave[index], Color.White);
                        }
                    }
                }
            }

            public override void Render()
            {
                if (_list.Count <= 0)
                    return;

                if (_list.Any(e => !e.Visible))
                    return;

                var color = Color;
                foreach (var entity in _list)
                    Draw.Rect(entity.Collider, color);

                if (_edges.Count == 0)
                    return;

                foreach (var edge in _edges)
                {
                    if (edge.Visible)
                    {
                        Vector2 vector2_1 = edge.Parent.Position + edge.A;
                        for (int index = 0; index <= (double) edge.Length; ++index)
                        {
                            Vector2 start = vector2_1 + edge.Normal * index;
                            Draw.Line(start, start + edge.Perpendicular * edge.Wave[index], color);
                        }
                    }
                }
            }

            private class Edge
            {
                public TEntity Parent;
                public bool Visible;
                public Vector2 A;
                public Vector2 B;
                public Vector2 Min;
                public Vector2 Max;
                public Vector2 Normal;
                public Vector2 Perpendicular;
                public float[] Wave;
                public float Length;

                public Edge(TEntity parent, Vector2 a, Vector2 b)
                {
                    Parent = parent;
                    Visible = true;
                    A = a;
                    B = b;
                    Min = new Vector2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
                    Max = new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
                    Normal = (b - a).SafeNormalize();
                    Perpendicular = -Normal.Perpendicular();
                    Length = (a - b).Length();
                }

                public void UpdateWave(float time)
                {
                    if (Wave == null || Wave.Length <= (double) Length)
                        Wave = new float[(int) Length + 2];
                    for (int index = 0; index <= (double) Length; ++index)
                        Wave[index] = getWaveAt(time, index, Length);
                }

                private static float getWaveAt(float offset, float along, float length)
                {
                    if (along <= 1.0 || along >= length - 1.0) // || (double)Parent.Solidify >= 1.0)
                        return 0.0f;
                    float num = offset + along * 0.25f;
                    return (float) ((1.0 + (Math.Sin(num) * 2.0 + Math.Sin(num * 0.25)) *
                        Ease.SineInOut(Calc.YoYo(along / length)))); // * (1.0 - (double)Parent.Solidify));
                }

                public bool InView(ref Rectangle view) => view.Left < Parent.X + (double) Max.X &&
                                                          view.Right > Parent.X + (double) Min.X &&
                                                          view.Top < Parent.Y + (double) Max.Y &&
                                                          view.Bottom > Parent.Y + (double) Min.Y;
            }
        }
    }

    public static class ConnectedFieldRendererExtensions
    {
        public static TConnectedFieldRenderer GetConnectedFieldRenderer<TConnectedFieldRenderer, TEntity>(this TEntity entity, Scene scene = null, bool? track = null)
            where TEntity : Entity, IConnectableField
            where TConnectedFieldRenderer : ConnectedFieldRenderer<TEntity>, new()
        {
            scene = scene as Level ?? entity.Scene as Level ?? Engine.Scene as Level;
            if (scene is not Level level) return null;
            var roomName = level.Session.LevelData.Name;

            if (track == false)
            {
                foreach (var r in scene.Entities.OfType<TConnectedFieldRenderer>())
                {
                    if (r.Untrack(entity))
                        return r;
                }
            }

            var renderer = scene.Entities.OfType<TConnectedFieldRenderer>().FirstOrDefault(r => r.InitialRoom == roomName) ??
                scene.Entities.ToAdd.OfType<TConnectedFieldRenderer>().FirstOrDefault();

            if (track == true)
            {
                if (renderer == null)
                    scene.Add(renderer = new TConnectedFieldRenderer());
                renderer.Track(entity);
            }

            return renderer;
        }
    }
}
