using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class TakeScreenshot : MonoBehaviour
{
    [SerializeField] Image whereToShowScreenshot;

    public IEnumerator TakeScreenshotAndShow()
    {
        yield return new WaitForEndOfFrame();

        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();

        Texture2D newScreenshot = new Texture2D(screenshot.width, screenshot.height, TextureFormat.RGB24, false);
        newScreenshot.SetPixels(screenshot.GetPixels());
        newScreenshot.Apply();

        Destroy(screenshot);

        Sprite screenshotSprite = Sprite.Create(newScreenshot, new Rect(0, 0, newScreenshot.width, newScreenshot.height), new Vector2(0.5f, 0.5f));

        whereToShowScreenshot.enabled = true;
        whereToShowScreenshot.sprite = screenshotSprite;
    }
}
