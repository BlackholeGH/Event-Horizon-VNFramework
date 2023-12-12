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
                return tileTexture;
            }
        }
    }
}
