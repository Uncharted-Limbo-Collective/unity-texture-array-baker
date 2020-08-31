using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class TextureArrayTest : MonoBehaviour
{
    public MeshRenderer mr;
    public Texture2DArray texArray;
    
    [Range(0,2048)]
    public int frame;
   
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (texArray == null)
            return;

        if (mr == null)
            return;

        mr.sharedMaterial.SetTexture("_TextureArray", texArray);
        mr.sharedMaterial.SetInt("_Frame", frame);  
    }
    
    
}
