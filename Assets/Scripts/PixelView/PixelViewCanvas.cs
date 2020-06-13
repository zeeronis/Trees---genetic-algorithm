using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PixelViewCanvas: MonoBehaviour
{
    private static PixelViewCanvas instance;
    public static PixelViewCanvas Instance { get => instance; set => instance = value; }

    private const float pixelOffset = 1f;

    [SerializeField] private PixelUI pixelPrefab;
    [SerializeField] private Transform backgroundTransform;
    [SerializeField] private Image backgroundImage;

    private int pixelSize;
    private Color defaultPixelsColor = Color.gray;
    public Color defaultBackgroundColor = Color.black;

    private Vector2Int mapSize;
    public PixelUI[,] pixels;

  
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void SetDefaultBackgroundColor(Color color)
    {
        backgroundImage.color = color;
    }

    public void SetDefaultPixelColor(Color color)
    {
        backgroundImage.color = color;
    }

    public void SetPixelIn(int x, int y, Color color)
    {
        pixels[x, y].image.color = color;
        //pixels[x, y].color = color;
    }

   /* public Color GetPixelIn(int x, int y)
    {
        return pixels[x, y].color;
    }*/

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
                SetPixelIn(x, y, Color.black);
            }
        }
    }

    public void InitView(Vector2Int _mapSize, int _pixelSize = 6)
    {
        pixelSize = _pixelSize;
        mapSize = _mapSize;

        backgroundImage.color = defaultBackgroundColor;

        pixels = new PixelUI[_mapSize.x, _mapSize.y];
        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                var pixel = Instantiate(pixelPrefab, backgroundTransform);
                pixel.image.color = defaultPixelsColor;
                pixel.color = defaultPixelsColor;
                pixels[x, y] = pixel;

                var rectTransform = pixel.GetComponent<RectTransform>();
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pixelSize);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pixelSize);

                var position = new Vector2(
                    10 + pixelSize / 2 + pixelSize * x + pixelOffset * x,
                    10 + pixelSize / 2 + pixelSize * y + pixelOffset * y);
                pixel.transform.position = position;
            }
        }
    }

    public void InitView(Vector2Int _mapSize, Color32 _defaultPixelsColor,  int _pixelSize = 6)
    {
        defaultPixelsColor = _defaultPixelsColor;
        InitView(_mapSize, _pixelSize);
    }

    public void InitView(Vector2Int _mapSize, Color32 _defaultPixelsColor, Color32 _defaultbackgroundColor, int _pixelSize = 6)
    {
        defaultBackgroundColor = _defaultbackgroundColor;
        InitView(_mapSize, _defaultPixelsColor, _pixelSize);
    }
}

