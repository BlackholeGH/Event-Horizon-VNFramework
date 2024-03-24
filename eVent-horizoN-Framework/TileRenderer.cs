using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using static System.Net.Mime.MediaTypeNames;
using System.Linq.Expressions;
using System.Windows.Forms;

namespace VNFramework
{
    public static partial class GraphicsTools
    {
        public static class TileRenderer
        {
            public struct Tileset
            {
                public Tileset(TAtlasInfo? tileAtlas = null, Dictionary<int, Point> tileLookup = null, Vector2[][] tileOffset = null, Vector2? tileOrigin = null, Vector2? drawScale = null, Color? tint = null, Color ? background = null)
                {
                    TileAtlas = tileAtlas ?? null;
                    TileLookup = tileLookup ?? null;
                    TileOffset = tileOffset ?? new Vector2[][] { new Vector2[] { TileAtlas != null ? VNFUtils.ConvertPoint(((TAtlasInfo)TileAtlas).DivDimensions) : new Vector2(20, 20) } };
                    TileOrigin = tileOrigin ?? new Vector2(0, 0);
                    DrawScale = drawScale ?? new Vector2(1, 1);
                    Tint = tint ?? new Color(255, 255, 255, 255);
                    Background = background ?? new Color(0, 0, 0, 0);
                }
                public TAtlasInfo? TileAtlas;
                public Dictionary<int, Point> TileLookup;
                public Vector2[][] TileOffset;
                public Vector2 TileOrigin;
                public Vector2 DrawScale;
                public Color Tint;
                public Color Background;
            }
            public static Texture2D RenderTiles(Tileset tileSet, int[,] tiles)
            {
                return RenderTiles(tileSet, tiles, null);
            }
            public static Texture2D RenderTiles(Tileset tileSet, int[,] tiles, Texture2D inputTexture)
            {
                if (tileSet.TileAtlas == null)
                {
                    throw new VNFUtils.EventHorizonException("Exception in TileRenderer pipeline: Tileset did not provide a texture atlas.");
                }
                GraphicsDevice graphicsDevice = Shell.PubGD;
                TAtlasInfo atlas = (TAtlasInfo)tileSet.TileAtlas;
                Point gridDims = new Point(tiles.GetLength(0), tiles.GetLength(1));
                Vector2 drawOrigin = tileSet.TileOrigin;
                Vector2 yLocation = drawOrigin;
                Vector2 xLocation = drawOrigin;
                Vector2 drawMax = new Vector2();
                Vector2 drawMin = new Vector2();
                //Calculate maximum texture size accounting for atypical offsets.
                for (int y = 0; y < gridDims.Y; y++)
                {
                    xLocation = yLocation;
                    for (int x = 0; x < gridDims.X; x++)
                    {
                        xLocation += tileSet.TileOffset[0][x % tileSet.TileOffset[0].Length];
                        if (xLocation.X + atlas.FrameSize().X > drawMax.X) { drawMax.X = xLocation.X + atlas.FrameSize().X; }
                        if (xLocation.Y + atlas.FrameSize().Y > drawMax.Y) { drawMax.Y = xLocation.Y + atlas.FrameSize().Y; }
                        if (xLocation.X < drawMin.X) { drawMin.X = xLocation.X; }
                        if (xLocation.Y < drawMin.Y) { drawMin.Y = xLocation.Y; }
                    }
                    yLocation += tileSet.TileOffset[1][y % tileSet.TileOffset[1].Length];
                }
                if(drawMin.X < 0)
                {
                    drawMax.X += -drawMin.X;
                    drawOrigin.X += -drawMin.X;
                }
                if (drawMin.Y < 0)
                {
                    drawMax.Y += -drawMin.Y;
                    drawOrigin.Y += -drawMin.Y;
                }
                Vector2 textureSize = drawMax;
                //Prepare to draw texture
                RenderTarget2D renderPane = new RenderTarget2D(graphicsDevice, (int)Math.Ceiling(textureSize.X), (int)Math.Ceiling(textureSize.Y), false,
                        graphicsDevice.PresentationParameters.BackBufferFormat,
                        DepthFormat.Depth24);
                if (inputTexture != null && inputTexture.Bounds.Size != new Point((int)Math.Ceiling(textureSize.X), (int)Math.Ceiling(textureSize.Y))) { return inputTexture; }
                graphicsDevice.SetRenderTarget(renderPane);
                graphicsDevice.Clear(tileSet.Background);
                SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
                Point atlasCoordinates = new Point(0, 0);
                Texture2D currentTexture = atlas.Atlas;
                Point size = atlas.FrameSize() * VNFUtils.ConvertVector(tileSet.DrawScale);
                //Draw texture
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
                if(inputTexture != null) { spriteBatch.Draw(inputTexture, new Vector2(), Color.White); }
                yLocation = drawOrigin;
                xLocation = drawOrigin;
                for (int y = 0; y < gridDims.Y; y++)
                {
                    xLocation = yLocation;
                    for(int x = 0; x < gridDims.X; x++)
                    {
                        currentTexture = atlas.Atlas;
                        atlasCoordinates = tileSet.TileLookup[tiles[x, y]];
                        spriteBatch.Draw(currentTexture, new Rectangle(VNFUtils.ConvertVector(xLocation), size), new Rectangle(new Point((atlas.SourceRect.Width / atlas.DivDimensions.X) * atlasCoordinates.X, (atlas.SourceRect.Height / atlas.DivDimensions.Y) * atlasCoordinates.Y), atlas.FrameSize()), tileSet.Tint);
                        xLocation += tileSet.TileOffset[0][x % tileSet.TileOffset[0].Length];
                    }
                    yLocation += tileSet.TileOffset[1][y % tileSet.TileOffset[1].Length];
                }
                spriteBatch.End();
                graphicsDevice.SetRenderTarget(null);
                Texture2D tileTexture = VNFUtils.GetFromRT(renderPane);
                renderPane.Dispose();
                return tileTexture;
            }
            public class TileDisplay : WorldEntity
            {
                public class TileAnimInfo
                {
                    public int Tile;
                    public Boolean Animated;
                    public int FrameLength;
                    public int Countdown;
                }
                public Boolean SetTiles(Queue<TileAnimInfo>[,] tileStates)
                {
                    _tileStates = tileStates;
                    return true;
                }
                public Boolean SetTiles(TileAnimInfo[,][] tileStates)
                {
                    try
                    {
                        _tileStates = new Queue<TileAnimInfo>[tileStates.GetLength(0), tileStates.GetLength(1)];
                        for (int y = 0; y < tileStates.GetLength(1); y++)
                        {
                            for (int x = 0; x < tileStates.GetLength(0); x++)
                            {
                                _tileStates[x, y] = new Queue<TileAnimInfo>(tileStates[x, y]);
                            }
                        }
                    }
                    catch { return false; }
                    return true;
                }
                public Boolean SetTiles(TileAnimInfo[,] tileStates)
                {
                    try
                    {
                        _tileStates = new Queue<TileAnimInfo>[tileStates.GetLength(0), tileStates.GetLength(1)];
                        for (int y = 0; y < tileStates.GetLength(1); y++)
                        {
                            for (int x = 0; x < tileStates.GetLength(0); x++)
                            {
                                _tileStates[x, y] = new Queue<TileAnimInfo>();
                                _tileStates[x, y].Enqueue(tileStates[x, y]);
                            }
                        }
                    }
                    catch { return false; }
                    return true;
                }
                public Boolean SetTiles(int[,] tileStates)
                {
                    try
                    {
                        _tileStates = new Queue<TileAnimInfo>[tileStates.GetLength(0), tileStates.GetLength(1)];
                        for (int y = 0; y < tileStates.GetLength(1); y++)
                        {
                            for (int x = 0; x < tileStates.GetLength(0); x++)
                            {
                                _tileStates[x, y] = new Queue<TileAnimInfo>();
                                TileAnimInfo tileAnimInfo = new TileAnimInfo();
                                tileAnimInfo.Tile = tileStates[x, y];
                                tileAnimInfo.Animated = false;
                                _tileStates[x, y].Enqueue(tileAnimInfo);
                            }
                        }
                    }
                    catch { return false; }
                    return true;
                }
                public Boolean SetTiles(int[,][][] tileStates)
                {
                    try
                    {
                        _tileStates = new Queue<TileAnimInfo>[tileStates.GetLength(0), tileStates.GetLength(1)];
                        for (int y = 0; y < tileStates.GetLength(1); y++)
                        {
                            for (int x = 0; x < tileStates.GetLength(0); x++)
                            {
                                _tileStates[x, y] = new Queue<TileAnimInfo>();
                                foreach (int[] animParams in tileStates[x, y])
                                {
                                    TileAnimInfo tileAnimInfo = new TileAnimInfo();
                                    if (animParams.Length > 0) { tileAnimInfo.Tile = animParams[0]; }
                                    if (animParams.Length > 1)
                                    {
                                        tileAnimInfo.Animated = true;
                                        tileAnimInfo.FrameLength = animParams[1];
                                        tileAnimInfo.Countdown = animParams[1];
                                    }
                                    if (animParams.Length > 2) { tileAnimInfo.Countdown = animParams[2]; }
                                    if (animParams.Length > 3) { tileAnimInfo.Animated = animParams[3] != 0; }
                                    _tileStates[x, y].Enqueue(tileAnimInfo);
                                }
                            }
                        }
                    }
                    catch { return false; }
                    return true;
                }
                //Extract nested arrays from string representation such that the data can be used to set tile states
                public static int[,][][] ParseTileInfoString(string tileStates)
                {
                    try
                    {
                        tileStates = tileStates.Replace(" ", "").Replace("\n", "");
                        while (!VNFUtils.Strings.ContainsExclosedFromNestingChars(tileStates, ',', '{', '}'))
                        {
                            tileStates = tileStates.Remove(0, tileStates.IndexOf("{") + 1);
                            tileStates = tileStates.Remove(tileStates.LastIndexOf("}"));
                        }
                        String[] rowDefinitions = VNFUtils.Strings.SplitAtExclosedFromNestingChars(tileStates, ',', '{', '}');
                        List<string>[] rowElements = new List<string>[rowDefinitions.Length];
                        int i = 0;
                        foreach (String rowDefinition in rowDefinitions)
                        {
                            String rawRowDef = rowDefinition.Remove(0, rowDefinition.IndexOf("{") + 1);
                            rawRowDef = rawRowDef.Remove(rawRowDef.LastIndexOf("}"));
                            rowElements[i] = new List<string>(VNFUtils.Strings.SplitAtExclosedFromNestingChars(rawRowDef, ',', '{', '}'));
                            i++;
                        }
                        int[,][][] tileGrid = new int[rowElements[0].Count, rowDefinitions.Length][][];
                        for (int y = 0; y < rowDefinitions.Length; y++)
                        {
                            for (int x = 0; x < rowElements[0].Count; x++)
                            {
                                String gridElem = rowElements[y][x];
                                if (!gridElem.Contains("{"))
                                {
                                    tileGrid[x, y] = new int[][] { new int[] { Convert.ToInt32(gridElem) } };
                                }
                                else
                                {
                                    Console.WriteLine(gridElem);
                                    gridElem = gridElem.Remove(0, gridElem.IndexOf("{") + 1);
                                    gridElem = gridElem.Remove(gridElem.LastIndexOf("}"));
                                    String[] gridElemR = VNFUtils.Strings.SplitAtExclosedFromNestingChars(gridElem, ',', '{', '}');
                                    if (!gridElem.Contains('{'))
                                    {
                                        tileGrid[x, y] = new int[][] { new int[4] };
                                        for (i = 0; i < 4; i++)
                                        {
                                            tileGrid[x, y][0][i] = Convert.ToInt32(gridElemR[i]);
                                        }
                                    }
                                    else
                                    {
                                        tileGrid[x, y] = new int[gridElemR.Length][];
                                        for (i = 0; i < gridElemR.Length; i++)
                                        {
                                            String tileFrame = gridElemR[i];
                                            if (!tileFrame.Contains('{'))
                                            {
                                                tileGrid[x, y][i] = new int[] { Convert.ToInt32(tileFrame) };
                                            }
                                            else
                                            {
                                                tileFrame = tileFrame.Remove(0, tileFrame.IndexOf("{") + 1);
                                                tileFrame = tileFrame.Remove(tileFrame.LastIndexOf("}"));
                                                tileGrid[x, y][i] = tileFrame.Split(',').Select(x => Convert.ToInt32(x)).ToArray();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        return tileGrid;
                    }
                    catch (Exception e)
                    {
                        throw new VNFUtils.EventHorizonException("Malformed tile state grid definition. Error data: " + e.ToString());
                    }
                }
                private Queue<TileAnimInfo>[,] _tileStates = new Queue<TileAnimInfo>[0,0];
                public TileDisplay(String name, Vector2 location, float depth, Tileset tileset) : base(name, location, null, depth)
                {
                    Tileset = tileset;
                }
                public int[,] GetTiles()
                {
                    int[,] tileInts = new int[_tileStates.GetLength(0), _tileStates.GetLength(1)];
                    for(int y = 0; y < tileInts.GetLength(1); y++)
                    {
                        for (int x = 0; x < tileInts.GetLength(0); x++)
                        {
                            tileInts[x, y] = _tileStates[x, y].Peek().Tile;
                        }
                    }
                    return tileInts;
                }
                private int[,] _liveTiles;
                private Boolean _needUpdateRender = false;
                public Boolean NeedUpdateRender { get { return _needUpdateRender; } }
                public override void Update()
                {
                    base.Update();
                    double elapsedMillis = Shell.LastUpdateGameTime.ElapsedGameTime.TotalMilliseconds;
                    if(UpdateTiles(elapsedMillis, false) == 1)
                    {
                        _needUpdateRender = true;
                    }
                }
                public void InitializeTileDisplay()
                {
                    UpdateTiles(0, true);
                    _needUpdateRender = true;
                }
                //Update tile animation frames and live tile lookups
                private int UpdateTiles(double elapsed, Boolean init)
                {
                    int returnCode = 0;
                    if(init)
                    {
                        _liveTiles = new int[_tileStates.GetLength(0), _tileStates.GetLength(1)];
                    }
                    for (int y = 0; y < _liveTiles.GetLength(1); y++)
                    {
                        for (int x = 0; x < _liveTiles.GetLength(0); x++)
                        {
                            TileAnimInfo current = _tileStates[x, y].Peek();
                            Boolean push = init;
                            if(current.Animated)
                            {
                                current.Countdown -= (int)Math.Round(elapsed);
                                if(current.Countdown <= 0)
                                {
                                    current.Countdown = current.FrameLength;
                                    _tileStates[x, y].Dequeue();
                                    _tileStates[x, y].Enqueue(current);
                                    push = true;
                                }
                            }
                            if(push)
                            {
                                _liveTiles[x, y] = _tileStates[x, y].Peek().Tile;
                                returnCode = 1;
                            }
                        }
                    }
                    return returnCode;
                }
                public void DoRenderUpdate()
                {
                    _needUpdateRender = false;
                    _currentTileTexture?.Dispose();
                    _currentTileTexture = RenderTiles(Tileset, _liveTiles, BackgroundTexture);
                }
                public Tileset Tileset { get; set; }
                private Texture2D _currentTileTexture = null;
                public Texture2D CurrentTileTexture
                {
                    get { return _currentTileTexture; }
                }
                public Texture2D BackgroundTexture { get; set; }
                public override void Draw(SpriteBatch spriteBatch)
                {
                    spriteBatch.Draw(_currentTileTexture, Position, _currentTileTexture.Bounds, ColourValue, RotationRads + _flipRotationAddit, AdjustedOrigin, Size, LocalSpriteEffect, LayerDepth);
                }
                public override void Draw(SpriteBatch spriteBatch, Camera camera)
                {
                    if (CameraImmune) { Draw(spriteBatch); }
                    else
                    {
                        spriteBatch.Draw(_currentTileTexture, (Position + camera.OffsetVector) * camera.ZoomFactor, _currentTileTexture.Bounds, ColourValue, RotationRads + _flipRotationAddit, AdjustedOrigin, Size * camera.ZoomFactor, LocalSpriteEffect, LayerDepth);
                    }
                }
            }
        }
    }
}
