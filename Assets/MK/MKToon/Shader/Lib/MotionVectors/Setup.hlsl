//////////////////////////////////////////////////////
// MK Toon Motion Vectors Setup			       		//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_MOTION_VECTORS_SETUP
	#define MK_TOON_MOTION_VECTORS_SETUP

	#ifndef MK_MOTION_VECTORS_PASS
		#define MK_MOTION_VECTORS_PASS
	#endif

	// Use URP helpers only: ObjectMotionVectors.hlsl also declares entry points
	// and instancing pragmas, which are already owned by the MK Toon passes.
	#ifdef MK_URP
		#include_with_pragmas "../DotsInstancingSetup.hlsl"
		#if UNITY_VERSION >= 202320
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
		#endif
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityInput.hlsl"
		#if UNITY_VERSION >= 202220 && defined(LOD_FADE_CROSSFADE)
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
		#endif
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MotionVectorsCommon.hlsl"
	#endif

	#include_with_pragmas "../Core.hlsl"

	//Hightmap is only needed if a UV is required
	#if !defined(MK_TEXCLR) && !defined(MK_DISSOLVE)
		#ifdef MK_PARALLAX
			#undef MK_PARALLAX
		#endif
	#endif

	#include "ProgramMotionVectors.hlsl"
#endif