using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;

namespace Content.Client._DV.NanoChat.UI;

/// <summary>
/// Extremely specialized version of <see cref="Robust.Client.UserInterface.Controls.TextureRect" /> designed for drawing Textures at fixed size with an additional offset
/// Circumvents the problem of not having "negative mergins" on TextureRect for manual alignment
/// This is fucking evil, please do not use this :3
/// </summary>
public sealed class EmojiControl : Control
{
    public const string StylePropertyTexture = "texture";
    public const string StylePropertyOffset = "offset";

    private Texture? _texture;
    private Vector2? _offset;

    private string? _texturePath;

    public string TexturePath
    {
        set
        {
            Texture = IoCManager.Resolve<IResourceCache>().GetResource<TextureResource>(value);
            _texturePath = value;
        }

    }

    protected override void OnThemeUpdated()
    {
        if (_texturePath != null) Texture = Theme.ResolveTexture(_texturePath);
        base.OnThemeUpdated();
    }

    /// <summary>
    ///     The texture to draw.
    /// </summary>
    public Texture? Texture
    {
        get
        {
            if (_texture is null)
            {
                if (TryGetStyleProperty(StylePropertyTexture, out Texture? texture))
                {
                    return texture;
                }
            }

            return _texture;
        }
        set
        {
            var oldSize = _texture?.Size;
            _texture = value;

            if (value?.Size != oldSize)
            {
                InvalidateMeasure();
            }
        }
    }

    /// <summary>
    ///     Vertical offset of the texture
    /// </summary>
    public Vector2 Offset
    {
        get
        {
            if (_offset is null)
            {
                if (TryGetStyleProperty(StylePropertyOffset, out Vector2 offset))
                    return offset;
            }

            return _offset ?? Vector2.Zero;
        }
        set
        {
            var oldOffset = _offset;
            _offset = value;

            if (oldOffset != value)
            {
                InvalidateArrange();
            }
        }
    }

    protected override void ArrangeCore(UIBox2 finalRect)
    {
        finalRect.Left += Offset.X;
        finalRect.Right += Offset.X;
        finalRect.Top += Offset.Y;
        finalRect.Bottom += Offset.Y;
        base.ArrangeCore(finalRect);
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        var texture = Texture;

        if (texture is null)
            return;

        handle.DrawTextureRect(texture, GetDrawDimensions());
    }

    private UIBox2 GetDrawDimensions()
    {
        var (texWidth, texHeight) = Texture?.Size ?? Vector2i.Zero;
        var width = texWidth * (PixelSize.Y / texHeight);
        var height = (float)PixelSize.Y;
        if (width > PixelSize.X)
        {
            width = PixelSize.X;
            height = texHeight * (PixelSize.X / texWidth);
        }

        var size = new Vector2(width, height);
        var position = (PixelSize - size) / 2 + Offset;

        return UIBox2.FromDimensions(position, size);
    }
}
