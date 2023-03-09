Shader "Unlit Color Only" {

    Properties{
        [PerRendererData]_Color("Color", Color) = (1,1,1)
    }

        SubShader{
            Color[_Color]
            Pass {}
    }

}
