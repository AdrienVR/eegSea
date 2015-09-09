Shader "Custom/seaSurface4Shader" {
	Properties {
		_DeepColor ("DeepColor", Color) = (0.0,0.25,0.5,1)
		_WaveMap ("WaveMap", 2D) = "bump" {}
		_Cube ("Cubemap", CUBE) = "" {}
		_windDir ("wind direction", Range(0.0, 6.28))=0
		_offsetRot ("rotation offset", Range(0.0, 6.28))=0
		_mapscale ("bumpmap scale", float)=0.1
		_coeffsHF("coefHF, coefHF1, coefHF2 and coefHF3", Vector)=(1,1,0.5,0.25)
		//_coefHF ("coefHF", Range(0.1,2)) = 1
		//_coefHF1 ("coefHF1", Range(0,1)) = 1
		//_coefHF2 ("coefHF2", Range(0,0.75)) = 0.5
		//_coefHF3 ("coefHF3", Range(0,0.5)) = 0.25
		
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_Specular ("Specular", Range (0.03, 1)) = 0.078125
		_Gloss    ("Gloss", Float) = 1
		
		_dStart ("Start attenuation",Float)= 1000
		_dFall("Attenuation during",Float)= 1000
		
		_WavesAmount ("Waves amount", Range (0.00,1.0)) = 0.5
		_MinReflectivity("Minimum reflectivity", Range(0.0,0.5)) = 0.
		
		_Wave1Parameters("1 angle, radius, period and lambda",Vector)=(0.0,0.8,30,70)
		_Wave1GroupParam("1 group density ]0,1]; wave advance [0,pi/2]",Vector)=(1,1,0,0)
		//_Wave1density("1 wave density ]0,1]",Range (0.001,1.0))=1
		//_Wave1advance("1 wave advance [0,pi/2]",Range (0.00,1.5707963))=1
		_Wave2Parameters("2  angle, radius, period and lambda",Vector)=(0.0,0.8,30,70)
		_Wave2GroupParam("2 group density ]0,1]; wave advance [0,pi/2]",Vector)=(1,1,0,0)
		//_Wave2density("2 wave density ]0,1]",Range (0.001,1.0))=1
		//_Wave2advance("2 wave advance [0,pi/2]",Range (0.00,1.5707963))=1
		_Wave3Parameters("3  angle, radius, period and lambda",Vector)=(0.3,0.2, 8,10)
		_Wave3GroupParam("3 group density ]0,1]; wave advance [0,pi/2]",Vector)=(1,1,0,0)
		//_Wave3density("3 wave density ]0,1]",Range (0.001,1.0))=1
		//_Wave3advance("3 wave advance [0,pi/2]",Range (0.00,1.5707963))=1
		_Wave4Parameters("4  angle, radius, period and lambda",Vector)=(0.4,0.1, 5, 9)
		_Wave4GroupParam("4 group density ]0,1]; wave advance [0,pi/2]",Vector)=(1,1,0,0)
		//_Wave4density("4 wave density ]0,1]",Range (0.001,1.0))=1
		//_Wave4advance("3 wave advance [0,pi/2]",Range (0.00,1.5707963))=1
		_Wave5Parameters("5  angle, radius, period and lambda",Vector)=(1.5,0.1,10,10)
		_Wave5GroupParam("5 group density ]0,1]; wave advance [0,pi/2]",Vector)=(1,1,0,0)
		//_Wave5density("5 wave density ]0,1]",Range (0.001,1.0))=1
		//_Wave5advance("5 wave advance [0,pi/2]",Range (0.00,1.5707963))=1
		_Wave6Parameters("6  angle, radius, period and lambda",Vector)=(1.0,0.1, 9, 7)
		_Wave6GroupParam("6 group density ]0,1]; wave advance [0,pi/2]",Vector)=(1,1,0,0)
		//_Wave6density("6 wave density ]0,1]",Range (0.001,1.0))=1
		//_Wave6advance("6 wave advance [0,pi/2]",Range (0.00,1.5707963))=1
		_Wave7Parameters("7  angle, radius, period and lambda",Vector)=(1.1,0.1, 8, 5)
		_Wave7GroupParam("7 group density ]0,1]; wave advance [0,pi/2]",Vector)=(1,1,0,0)
		//_Wave7density("7 wave density ]0,1]",Range (0.001,1.0))=1
		//_Wave7advance("7 wave advance [0,pi/2]",Range (0.00,1.5707963))=1
		_Wave8Parameters("8  angle, radius, period and lambda",Vector)=(1.2,0.1, 7, 4)
		_Wave8GroupParam("8 group density ]0,1]; wave advance [0,pi/2]",Vector)=(1,1,0,0)
		//_Wave8density("8 wave density ]0,1]",Range (0.001,1.0))=1
		//_Wave8advance("8 wave advance [0,pi/2]",Range (0.00,1.5707963))=1
		//_Wave9Parameters("9  angle, radius, period and lambda",Vector)=(0.0,0.1, 1.5, 1.5)
		//_Wave10Parameters("10 angle, radius, period and lambda",Vector)=(0.0,0.0, 1.5, 1.5)
		//_Wave11Parameters("11 angle, radius, period and lambda",Vector)=(0.0,0.0, 1.5, 1.5)
		//_Wave12Parameters("12 angle, radius, period and lambda",Vector)=(0.0,0.0, 1.5, 1.5)
				
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		
		CGPROGRAM
		#pragma surface surf BlinnPhong vertex:vert nolightmap
		#pragma target 3.0
		#pragma glsl
		
		float4 _DeepColor;
		sampler2D _WaveMap;
		samplerCUBE _Cube;
		float _windDir;
		float _offsetRot;
		float _mapscale;
		float4 _coeffsHF;
		//float _coefHF;
		//float _coefHF1;
		//float _coefHF2;
		//float _coefHF3;
		
		float _Specular;
		float _Gloss;
		
		float _dStart;
		float _dFall;
		
		float _WavesAmount;
		float _MinReflectivity;
		
        float4 _Wave1Parameters;
		float4 _Wave1GroupParam;
        //float _Wave1density;     
        //float _Wave1advance;        
        float4 _Wave2Parameters; 
		float4 _Wave2GroupParam;
        //float _Wave2density;    
        //float _Wave2advance;             
        float4 _Wave3Parameters; 
		float4 _Wave3GroupParam;
        //float _Wave3density;   
        //float _Wave3advance;              
        float4 _Wave4Parameters;
		float4 _Wave4GroupParam;
        //float _Wave4density;    
        //float _Wave4advance;      
        float4 _Wave5Parameters;
		float4 _Wave5GroupParam;
        //float _Wave5density;    
        //float _Wave5advance;      
        float4 _Wave6Parameters;
		float4 _Wave6GroupParam;
        //float _Wave6density;   
        //float _Wave6advance;       
        float4 _Wave7Parameters;
		float4 _Wave7GroupParam;
        //float _Wave7density;    
        //float _Wave7advance;      
        float4 _Wave8Parameters;
		float4 _Wave8GroupParam;
        //float _Wave8density;   
        //float _Wave8advance;       
        //float4 _Wave9Parameters;
        //float4 _Wave10Parameters;
        //float4 _Wave11Parameters;
        //float4 _Wave12Parameters;
        
		
	
		struct Input {
			float2 uv_WaveMap;
			float4 startPoint;
			float3 viewDir;
		};

//		void waveOffset(in float4 startPosition, out float3 deltaPosition, out float2 slope,
//						in float waveAngle, in float radius, in float periodDuration, in float periodSize) {
//			float cosWaveAngle = cos(waveAngle);
//			float sinWaveAngle = sin(waveAngle); 
//			float xIn = cosWaveAngle*startPosition.x + sinWaveAngle*startPosition.z;
//			const float TWOPI = 2.0 * 3.14159;
//			float k = TWOPI / periodSize;
//			float w = TWOPI / periodDuration;
//			float angle = w * _Time.y + k * xIn;
//			float radius2 = radius * _WavesAmount;
//			float xw = cos(angle)*radius2;
//			float yw = sin(angle)*radius2;
//		    float R = periodSize / TWOPI;
//			float localSlope = xw / (R - yw);						
//			deltaPosition = float3(cosWaveAngle*xw,yw,sinWaveAngle*xw);
//			slope = float2(localSlope * cosWaveAngle, localSlope * sinWaveAngle);
//		}

		float poorRand(in int x, in int z) { // random in [0,1]
			//const  float n=abs(fmod(x,1024)+983*fmod(z,1024));
			const  int n=((sign(x)*x)%1024)+983*((sign(z)*z)%1024);
			//return fmod(n * 12456 + 1234, 10000)*0.0001;
			return ((n * 12456 + 1234)%10000)*0.0001;
		}
		
		float4 poorRand4(in int x, in int z) { // random in [0,1]
			//const  float n=abs(fmod(x,1024)+983*fmod(z,1024));
			const int4 n=int4(
						((sign(x)*x)%1024)+983*((sign(z)*z)%1024),
						((sign(x+1)*(x+1))%1024)+983*((sign(z)*z)%1024),
						((sign(x)*x)%1024)+983*((sign(z+1)*(z+1))%1024),
						((sign(x+1)*(x+1))%1024)+983*((sign(z+1)*(z+1))%1024)
						);
			//return fmod(n * 12456 + 1234, 10000)*0.0001;
			return ((n * 12456 + 1234)%10000)*0.0001;
		}
		
		float instantPhase(in float theta) {
			//y=(0.5*(1-sin(phase)).^2)-1;
			float ip=0.5*(1-sin(theta));
			ip*=ip;
    		return ip-1;
		}
		
		
		float instantPhasePrim(in float theta) {
			// y=-0.5*cos(phase).*(1-sin(phase));
			float ip=0.5*(1-sin(theta));
			return -cos(theta)*ip;
		}
		
		float3 waveOffset_noSlope(in float4 startPosition, in float4 waveParameters, in float4 groupParam) {
		    const float waveAngle = waveParameters.x;
		    const float radius = waveParameters.y;
		    const float period = waveParameters.z;
		    const float lambda = waveParameters.w;
			const float k = 6.2831853 / lambda;
			const float w = 6.2831853 / period;
			const float cosWaveAngle = cos(waveAngle);
			const float sinWaveAngle = sin(waveAngle);
			const float xIn = cosWaveAngle*startPosition.x + sinWaveAngle*startPosition.z;
			const float gs=w*lambda*0.079577472; // =w/(2*k)
			const float Lg=lambda*6/groupParam.x;
			const float gx_ref=cosWaveAngle*gs*_Time.y; //fmod(cosWaveAngle*gs*_Time.y,Lg);
			const float gz_ref=sinWaveAngle*gs*_Time.y; //fmod(sinWaveAngle*gs*_Time.y,Lg);
			const int ix=(int)floor(2*(startPosition.x-gx_ref)/Lg);
			const int iz=(int)floor(2*(startPosition.z-gz_ref)/Lg);
			const float phase=k * xIn -w * _Time.y;
			// premiere vague (0,0)
			float gx=gx_ref+0.5*Lg*(ix);
			float gz=gz_ref+0.5*Lg*(iz);
			float cx=(startPosition.x-gx)*k*0.1;
			float cz=(startPosition.z-gz)*k*0.1;
			float gauss=exp(-(cx*cx+cz*cz));
			float index=poorRand(ix, iz); // random in [0,1] (always the same for ix and iy
			float angle = phase + index*6.2831853;
			float radius2 = radius * _WavesAmount * gauss;
			float advance=gauss*_WavesAmount*groupParam.y*instantPhase(angle);
			float xw = cos(angle+advance)*radius2;
			float yw = sin(angle)*radius2;
			
			// second groupe (1,0)
			gx=gx_ref+0.5*Lg*(ix+1);
			gz=gz_ref+0.5*Lg*(iz);
			cx=(startPosition.x-gx)*k*0.1;
			cz=(startPosition.z-gz)*k*0.1;
			gauss=exp(-(cx*cx+cz*cz));
			index=poorRand(ix+1, iz); // random in [0,1] (always the same for ix and iy
			angle = phase + index*6.2831853;
			radius2 = radius * _WavesAmount * gauss;
			advance=gauss*_WavesAmount*groupParam.y*instantPhase(angle);
			xw += cos(angle+advance)*radius2;
			yw += sin(angle)*radius2;
			
			//troisieme groupe (0,1)
			gx=gx_ref+0.5*Lg*(ix);
			gz=gz_ref+0.5*Lg*(iz+1);
			cx=(startPosition.x-gx)*k*0.1;
			cz=(startPosition.z-gz)*k*0.1;
			gauss=exp(-(cx*cx+cz*cz));
			index=poorRand(ix, iz+1); // random in [0,1] (always the same for ix and iy
			angle = phase + index*6.2831853;
			radius2 = radius * _WavesAmount * gauss;
			advance=gauss*_WavesAmount*groupParam.y*instantPhase(angle);
			xw += cos(angle+advance)*radius2;
			yw += sin(angle)*radius2;
			
			//quatrieme groupe (1,1)
			gx=gx_ref+0.5*Lg*(ix+1);
			gz=gz_ref+0.5*Lg*(iz+1);
			cx=(startPosition.x-gx)*k*0.1;
			cz=(startPosition.z-gz)*k*0.1;
			gauss=exp(-(cx*cx+cz*cz));
			index=poorRand(ix+1, iz+1); // random in [0,1] (always the same for ix and iy
			angle = phase + index*6.2831853;
			radius2 = radius * _WavesAmount * gauss;
			advance=gauss*_WavesAmount*groupParam.y*instantPhase(angle);
			xw += cos(angle+advance)*radius2;
			yw += sin(angle)*radius2;
		
			return float3(cosWaveAngle*xw,yw,sinWaveAngle*xw);
		}
		
		half2 waveOffset_slopeOnly(in float4 startPosition, in float dist, 
								   in float4 waveParameters, in float2 groupParam) {
			const float waveAngle = waveParameters.x;
		    const float radius = waveParameters.y;
		    const float period = waveParameters.z;
		    const float lambda = waveParameters.w;
			const float k = 6.2831853 / lambda;
			const float w = 6.2831853 / period;
			const float cosWaveAngle = cos(waveAngle);
			const float sinWaveAngle = sin(waveAngle);
			const float xIn = cosWaveAngle*startPosition.x + sinWaveAngle*startPosition.z;
			const float gs=w*lambda*0.079577472; // =w/(2*k)
			const float Lg=lambda*6/groupParam.x;
			const float gx_ref=cosWaveAngle*gs*_Time.y; //fmod(cosWaveAngle*gs*_Time.y,Lg);
			const float gz_ref=sinWaveAngle*gs*_Time.y; //fmod(sinWaveAngle*gs*_Time.y,Lg);
			const float ix=floor(2*(startPosition.x-gx_ref)/Lg);
			const float iz=floor(2*(startPosition.z-gz_ref)/Lg);
			const float phase=k * xIn -w * _Time.y;
			// premier groupe (0,0)
			float gx=gx_ref+0.5*Lg*(ix);
			float gz=gz_ref+0.5*Lg*(iz);
			float index=poorRand(ix, iz); // random in [0,1] (always the same for ix and iy
			float cx=(startPosition.x-gx)*k*0.1;
			float cz=(startPosition.z-gz)*k*0.1;
			float gauss=exp(-(cx*cx+cz*cz));
			float angle = phase + index*6.2831853;
			float _a=gauss*_WavesAmount;
			float radius2 = radius * _a;
			float cumsumRadius=radius2;
			float advance=_a*groupParam.y*instantPhase(angle);
			//xw=x+a*cos(x+phi*a*ip(x))
			//yw=a*sin(x)
			//dxw=(1-a*sin(x+phi*a*ip(x)).*(1+phi*a*ipprim(x)));
			//dyw=(a*cos(x));
			// dxw et dyw ne sont pas multipliés par k*radius2 car on fait le quotient à la fin pour la pente
			float dxw = (1-_a*sin(angle+advance))*(1+_a*groupParam.y*instantPhasePrim(angle));
			float dyw = _a*cos(angle);
			float localSlope=dyw/dxw;
			
			// second groupe (1,0)
			gx=gx_ref+0.5*Lg*(ix+1);
			gz=gz_ref+0.5*Lg*(iz);
			index=poorRand(ix+1, iz);
			cx=(startPosition.x-gx)*k*0.1;
			cz=(startPosition.z-gz)*k*0.1;
			gauss=exp(-(cx*cx+cz*cz));
			angle = phase + index*6.2831853;
			_a=gauss*_WavesAmount;
			radius2 = radius * _a;
			cumsumRadius+=radius2;
			advance=_a*groupParam.y*instantPhase(angle);
			dxw = (1-_a*sin(angle+advance)) * (1+_a*groupParam.y*instantPhasePrim(angle));
			dyw = _a*cos(angle);
			localSlope+=dyw/dxw;
			
			// troisieme groupe (0,1)
			gx=gx_ref+0.5*Lg*(ix);
			gz=gz_ref+0.5*Lg*(iz+1);
			index=poorRand(ix, iz+1);
			cx=(startPosition.x-gx)*k*0.1;
			cz=(startPosition.z-gz)*k*0.1;
			gauss=exp(-(cx*cx+cz*cz));
			angle = phase + index*6.2831853;
			_a=gauss*_WavesAmount;
			radius2 = radius * _a;
			cumsumRadius+=radius2;
			advance=_a*groupParam.y*instantPhase(angle);
			dxw = (1-_a*sin(angle+advance)) * (1+_a*groupParam.y*instantPhasePrim(angle));
			dyw = _a*cos(angle);
			localSlope+=dyw/dxw;
			
			// quatrieme groupe (1,1)
			gx=gx_ref+0.5*Lg*(ix+1);
			gz=gz_ref+0.5*Lg*(iz+1);
			index=poorRand(ix+1, iz+1);
			cx=(startPosition.x-gx)*k*0.1;
			cz=(startPosition.z-gz)*k*0.1;
			gauss=exp(-(cx*cx+cz*cz));
			angle = phase + index*6.2831853;
			_a=gauss*_WavesAmount;
			radius2 = radius * _a;
			cumsumRadius+=radius2;
			advance=_a*groupParam.y*instantPhase(angle);
			dxw = (1-_a*sin(angle+advance)) * (1+_a*groupParam.y*instantPhasePrim(angle));
			dyw = _a*cos(angle);
			localSlope+=dyw/dxw;
			
			// compute slope
		    //const half R = lambda * 0.15915494; 
		    //const half localSlope=-xw/max(0.0001,(R-yw));
			const half amount = max( min(1.0,  1.0 - (dist-_dStart*cumsumRadius)/(0.0001+_dFall*cumsumRadius) ),0.0);
			return -amount*half2(localSlope * cosWaveAngle, localSlope * sinWaveAngle);
		}

	
//		void wavesOffset(in float4 startPosition, out float3 deltaPosition, out float2 slope) {
//		  slope = float2(0.0,0.0);
//		  float2 oneSlope;
//		  float3 oneDeltaPosition;
//		  waveOffset(startPosition, oneDeltaPosition, oneSlope, _Wave1Angle, _Wave1Radius, _Wave1PeriodDuration, _Wave1PeriodSize);
//		  deltaPosition = oneDeltaPosition;
//		  slope += oneSlope;
//		  waveOffset(startPosition, oneDeltaPosition, oneSlope, _Wave2Angle, _Wave2Radius, _Wave2PeriodDuration, _Wave2PeriodSize);
//		  deltaPosition += oneDeltaPosition;
//		  slope += oneSlope;
//		  waveOffset(startPosition, oneDeltaPosition, oneSlope, _Wave3Angle, _Wave3Radius, _Wave3PeriodDuration, _Wave3PeriodSize);
//		  deltaPosition += oneDeltaPosition;
//		  slope += oneSlope;
//		  waveOffset(startPosition, oneDeltaPosition, oneSlope, _Wave4Angle, _Wave4Radius, _Wave4PeriodDuration, _Wave4PeriodSize);
//		  deltaPosition += oneDeltaPosition;
//		  slope += oneSlope;
//		  waveOffset(startPosition, oneDeltaPosition, oneSlope, _Wave5Angle, _Wave5Radius, _Wave5PeriodDuration, _Wave5PeriodSize);
//		  deltaPosition += oneDeltaPosition;
//		  slope = slope + oneSlope;
//		}
		

		
		float3 wavesOffset_noSlope(in float4 startPosition) {
		  return (
		      waveOffset_noSlope(startPosition, _Wave1Parameters, _Wave1GroupParam)
		    + waveOffset_noSlope(startPosition, _Wave2Parameters, _Wave2GroupParam)
            + waveOffset_noSlope(startPosition, _Wave3Parameters, _Wave3GroupParam)
            + waveOffset_noSlope(startPosition, _Wave4Parameters, _Wave4GroupParam)
            + waveOffset_noSlope(startPosition, _Wave5Parameters, _Wave5GroupParam)
            + waveOffset_noSlope(startPosition, _Wave6Parameters, _Wave6GroupParam)
            + waveOffset_noSlope(startPosition, _Wave7Parameters, _Wave7GroupParam)
            + waveOffset_noSlope(startPosition, _Wave8Parameters, _Wave8GroupParam)
		  );
		}
		
		half2 wavesOffset_slopeOnly(in float4 startPosition, in float dist) {
			return (
				  waveOffset_slopeOnly(startPosition, dist, _Wave1Parameters, _Wave1GroupParam)
				+ waveOffset_slopeOnly(startPosition, dist, _Wave2Parameters, _Wave2GroupParam)
				+ waveOffset_slopeOnly(startPosition, dist, _Wave3Parameters, _Wave3GroupParam)
				+ waveOffset_slopeOnly(startPosition, dist, _Wave4Parameters, _Wave4GroupParam)
				+ waveOffset_slopeOnly(startPosition, dist, _Wave5Parameters, _Wave5GroupParam)
				+ waveOffset_slopeOnly(startPosition, dist, _Wave6Parameters, _Wave6GroupParam)
				+ waveOffset_slopeOnly(startPosition, dist, _Wave7Parameters, _Wave7GroupParam)
				+ waveOffset_slopeOnly(startPosition, dist, _Wave8Parameters, _Wave8GroupParam)
				);
		}

		void vert(inout appdata_tan v, out Input o) {
		
		  float4 world_v = mul (_Object2World, v.vertex);	  	  
		  UNITY_INITIALIZE_OUTPUT(Input,o);
		  o.startPoint = world_v;
		  v.texcoord = float4(world_v.x, world_v.z, 0.0, 0.0);
	
		  world_v.xyz += wavesOffset_noSlope(world_v);			
		  v.vertex = mul( _World2Object, world_v );
		  
          //do not modify the normal, which is wrong and leads to a wrong tangent space, but a useful one as we compute the normal per pixel
          //and Unity wants a tangent space when normal map are used.
          //This is stupid : we got normal and tangent interpolation of constant values, how to avoid that ?
		}
		
		void surf (Input IN, inout SurfaceOutput o) {
		    half t = _Time.y;
		    half windcos=cos((_windDir+_offsetRot));
		    half windsin=sin((_windDir+_offsetRot));
		    // normal map
		    half2 uv_wind=_mapscale*half2(( (windcos*IN.uv_WaveMap.x) + (windsin*IN.uv_WaveMap.y)),
		    					(-(windsin*IN.uv_WaveMap.x) + (windcos*IN.uv_WaveMap.y))
							   );
			half2 wind=(0.01*t/(_mapscale))*half2(windsin,windcos);
		    half2 hf1=0.2*uv_wind-1.0*wind;
		    half2 hf2=0.75*uv_wind-0.81*wind;
		    half2 hf3=2*uv_wind-0.74*wind;
			
			half3 n =  normalize((_coeffsHF.y) * UnpackNormal( tex2D(_WaveMap,hf1) ) +
			                     (_coeffsHF.z) * UnpackNormal( tex2D(_WaveMap,hf2) ) +
			                     (_coeffsHF.w) * UnpackNormal( tex2D(_WaveMap,hf3) )
			                     );		 
			
			// sea state normal
			float d = length(IN.viewDir);
			half2 slope = wavesOffset_slopeOnly(IN.startPoint,d);
			float4 wavesNormal =  normalize( float4(slope.x,slope.y,1.0,0.0));
			//o.Normal = normalize(0.01*n+half3(0.0,0.0,1.0)); 
			//float cosT0 = 1.0 - dot(normalize(IN.viewDir),float3(0.0,0.0,1.0));
            //o.Normal = normalize( cosT0*float3(0.0,0.0,1.0) + 
			 //             		  (1.0-cosT0) * normalize((0.5*n+normalize(wavesNormal.xyz))) ); 
		
	        
			//float amount = max(min(1.0, 1.0 - (d-_dStart)/_dFall),0.0);
            //o.Normal =  (1-amount)*float3(0.0,0.0,1.0) + amount*normalize(n+normalize(wavesNormal.xyz)); 
            
            // combine diffent contributions to normal
            o.Normal = normalize((_coeffsHF.x+1)*n+2*normalize(wavesNormal.xyz)); // 2 pour maxcoefHF
			float cosT = 1.0 - max(0.0,dot(normalize(IN.viewDir),o.Normal));
            //o.Normal = normalize(wavesNormal.xyz); 
			float3 refl = reflect(IN.viewDir,o.Normal);
			half3 c = texCUBE (_Cube,  float3(refl.x,-refl.z,refl.y)).rgb;
			
			float fresnel = _MinReflectivity + (1-_MinReflectivity) * cosT * cosT * cosT * cosT * cosT;
			o.Albedo = (1-fresnel)*_DeepColor.rgb+fresnel*c.rgb;
		    o.Specular = _Specular;
		    o.Gloss = _Gloss;
		}
		ENDCG
	} 
	FallBack "Diffuse"

}
