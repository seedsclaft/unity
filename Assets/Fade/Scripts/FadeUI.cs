using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent (typeof(RawImage))]
[RequireComponent (typeof(Mask))]
public class FadeUI : UnityEngine.UI.Graphic, IFade
{
    [SerializeField]
    private Texture maskTexture = null;

    [SerializeField, Range (0,1)]
	private float cutoutRange;

    protected override void Start() 
	{
        base.Start();
        UpdateMaskTexture(maskTexture);
    }

    public void UpdateMaskTexture(Texture texture) 
	{
        material.SetTexture("_MaskTex",texture);
        material.SetColor("_Color",color);
    }

    public float Range {
		get {
			return cutoutRange;
		}
		set {
			cutoutRange = value;
			UpdateMaskCutout (cutoutRange);
		}
	}

	[SerializeField] Material mat = null;
	[SerializeField] RenderTexture rt = null;

	[SerializeField] Texture texture = null;

	private void UpdateMaskCutout (float range)
	{
//#if !UNITY_EDITOR
		mat.SetFloat ("_Range", range);
		rt.Release();
		rt.width = Screen.width;
		rt.height = Screen.height;
		rt.Create();
		
		Graphics.Blit (texture, rt, mat);
		
		var mask = GetComponent<Mask> ();
		mask.enabled = false;
		mask.enabled = true;
//#endif
	}

#if UNITY_EDITOR
    protected override void OnValidate() 
	{
        base.OnValidate();
        UpdateMaskCutout(Range);
        UpdateMaskTexture(maskTexture);
    }
#endif

}