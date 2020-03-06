using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PixelViewScene : MonoBehaviour
{
    private static PixelViewScene instance;
    public static PixelViewScene Instance { get => instance; set => instance = value; }

    private const float pixelOffset = 0.05f;

    [SerializeField] private Pixel pixelPrefab;
    [SerializeField] private Transform backgroundTransform;
    [SerializeField] private SpriteRenderer backgroundSprite;

    private float pixelNativeSize = 0.16f;
    private float pixelScale;
    private bool pixelsOutlineEnable;
    private Color defaultPixelsColor = Color.gray;
    private Color defaultPixelsOutline = Color.white;
    public Color defaultBackgroundColor = Color.black;

    private Vector2Int mapSize;
    public Pixel[,] pixels;


    private void Start()
    {
        if (Instance == null)
            Instance = this;
    }

    public void SetDefaultBackgroundColor(Color color)
    {
        backgroundSprite.color = color;
    }

    public void SetDefaultPixelColor(Color color)
    {
        backgroundSprite.color = color;
    }

    public void SetPixelIn(int x, int y, Color color)
    {
        if (pixels[x, y].color.r == color.r &&
            pixels[x, y].color.g == color.g &&
            pixels[x, y].color.b == color.b)
            return;

        pixels[x, y].image.color = color;
        pixels[x, y].color = color;

        if (pixelsOutlineEnable == true)
        {
            /*if (pixels[x, y].color.r == backgroundSprite.color.r &&
            pixels[x, y].color.g == backgroundSprite.color.g &&
            pixels[x, y].color.b == backgroundSprite.color.b)
            {
                pixels[x, y].outline.enabled = false;
            }
            else if (pixels[x, y].outline.enabled == false)
            {
                pixels[x, y].outline.enabled = true;
            }*/
        }
    }

    public Color GetPixelIn(int x, int y)
    {
        return pixels[x, y].color;
    }

    public bool IsDefaultPixel(int x, int y)
    {
        if (pixels[x, y].color.r == defaultBackgroundColor.r &&
            pixels[x, y].color.g == defaultBackgroundColor.g &&
            pixels[x, y].color.b == defaultBackgroundColor.b)
            return true;
        return false;
    }

    public void Clear()
    {
        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                SetPixelIn(x, y, defaultPixelsColor);
            }
        }
    }

    public void InitView(Vector2Int _mapSize, float _pixelScale = 1f, bool _pixelOutlineEnable = true)
    {
        pixelsOutlineEnable = _pixelOutlineEnable;
        pixelScale = 0.5f;
        mapSize = _mapSize;

        backgroundSprite.color = defaultBackgroundColor;

        pixels = new Pixel[_mapSize.x, _mapSize.y];
        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                var pixel = Instantiate(pixelPrefab, backgroundTransform);
                pixel.transform.localScale = new Vector3(pixelScale, pixelScale, pixelScale);
                pixel.image.color = defaultPixelsColor;
                pixel.color = defaultPixelsColor;
                pixels[x, y] = pixel;

                //var rectTransform = pixel.GetComponent<RectTransform>();
                //rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pixelSize);
                //rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pixelSize);


                var position = new Vector2(
                    backgroundTransform.position.x + (pixelNativeSize * pixelScale) * x + pixelOffset * x,
                    backgroundTransform.position.y + (pixelNativeSize * pixelScale) * y + pixelOffset * y);
                pixel.transform.position = position;
            }
        }
    }

    public void InitView(Vector2Int _mapSize, Color32 _defaultPixelsColor, float _pixelScale = 1, bool _pixelOutlineEnable = true)
    {
        defaultPixelsColor = _defaultPixelsColor;
        InitView(_mapSize, _pixelScale, _pixelOutlineEnable);
    }

    public void InitView(Vector2Int _mapSize, Color32 _defaultPixelsColor, Color32 _defaultbackgroundColor, float _pixelScale = 1, bool _pixelOutlineEnable = true)
    {
        defaultBackgroundColor = _defaultbackgroundColor;
        InitView(_mapSize, _defaultPixelsColor, _pixelScale, _pixelOutlineEnable);
    }
}

