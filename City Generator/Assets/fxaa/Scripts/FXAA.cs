
using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent( typeof( Camera ) )]
[AddComponentMenu( "Image Effects/FXAA" )]
public class FXAA : FXAAPostEffectsBase	
{
	public Shader shader;
	private Material mat;
	
	void CreateMaterials () 
	{
		if ( mat == null )
			mat = CheckShaderAndCreateMaterial( shader, mat );
	}
	
	void Start() 
	{
		shader = Shader.Find( "Hidden/FXAA3" );
		CreateMaterials();
		CheckSupport( false );
	}

	public void OnRenderImage( RenderTexture source, RenderTexture destination )
	{	
		CreateMaterials();

		float rcpWidth = 1.0f / Screen.width;
		float rcpHeight = 1.0f / Screen.height;

		mat.SetVector( "_rcpFrame", new Vector4( rcpWidth, rcpHeight, 0, 0 ) );
		mat.SetVector( "_rcpFrameOpt", new Vector4( rcpWidth * 2, rcpHeight * 2, rcpWidth * 0.5f, rcpHeight * 0.5f ) );

		Graphics.Blit( source, destination, mat );
	}
}
	


