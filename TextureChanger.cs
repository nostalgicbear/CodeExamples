using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureChanger : MonoBehaviour {

    public List<Texture> mkTextures = new List<Texture>();

    public void SetTextureByIndex(int textureToSet)
    {
        
        if (textureToSet < 0 || textureToSet > mkTextures.Count - 1) //Check that the index value is in range
        {
            Debug.LogError("You tried to access index ' " + textureToSet + "' in the materialList array on object " + gameObject.name + ". Please enter a valid index for the material you wish to set");
            return;
        }
        
        GetComponent<Renderer>().material.mainTexture = mkTextures[textureToSet];
    }
}
