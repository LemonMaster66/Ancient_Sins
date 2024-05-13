using UnityEngine;

public class Photo
{
    public Texture2D image;
    public float value;

    public Photo(Texture2D NewImage, float NewValue)
    {
        image = NewImage;
        value = NewValue;
    }
}
